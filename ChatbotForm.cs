using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinFormsApp2
{
    public class ChatbotForm : Form
    {
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private string _ogrenciNo;
        private bool _ilkYukleme = true; // NavigationCompleted'ın sadece ilk açılışta çalışması için

        private const string ApiKey = "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";
        private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

        public ChatbotForm(string ogrenciNo)
        {
            _ogrenciNo = ogrenciNo;

            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            webView21.Dock = DockStyle.Fill;
            this.Controls.Add(webView21);

            this.Text = "OkulSis - Teacher Bot";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeWebView();
        }

        // ── WebView başlat ──────────────────────────────────────────────────
        private async void InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;
            webView21.CoreWebView2.NavigationCompleted += NavigationCompleted;

            string path = System.IO.Path.Combine(Application.StartupPath, "chatbot.html");
            webView21.CoreWebView2.Navigate("file:///" + path.Replace("\\", "/"));
        }

        // ── Sayfa yüklendi ──────────────────────────────────────────────────
        private void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!_ilkYukleme) return; // Sadece ilk açılışta çalış
            _ilkYukleme = false;

            // NavigationCompleted bazen arka plan thread'den gelir — BeginInvoke ile güvence al
            if (this.InvokeRequired)
                this.BeginInvoke(new Action(SayfaHazir));
            else
                SayfaHazir();
        }

        private void SayfaHazir()
        {
            try
            {
                PostJson(new { tip = "init", ogrenciNo = _ogrenciNo });
                GecmisiGonder();
            }
            catch { }
        }

        // ── Geçmiş mesajları gönder ─────────────────────────────────────────
        private void GecmisiGonder()
        {
            DataTable gecmis = DatabaseLayer.BotGecmisiniGetir(_ogrenciNo);
            if (gecmis == null || gecmis.Rows.Count == 0) return;

            var liste = new List<object>();
            foreach (DataRow row in gecmis.Rows)
                liste.Add(new
                {
                    gonderen = row["Gonderen"].ToString(),
                    mesaj = row["Mesaj"].ToString(),
                    saat = ""
                });

            PostJson(new { tip = "gecmis", mesajlar = liste });
        }

        // ── HTML'den gelen mesajlar ─────────────────────────────────────────
        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string raw = e.TryGetWebMessageAsString();

            // Her ihtimale karşı UI thread'e taşı
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => IsleHtmlMesaj(raw)));
            }
            else
            {
                IsleHtmlMesaj(raw);
            }
        }

        private void IsleHtmlMesaj(string raw)
        {
            JObject data;
            try { data = JObject.Parse(raw); }
            catch { return; }

            string tip = data["tip"]?.ToString();

            if (tip == "mesajGonder")
            {
                string metin = data["metin"]?.ToString();
                if (!string.IsNullOrEmpty(metin))
                    _ = MesajIsleAsync(metin); // async başlat, UI thread serbest kalsın
            }
            else if (tip == "anaMenu")
            {
                AnaMenuye();
            }
        }

        // ── API çağrısı ─────────────────────────────────────────────────────
        private async Task MesajIsleAsync(string mesaj)
        {
            try
            {
                string botCevabi = await GroqCevapAlAsync(mesaj);

                // Veritabanına kaydet
                if (!botCevabi.StartsWith("Connection error"))
                {
                    DatabaseLayer.BotMesajKaydet(_ogrenciNo, "user", mesaj);
                    DatabaseLayer.BotMesajKaydet(_ogrenciNo, "assistant", botCevabi);
                }

                // UI thread'e dön, sonra HTML'e gönder
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (botCevabi.StartsWith("Connection error"))
                            PostJson(new { tip = "hata", metin = botCevabi });
                        else
                            PostJson(new { tip = "botCevabi", metin = botCevabi });
                    }
                    catch { }
                }));
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new Action(() =>
                {
                    try { PostJson(new { tip = "hata", metin = "Error: " + ex.Message }); }
                    catch { }
                }));
            }
        }

        // ── Groq API ────────────────────────────────────────────────────────
        private async Task<string> GroqCevapAlAsync(string yeniMesaj)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            DataTable gecmis = DatabaseLayer.BotGecmisiniGetir(_ogrenciNo);

            var mesajListesi = new List<object>
            {
                new { role = "system", content = "You are a friendly English tutor. Speak only English. Keep your answers short, suitable for A2-B1 level students. Gently correct grammatical mistakes." }
            };

            if (gecmis != null)
                foreach (DataRow row in gecmis.Rows)
                    mesajListesi.Add(new
                    {
                        role = row["Gonderen"].ToString(),
                        content = row["Mesaj"].ToString()
                    });

            mesajListesi.Add(new { role = "user", content = yeniMesaj });

            string jsonIstek = JsonConvert.SerializeObject(new
            {
                model = "llama-3.1-8b-instant",
                messages = mesajListesi.ToArray(),
                temperature = 0.7
            });

            var icerik = new StringContent(jsonIstek, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage yanit = await client.PostAsync(ApiUrl, icerik);

                if (!yanit.IsSuccessStatusCode)
                {
                    string hata = await yanit.Content.ReadAsStringAsync();
                    return $"Connection error: {yanit.StatusCode} — {hata}";
                }

                string jsonYanit = await yanit.Content.ReadAsStringAsync();
                JObject parsed = JObject.Parse(jsonYanit);
                return parsed["choices"][0]["message"]["content"].ToString();
            }
            catch (Exception ex)
            {
                return "Connection error: " + ex.Message;
            }
        }

        // ── Yardımcılar ─────────────────────────────────────────────────────
        private void PostJson(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            webView21.CoreWebView2.PostWebMessageAsString(json);
        }

        private void AnaMenuye()
        {
            Form3 anaMenu = new Form3(_ogrenciNo);
            anaMenu.Show();
            this.Close();
        }
    }
}