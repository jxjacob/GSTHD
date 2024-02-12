using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GSTHD
{
    public class OrganicImage : Control
    {
        public bool isFaded = false;

        public Graphics imgGra = null;
        public Image Image = null;

        public bool isMarked = false;
        public Image markedImage = null;

        //TODO: make this actually work
        public PictureBoxSizeMode SizeMode = PictureBoxSizeMode.Zoom;

        public delegate void UpdateImageCallback();

        //public OrganicImage()
        //{
        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            DisplayImage();
            base.OnPaint(e);
        }

        public void DisplayImage()
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateImageCallback(DisplayImage));
                return;
            }
            else
            {
                if (imgGra == null)
                {
                    imgGra = this.CreateGraphics();
                }
                else
                {
                   imgGra.Clear(BackColor);
                }

                float howFaded = (isFaded) ? (float)0.5 : 1;

                ColorMatrix cm = new ColorMatrix();
                cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                cm.Matrix33 = howFaded;

                ImageAttributes ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                // makes a psuedo SizeMode.Zoom feature
                int newWidth, newHeight, newX, newY;
                if (this.Image.Width > this.Image.Height)
                {
                    newX = 0;
                    float ratio = (float)Image.Width / (float)Width;
                    newWidth = (int)(Image.Width / ratio);
                    newHeight = (int)(Image.Height / ratio);
                    newY = (this.Height - newHeight) / 2;
                }
                else
                {
                    newY = 0;
                    float ratio = (float)Image.Height / (float)Height;
                    newWidth = (int)(Image.Width / ratio);
                    newHeight = (int)(Image.Height / ratio);
                    newX = (this.Width - newWidth) / 2;
                }


                imgGra.DrawImage(Image,
                    new Rectangle(newX, newY, newWidth, newHeight),
                    0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel,
                    ia);

                if (isMarked)
                {
                    if (markedImage == null)
                    {
                        markedImage = Image.FromFile(@"Resources/checkmark.png");
                    }
                    imgGra.DrawImage(markedImage,
                    new Rectangle(Width-8, 0, 8, 8),
                        0, 0, markedImage.Width, markedImage.Height, GraphicsUnit.Pixel);
                }
            }
        }
    }
}
