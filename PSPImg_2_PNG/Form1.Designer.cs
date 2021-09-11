namespace PSPImg_2_PNG
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.TextFileNameDir = new System.Windows.Forms.Label();
            this.TextFileName = new System.Windows.Forms.Label();
            this.TextConverted = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.AlphaOverride_CheckBox = new System.Windows.Forms.CheckBox();
            this.ExportColorPal_CheckBox = new System.Windows.Forms.CheckBox();
            this.ToolText = new System.Windows.Forms.Label();
            this.UseColorPal_CheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textureDisplayText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 41);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(202, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Convert To .png";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(202, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Open .img File";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // TextFileNameDir
            // 
            this.TextFileNameDir.AutoSize = true;
            this.TextFileNameDir.Location = new System.Drawing.Point(9, 190);
            this.TextFileNameDir.Name = "TextFileNameDir";
            this.TextFileNameDir.Size = new System.Drawing.Size(135, 13);
            this.TextFileNameDir.TabIndex = 3;
            this.TextFileNameDir.Text = "Opened File Directory: N/A";
            // 
            // TextFileName
            // 
            this.TextFileName.AutoSize = true;
            this.TextFileName.Location = new System.Drawing.Point(9, 212);
            this.TextFileName.Name = "TextFileName";
            this.TextFileName.Size = new System.Drawing.Size(121, 13);
            this.TextFileName.TabIndex = 4;
            this.TextFileName.Text = "Opened File Name: N/A";
            // 
            // TextConverted
            // 
            this.TextConverted.AutoSize = true;
            this.TextConverted.Location = new System.Drawing.Point(9, 234);
            this.TextConverted.Name = "TextConverted";
            this.TextConverted.Size = new System.Drawing.Size(122, 13);
            this.TextConverted.TabIndex = 5;
            this.TextConverted.Text = "Console Messages: N/A";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 98);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(202, 23);
            this.button3.TabIndex = 6;
            this.button3.Text = "Open .png";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 127);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(202, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "Convert To .img";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(240, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = ".img to .png options";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // AlphaOverride_CheckBox
            // 
            this.AlphaOverride_CheckBox.AutoSize = true;
            this.AlphaOverride_CheckBox.Location = new System.Drawing.Point(243, 41);
            this.AlphaOverride_CheckBox.Name = "AlphaOverride_CheckBox";
            this.AlphaOverride_CheckBox.Size = new System.Drawing.Size(257, 17);
            this.AlphaOverride_CheckBox.TabIndex = 9;
            this.AlphaOverride_CheckBox.Text = "Override .img Alpha (Makes all alpha values max)";
            this.AlphaOverride_CheckBox.UseVisualStyleBackColor = true;
            // 
            // ExportColorPal_CheckBox
            // 
            this.ExportColorPal_CheckBox.AutoSize = true;
            this.ExportColorPal_CheckBox.Location = new System.Drawing.Point(243, 64);
            this.ExportColorPal_CheckBox.Name = "ExportColorPal_CheckBox";
            this.ExportColorPal_CheckBox.Size = new System.Drawing.Size(318, 17);
            this.ExportColorPal_CheckBox.TabIndex = 10;
            this.ExportColorPal_CheckBox.Text = "Export color pal as separate .png (Only on .img with color pals)";
            this.ExportColorPal_CheckBox.UseVisualStyleBackColor = true;
            // 
            // ToolText
            // 
            this.ToolText.AutoSize = true;
            this.ToolText.Location = new System.Drawing.Point(752, 234);
            this.ToolText.Name = "ToolText";
            this.ToolText.Size = new System.Drawing.Size(125, 13);
            this.ToolText.TabIndex = 11;
            this.ToolText.Text = "Tool Made By Iron Hawk";
            // 
            // UseColorPal_CheckBox
            // 
            this.UseColorPal_CheckBox.AutoSize = true;
            this.UseColorPal_CheckBox.Location = new System.Drawing.Point(243, 127);
            this.UseColorPal_CheckBox.Name = "UseColorPal_CheckBox";
            this.UseColorPal_CheckBox.Size = new System.Drawing.Size(349, 17);
            this.UseColorPal_CheckBox.TabIndex = 13;
            this.UseColorPal_CheckBox.Text = "Use color pal (Make sure image is 256 colors or less when using this)";
            this.UseColorPal_CheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(240, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = ".png to .img options";
            // 
            // textureDisplayText
            // 
            this.textureDisplayText.AutoSize = true;
            this.textureDisplayText.Location = new System.Drawing.Point(12, 270);
            this.textureDisplayText.Name = "textureDisplayText";
            this.textureDisplayText.Size = new System.Drawing.Size(0, 13);
            this.textureDisplayText.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(889, 256);
            this.Controls.Add(this.textureDisplayText);
            this.Controls.Add(this.UseColorPal_CheckBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ToolText);
            this.Controls.Add(this.ExportColorPal_CheckBox);
            this.Controls.Add(this.AlphaOverride_CheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.TextConverted);
            this.Controls.Add(this.TextFileName);
            this.Controls.Add(this.TextFileNameDir);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "PSP .img to .png";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label TextFileNameDir;
        private System.Windows.Forms.Label TextFileName;
        private System.Windows.Forms.Label TextConverted;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox AlphaOverride_CheckBox;
        private System.Windows.Forms.CheckBox ExportColorPal_CheckBox;
        private System.Windows.Forms.Label ToolText;
        private System.Windows.Forms.CheckBox UseColorPal_CheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label textureDisplayText;
    }
}

