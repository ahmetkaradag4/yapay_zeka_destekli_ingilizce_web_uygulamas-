using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public class MesajKutusu : Panel
    {
        private Label lblMesaj;
        private Label lblSaat;
        private Label lblGonderen;

        public MesajKutusu(string gonderen, string mesaj, string saat, bool benGonderdim)
        {
            // Panel Ayarları
            this.AutoSize = true;
            this.Padding = new Padding(10);
            this.Margin = new Padding(10, 5, 10, 5); // Dış boşluk
            this.MaximumSize = new Size(350, 0); // Baloncuğun maksimum genişliği

            // Renk Ayarları (Whatsapp Tarzı)
            if (benGonderdim)
            {
                this.BackColor = Color.FromArgb(220, 248, 198); // Açık yeşil (Gönderilen)
                this.Dock = DockStyle.Right; // Sağa yasla
            }
            else
            {
                this.BackColor = Color.White; // Beyaz (Gelen)
                this.Dock = DockStyle.Left; // Sola yasla
            }

            // Gönderen İsmi (Sadece karşıdan geldiyse gösterelim)
            if (!benGonderdim)
            {
                lblGonderen = new Label();
                lblGonderen.Text = gonderen;
                lblGonderen.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                lblGonderen.ForeColor = Color.OrangeRed;
                lblGonderen.AutoSize = true;
                lblGonderen.Location = new Point(10, 5);
                this.Controls.Add(lblGonderen);
            }

            // Mesaj Metni
            lblMesaj = new Label();
            lblMesaj.Text = mesaj;
            lblMesaj.Font = new Font("Segoe UI", 10);
            lblMesaj.ForeColor = Color.Black;
            lblMesaj.AutoSize = true;
            lblMesaj.MaximumSize = new Size(330, 0); // Taşarsa aşağı insin

            // Konumunu gönderen ismine göre ayarla
            int yKonum = (!benGonderdim) ? 25 : 10;
            lblMesaj.Location = new Point(10, yKonum);

            this.Controls.Add(lblMesaj);

            // Saat
            lblSaat = new Label();
            lblSaat.Text = saat;
            lblSaat.Font = new Font("Segoe UI", 7, FontStyle.Italic);
            lblSaat.ForeColor = Color.Gray;
            lblSaat.AutoSize = true;

            // Saati mesajın altına ekle
            this.Controls.Add(lblSaat);

            // Yükseklik ayarı için event
            this.Paint += (s, e) =>
            {
                lblSaat.Location = new Point(this.Width - lblSaat.Width - 5, this.Height - 20);
            };
        }
    }
}