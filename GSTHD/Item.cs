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
        public int DefaultIndex = 0;
        public int DK64_ID;
        private string DragImage = null;

        bool isBroadcastable;
        string DoubleBroadcastSide;
        string DoubleBroadcastName;
        bool isDraggable;

        public string AutoName = null;

        delegate void SetStateCallback(int state);

        public Item(ObjectPoint data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;

            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            Name = data.Name;
            BackColor = data.BackColor;
            this.isBroadcastable = data.isBroadcastable && !isBroadcast;

            this.isDraggable = data.isDraggable;

            this.AutoName = data.AutoName;

            this.DragImage = data.DragImage;
            this.DK64_ID = data.DK64_ID;

            this.DefaultIndex = data.DefaultIndex;
            ImageIndex = DefaultIndex;

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

            Control thething = this.Parent;
            if (thething != null) { MessageBox.Show("Do you want to save changes to your text?", thething.Name); }
            if (!isBroadcast)
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

        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = null;
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
            if (this.InvokeRequired)
            {
                Invoke(new SetStateCallback(SetState), new object[] { state });
                return;
            }
            else
            {
                ImageIndex = state;
                VerifyState();
                UpdateImage();
                DragBehaviour.SaveChanges();
            }
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

        private void VerifyState()
        {
            if (ImageIndex > ImageNames.Length - 1) ImageIndex = (ImageNames.Length - 1);
            if (ImageIndex < 0) ImageIndex = 0;
        }

        public void StartDragDrop()
        {
            if (DragImage != null)
            {
                var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, DragImage, DK64_ID);
                DoDragDrop(dropContent, DragDropEffects.Copy);
            }
            else
            {
                var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[System.Math.Max(ImageIndex, 1)], DK64_ID);
                DoDragDrop(dropContent, DragDropEffects.Copy);
            }
        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
