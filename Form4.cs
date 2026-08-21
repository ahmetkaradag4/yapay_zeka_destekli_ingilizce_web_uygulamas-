using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System;

namespace WinFormsApp2
{
    public class Form4 : Form
    {
        string ogrenciNo;
        string ogrenciAdi;
        string bagliOlduguOgretmen;
        private Timer timer1;
        private bool webViewHazir = false;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;

        public Form4(string gelenNo)
        {
            ogrenciNo = gelenNo;

            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            webView21.Dock = DockStyle.Fill;
            this.Controls.Add(webView21);

            this.ClientSize = new System.Drawing.Size(940, 640);
            this.Text = "OkulSis - Sınıf Sohbeti";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(11, 17, 32);
            // Form3 gibi tam ekran, ayrı pencere değil
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView21.EnsureCoreWebView2Async(env);

            string path = Path.Combine(Application.StartupPath, "chat.html");
            webView21.CoreWebView2.Navigate("file:///" + path.Replace("\\", "/"));
            webView21.CoreWebView2.WebMessageReceived += WebView_MessageReceived;

            webView21.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                webViewHazir = true;
                IsmiBul();
                string initJson = JsonSerializer.Serialize(new
                {
                    tip = "init",
                    no = ogrenciNo,
                    adi = ogrenciAdi,
                    ogretmen = bagliOlduguOgretmen
                });
                webView21.CoreWebView2.PostWebMessageAsString(initJson);

                // Timer sadece WebView hazır olduktan sonra başlıyor
                timer1 = new Timer();
                timer1.Interval = 3000;
                timer1.Tick += (ts, te) =>
                {
                    if (webViewHazir) SohbetiGonder();
                };
                timer1.Start();
            };
        }

        private void IsmiBul()
        {
            DataRow satir = DatabaseLayer.OgrenciBilgisiGetir(ogrenciNo);
            if (satir != null)
            {
                ogrenciAdi = satir["OgrenciIsmi"].ToString();
                bagliOlduguOgretmen = satir["Ogretmen"].ToString();
            }
            else
            {
                ogrenciAdi = "Bilinmeyen";
                bagliOlduguOgretmen = "Yok";
            }
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.TryGetWebMessageAsString();
            var mesaj = JsonSerializer.Deserialize<ChatMesaj>(json);
            if (mesaj == null) return;

            this.Invoke(new Action(() =>
            {
                switch (mesaj.tip)
                {
                    case "sohbetiGetir":
                        SohbetiGonder();
                        break;

                    case "mesajGonder":
                        if (!string.IsNullOrWhiteSpace(mesaj.textBox1))
                        {
                            bool ok = DatabaseLayer.SohbetMesajiEkle(
                                ogrenciAdi, mesaj.textBox1, bagliOlduguOgretmen);
                            string r = JsonSerializer.Serialize(
                                new { tip = ok ? "mesajOk" : "mesajHata" });
                            webView21.CoreWebView2.PostWebMessageAsString(r);
                        }
                        break;

                    case "anaMenu":
                        webViewHazir = false;
                        timer1?.Stop();
                        Form3 form3 = new Form3(ogrenciNo);
                        form3.Show();
                        this.Close();
                        break;
                }
            }));
        }

        private void SohbetiGonder()
        {
            if (!webViewHazir) return;
            try
            {
                DataTable tablo = DatabaseLayer.SohbetMesajlariniGetir(bagliOlduguOgretmen);
                var rows = new List<string[]>();
                if (tablo != null)
                    foreach (DataRow row in tablo.Rows)
                        rows.Add(new string[] {
                            row["Saat"]?.ToString() ?? "",
                            row["GonderenIsmi"]?.ToString() ?? "",
                            row["Mesaj"]?.ToString() ?? "",
                            "",
                            ""
                        });

                string json = JsonSerializer.Serialize(new { tip = "sohbetData", rows });
                webView21.CoreWebView2.PostWebMessageAsString(json);
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            webViewHazir = false;
            base.OnFormClosing(e);
            timer1?.Stop();
            timer1?.Dispose();
        }

        private class ChatMesaj
        {
            public string tip { get; set; }
            public string textBox1 { get; set; }
            public string gorsel { get; set; }
        }
    }
}