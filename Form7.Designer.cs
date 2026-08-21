namespace WinFormsApp2
{
    partial class Form7
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnKayitBasla = new System.Windows.Forms.Button();
            btnKayitBitir = new System.Windows.Forms.Button();
            btnDinle = new System.Windows.Forms.Button();
            btnTesteGec = new System.Windows.Forms.Button();
            lblDurum = new System.Windows.Forms.Label();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // btnKayitBasla
            // 
            btnKayitBasla.Location = new System.Drawing.Point(191, 268);
            btnKayitBasla.Name = "btnKayitBasla";
            btnKayitBasla.Size = new System.Drawing.Size(75, 23);
            btnKayitBasla.TabIndex = 0;
            btnKayitBasla.Text = "kayıt başla";
            btnKayitBasla.UseVisualStyleBackColor = true;
            // 
            // btnKayitBitir
            // 
            btnKayitBitir.Enabled = false;
            btnKayitBitir.Location = new System.Drawing.Point(422, 268);
            btnKayitBitir.Name = "btnKayitBitir";
            btnKayitBitir.Size = new System.Drawing.Size(75, 23);
            btnKayitBitir.TabIndex = 1;
            btnKayitBitir.Text = "kayıt bitir";
            btnKayitBitir.UseVisualStyleBackColor = true;
            // 
            // btnDinle
            // 
            btnDinle.Enabled = false;
            btnDinle.Location = new System.Drawing.Point(191, 318);
            btnDinle.Name = "btnDinle";
            btnDinle.Size = new System.Drawing.Size(75, 23);
            btnDinle.TabIndex = 2;
            btnDinle.Text = "kaydı dinle";
            btnDinle.UseVisualStyleBackColor = true;
            // 
            // btnTesteGec
            // 
            btnTesteGec.Enabled = false;
            btnTesteGec.Location = new System.Drawing.Point(422, 318);
            btnTesteGec.Name = "btnTesteGec";
            btnTesteGec.Size = new System.Drawing.Size(75, 23);
            btnTesteGec.TabIndex = 3;
            btnTesteGec.Text = "teste geç";
            btnTesteGec.UseVisualStyleBackColor = true;
            // 
            // lblDurum
            // 
            lblDurum.AutoSize = true;
            lblDurum.Location = new System.Drawing.Point(79, 372);
            lblDurum.Name = "lblDurum";
            lblDurum.Size = new System.Drawing.Size(114, 15);
            lblDurum.TabIndex = 4;
            lblDurum.Text = "Durum: Bekleniyor...";
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new System.Drawing.Point(168, 31);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(408, 206);
            richTextBox1.TabIndex = 5;
            richTextBox1.Text = "";
            // 
            // Form7
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(richTextBox1);
            Controls.Add(lblDurum);
            Controls.Add(btnTesteGec);
            Controls.Add(btnDinle);
            Controls.Add(btnKayitBitir);
            Controls.Add(btnKayitBasla);
            Name = "Form7";
            Text = "Form7";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnKayitBasla;
        private System.Windows.Forms.Button btnKayitBitir;
        private System.Windows.Forms.Button btnDinle;
        private System.Windows.Forms.Button btnTesteGec;
        private System.Windows.Forms.Label lblDurum;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}