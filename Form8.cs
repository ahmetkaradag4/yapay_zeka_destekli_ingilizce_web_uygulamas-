using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form8 : Form
    {
        string ogretmenAdi;
        string ogretmenKlasoru;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;

        public Form8(string ogretmeninAdi)
        {
            ogretmenAdi = ogretmeninAdi;
            ogretmenKlasoru = Path.Combine(Application.StartupPath, "Kutuphane", ogretmenAdi);

            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            webView21.Dock = DockStyle.Fill;
            this.Controls.Add(webView21);

            this.ClientSize = new System.Drawing.Size(1200, 780);
            this.Text = "OkulSis - Kütüphane";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(244, 246, 251);
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            string path = Path.Combine(Application.StartupPath, "library.html");
            webView21.CoreWebView2.Navigate("file:///" + path.Replace("\\", "/"));
            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;

            webView21.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                KitaplariGonder();
            };
        }

        private void KitaplariGonder()
        {
            var kitaplar = new List<string>();

            if (Directory.Exists(ogretmenKlasoru))
            {
                string[] dosyalar = Directory.GetFiles(ogretmenKlasoru, "*.pdf");
                foreach (string d in dosyalar)
                    kitaplar.Add(Path.GetFileName(d));
            }

            string json = JsonSerializer.Serialize(new
            {
                tip = "init",
                kitaplar = kitaplar
            });
            webView21.CoreWebView2.PostWebMessageAsString(json);
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.TryGetWebMessageAsString();
            var mesaj = JsonSerializer.Deserialize<LibMesaj>(json);
            if (mesaj == null) return;

            this.Invoke(new Action(() =>
            {
                switch (mesaj.tip)
                {
                    case "kitapSec":
                        PdfGonder(mesaj.ad);
                        break;

                    case "kitapEkle": // HTML'den gelen Kitap Ekle komutunu burada yakalıyoruz
                        using (OpenFileDialog ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "PDF Dosyaları (*.pdf)|*.pdf";
                            ofd.Title = "Kütüphaneye Kitap (PDF) Ekle";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                try
                                {
                                    if (!Directory.Exists(ogretmenKlasoru))
                                    {
                                        Directory.CreateDirectory(ogretmenKlasoru);
                                    }

                                    string hedef = Path.Combine(ogretmenKlasoru, Path.GetFileName(ofd.FileName));
                                    File.Copy(ofd.FileName, hedef, true);

                                    KitaplariGonder(); // Kütüphane listesini anında yenile

                                    // HTML arayüzüne başarı mesajı gönder
                                    string basari = JsonSerializer.Serialize(new { tip = "bilgi", mesaj = "Kitap başarıyla eklendi!" });
                                    webView21.CoreWebView2.PostWebMessageAsString(basari);
                                }
                                catch (Exception ex)
                                {
                                    string hata = JsonSerializer.Serialize(new { tip = "hata", mesaj = "Eklerken hata oluştu: " + ex.Message });
                                    webView21.CoreWebView2.PostWebMessageAsString(hata);
                                }
                            }
                        }
                        break;

                    case "deftereGit":
                        MessageBox.Show("Not defteri yakında eklenecek!");
                        break;

                    case "geriDon":
                        this.Close();
                        break;
                }
            }));
        }

        private void PdfGonder(string kitapAdi)
        {
            try
            {
                string tamYol = Path.Combine(ogretmenKlasoru, kitapAdi);
                if (!File.Exists(tamYol))
                {
                    string hata = JsonSerializer.Serialize(new { tip = "hata", mesaj = "Dosya bulunamadı." });
                    webView21.CoreWebView2.PostWebMessageAsString(hata);
                    return;
                }

                byte[] bytes = File.ReadAllBytes(tamYol);
                string base64 = Convert.ToBase64String(bytes);

                string json = JsonSerializer.Serialize(new
                {
                    tip = "pdfData",
                    ad = kitapAdi,
                    base64 = base64
                });
                webView21.CoreWebView2.PostWebMessageAsString(json);
            }
            catch (Exception ex)
            {
                string hata = JsonSerializer.Serialize(new { tip = "hata", mesaj = ex.Message });
                webView21.CoreWebView2.PostWebMessageAsString(hata);
            }
        }

        private class LibMesaj
        {
            public string tip { get; set; }
            public string ad { get; set; }
        }
    }
}