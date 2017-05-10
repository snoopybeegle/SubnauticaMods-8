namespace PixelArtCreator
{
    partial class PixelArt
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
            this.convert = new System.Windows.Forms.Button();
            this.output = new System.Windows.Forms.TextBox();
            this.path = new System.Windows.Forms.TextBox();
            this.filePicker = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // convert
            // 
            this.convert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.convert.Location = new System.Drawing.Point(232, 12);
            this.convert.Name = "convert";
            this.convert.Size = new System.Drawing.Size(163, 23);
            this.convert.TabIndex = 0;
            this.convert.Text = "convert";
            this.convert.UseVisualStyleBackColor = true;
            this.convert.Click += new System.EventHandler(this.convert_Click);
            // 
            // output
            // 
            this.output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.output.Location = new System.Drawing.Point(12, 41);
            this.output.Multiline = true;
            this.output.Name = "output";
            this.output.Size = new System.Drawing.Size(383, 134);
            this.output.TabIndex = 1;
            // 
            // path
            // 
            this.path.Location = new System.Drawing.Point(12, 12);
            this.path.Name = "path";
            this.path.Size = new System.Drawing.Size(172, 20);
            this.path.TabIndex = 2;
            // 
            // filePicker
            // 
            this.filePicker.Location = new System.Drawing.Point(190, 12);
            this.filePicker.Name = "filePicker";
            this.filePicker.Size = new System.Drawing.Size(36, 23);
            this.filePicker.TabIndex = 3;
            this.filePicker.Text = "...";
            this.filePicker.UseVisualStyleBackColor = true;
            this.filePicker.Click += new System.EventHandler(this.filePicker_Click);
            // 
            // PixelArt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 187);
            this.Controls.Add(this.filePicker);
            this.Controls.Add(this.path);
            this.Controls.Add(this.output);
            this.Controls.Add(this.convert);
            this.Name = "PixelArt";
            this.Text = "Subnautica pixel art";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button convert;
        private System.Windows.Forms.TextBox output;
        private System.Windows.Forms.TextBox path;
        private System.Windows.Forms.Button filePicker;
    }
}

