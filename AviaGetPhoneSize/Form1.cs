using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AviaGetPhoneSize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Bitmap img = new Bitmap(@"C:\ProgramData\FutureDial\AVIA\frames\frame_01863.jpg");
            //img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            //pictureBox1.Image = img;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            VideoCapture vc = new VideoCapture(0, VideoCapture.API.DShow);
            vc.SetCaptureProperty(CapProp.FrameHeight, 1944);
            vc.SetCaptureProperty(CapProp.FrameWidth, 2592);
            this.Tag = vc;
            Task.Run(() => { capture_frame(); });
        }

        void capture_frame()
        {
#if true
            VideoCapture vc = (VideoCapture)this.Tag;
            while (true)
            {
                Mat m = new Mat();
                vc.Read(m);
                if (!m.IsEmpty)
                {
                    Image<Bgr, Byte> img = prepare_image(m.ToImage<Bgr,Byte>());
                    pictureBox1.Invoke(new Action(() =>
                    {
                        pictureBox1.Image = img.Bitmap;
                    }));
                }
                System.Threading.Thread.Sleep(500);
            }
#endif
        }
        [STAThread]
        private void Button1_Click(object sender, EventArgs e)
        {
            // save image.
            Bitmap img = new Bitmap(pictureBox1.Image);
            //if (!string.IsNullOrEmpty(textBoxComment.Text))
            //{
            //    // save the comment into jpg
            //    byte[] b = System.Text.Encoding.Unicode.GetBytes(textBoxComment.Text);
            //    PropertyItem pic = img.GetPropertyItem(40092);
            //    pic.Value = b;
            //    pic.Len = b.Length;
            //    pic.Type = 1;
            //    img.SetPropertyItem(pic);
            //}
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.FileName = textBoxComment.Text;
            saveFileDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
            {
                img.Save(saveFileDialog1.FileName);
            }
        }

        private void PictureBox1_DoubleClick(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            //saveFileDialog1.FileName = textBoxComment.Text;
            saveFileDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName);
            }
        }

        Image<Bgr, Byte> prepare_image(Image<Bgr,Byte> img)
        {
            Rectangle r = new Rectangle(534, 352, 606, 1118);
            Image<Bgr, Byte> ret = null;
            // 
            ret = img.Rotate(-90.0, new Bgr(0, 0, 0), false);
            ret.Draw(r, new Bgr(0, 0, 255), 5);
            return ret;
        }
    }
}
