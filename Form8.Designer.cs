namespace WinFormsApp2
{
    partial class Form8
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
            lstKitaplar = new System.Windows.Forms.ListBox();
            picSolSayfa = new System.Windows.Forms.PictureBox();
            picSagSayfa = new System.Windows.Forms.PictureBox();
            btnGeri = new System.Windows.Forms.Button();
            btnIleri = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)picSolSayfa).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSagSayfa).BeginInit();
            SuspendLayout();
            // 
            // lstKitaplar
            // 
            lstKitaplar.FormattingEnabled = true;
            lstKitaplar.ItemHeight = 15;
            lstKitaplar.Location = new System.Drawing.Point(24, 21);
            lstKitaplar.Name = "lstKitaplar";
            lstKitaplar.Size = new System.Drawing.Size(120, 229);
            lstKitaplar.TabIndex = 0;
            // 
            // picSolSayfa
            // 
            picSolSayfa.Location = new System.Drawing.Point(185, 32);
            picSolSayfa.Name = "picSolSayfa";
            picSolSayfa.Size = new System.Drawing.Size(230, 304);
            picSolSayfa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picSolSayfa.TabIndex = 1;
            picSolSayfa.TabStop = false;
            // 
            // picSagSayfa
            // 
            picSagSayfa.Location = new System.Drawing.Point(477, 32);
            picSagSayfa.Name = "picSagSayfa";
            picSagSayfa.Size = new System.Drawing.Size(226, 304);
            picSagSayfa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picSagSayfa.TabIndex = 2;
            picSagSayfa.TabStop = false;
            // 
            // btnGeri
            // 
            btnGeri.Location = new System.Drawing.Point(302, 366);
            btnGeri.Name = "btnGeri";
            btnGeri.Size = new System.Drawing.Size(75, 23);
            btnGeri.TabIndex = 3;
            btnGeri.Text = "button1";
            btnGeri.UseVisualStyleBackColor = true;
            // 
            // btnIleri
            // 
            btnIleri.Location = new System.Drawing.Point(499, 373);
            btnIleri.Name = "btnIleri";
            btnIleri.Size = new System.Drawing.Size(75, 23);
            btnIleri.TabIndex = 4;
            btnIleri.Text = "button2";
            btnIleri.UseVisualStyleBackColor = true;
            // 
            // Form8
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(btnIleri);
            Controls.Add(btnGeri);
            Controls.Add(picSagSayfa);
            Controls.Add(picSolSayfa);
            Controls.Add(lstKitaplar);
            Name = "Form8";
            Text = "Form8";
            ((System.ComponentModel.ISupportInitialize)picSolSayfa).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSagSayfa).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListBox lstKitaplar;
        private System.Windows.Forms.PictureBox picSolSayfa;
        private System.Windows.Forms.PictureBox picSagSayfa;
        private System.Windows.Forms.Button btnGeri;
        private System.Windows.Forms.Button btnIleri;
    }
}