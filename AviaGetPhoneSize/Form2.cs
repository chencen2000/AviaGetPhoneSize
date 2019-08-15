using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AviaGetPhoneSize
{
    public partial class Form2 : Form
    {
        //Rectangle roi = new Rectangle(534, 352, 606, 1118);
        Rectangle roi = new Rectangle(615, 345, 610, 1110);

        public Form2()
        {
            InitializeComponent();
        }

        private void ToolStripLoad_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = @"C:\Tools\avia\images\avia_m0_pc\FromMyCam"; // @"C:\Tools\avia\images\FromMyCam";
                    openFileDialog.Filter = "jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bp = new Bitmap(openFileDialog.FileName);
                        Image<Bgr, Byte> img = prepare_image(bp);
                        pictureBox1.Image = img.Bitmap;
                        pictureBox1.Tag = openFileDialog.FileName;
                    }
                }
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {

            }
        }
        private void ToolStripSave_Click(object sender, EventArgs e)
        {
            if(tabControl1.SelectedTab == tabPage1)
            {

            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                if (pictureBoxROI.Image != null)
                {
                    using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                    {
                        saveFileDialog1.Filter = "jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                        saveFileDialog1.FilterIndex = 2;
                        saveFileDialog1.RestoreDirectory = true;
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            pictureBoxROI.Image.Save(saveFileDialog1.FileName);
                        }
                    }
                }
            }
        }

        Image<Bgr, Byte> prepare_image(Bitmap bp)
        {
            //Rectangle r = new Rectangle(534, 352, 606, 1118);
            Image<Bgr, Byte> img = new Image<Bgr, byte>(bp);
            // 
            Image<Bgr, Byte> ret = img.Rotate(-90.0, new Bgr(0, 0, 0), false);
            ret.Draw(roi, new Bgr(0, 0, 255), 5);
            return ret;
         }

        #region tab 2
        Image<Bgr, Byte> img_roi_bgr = null;
        Image<Hsv, byte> img_roi_hsv = null;
        private void TabPage2_Enter(object sender, EventArgs e)
        {
            Object o = pictureBox1.Tag;
            if (o != null && System.IO.File.Exists(o.ToString()))
            {
                Image<Bgr, Byte> img = new Image<Bgr, byte>(o.ToString()).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(roi);
                //img.Draw(new Rectangle(375,450,30,200), new Bgr(0, 0, 255), 5);
                pictureBoxROI.Image = img.Bitmap;
                img_roi_bgr = img;
                img_roi_hsv = img.Convert<Hsv, byte>();
            }
        }
        #endregion

        private void PictureBoxROI_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (img_roi_bgr!=null && img_roi_hsv!=null)
                {
                    Bgr bgr = img_roi_bgr[e.Location];
                    Hsv hsv = img_roi_hsv[e.Location];
                    Program.logIt($"[{e.Location}]: bgr={bgr}, hsv={hsv}");
                }
            }
            catch (Exception) { }
        }

        private void ToolStripCheck_Click(object sender, EventArgs e)
        {

        }
    }
}
