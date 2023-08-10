using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    public class DoubleItem : PictureBox
    {
        private string[] ImageNames;
        private int ImageIndex = 0;

        bool isMouseDown = false;
        bool isColoredLeft = false;
        bool isColoredRight = false;
        Size DoubleItemSize;

        bool isBroadcastable;
        public string AutoName = null;

        public DoubleItem(ObjectPoint data, bool isBroadcast = false)
        {
            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            DoubleItemSize = data.Size;
            this.Name = data.Name;
            this.Size = DoubleItemSize;

            if (ImageNames.Length > 0)
            {
                this.Image = Image.FromFile(@"Resources/" + ImageNames[0]);
                this.SizeMode = PictureBoxSizeMode.Zoom;
                
            }

            this.BackColor = Color.Transparent;
            this.isBroadcastable = data.isBroadcastable;
            this.AutoName = data.AutoName;
            this.Location = new Point(data.X, data.Y);
            this.TabStop = false;
            this.AllowDrop = false;

           if (!isBroadcast)
            {
                this.MouseUp += this.Click_MouseUp;
                this.MouseDown += this.Click_MouseDown;
                this.MouseMove += this.Click_MouseMove;
            }
        }

        private void Click_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ToggleLeftState();

            }
            if (e.Button == MouseButtons.Right)
            {
                ToggleRightState();
            }
        }

        private void Click_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 1)
                isMouseDown = false;
            else isMouseDown = true;
        }


        private void Click_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMouseDown)
            {
                // TODO change that bool to DragBehaviour.AutocheckDragDrop
                var dropContent = new DragDropContent(false, ImageNames[4]);
                this.DoDragDrop(dropContent, DragDropEffects.Copy);
                isMouseDown = false;
            }
            if (e.Button == MouseButtons.Right && isMouseDown)
            {
                var dropContent = new DragDropContent(false, ImageNames[5]);
                this.DoDragDrop(dropContent, DragDropEffects.Copy);
                isMouseDown = false;
            }
        }

        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isColoredLeft = isColoredLeft;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isColoredRight = isColoredRight;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
            };
        }

        public void IncrementLeftState()
        {
            if (!isColoredLeft)
            {
                isColoredLeft = true;
                if (!isColoredRight) ImageIndex = 1; else ImageIndex = 3;
            }
            UpdateImage();
        }
        public void DecrementLeftState()
        {
            if (isColoredLeft)
            {
                isColoredLeft = false;
                if (!isColoredRight) ImageIndex = 0; else ImageIndex = 2;
            }
            UpdateImage();
        }
        public void ToggleLeftState()
        {
            if (isColoredLeft) DecrementLeftState(); else IncrementLeftState();
        }

        public void IncrementRightState()
        {
            if (!isColoredRight)
            {
                isColoredRight = true;
                if (!isColoredLeft) ImageIndex = 2; else ImageIndex = 3;
            }
            UpdateImage();
        }
        public void DecrementRightState()
        {
            if (isColoredRight)
            {
                isColoredRight = false;
                if (!isColoredLeft) ImageIndex = 0; else ImageIndex = 1;
            }
            UpdateImage();
        }
        public void ToggleRightState()
        {
            if (isColoredRight) DecrementRightState(); else IncrementRightState();
        }

        public int GetState()
        {
            int run = 0;
            if (isColoredLeft) { run = run ^ 1; }
            if (isColoredRight) { run = run ^ 2; }
            return run;
        }

        public void SetState(int state)
        {
            if ((state & 1) == 1) { IncrementLeftState(); }
            if ((state & 2) == 2) { IncrementRightState(); }
        }

        public void ResetState()
        {
            ImageIndex = 0;
            isColoredRight = false;
            isColoredLeft = false;
            UpdateImage();

        }


    }
}
