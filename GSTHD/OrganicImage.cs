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
    public enum MarkedImageIndex
    {
        none,
        mark_check,
        mark_x
    }

    public class OrganicImage : Control
    {
        public bool isFaded = false;

        public Image Image = null;

        public MarkedImageIndex isMarked = 0;
        public int MarkedMax = Enum.GetNames(typeof(MarkedImageIndex)).Length;
        public Image markedImage = null;
        public bool isMarkable = true;

        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;

        public delegate void UpdateImageCallback(PaintEventArgs state);

        public OrganicImage()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DisplayImage(e);
            base.OnPaint(e);
        }

        public void IncrementMarked(bool MarkModeCycle)
        {
            if (isMarkable) isMarked = (MarkedImageIndex)((((int)isMarked)+1) % ( (MarkModeCycle) ? MarkedMax : 2));
        }

        public int[] GetSizeDims()
        {
            // newX newY newWidth newHeight
            // imgX imgY imgWidth imgHeight


            int newX, newY, newWidth, newHeight;
            switch (SizeMode){
                case PictureBoxSizeMode.Zoom:
                    // this needs to be fixed for the world names
                    float ratio = System.Math.Max((float)Image.Width / (float)Width, (float)Image.Height / (float)Height);
                    newWidth = (int)(Image.Width / ratio);
                    newHeight = (int)(Image.Height / ratio);
                    newY = (this.Height - newHeight) / 2;
                    newX = (this.Width - newWidth) / 2;
                    
                    return new int[] { newX, newY, newWidth, newHeight, 0, 0, Image.Width, Image.Height };
                case PictureBoxSizeMode.CenterImage:
                    newX = (Width-Image.Width) / 2;
                    newY = (Height-Image.Height) / 2;
                    return new int[] { newX, newY, Image.Width, Image.Height, 0, 0, Image.Width, Image.Height };
                case PictureBoxSizeMode.Normal:
                    return new int[] { 0, 0, this.Width, this.Height, 0, 0, this.Width, this.Height };
                case PictureBoxSizeMode.StretchImage:
                    return new int[] { 0, 0, this.Width, this.Height, 0, 0, Image.Width, Image.Height };
                case PictureBoxSizeMode.AutoSize:
                default:
                    if (Width < Image.Width) Width = Image.Width;
                    if (Height < Image.Height) Height = Image.Height;
                    return new int[] { 0, 0, System.Math.Min(this.Width, Image.Width), System.Math.Min(this.Height, Image.Height), 0, 0, System.Math.Min(this.Width, Image.Width), System.Math.Min(this.Height, Image.Height) };
            }
        }

        public void DisplayImage(PaintEventArgs e)
        {
            // self reminder that in order to force an image update, you need to call Invalidat()
            if (this.InvokeRequired)
            {
                Invoke(new UpdateImageCallback(DisplayImage), new object[] { e });
                return;
            }
            else
            {
                float howFaded = (isFaded) ? (float)0.5 : 1;

                ColorMatrix cm = new ColorMatrix();
                cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                cm.Matrix33 = howFaded;

                ImageAttributes ia = new ImageAttributes();
                ia.SetColorMatrix(cm);


                var results = GetSizeDims();

                
                e.Graphics.DrawImage(Image,
                    new Rectangle(results[0], results[1], results[2], results[3]),
                    results[4], results[5], results[6], results[7], GraphicsUnit.Pixel,
                    ia);


                // hide check if too dang small (if checkmark is over half of the visible image area)
                if (isMarked > 0 && (8*8 < (results[2] * results[3]/2)))
                {
                    markedImage = Image.FromFile($@"Resources/{isMarked}.png");
                    e.Graphics.DrawImage(markedImage,
                    new Rectangle(Width-8, 0, 8, 8),
                        0, 0, markedImage.Width, markedImage.Height, GraphicsUnit.Pixel);
                }
            }
        }
    }
}
