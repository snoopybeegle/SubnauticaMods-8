using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PixelArtCreator
{
    public partial class PixelArt : Form
    {
        public PixelArt()
        {
            InitializeComponent();
        }

        public enum PaletteColor
        {

        }

        public List<Color> pcd = new List<Color>() {
            Color.FromArgb(255, 222, 147),
            Color.FromArgb(255, 205, 119),
            Color.FromArgb(255, 185, 107),
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(163, 73, 164),
            Color.FromArgb(228, 142, 255),
            Color.FromArgb(211, 165, 0),
            Color.FromArgb(192, 192, 192),
            Color.FromArgb(26, 106, 88),
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 106, 0),
            Color.FromArgb(0, 5, 255),
            Color.FromArgb(127, 127, 127),
            Color.FromArgb(127, 0, 0),
            Color.FromArgb(124, 89, 0)
        };

        public List<string> hexList = new List<string>() {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"
        };

        private void filePicker_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(fileDialog.FileName) && File.Exists(fileDialog.FileName))
                {
                    path.Text = fileDialog.FileName;
                }
            }
        }

        BackgroundWorker makeText;
        
        string finalText = "";

        private void convert_Click(object sender, EventArgs e)
        {
            finalText = "";
            makeText = new BackgroundWorker();
            makeText.DoWork += new DoWorkEventHandler(makeTextHandler);
            makeText.ProgressChanged += new ProgressChangedEventHandler(makeTextHandlerProgress);
            makeText.WorkerReportsProgress = true;
            makeText.RunWorkerAsync();
        }

        public void makeTextHandler(object sender, DoWorkEventArgs e)
        {
            using (MagickImage image = new MagickImage(path.Text))
            {
                PixelCollection pc = image.GetPixels();
                int pixelsMade = 0;
                int pixelWidth = image.Width;
                foreach (Pixel pixel in pc)
                {
                    Color color = pixel.ToColor().ToColor();
                    if (pcd.Contains(color)) //could possibly not work because not same obj
                    {
                        try
                        {
                            int index = pcd.FindIndex(a => a == color);
                            makeText.ReportProgress(0, hexList[index]);
                            //output.Text += hexList[index];
                        }
                        catch (Exception ex) { }
                    }
                    pixelsMade++;
                    if (pixelsMade % pixelWidth == 0)
                    {
                        makeText.ReportProgress(0, "\n");
                        //output.Text += "\n";
                    }
                }
            }
        }

        public void makeTextHandlerProgress(object sender, ProgressChangedEventArgs e)
        {
            string userState = (string)e.UserState;
            finalText += userState;
            if (userState.Substring(userState.Length - 1, 1) == "\n")
            {
                output.Text = finalText;
            }
        }

    }
}
