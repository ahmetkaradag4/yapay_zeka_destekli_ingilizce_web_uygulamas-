using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        string dosyaYolu = @"Data Source=okul.db.db;Version=3;Pooling=False;";

        public Form1()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            // WebView2 ortamını başlat
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            // HTML dosyasını yükle (exe ile aynı klasörde olmalı)
            string htmlPath = Path.Combine(Application.StartupPath, "login.html");
            webView21.CoreWebView2.Navigate("file:///" + htmlPath.Replace("\\", "/"));

            // JavaScript'ten gelen mesajları dinle
            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // JS tarafından gönderilen JSON mesajı al
            string json = e.TryGetWebMessageAsString();
            var mesaj = JsonSerializer.Deserialize<LoginMesaj>(json);

            if (mesaj == null) return;

            if (mesaj.tip == "ogrenci")
            {
                // WinForms'daki btnOgrenciGiris_Click mantığıyla birebir aynı
                string ogrenciIsmi = DatabaseLayer.OgrenciGirisYap(mesaj.textBox1, mesaj.textBox2);

                if (ogrenciIsmi != null)
                {
                    // Başarı mesajını JS'e gönder
                    string successJson = JsonSerializer.Serialize(new { basarili = true, isim = ogrenciIsmi });
                    webView21.CoreWebView2.PostWebMessageAsString(successJson);

                    // Form3'ü aç
                    this.Invoke(new Action(() =>
                    {
                        Form3 anaSayfa = new Form3(mesaj.textBox1);
                        anaSayfa.Show();
                        this.Hide();
                    }));
                }
                else
                {
                    string errorJson = JsonSerializer.Serialize(new { basarili = false, hata = "Hatalı Numara veya Şifre!" });
                    webView21.CoreWebView2.PostWebMessageAsString(errorJson);
                }
            }
            else if (mesaj.tip == "ogretmen")
            {
                // WinForms'daki btnOgretmenGiris_Click mantığıyla birebir aynı
                string girisYapan = DatabaseLayer.OgretmenGirisYap(mesaj.textBox3, mesaj.textBox4);

                if (girisYapan != null)
                {
                    string successJson = JsonSerializer.Serialize(new { basarili = true, isim = girisYapan });
                    webView21.CoreWebView2.PostWebMessageAsString(successJson);

                    // Form2'yi aç
                    this.Invoke(new Action(() =>
                    {
                        Form2 yonetim = new Form2(girisYapan);
                        yonetim.Show();
                        this.Hide();
                    }));
                }
                else
                {
                    string errorJson = JsonSerializer.Serialize(new { basarili = false, hata = "Hatalı Giriş!" });
                    webView21.CoreWebView2.PostWebMessageAsString(errorJson);
                }
            }
        }

        // JSON deserialize için model
        private class LoginMesaj
        {
            public string tip { get; set; }      // "ogrenci" veya "ogretmen"
            public string textBox1 { get; set; } // Öğrenci No
            public string textBox2 { get; set; } // Öğrenci Şifre
            public string textBox3 { get; set; } // Öğretmen Ad
            public string textBox4 { get; set; } // Öğretmen Şifre
        }
    }
}