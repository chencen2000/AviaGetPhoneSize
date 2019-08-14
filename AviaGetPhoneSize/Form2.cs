using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AviaGetPhoneSize
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void ToolStripLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Tools\avia\images\FromMyCam";
                openFileDialog.Filter = "jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bp = new Bitmap(openFileDialog.FileName);
                    Image<Bgr, Byte> img = prepare_image(bp);
                    pictureBox1.Image = img.Bitmap;
                }
            }
        }
        Image<Bgr, Byte> prepare_image(Bitmap bp)
        {
            Rectangle r = new Rectangle(534, 352, 606, 1118);
            Image<Bgr, Byte> img = new Image<Bgr, byte>(bp);
            // 
            Image<Bgr, Byte> ret = img.Rotate(-90.0, new Bgr(0, 0, 0), false);
            ret.Draw(r, new Bgr(0, 0, 255), 5);
            return ret;
         }

        private void TabPage2_Enter(object sender, EventArgs e)
        {
            Bitmap bp = (Bitmap)pictureBox1.Image;
            if (bp != null)
            {

            }
        }
    }
}
