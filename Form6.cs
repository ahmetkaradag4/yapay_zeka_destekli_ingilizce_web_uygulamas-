using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form6 : Form
    {
        string ogrenciNo;
        string aktifKur;
        double hesaplananKonusmaPuani;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;

        // --- SÜRE DEĞİŞKENLERİ ---
        private System.Windows.Forms.Timer sinavTimer;
        private int kalanSaniye = 1500; // 25 dakika * 60 saniye

        // ── Veri yapısı ──
        struct Soru
        {
            public string ParagrafMetni;
            public string SoruMetni;
            public string[] Sikklar;
            public int DogruCevapIndex;
        }

        List<Soru> sorular = new List<Soru>();

        public Form6(string no, string kur, double konusmaPuani)
        {
            ogrenciNo = no;
            aktifKur = kur;
            hesaplananKonusmaPuani = konusmaPuani;

            SorulariYukle();

            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            webView21.Dock = DockStyle.Fill;
            this.Controls.Add(webView21);

            this.Text = "OkulSis - İngilizce Sınavı";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            InitializeTimer();
            InitializeWebView();
        }

        // ── ZAMANLAYICI METOTLARI ──
        private void InitializeTimer()
        {
            sinavTimer = new System.Windows.Forms.Timer();
            sinavTimer.Interval = 1000; // Saniyede 1 kez çalışır
            sinavTimer.Tick += SinavTimer_Tick;
        }

        private void SinavTimer_Tick(object sender, EventArgs e)
        {
            kalanSaniye--;
            TimeSpan ts = TimeSpan.FromSeconds(kalanSaniye);
            string formatliZaman = ts.ToString(@"mm\:ss");

            // Kalan süreyi pencere başlığında göstermeye devam edebiliriz (arkada kalsa bile)
            this.Text = $"OkulSis - İngilizce Sınavı | Kalan Süre: {formatliZaman}";

            // HTML tarafına anlık zamanı gönderiyoruz
            string zamanJson = "{\"tip\":\"zamanGuncelle\",\"zaman\":\"" + formatliZaman + "\"}";
            webView21.CoreWebView2.PostWebMessageAsString(zamanJson);

            if (kalanSaniye <= 0)
            {
                sinavTimer.Stop();
                MessageBox.Show("Sınav süreniz (25 dakika) doldu! Mevcut cevaplarınızla sınav otomatik olarak sonlandırılıyor.", "Süre Bitti", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // HTML tarafına sürenin bittiğini bildiriyoruz.
                string sureBittiJson = "{\"tip\":\"sureBitti\"}";
                webView21.CoreWebView2.PostWebMessageAsString(sureBittiJson);
            }
        }

        private async void InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            string path = System.IO.Path.Combine(Application.StartupPath, "exam.html");
            webView21.CoreWebView2.Navigate("file:///" + path.Replace("\\", "/"));
            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;

            webView21.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                var sorularJson = new System.Text.StringBuilder();
                sorularJson.Append("[");
                for (int i = 0; i < sorular.Count; i++)
                {
                    if (i > 0) sorularJson.Append(",");
                    var sr = sorular[i];
                    sorularJson.Append("{");
                    sorularJson.Append("\"paragraf\":" + JsonSerializer.Serialize(sr.ParagrafMetni ?? "") + ",");
                    sorularJson.Append("\"soru\":" + JsonSerializer.Serialize(sr.SoruMetni) + ",");
                    sorularJson.Append("\"sikklar\":" + JsonSerializer.Serialize(sr.Sikklar) + ",");
                    sorularJson.Append("\"dogru\":" + sr.DogruCevapIndex);
                    sorularJson.Append("}");
                }
                sorularJson.Append("]");

                // sureDakika bilgisini de init ile birlikte HTML tarafına gönderdik
                string initJson = "{\"tip\":\"init\",\"kur\":\"" + aktifKur + "\",\"sureDakika\":25,\"sorular\":" + sorularJson + "}";
                webView21.CoreWebView2.PostWebMessageAsString(initJson);

                // Sayfa yüklendiğinde geri sayım başlasın
                sinavTimer.Start();
            };
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.TryGetWebMessageAsString();
            this.Invoke(new Action(() =>
            {
                using var doc = JsonDocument.Parse(json);
                string tip = doc.RootElement.GetProperty("tip").GetString();

                if (tip == "sinavBitti")
                {
                    sinavTimer.Stop(); // Sınav bittiğinde zamanlayıcıyı durduruyoruz
                    double puan = doc.RootElement.GetProperty("puan").GetDouble();
                    bool gecti = doc.RootElement.GetProperty("gecti").GetBoolean();

                    DatabaseLayer.PuanKaydet(ogrenciNo, aktifKur, puan, hesaplananKonusmaPuani);
                }
                else if (tip == "cikis")
                {
                    sinavTimer.Stop();
                    Form3 anaMenu = new Form3(ogrenciNo);
                    anaMenu.Show();
                    this.Close();
                }
                else if (tip == "kapat")
                {
                    sinavTimer.Stop();
                    Form3 anaMenu2 = new Form3(ogrenciNo);
                    anaMenu2.Show();
                    this.Close();
                }
            }));
        }

        // ── YARDIMCI METOTLAR: Havuzdan Rastgele Soru Seçme ve Numaralandırma ──
        private void HavuzdanSecVeEkle(List<Soru> havuz, int miktar)
        {
            Random rnd = new Random();
            var secilenler = havuz.OrderBy(x => rnd.Next()).Take(miktar).ToList();
            for (int i = 0; i < secilenler.Count; i++)
            {
                var s = secilenler[i];
                s.SoruMetni = (sorular.Count + 1) + "- " + s.SoruMetni;
                sorular.Add(s);
            }
        }

        private void SabitSorulariEkle(List<Soru> sabitListesi)
        {
            for (int i = 0; i < sabitListesi.Count; i++)
            {
                var s = sabitListesi[i];
                s.SoruMetni = (sorular.Count + 1) + "- " + s.SoruMetni;
                sorular.Add(s);
            }
        }

        // ── SORULAR ──
        private void SorulariYukle()
        {
            if (aktifKur == "A1")
            {
                List<Soru> gramerHavuzu = new List<Soru>();

                // Eski A1 Soruları (25 Adet)
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She usually (...) to work by bus.", Sikklar = new string[] { "go", "goes", "is going", "went" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I (...) my homework right now.", Sikklar = new string[] { "do", "am doing", "did", "have done" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "There (...) two books on the table.", Sikklar = new string[] { "is", "are", "was", "be" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We don't have (...) milk left.", Sikklar = new string[] { "some", "any", "many", "few" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "My sister is (...) than me.", Sikklar = new string[] { "tall", "tallest", "taller", "more tall" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "What time (...) you get up?", Sikklar = new string[] { "do", "does", "are", "did" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I have lived here (...) 2020.", Sikklar = new string[] { "for", "since", "from", "at" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She can't drive, (...) she?", Sikklar = new string[] { "can", "does", "is", "did" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "There isn't (...) sugar in my coffee.", Sikklar = new string[] { "many", "much", "a few", "several" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He was tired, (...) he went to bed early.", Sikklar = new string[] { "but", "because", "so", "although" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "This is the restaurant (...) we had dinner yesterday.", Sikklar = new string[] { "who", "which", "where", "what" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I'm looking forward to (...) you soon.", Sikklar = new string[] { "see", "seeing", "saw", "to see" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She (...) to the gym three times a week.", Sikklar = new string[] { "go", "goes", "is going", "went" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We were watching TV when the phone (...)", Sikklar = new string[] { "ring", "rings", "rang", "is ringing" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "There are (...) people waiting outside.", Sikklar = new string[] { "much", "little", "a lot of", "any" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If it (...) tomorrow, we will stay home.", Sikklar = new string[] { "rain", "rains", "rained", "raining" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He bought (...) umbrella because it was raining.", Sikklar = new string[] { "a", "an", "the", "----" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is interested (...) learning Spanish.", Sikklar = new string[] { "in", "on", "at", "to" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "That's the girl (...) brother is my friend.", Sikklar = new string[] { "who", "which", "whose", "where" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I've never (...) sushi before.", Sikklar = new string[] { "eat", "eaten", "ate", "eating" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "My father works (...) an engineer.", Sikklar = new string[] { "as", "like", "for", "with" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "There isn't (...) time. Hurry up!", Sikklar = new string[] { "enough", "too", "very", "so" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She speaks English very (...)", Sikklar = new string[] { "good", "well", "better", "best" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We went to the cinema (...) Friday night.", Sikklar = new string[] { "in", "at", "on", "by" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He didn't come to the party (...) he was sick.", Sikklar = new string[] { "but", "so", "because", "and" }, DogruCevapIndex = 2 });

                // Yeni A1 Soruları (Havuz Txt'den - 25 Adet)
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ a student.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ very happy today.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ at school now.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ friends.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ a doctor.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ coffee every morning.", Sikklar = new string[] { "drink", "drinks", "drinking", "drank" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ to school every day.", Sikklar = new string[] { "go", "goes", "going", "went" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ football on weekends.", Sikklar = new string[] { "play", "plays", "playing", "played" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ TV in the evening.", Sikklar = new string[] { "watch", "watches", "watching", "watched" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ pizza.", Sikklar = new string[] { "like", "likes", "liking", "liked" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The book is ___ the table.", Sikklar = new string[] { "in", "on", "at", "under" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I go to school ___ bus.", Sikklar = new string[] { "with", "by", "on", "in" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is ___ the room.", Sikklar = new string[] { "in", "on", "at", "by" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I have ___ dog.", Sikklar = new string[] { "a", "an", "the", "-" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She has ___ apple.", Sikklar = new string[] { "a", "an", "the", "-" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ watching TV now.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ playing football.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ studying English.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ to the cinema yesterday.", Sikklar = new string[] { "go", "goes", "went", "going" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ his homework yesterday.", Sikklar = new string[] { "do", "does", "did", "doing" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Big” kelimesinin zıt anlamı nedir?", Sikklar = new string[] { "long", "tall", "small", "short" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Teacher” ne demektir?", Sikklar = new string[] { "öğrenci", "öğretmen", "doktor", "mühendis" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Buy” ne demektir?", Sikklar = new string[] { "satmak", "almak", "vermek", "yapmak" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ very tired.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ my best friend.", Sikklar = new string[] { "am", "is", "are", "be" }, DogruCevapIndex = 1 });

                // Rastgele 15 gramer sorusunu ekle
                HavuzdanSecVeEkle(gramerHavuzu, 15);

                // Sabit A1 Paragraf Soruları
                List<Soru> paragrafSorulari = new List<Soru>();
                string emmaMetni = "Emma is 23 years old and lives in a small town near London. She works as a nurse in a hospital. She usually works five days a week, but sometimes she works at night. In her free time, she enjoys reading books and meeting her friends. Last summer, she travelled to Italy and visited Rome and Florence. She says it was the best holiday of her life. Next year, she is planning to visit Spain.";
                paragrafSorulari.Add(new Soru { ParagrafMetni = emmaMetni, SoruMetni = "Where does Emma live?", Sikklar = new string[] { "In London", "Near London", "In Italy", "In Spain" }, DogruCevapIndex = 1 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = emmaMetni, SoruMetni = "What does she do?", Sikklar = new string[] { "Teacher", "Doctor", "Nurse", "Student" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = emmaMetni, SoruMetni = "What did she do last summer?", Sikklar = new string[] { "Stayed home", "Worked at night", "Travelled to Italy", "Visited Spain" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = emmaMetni, SoruMetni = "How does she feel about her holiday?", Sikklar = new string[] { "It was boring", "It was difficult", "It was the best", "It was expensive" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = emmaMetni, SoruMetni = "What is she planning to do next year?", Sikklar = new string[] { "Move to Italy", "Visit Spain", "Change her job", "Study nursing" }, DogruCevapIndex = 1 });
                SabitSorulariEkle(paragrafSorulari);

                // Sabit A1 Dinleme Soruları
                List<Soru> dinlemeSorulari = new List<Soru>();
                string listeningMetni = "🎧 Lütfen az önce sesli olarak okuduğunuz/çalıştığınız metni hatırlayarak soruları cevaplayınız.";
                dinlemeSorulari.Add(new Soru { ParagrafMetni = listeningMetni, SoruMetni = "According to the speaker, what is the most important part of learning a language?", Sikklar = new string[] { "Having a good teacher", "Spending a lot of time", "Buying expensive books", "Living in a different country" }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = listeningMetni, SoruMetni = "How does the speaker feel when they cannot understand someone?", Sikklar = new string[] { "Happy and excited", "Bored and tired", "Sad or frustrated", "Relaxed and calm" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = listeningMetni, SoruMetni = "What does the speaker say about making mistakes?", Sikklar = new string[] { "You should try to never make mistakes.", "Mistakes are bad for your English.", "You cannot improve if you don't make mistakes.", "Only students make mistakes, not teachers." }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = listeningMetni, SoruMetni = "What is the speaker's advice for choosing books or videos?", Sikklar = new string[] { "Always choose the most difficult ones.", "Choose topics that are interesting to you.", "Only read grammar books.", "Watch videos that your teacher recommends." }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = listeningMetni, SoruMetni = "Which of the following is NOT one of the speaker's tips?", Sikklar = new string[] { "Reading books that are interesting.", "Finding an English practice group.", "Watching YouTube videos about your hobbies.", "Memorizing all the grammar rules." }, DogruCevapIndex = 3 });
                SabitSorulariEkle(dinlemeSorulari);
            }
            else if (aktifKur == "A2")
            {
                List<Soru> gramerHavuzu = new List<Soru>();

                // Eski A2 Soruları
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I have lived here (...) 2020.", Sikklar = new string[] { "for", "since", "from", "at" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is interested (...) learning Spanish.", Sikklar = new string[] { "in", "on", "at", "to" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I've never (...) sushi before.", Sikklar = new string[] { "eat", "eaten", "ate", "eating" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We (...) to the beach yesterday.", Sikklar = new string[] { "go", "went", "going", "gone" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Have you ever (...) to Paris?", Sikklar = new string[] { "be", "been", "went", "go" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He is (...) than his brother.", Sikklar = new string[] { "tall", "tallest", "more tall", "taller" }, DogruCevapIndex = 3 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "This is the (...) movie I have ever seen.", Sikklar = new string[] { "good", "better", "best", "most good" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If it rains, I (...) at home.", Sikklar = new string[] { "stayed", "will stay", "am stay", "staying" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You (...) smoke in the hospital.", Sikklar = new string[] { "don't have to", "mustn't", "should", "can" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "(...) you help me with this box?", Sikklar = new string[] { "Do", "Are", "Can", "Have" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I was watching TV when the phone (...).", Sikklar = new string[] { "rings", "ring", "rang", "ringing" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She doesn't have (...) money.", Sikklar = new string[] { "some", "any", "many", "a few" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "How (...) apples are there?", Sikklar = new string[] { "much", "many", "some", "any" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Look at those clouds! It (...).", Sikklar = new string[] { "rains", "is going to rain", "rained", "will rain" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He drives very (...).", Sikklar = new string[] { "slow", "slower", "slowly", "slowest" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The letter (...) yesterday.", Sikklar = new string[] { "was sent", "sent", "is sent", "sends" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We haven't seen them (...) three years.", Sikklar = new string[] { "since", "for", "in", "from" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They (...) play tennis on Sundays.", Sikklar = new string[] { "is usually", "usually", "usually are", "are usually" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "My car is not as fast (...) yours.", Sikklar = new string[] { "than", "then", "like", "as" }, DogruCevapIndex = 3 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "What (...) you do last weekend?", Sikklar = new string[] { "do", "does", "did", "are" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I would like (...) tea, please.", Sikklar = new string[] { "a", "an", "some", "any" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The man (...) lives next door is a doctor.", Sikklar = new string[] { "which", "who", "where", "whose" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I think she (...) win the race.", Sikklar = new string[] { "is going", "will", "going to", "wins" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You (...) to wear a uniform at this school.", Sikklar = new string[] { "must", "should", "have", "can" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Let's go to the cinema, (...) ?", Sikklar = new string[] { "do we", "shall we", "let we", "are we" }, DogruCevapIndex = 1 });

                // Yeni A2 Soruları (Havuz Txt'den)
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ to school every day.", Sikklar = new string[] { "go", "goes", "going", "went" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ TV now.", Sikklar = new string[] { "watch", "watched", "are watching", "watches" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ my homework yesterday.", Sikklar = new string[] { "do", "did", "doing", "does" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ dinner at the moment.", Sikklar = new string[] { "have", "has", "are having", "had" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ to Ankara last week.", Sikklar = new string[] { "go", "goes", "went", "going" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The book is ___ the table.", Sikklar = new string[] { "in", "on", "at", "under" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I go to school ___ bus.", Sikklar = new string[] { "with", "by", "on", "at" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is waiting ___ the bus stop.", Sikklar = new string[] { "in", "on", "at", "under" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We have a meeting ___ Monday.", Sikklar = new string[] { "in", "on", "at", "by" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The cat is ___ the box.", Sikklar = new string[] { "in", "on", "at", "by" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ swim very well.", Sikklar = new string[] { "can", "must", "should", "need" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You ___ study for the exam.", Sikklar = new string[] { "can", "must", "may", "could" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ speak English.", Sikklar = new string[] { "can", "must", "should", "need" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ wear uniforms at school.", Sikklar = new string[] { "can", "must", "should", "may" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You ___ eat more vegetables.", Sikklar = new string[] { "can", "must", "should", "need" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Big” kelimesinin zıt anlamı:", Sikklar = new string[] { "small", "long", "tall", "short" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Happy” kelimesinin zıt anlamı:", Sikklar = new string[] { "angry", "sad", "tired", "hungry" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Teacher” ne demektir?", Sikklar = new string[] { "öğrenci", "öğretmen", "doktor", "mühendis" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Buy” ne demektir?", Sikklar = new string[] { "satmak", "almak", "vermek", "yapmak" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Fast” ne demektir?", Sikklar = new string[] { "yavaş", "hızlı", "büyük", "küçük" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ coffee every morning.", Sikklar = new string[] { "drink", "drinks", "drinking", "drank" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ playing football.", Sikklar = new string[] { "like", "likes", "liking", "liked" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ in Istanbul.", Sikklar = new string[] { "lives", "live", "living", "lived" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ to the cinema yesterday.", Sikklar = new string[] { "go", "goes", "went", "going" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ a new car.", Sikklar = new string[] { "have", "has", "having", "had" }, DogruCevapIndex = 1 });

                // Rastgele 15 gramer sorusunu ekle
                HavuzdanSecVeEkle(gramerHavuzu, 15);

                // Sabit A2 Paragraf Soruları
                List<Soru> paragrafSorulari = new List<Soru>();
                string a2ReadingMetni = "Mark is a software engineer who lives in New York. He loves his job because he enjoys solving computer problems. Last year, he travelled to Japan for a business trip. He tried traditional Japanese food and visited beautiful temples. Next month, his company is sending him to Germany. He is currently learning German so he can communicate with his new team. In his free time, Mark plays the guitar and writes his own songs.";
                paragrafSorulari.Add(new Soru { ParagrafMetni = a2ReadingMetni, SoruMetni = "Where does Mark live?", Sikklar = new string[] { "In New York", "In London", "In Japan", "In Germany" }, DogruCevapIndex = 0 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = a2ReadingMetni, SoruMetni = "Why does he love his job?", Sikklar = new string[] { "Because he travels a lot", "Because it pays well", "Because he enjoys solving computer problems", "Because he works from home" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = a2ReadingMetni, SoruMetni = "Where did he travel last year?", Sikklar = new string[] { "Germany", "Japan", "London", "Spain" }, DogruCevapIndex = 1 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = a2ReadingMetni, SoruMetni = "Why is he learning German?", Sikklar = new string[] { "To read books", "To live there forever", "To talk to his family", "To communicate with his new team" }, DogruCevapIndex = 3 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = a2ReadingMetni, SoruMetni = "What does he do in his free time?", Sikklar = new string[] { "Plays the guitar", "Plays video games", "Watches TV", "Cooks food" }, DogruCevapIndex = 0 });
                SabitSorulariEkle(paragrafSorulari);

                // Sabit A2 Dinleme Soruları
                List<Soru> dinlemeSorulari = new List<Soru>();
                string a2ListeningMetni = "🎧 Lütfen az önce sesli olarak okuduğunuz/çalıştığınız metni hatırlayarak soruları cevaplayınız.";
                dinlemeSorulari.Add(new Soru { ParagrafMetni = a2ListeningMetni, SoruMetni = "When did the speaker first start taking fashion seriously and paying attention to their clothes?", Sikklar = new string[] { "In elementary school", "In middle school", "In high school", "In college" }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = a2ListeningMetni, SoruMetni = "What kind of clothing phase does the speaker describe as 'not a good phase' for them?", Sikklar = new string[] { "Wearing only black clothes", "Wearing very tight clothes", "Wearing really baggy clothes", "Wearing formal suits" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = a2ListeningMetni, SoruMetni = "How did the speaker's style change once they reached high school?", Sikklar = new string[] { "Professional business attire", "Expensive designer suits", "Matching shoes with shirts (basketball)", "Stopped wearing shoes" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = a2ListeningMetni, SoruMetni = "Where did the speaker have their first job at the age of 17?", Sikklar = new string[] { "H&M", "Hollister", "Zara", "JCPenney" }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = a2ListeningMetni, SoruMetni = "Why does the speaker like shopping at 'department stores'?", Sikklar = new string[] { "They are very small and quiet.", "They only sell one brand.", "Huge selection of clothes and other items.", "Always located outside of malls." }, DogruCevapIndex = 2 });
                SabitSorulariEkle(dinlemeSorulari);
            }
            else if (aktifKur == "B1")
            {
                List<Soru> gramerHavuzu = new List<Soru>();

                // Eski B1 Soruları
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If I (...) about the traffic, I would have left earlier.", Sikklar = new string[] { "knew", "had known", "know", "would know" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She suggested (...) to the new restaurant downtown.", Sikklar = new string[] { "to go", "go", "going", "went" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The project, (...) was completed last week, received positive feedback.", Sikklar = new string[] { "who", "which", "where", "what" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "There is very little (...) about this topic online.", Sikklar = new string[] { "informations", "information", "informative", "informing" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He speaks as if he (...) everything.", Sikklar = new string[] { "knows", "knew", "had known", "will know" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I regret (...) her the truth.", Sikklar = new string[] { "telling", "to tell", "tell", "told" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The meeting was cancelled (...) the manager's absence.", Sikklar = new string[] { "although", "because", "because of", "despite" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Not only (...) late, but he also forgot the documents.", Sikklar = new string[] { "he arrived", "did he arrive", "he did arrive", "arrived he" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is one of the few employees who (...) remotely.", Sikklar = new string[] { "works", "work", "working", "has worked" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "By next year, I (...) my degree.", Sikklar = new string[] { "complete", "completed", "will have completed", "am completing" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The film was (...) boring that we left before it ended.", Sikklar = new string[] { "such", "very", "too", "so" }, DogruCevapIndex = 3 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I'm looking forward to (...) from you soon.", Sikklar = new string[] { "hear", "hearing", "to hear", "heard" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He denied (...) the mistake.", Sikklar = new string[] { "make", "to make", "making", "made" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The company aims to reduce costs (...) increasing productivity.", Sikklar = new string[] { "despite", "while", "because", "although" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She asked me where I (...) the day before.", Sikklar = new string[] { "was", "have been", "had been", "am" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "There were significantly (...) applicants this year than last year.", Sikklar = new string[] { "fewer", "less", "little", "much" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Hardly (...) the meeting when the fire alarm went off.", Sikklar = new string[] { "we had started", "had we started", "we started", "did we start" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He didn't pass the exam, (...) he had studied hard.", Sikklar = new string[] { "although", "because", "so", "unless" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I'm not convinced (...) his explanation.", Sikklar = new string[] { "by", "with", "about", "for" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The more you practice, (...) you become.", Sikklar = new string[] { "better", "the better", "the best", "more better" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If she (...) more confident, she would apply for the job.", Sikklar = new string[] { "is", "were", "had been", "will be" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He apologized for (...) late.", Sikklar = new string[] { "to be", "be", "being", "been" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She prefers working from home (...) commuting every day.", Sikklar = new string[] { "than", "to", "rather", "more" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The book was far more interesting (...) I expected.", Sikklar = new string[] { "that", "than", "what", "as" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He managed to solve the problem (...) the limited time.", Sikklar = new string[] { "despite", "because", "although", "unless" }, DogruCevapIndex = 0 });

                // Yeni B1 Soruları (Havuz Txt'den)
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ my homework when you called me.", Sikklar = new string[] { "do", "was doing", "did", "am doing" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ to Istanbul three times.", Sikklar = new string[] { "went", "has gone", "has been", "goes" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If it ___ tomorrow, we will stay at home.", Sikklar = new string[] { "rain", "rains", "rained", "raining" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ dinner when I arrived.", Sikklar = new string[] { "have", "had", "were having", "are having" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ here since 2020.", Sikklar = new string[] { "live", "lived", "am living", "have lived" }, DogruCevapIndex = 3 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You ___ smoke here. It’s forbidden.", Sikklar = new string[] { "can", "must", "mustn’t", "should" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We ___ finish this project today. It’s very important.", Sikklar = new string[] { "can", "must", "could", "may" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ be at home now, I saw her car.", Sikklar = new string[] { "must", "can’t", "may", "should" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You ___ see a doctor if you feel sick.", Sikklar = new string[] { "must", "should", "can", "may" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ drive when he was 18.", Sikklar = new string[] { "can", "could", "must", "should" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Give up” ne demektir?", Sikklar = new string[] { "başlamak", "pes etmek", "almak", "vermek" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is looking ___ her keys.", Sikklar = new string[] { "at", "for", "on", "in" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Improve” ne demektir?", Sikklar = new string[] { "kötüleşmek", "geliştirmek", "unutmak", "kaybetmek" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ his jacket because it was cold.", Sikklar = new string[] { "put on", "put off", "take on", "take off" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Borrow” ne demektir?", Sikklar = new string[] { "ödünç vermek", "ödünç almak", "satın almak", "satmak" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She didn’t go to the party ___ she was tired.", Sikklar = new string[] { "but", "because", "so", "and" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I like coffee, ___ I don’t like tea.", Sikklar = new string[] { "because", "but", "so", "and" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We were late, ___ we missed the bus.", Sikklar = new string[] { "because", "but", "so", "and" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He studied hard, ___ he passed the exam.", Sikklar = new string[] { "but", "because", "so", "and" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She was hungry ___ she ate a sandwich.", Sikklar = new string[] { "because", "but", "so", "and" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I enjoy ___ books in my free time.", Sikklar = new string[] { "read", "reading", "to read", "reads" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is interested ___ music.", Sikklar = new string[] { "on", "at", "in", "for" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They decided ___ a new car.", Sikklar = new string[] { "buy", "buying", "to buy", "bought" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He is ___ than his brother.", Sikklar = new string[] { "tall", "taller", "tallest", "more tall" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "This is the ___ movie I have ever seen.", Sikklar = new string[] { "good", "better", "best", "well" }, DogruCevapIndex = 2 });

                // Rastgele 15 gramer sorusunu ekle
                HavuzdanSecVeEkle(gramerHavuzu, 15);

                // Sabit B1 Paragraf Soruları
                List<Soru> paragrafSorulari = new List<Soru>();
                string b1ReadingMetni = "Laura recently started working remotely for an international marketing company. At first, she was excited about avoiding daily commuting and having more flexible hours. However, after a few months, she realized that working from home required strong self-discipline and time management skills. She sometimes found it difficult to separate her professional responsibilities from her personal life. Despite these challenges, she appreciates the opportunity to collaborate with colleagues from different countries and cultures.";
                paragrafSorulari.Add(new Soru { ParagrafMetni = b1ReadingMetni, SoruMetni = "Why was Laura initially excited about remote work?", Sikklar = new string[] { "She wanted to travel abroad", "She liked flexible hours and no commuting", "She disliked her colleagues", "She wanted a higher salary" }, DogruCevapIndex = 1 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b1ReadingMetni, SoruMetni = "What challenge did she face after a few months?", Sikklar = new string[] { "Technical problems", "Communication issues", "Managing her time effectively", "Lack of experience" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b1ReadingMetni, SoruMetni = "What does separating professional responsibilities from personal life imply?", Sikklar = new string[] { "She worked too many jobs", "She struggled to balance work and private life", "She changed her career", "She moved to another country" }, DogruCevapIndex = 1 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b1ReadingMetni, SoruMetni = "What positive aspect of remote work does she mention?", Sikklar = new string[] { "Higher income", "Shorter working hours", "Cultural collaboration", "Easier tasks" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b1ReadingMetni, SoruMetni = "What is Laura's overall opinion about remote work?", Sikklar = new string[] { "It is perfect for everyone", "It is completely negative", "It has both pros and cons", "She plans to quit soon" }, DogruCevapIndex = 2 });
                SabitSorulariEkle(paragrafSorulari);

                // Sabit B1 Dinleme Soruları
                List<Soru> dinlemeSorulari = new List<Soru>();
                string b1ListeningMetni = "🎧 Lütfen az önce sesli olarak okuduğunuz/çalıştığınız metni hatırlayarak soruları cevaplayınız.";
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b1ListeningMetni, SoruMetni = "What are family-owned restaurants often called in the United States?", Sikklar = new string[] { "Fast food joints", "High-end restaurants", "Mom and pop restaurants", "Chain restaurants" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b1ListeningMetni, SoruMetni = "What is a unique feature of the classic American 'diner'?", Sikklar = new string[] { "They only serve dinner after 6:00 PM.", "They often serve breakfast food all day long.", "They are only found in big cities.", "You are not allowed to tip." }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b1ListeningMetni, SoruMetni = "What does the speaker say about the tipping culture in the US?", Sikklar = new string[] { "It is optional", "It is mandated by law", "It is an unspoken rule", "You only tip if food is good" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b1ListeningMetni, SoruMetni = "What percentage does the speaker traditionally give as a tip?", Sikklar = new string[] { "10%", "15%", "18%", "25%" }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b1ListeningMetni, SoruMetni = "According to the speaker, how do you usually get the check (bill) in the US?", Sikklar = new string[] { "You ask for it", "You pay at the counter", "The waiter brings it without asking", "The manager brings it" }, DogruCevapIndex = 2 });
                SabitSorulariEkle(dinlemeSorulari);
            }
            else if (aktifKur == "B2")
            {
                List<Soru> gramerHavuzu = new List<Soru>();

                // Eski B2 Soruları
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Had I (...) about the deadline, I would have submitted the report earlier.", Sikklar = new string[] { "know", "known", "knew", "knowing" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Not until the meeting ended (...) the seriousness of the issue.", Sikklar = new string[] { "we realized", "did we realize", "we did realize", "had we realized" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She spoke as though she (...) personally responsible for the failure.", Sikklar = new string[] { "is", "was", "had been", "were" }, DogruCevapIndex = 3 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The proposal was rejected, (...) its innovative approach.", Sikklar = new string[] { "despite", "although", "because", "however" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The research aims to shed light (...) the causes of climate change.", Sikklar = new string[] { "in", "at", "on", "over" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Hardly (...) the announcement when the audience began to protest.", Sikklar = new string[] { "had they made", "they had made", "did they make", "they made" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She is widely regarded (...) one of the most influential scholars in her field.", Sikklar = new string[] { "as", "like", "for", "with" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The more complex the system becomes, (...) it is to maintain.", Sikklar = new string[] { "more difficult", "the more difficult", "most difficult", "difficult" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "No sooner (...) the train arrived than it departed again.", Sikklar = new string[] { "had", "has", "did", "was" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The manager demanded that the employees (...) the policy immediately.", Sikklar = new string[] { "follow", "followed", "will follow", "following" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Despite (...) extensive training, the team failed to meet expectations.", Sikklar = new string[] { "receive", "receiving", "to receive", "received" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The theory, (...) has been debated for decades, remains controversial.", Sikklar = new string[] { "which", "what", "where", "whose" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "It is essential that he (...) present at the meeting.", Sikklar = new string[] { "is", "was", "be", "being" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Had she been more attentive, she (...) the mistake.", Sikklar = new string[] { "would notice", "would have noticed", "noticed", "has noticed" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The results were far less impressive (...) anticipated.", Sikklar = new string[] { "as", "than", "that", "what" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Rarely (...) such a compelling argument presented so clearly.", Sikklar = new string[] { "we have seen", "have we seen", "we saw", "did we saw" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The project was postponed (...) budget constraints.", Sikklar = new string[] { "due to", "although", "despite", "whereas" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She objected to (...) treated unfairly.", Sikklar = new string[] { "be", "being", "been", "to be" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The findings suggest a correlation, (...) not necessarily causation.", Sikklar = new string[] { "but", "and", "so", "for" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Little (...) about the long-term consequences of the decision.", Sikklar = new string[] { "is known", "are known", "knows", "known" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Were the data (...) thoroughly analyzed, the conclusions might differ.", Sikklar = new string[] { "more", "most", "much", "many" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The lecture was so intellectually stimulating that it left the audience (...).", Sikklar = new string[] { "speechless", "speechlessly", "speech", "speaking" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He denied (...) any prior knowledge of the incident.", Sikklar = new string[] { "having", "to have", "have", "had" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "The author's argument is compelling; (...) it lacks empirical evidence.", Sikklar = new string[] { "therefore", "moreover", "nevertheless", "consequently" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "Under no circumstances (...) confidential information be disclosed.", Sikklar = new string[] { "should", "must", "will", "can" }, DogruCevapIndex = 0 });

                // Yeni B2 Soruları (Havuz Txt'den)
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "By the time I arrived, they ___ the meeting.", Sikklar = new string[] { "finish", "finished", "had finished", "have finished" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I ___ English for 5 years before I moved abroad.", Sikklar = new string[] { "study", "studied", "had studied", "have studied" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ dinner when the phone rang.", Sikklar = new string[] { "cooks", "cooked", "was cooking", "has cooked" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I wish I ___ more time yesterday.", Sikklar = new string[] { "have", "had", "have had", "had had" }, DogruCevapIndex = 3 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If I ___ you, I would accept the offer.", Sikklar = new string[] { "am", "was", "were", "be" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You ___ have told me earlier!", Sikklar = new string[] { "should", "must", "could", "would" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ be at home; the lights are off.", Sikklar = new string[] { "must", "might", "can’t", "should" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ have missed the bus, that’s why she is late.", Sikklar = new string[] { "must", "can", "should", "would" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "You ___ park here. It’s illegal.", Sikklar = new string[] { "don’t have to", "mustn’t", "shouldn’t", "can" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "They ___ have finished the work by now.", Sikklar = new string[] { "should", "must", "can", "would" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Carry out” ne demektir?", Sikklar = new string[] { "taşımak", "gerçekleştirmek", "bırakmak", "almak" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Look forward to” ne demektir?", Sikklar = new string[] { "korkmak", "beklemek (heyecanla)", "kaçınmak", "vazgeçmek" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He ___ the meeting because he was sick.", Sikklar = new string[] { "called off", "called in", "called up", "called out" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "“Take after” ne demektir?", Sikklar = new string[] { "bakmak", "benzemek", "almak", "kovalamak" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She ___ smoking last year.", Sikklar = new string[] { "gave up", "gave in", "gave out", "gave off" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "___ he was tired, he finished the work.", Sikklar = new string[] { "Although", "Because", "So", "And" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I stayed at home ___ it was raining.", Sikklar = new string[] { "although", "because", "so", "but" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He is rich, ___ he is not happy.", Sikklar = new string[] { "because", "but", "so", "and" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "We left early ___ we wouldn’t miss the train.", Sikklar = new string[] { "although", "because", "so that", "but" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "___ she studied hard, she failed the exam.", Sikklar = new string[] { "Because", "So", "Although", "And" }, DogruCevapIndex = 2 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "I regret ___ him the truth.", Sikklar = new string[] { "tell", "telling", "to tell", "told" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "She made me ___ my homework again.", Sikklar = new string[] { "do", "to do", "doing", "did" }, DogruCevapIndex = 0 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "This is the book ___ I told you about.", Sikklar = new string[] { "who", "which", "where", "when" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "He is used to ___ early.", Sikklar = new string[] { "wake", "waking", "to wake", "woke" }, DogruCevapIndex = 1 });
                gramerHavuzu.Add(new Soru { ParagrafMetni = "", SoruMetni = "If I ___ harder, I would have passed the exam.", Sikklar = new string[] { "study", "studied", "had studied", "have studied" }, DogruCevapIndex = 2 });

                // Rastgele 15 gramer sorusunu ekle
                HavuzdanSecVeEkle(gramerHavuzu, 15);

                // Sabit B2 Paragraf Soruları
                List<Soru> paragrafSorulari = new List<Soru>();
                string b2ReadingMetni = "In recent years, remote work has evolved from a temporary solution into a long-term structural shift in global employment patterns. While proponents argue that it enhances productivity and work-life balance, critics highlight concerns regarding employee isolation and diminished team cohesion. Research indicates that although remote workers often report higher individual efficiency, collaborative innovation may suffer in the absence of spontaneous interpersonal interaction. Ultimately, the effectiveness of remote work appears contingent upon industry type, managerial strategy, and individual personality traits.";
                paragrafSorulari.Add(new Soru { ParagrafMetni = b2ReadingMetni, SoruMetni = "What is the main idea of the text?", Sikklar = new string[] { "Remote work increases salaries", "Remote work has permanently changed employment structures", "Remote work should be banned", "Remote work only benefits companies" }, DogruCevapIndex = 1 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b2ReadingMetni, SoruMetni = "What concern do critics raise?", Sikklar = new string[] { "Increased travel expenses", "Higher competition", "Isolation and reduced cohesion", "Lower technology use" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b2ReadingMetni, SoruMetni = "What does 'contingent upon' most nearly mean?", Sikklar = new string[] { "independent of", "dependent on", "opposed to", "similar to" }, DogruCevapIndex = 1 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b2ReadingMetni, SoruMetni = "According to the text, what may decline in remote settings?", Sikklar = new string[] { "Salary levels", "Individual efficiency", "Collaborative innovation", "Corporate profits" }, DogruCevapIndex = 2 });
                paragrafSorulari.Add(new Soru { ParagrafMetni = b2ReadingMetni, SoruMetni = "What can be inferred from the text?", Sikklar = new string[] { "Remote work is entirely beneficial", "Remote work effectiveness varies", "Remote work reduces productivity", "All industries benefit equally" }, DogruCevapIndex = 1 });
                SabitSorulariEkle(paragrafSorulari);

                // Sabit B2 Dinleme Soruları
                List<Soru> dinlemeSorulari = new List<Soru>();
                string b2ListeningMetni = "🎧 Lütfen az önce sesli olarak okuduğunuz/çalıştığınız metni hatırlayarak soruları cevaplayınız.";
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b2ListeningMetni, SoruMetni = "How does the speaker describe the feeling of arriving in a new country to live?", Sikklar = new string[] { "Like a boring vacation", "Like being hit by a truck / having giant moths in the stomach", "Like a calm experience", "Like a dream" }, DogruCevapIndex = 1 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b2ListeningMetni, SoruMetni = "What is the speaker's advice regarding learning the local language?", Sikklar = new string[] { "Don't worry about it", "Study it after a year", "Study as much as possible before you go", "Don't bother studying" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b2ListeningMetni, SoruMetni = "According to the speaker, what is the biggest negative aspect of living abroad?", Sikklar = new string[] { "Expensive plane tickets", "Learning a difficult language", "Being far away from loved ones", "Trying new foods" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b2ListeningMetni, SoruMetni = "What does the speaker mention as a specific challenge they faced while adapting?", Sikklar = new string[] { "High taxes", "Poor weather", "Low-quality infrastructure and lack of punctuality", "Finding a job" }, DogruCevapIndex = 2 });
                dinlemeSorulari.Add(new Soru { ParagrafMetni = b2ListeningMetni, SoruMetni = "How does the speaker feel about video chatting with family?", Sikklar = new string[] { "Exactly the same as in person", "Useless and makes them homesick", "It helps, even though it's not physical contact", "Too difficult" }, DogruCevapIndex = 2 });
                SabitSorulariEkle(dinlemeSorulari);
            }
        }
    }
}