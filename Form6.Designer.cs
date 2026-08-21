namespace WinFormsApp2
{
    partial class Form6
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
            btnCevapla = new System.Windows.Forms.Button();
            lblSoru = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            rbD = new System.Windows.Forms.RadioButton();
            rbC = new System.Windows.Forms.RadioButton();
            rbB = new System.Windows.Forms.RadioButton();
            rbA = new System.Windows.Forms.RadioButton();
            lblBilgi = new System.Windows.Forms.Label();
            rtbParagraf = new System.Windows.Forms.RichTextBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnCevapla
            // 
            btnCevapla.Location = new System.Drawing.Point(151, 190);
            btnCevapla.Name = "btnCevapla";
            btnCevapla.Size = new System.Drawing.Size(75, 23);
            btnCevapla.TabIndex = 0;
            btnCevapla.Text = "CEVAPLA";
            btnCevapla.UseVisualStyleBackColor = true;
            // 
            // lblSoru
            // 
            lblSoru.Location = new System.Drawing.Point(100, 57);
            lblSoru.Name = "lblSoru";
            lblSoru.Size = new System.Drawing.Size(627, 70);
            lblSoru.TabIndex = 1;
            lblSoru.Text = "label1";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(rbD);
            groupBox1.Controls.Add(rbC);
            groupBox1.Controls.Add(btnCevapla);
            groupBox1.Controls.Add(rbB);
            groupBox1.Controls.Add(rbA);
            groupBox1.Location = new System.Drawing.Point(193, 201);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(467, 237);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Cevaplar";
            // 
            // rbD
            // 
            rbD.AutoSize = true;
            rbD.Location = new System.Drawing.Point(31, 146);
            rbD.Name = "rbD";
            rbD.Size = new System.Drawing.Size(94, 19);
            rbD.TabIndex = 3;
            rbD.TabStop = true;
            rbD.Text = "radioButton4";
            rbD.UseVisualStyleBackColor = true;
            // 
            // rbC
            // 
            rbC.AutoSize = true;
            rbC.Location = new System.Drawing.Point(31, 105);
            rbC.Name = "rbC";
            rbC.Size = new System.Drawing.Size(94, 19);
            rbC.TabIndex = 2;
            rbC.TabStop = true;
            rbC.Text = "radioButton3";
            rbC.UseVisualStyleBackColor = true;
            // 
            // rbB
            // 
            rbB.AutoSize = true;
            rbB.Location = new System.Drawing.Point(31, 66);
            rbB.Name = "rbB";
            rbB.Size = new System.Drawing.Size(94, 19);
            rbB.TabIndex = 1;
            rbB.TabStop = true;
            rbB.Text = "radioButton2";
            rbB.UseVisualStyleBackColor = true;
            // 
            // rbA
            // 
            rbA.AutoSize = true;
            rbA.Location = new System.Drawing.Point(31, 23);
            rbA.Name = "rbA";
            rbA.Size = new System.Drawing.Size(94, 19);
            rbA.TabIndex = 0;
            rbA.TabStop = true;
            rbA.Text = "radioButton1";
            rbA.UseVisualStyleBackColor = true;
            // 
            // lblBilgi
            // 
            lblBilgi.AutoSize = true;
            lblBilgi.Location = new System.Drawing.Point(103, 320);
            lblBilgi.Name = "lblBilgi";
            lblBilgi.Size = new System.Drawing.Size(38, 15);
            lblBilgi.TabIndex = 3;
            lblBilgi.Text = "label1";
            // 
            // rtbParagraf
            // 
            rtbParagraf.Location = new System.Drawing.Point(226, 46);
            rtbParagraf.Name = "rtbParagraf";
            rtbParagraf.Size = new System.Drawing.Size(417, 149);
            rtbParagraf.TabIndex = 4;
            rtbParagraf.Text = "";
            // 
            // Form6
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(rtbParagraf);
            Controls.Add(lblBilgi);
            Controls.Add(groupBox1);
            Controls.Add(lblSoru);
            Name = "Form6";
            Text = "Form6";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnCevapla;
        private System.Windows.Forms.Label lblSoru;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbD;
        private System.Windows.Forms.RadioButton rbC;
        private System.Windows.Forms.RadioButton rbB;
        private System.Windows.Forms.RadioButton rbA;
        private System.Windows.Forms.Label lblBilgi;
        private System.Windows.Forms.RichTextBox rtbParagraf;
    }
}