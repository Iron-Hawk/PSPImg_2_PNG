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
            this.convertIMGToPNGFileButton = new System.Windows.Forms.Button();
            this.openIMGFileButton = new System.Windows.Forms.Button();
            this.openedFileDirText = new System.Windows.Forms.Label();
            this.openedFileNameText = new System.Windows.Forms.Label();
            this.consoleText = new System.Windows.Forms.Label();
            this.openPNGFileButton = new System.Windows.Forms.Button();
            this.convertPNGToIMGFileButton = new System.Windows.Forms.Button();
            this.imgToPNGText = new System.Windows.Forms.Label();
            this.alphaOverrideCheckbox = new System.Windows.Forms.CheckBox();
            this.exportColorPalCheckbox = new System.Windows.Forms.CheckBox();
            this.textCreatorName = new System.Windows.Forms.Label();
            this.useColorPalCheckbox = new System.Windows.Forms.CheckBox();
            this.pngToIMGText = new System.Windows.Forms.Label();
            this.textureDisplayText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // convertIMGToPNGFileButton
            // 
            this.convertIMGToPNGFileButton.Location = new System.Drawing.Point(12, 41);
            this.convertIMGToPNGFileButton.Name = "convertIMGToPNGFileButton";
            this.convertIMGToPNGFileButton.Size = new System.Drawing.Size(202, 23);
            this.convertIMGToPNGFileButton.TabIndex = 0;
            this.convertIMGToPNGFileButton.Text = "Convert To .png";
            this.convertIMGToPNGFileButton.UseVisualStyleBackColor = true;
            this.convertIMGToPNGFileButton.Click += new System.EventHandler(this.convertIMGToPNGFile);
            // 
            // openIMGFileButton
            // 
            this.openIMGFileButton.Location = new System.Drawing.Point(12, 12);
            this.openIMGFileButton.Name = "openIMGFileButton";
            this.openIMGFileButton.Size = new System.Drawing.Size(202, 23);
            this.openIMGFileButton.TabIndex = 1;
            this.openIMGFileButton.Text = "Open .img File";
            this.openIMGFileButton.UseVisualStyleBackColor = true;
            this.openIMGFileButton.Click += new System.EventHandler(this.openIMGFile);
            // 
            // openedFileDirText
            // 
            this.openedFileDirText.AutoSize = true;
            this.openedFileDirText.Location = new System.Drawing.Point(9, 190);
            this.openedFileDirText.Name = "openedFileDirText";
            this.openedFileDirText.Size = new System.Drawing.Size(135, 13);
            this.openedFileDirText.TabIndex = 3;
            this.openedFileDirText.Text = "Opened File Directory: N/A";
            // 
            // openedFileNameText
            // 
            this.openedFileNameText.AutoSize = true;
            this.openedFileNameText.Location = new System.Drawing.Point(9, 212);
            this.openedFileNameText.Name = "openedFileNameText";
            this.openedFileNameText.Size = new System.Drawing.Size(121, 13);
            this.openedFileNameText.TabIndex = 4;
            this.openedFileNameText.Text = "Opened File Name: N/A";
            // 
            // consoleText
            // 
            this.consoleText.AutoSize = true;
            this.consoleText.Location = new System.Drawing.Point(9, 234);
            this.consoleText.Name = "consoleText";
            this.consoleText.Size = new System.Drawing.Size(122, 13);
            this.consoleText.TabIndex = 5;
            this.consoleText.Text = "Console Messages: N/A";
            // 
            // openPNGFileButton
            // 
            this.openPNGFileButton.Location = new System.Drawing.Point(12, 98);
            this.openPNGFileButton.Name = "openPNGFileButton";
            this.openPNGFileButton.Size = new System.Drawing.Size(202, 23);
            this.openPNGFileButton.TabIndex = 6;
            this.openPNGFileButton.Text = "Open .png";
            this.openPNGFileButton.UseVisualStyleBackColor = true;
            this.openPNGFileButton.Click += new System.EventHandler(this.openPNGFile);
            // 
            // convertPNGToIMGFileButton
            // 
            this.convertPNGToIMGFileButton.Location = new System.Drawing.Point(12, 127);
            this.convertPNGToIMGFileButton.Name = "convertPNGToIMGFileButton";
            this.convertPNGToIMGFileButton.Size = new System.Drawing.Size(202, 23);
            this.convertPNGToIMGFileButton.TabIndex = 7;
            this.convertPNGToIMGFileButton.Text = "Convert To .img";
            this.convertPNGToIMGFileButton.UseVisualStyleBackColor = true;
            this.convertPNGToIMGFileButton.Click += new System.EventHandler(this.convertPNGToIMGFile);
            // 
            // imgToPNGText
            // 
            this.imgToPNGText.AutoSize = true;
            this.imgToPNGText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imgToPNGText.Location = new System.Drawing.Point(240, 12);
            this.imgToPNGText.Name = "imgToPNGText";
            this.imgToPNGText.Size = new System.Drawing.Size(123, 16);
            this.imgToPNGText.TabIndex = 8;
            this.imgToPNGText.Text = ".img to .png options";
            this.imgToPNGText.Click += new System.EventHandler(this.label1_Click);
            // 
            // alphaOverrideCheckbox
            // 
            this.alphaOverrideCheckbox.AutoSize = true;
            this.alphaOverrideCheckbox.Location = new System.Drawing.Point(243, 41);
            this.alphaOverrideCheckbox.Name = "alphaOverrideCheckbox";
            this.alphaOverrideCheckbox.Size = new System.Drawing.Size(257, 17);
            this.alphaOverrideCheckbox.TabIndex = 9;
            this.alphaOverrideCheckbox.Text = "Override .img Alpha (Makes all alpha values max)";
            this.alphaOverrideCheckbox.UseVisualStyleBackColor = true;
            // 
            // exportColorPalCheckbox
            // 
            this.exportColorPalCheckbox.AutoSize = true;
            this.exportColorPalCheckbox.Location = new System.Drawing.Point(243, 64);
            this.exportColorPalCheckbox.Name = "exportColorPalCheckbox";
            this.exportColorPalCheckbox.Size = new System.Drawing.Size(318, 17);
            this.exportColorPalCheckbox.TabIndex = 10;
            this.exportColorPalCheckbox.Text = "Export color pal as separate .png (Only on .img with color pals)";
            this.exportColorPalCheckbox.UseVisualStyleBackColor = true;
            // 
            // textCreatorName
            // 
            this.textCreatorName.AutoSize = true;
            this.textCreatorName.Location = new System.Drawing.Point(752, 234);
            this.textCreatorName.Name = "textCreatorName";
            this.textCreatorName.Size = new System.Drawing.Size(125, 13);
            this.textCreatorName.TabIndex = 11;
            this.textCreatorName.Text = "Tool Made By Iron Hawk";
            // 
            // useColorPalCheckbox
            // 
            this.useColorPalCheckbox.AutoSize = true;
            this.useColorPalCheckbox.Location = new System.Drawing.Point(243, 127);
            this.useColorPalCheckbox.Name = "useColorPalCheckbox";
            this.useColorPalCheckbox.Size = new System.Drawing.Size(349, 17);
            this.useColorPalCheckbox.TabIndex = 13;
            this.useColorPalCheckbox.Text = "Use color pal (Make sure image is 256 colors or less when using this)";
            this.useColorPalCheckbox.UseVisualStyleBackColor = true;
            // 
            // pngToIMGText
            // 
            this.pngToIMGText.AutoSize = true;
            this.pngToIMGText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pngToIMGText.Location = new System.Drawing.Point(240, 98);
            this.pngToIMGText.Name = "pngToIMGText";
            this.pngToIMGText.Size = new System.Drawing.Size(123, 16);
            this.pngToIMGText.TabIndex = 12;
            this.pngToIMGText.Text = ".png to .img options";
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
            this.Controls.Add(this.useColorPalCheckbox);
            this.Controls.Add(this.pngToIMGText);
            this.Controls.Add(this.textCreatorName);
            this.Controls.Add(this.exportColorPalCheckbox);
            this.Controls.Add(this.alphaOverrideCheckbox);
            this.Controls.Add(this.imgToPNGText);
            this.Controls.Add(this.convertPNGToIMGFileButton);
            this.Controls.Add(this.openPNGFileButton);
            this.Controls.Add(this.consoleText);
            this.Controls.Add(this.openedFileNameText);
            this.Controls.Add(this.openedFileDirText);
            this.Controls.Add(this.openIMGFileButton);
            this.Controls.Add(this.convertIMGToPNGFileButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "PSP .img to .png";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button convertIMGToPNGFileButton;
        private System.Windows.Forms.Button openIMGFileButton;
        private System.Windows.Forms.Label openedFileDirText;
        private System.Windows.Forms.Label openedFileNameText;
        private System.Windows.Forms.Label consoleText;
        private System.Windows.Forms.Button openPNGFileButton;
        private System.Windows.Forms.Button convertPNGToIMGFileButton;
        private System.Windows.Forms.Label imgToPNGText;
        private System.Windows.Forms.CheckBox alphaOverrideCheckbox;
        private System.Windows.Forms.CheckBox exportColorPalCheckbox;
        private System.Windows.Forms.Label textCreatorName;
        private System.Windows.Forms.CheckBox useColorPalCheckbox;
        private System.Windows.Forms.Label pngToIMGText;
        private System.Windows.Forms.Label textureDisplayText;
    }
}

