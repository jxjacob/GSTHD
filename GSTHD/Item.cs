using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    public class Item : PictureBox, ProgressibleElement<int>, DraggableAutocheckElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<int> DragBehaviour;

        private string[] ImageNames;
        private int ImageIndex = 0;

        bool isBroadcastable;
        string DoubleBroadcastSide;
        string DoubleBroadcastName;
        bool isDraggable;

        public Item(ObjectPoint data, Settings settings)
        {
            Settings = settings;

            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            Name = data.Name;
            BackColor = data.BackColor;
            this.isBroadcastable = data.isBroadcastable;

            this.isDraggable = data.isDraggable;

            if (data.DoubleBroadcastSide != null) this.DoubleBroadcastSide = data.DoubleBroadcastSide;
            if (data.DoubleBroadcastName != null) this.DoubleBroadcastName = data.DoubleBroadcastName;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                SizeMode = (PictureBoxSizeMode)data.SizeMode;
                Size = data.Size;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<int>(this, Settings);

            Location = new Point(data.X, data.Y);
            TabStop = false;
            AllowDrop = false;
            // this is the most scuffed way of only giving the ability to click to the items to form1 (main window) and not form2 (broadcast)
            //if (Application.OpenForms[Application.OpenForms.Count - 1] is Form1)
            if (true)
            {
                if (isDraggable)
                {
                    MouseUp += DragBehaviour.Mouse_ClickUp;
                    MouseDown += DragBehaviour.Mouse_ClickDown;
                    MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                    MouseWheel += DragBehaviour.Mouse_Wheel;
                }
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseWheel += Mouse_Wheel;
            }
        }

        public void Mouse_Wheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                ImageIndex += Settings.InvertScrollWheel ? scrolls : -scrolls;
                if (ImageIndex < 0) ImageIndex = 0;
                else if (ImageIndex >= ImageNames.Length) ImageIndex = ImageNames.Length - 1;
                UpdateImage();
            }
        }

        private void UpdateImage()
        {
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                if (DoubleBroadcastName == null || DoubleBroadcastSide == null)
                {
                    ((Item)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                    ((Item)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
                } else
                {
                    //TODO: make this block cleaner
                    if (DoubleBroadcastSide == "left")
                    {
                        if (ImageIndex == 0)
                        {
                            ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(DoubleBroadcastName, true)[0]).DecrementLeftState();
                        } else
                        {
                            ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(DoubleBroadcastName, true)[0]).IncrementLeftState();
                        }
                        
                    } else
                    {
                        if (ImageIndex == 0)
                        {
                            ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(DoubleBroadcastName, true)[0]).DecrementRightState();
                        }
                        else
                        {
                            ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(DoubleBroadcastName, true)[0]).IncrementRightState();
                        }
                    }
                }
                
            };
        }

        public int GetState()
        {
            return ImageIndex;
        }

        public void SetState(int state)
        {
            ImageIndex = state;
            UpdateImage();
        }

        public void IncrementState()
        {
            if (ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
            UpdateImage();
        }

        public void DecrementState()
        {
            if (ImageIndex > 0) ImageIndex -= 1;
            UpdateImage();
        }

        public void ResetState()
        {
            ImageIndex = 0;
            UpdateImage();
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[System.Math.Max(ImageIndex, 1)]);
            DoDragDrop(dropContent, DragDropEffects.Copy);
        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
