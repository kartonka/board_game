using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Marci30
{
    public partial class BigPic : Form
    {
        private const string BTN_TEXT_BASE = "Dobj egy kockával (ha kell)! Legutóbbi dobás:";
        public BigPic(string title, string task, string picPath)
        {
            InitializeComponent();
            textBox1.Text = title;
            pictureBox1.Image = LoadPic(picPath);
            textBox2.Text =task;
            button1.Text = BTN_TEXT_BASE + "-";
            button1.BackColor = Color.White;
            button1.ForeColor = Color.Black;
            Show();
            KeyPress += BigPic_KeyPress;
        }

        private void BigPic_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char) Keys.Escape)
            {
                Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            button1.Text = BTN_TEXT_BASE + (r.Next(6) + 1);

            if (button1.BackColor == Color.White)
            {
                button1.BackColor = Color.Black;
                button1.ForeColor = Color.White;
            }
            else
            {
                button1.BackColor = Color.White;
                button1.ForeColor = Color.Black;
            }
        }

        private Bitmap LoadPic(string path)
        {
            Image i = Image.FromFile(path);
            PropertyItem pi = i.PropertyItems.Select(x => x)
                                 .FirstOrDefault(x => x.Id == 0x0112);

            if (pi == null)
            {
                pi = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));

                pi.Id = 0x0112;   // orientation
                pi.Len = 2;
                pi.Type = 3;
                pi.Value = new byte[2] { 1, 0 };

                pi.Value[0] = 1;

                i.SetPropertyItem(pi);
            }

            Bitmap bmp = new Bitmap(i);

            byte o = pi.Value[0];

            if (o == 2) bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            if (o == 3) bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            if (o == 4) bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if (o == 5) bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
            if (o == 6) bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            if (o == 7) bmp.RotateFlip(RotateFlipType.Rotate90FlipY);
            if (o == 8) bmp.RotateFlip(RotateFlipType.Rotate90FlipXY);

            return bmp;
        }
    }
}
