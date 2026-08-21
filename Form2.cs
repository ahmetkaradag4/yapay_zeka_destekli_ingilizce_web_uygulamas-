using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace WinFormsApp2
{
    public partial class Form2 : Form
    {
        string _girisYapanOgretmen;

        public Form2(string ogretmenAdi)
        {
            InitializeComponent();
            _girisYapanOgretmen = ogretmenAdi;
            this.Text = "OkulSis — Öğretmen Paneli";
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            string htmlPath = Path.Combine(Application.StartupPath, "admin.html");
            webView21.CoreWebView2.Navigate("file:///" + htmlPath.Replace("\\", "/"));
            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;

            // Sayfa yüklenince öğretmen adını gönder
            webView21.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                string initJson = JsonSerializer.Serialize(new
                {
                    tip = "init",
                    ogretmenAdi = _girisYapanOgretmen
                });
                webView21.CoreWebView2.PostWebMessageAsString(initJson);
            };
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.TryGetWebMessageAsString();
            var mesaj = JsonSerializer.Deserialize<AdminMesaj>(json);
            if (mesaj == null) return;

            this.Invoke(new Action(() =>
            {
                switch (mesaj.tip)
                {
                    case "ogrencileriGetir":
                        OgrencileriGonder();
                        break;

                    case "ogrenciEkle":
                        bool ekleOk = DatabaseLayer.OgrenciEkle(
                            mesaj.textBox1, mesaj.textBox2, mesaj.textBox3, _girisYapanOgretmen);
                        string ekleJson = JsonSerializer.Serialize(new
                        {
                            tip = ekleOk ? "ogrenciEkleOk" : "ogrenciEkleHata"
                        });
                        webView21.CoreWebView2.PostWebMessageAsString(ekleJson);
                        break;

                    case "ogrenciSil":
                        bool silOk = DatabaseLayer.OgrenciSil(mesaj.numara);
                        string silJson = JsonSerializer.Serialize(new
                        {
                            tip = silOk ? "ogrenciSilOk" : "ogrenciSilHata"
                        });
                        webView21.CoreWebView2.PostWebMessageAsString(silJson);
                        break;

                    case "mesajGonder":
                        bool mesajOk = DatabaseLayer.MesajGonder(
                            _girisYapanOgretmen, mesaj.textBox4, mesaj.textBox5);
                        string mesajJson = JsonSerializer.Serialize(new
                        {
                            tip = mesajOk ? "mesajOk" : "mesajHata"
                        });
                        webView21.CoreWebView2.PostWebMessageAsString(mesajJson);
                        break;

                    case "sinavSonuclariGetir":
                        SinavSonuclariGonder();
                        break;

                    case "kitapYukle":
                        KitapYukle(mesaj.dosyaAdi);
                        break;

                    case "cikis":
                        Form1 girisEkrani = new Form1();
                        girisEkrani.Show();
                        this.Close();
                        break;
                }
            }));
        }

        private void OgrencileriGonder()
        {
            DataTable tablo = DatabaseLayer.OgrencileriGetir(_girisYapanOgretmen);
            var rows = new System.Collections.Generic.List<string[]>();

            if (tablo != null)
            {
                foreach (DataRow row in tablo.Rows)
                {
                    rows.Add(new string[]
                    {
                        row[0]?.ToString() ?? "",
                        row[1]?.ToString() ?? "",
                        row[2]?.ToString() ?? "",
                        row[3]?.ToString() ?? ""
                    });
                }
            }

            string responseJson = JsonSerializer.Serialize(new { tip = "ogrencilerData", rows });
            webView21.CoreWebView2.PostWebMessageAsString(responseJson);
        }

        private void SinavSonuclariGonder()
        {
            DataTable tablo = DatabaseLayer.TumPuanlariGetir(_girisYapanOgretmen);
            var rows = new System.Collections.Generic.List<string[]>();

            if (tablo != null)
            {
                foreach (DataRow row in tablo.Rows)
                {
                    rows.Add(new string[]
                    {
                        row[0]?.ToString() ?? "",
                        row[1]?.ToString() ?? "",
                        row[2]?.ToString() ?? "",
                        row[3]?.ToString() ?? "",
                        row[4]?.ToString() ?? ""
                    });
                }
            }

            string responseJson = JsonSerializer.Serialize(new { tip = "sinavData", rows });
            webView21.CoreWebView2.PostWebMessageAsString(responseJson);
        }

        private void KitapYukle(string dosyaAdi)
        {
            try
            {
                // OpenFileDialog ile dosya seçtir
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "PDF Dosyaları|*.pdf";
                dialog.Title = "Öğrenciler için kitap seçin";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string hedefKlasor = Path.Combine(Application.StartupPath, "Kutuphane", _girisYapanOgretmen);
                    if (!Directory.Exists(hedefKlasor))
                        Directory.CreateDirectory(hedefKlasor);

                    string hedefYol = Path.Combine(hedefKlasor, Path.GetFileName(dialog.FileName));
                    File.Copy(dialog.FileName, hedefYol, true);

                    string okJson = JsonSerializer.Serialize(new { tip = "kitapOk" });
                    webView21.CoreWebView2.PostWebMessageAsString(okJson);
                }
            }
            catch
            {
                string hataJson = JsonSerializer.Serialize(new { tip = "kitapHata" });
                webView21.CoreWebView2.PostWebMessageAsString(hataJson);
            }
        }

        private class AdminMesaj
        {
            public string tip { get; set; }
            public string textBox1 { get; set; }
            public string textBox2 { get; set; }
            public string textBox3 { get; set; }
            public string textBox4 { get; set; }
            public string textBox5 { get; set; }
            public string dosyaAdi { get; set; }
            public string numara { get; set; }
        }
    }
}