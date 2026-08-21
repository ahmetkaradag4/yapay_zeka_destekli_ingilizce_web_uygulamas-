using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace WinFormsApp2
{
    public partial class Form3 : Form
    {
        string gelenOgrenciNo;

        public Form3(string no)
        {
            InitializeComponent();
            gelenOgrenciNo = no;
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            string htmlPath = Path.Combine(Application.StartupPath, "student.html");
            webView21.CoreWebView2.Navigate("file:///" + htmlPath.Replace("\\", "/"));
            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;

            // Sayfa yüklenince öğrenci bilgilerini gönder
            webView21.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                // Öğrenci ismini ve kurunu çek
                DataRow ogrenci = DatabaseLayer.OgrenciBilgisiGetir(gelenOgrenciNo);
                string isim = ogrenci != null ? ogrenci["OgrenciIsmi"]?.ToString() ?? "" : "";
                string kur = DatabaseLayer.OgrencininKurunuGetir(gelenOgrenciNo);

                string initJson = JsonSerializer.Serialize(new
                {
                    tip = "init",
                    no = gelenOgrenciNo,
                    isim = isim,
                    kur = kur
                });
                webView21.CoreWebView2.PostWebMessageAsString(initJson);
            };
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.TryGetWebMessageAsString();
            var mesaj = JsonSerializer.Deserialize<StudentMesaj>(json);
            if (mesaj == null) return;

            this.Invoke(new Action(() =>
            {
                switch (mesaj.tip)
                {
                    case "notlarAc":
                        // İstersen Form5 veya mevcut bir form aç
                        break;

                    case "mesajlariGetir":
                        MesajlariGonder();
                        break;

                    case "egitimlerAc":
                        // Form3'deki button2_Click mantığı
                        string aktifKur = DatabaseLayer.OgrencininKurunuGetir(gelenOgrenciNo);
                        if (aktifKur == "MEZUN")
                        {
                            string mezunJson = JsonSerializer.Serialize(new { tip = "mezun" });
                            webView21.CoreWebView2.PostWebMessageAsString(mezunJson);
                        }
                        else
                        {
                            Form7 okumaFormu = new Form7(gelenOgrenciNo, aktifKur);
                            okumaFormu.Show();
                            this.Hide();
                        }
                        break;

                    case "sohbetAc":
                        Form4 sohbet = new Form4(gelenOgrenciNo);
                        sohbet.Show();
                        this.Hide(); // Form3'ü gizle, arkada kalmasın
                        break;

                    case "kutuphaneAc":
                        // Form3'deki button4_Click mantığı
                        DataRow ogrenci = DatabaseLayer.OgrenciBilgisiGetir(gelenOgrenciNo);
                        if (ogrenci != null)
                        {
                            string hocasi = ogrenci["Ogretmen"].ToString();
                            Form8 kutuphane = new Form8(hocasi);
                            kutuphane.Show();
                        }
                        else
                        {
                            string hataJson = JsonSerializer.Serialize(new
                            {
                                tip = "hata",
                                mesaj = "Ogrenci bilgileri bulunamadi."
                            });
                            webView21.CoreWebView2.PostWebMessageAsString(hataJson);
                        }
                        break;
                    case "chatbotAc":
                        // Az önce yazdığımız ChatbotForm sınıfını çağırıyoruz
                        ChatbotForm botSayfasi = new ChatbotForm(gelenOgrenciNo);
                        botSayfasi.Show();

                        // Eğer bot açıldığında arkadaki ana sayfanın kapanmasını isterseniz:
                        // this.Hide(); 
                        break;

                    case "cikis":
                        // Form3'deki btnCikis_Click mantığı
                        Form1 giris = new Form1();
                        giris.Show();
                        this.Close();
                        break;
                }
            }));
        }

        private void MesajlariGonder()
        {
            DataTable tablo = DatabaseLayer.OgrenciOzelMesajlariGetir(gelenOgrenciNo);
            var rows = new System.Collections.Generic.List<string[]>();

            if (tablo != null)
            {
                foreach (DataRow row in tablo.Rows)
                {
                    rows.Add(new string[]
                    {
                        row[0]?.ToString() ?? "",
                        row[1]?.ToString() ?? ""
                    });
                }
            }

            string responseJson = JsonSerializer.Serialize(new { tip = "mesajlarData", rows });
            webView21.CoreWebView2.PostWebMessageAsString(responseJson);
        }

        private class StudentMesaj
        {
            public string tip { get; set; }
        }
    }
}