using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    public struct GossipStoneState
    {
        public bool HoldsImage;
        public string HeldImageName;
        public int ImageIndex;
    }

    public class GossipStone : PictureBox, ProgressibleElement<GossipStoneState>, DraggableElement<GossipStoneState>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<GossipStoneState> ProgressBehaviour;
        private readonly DraggableElementBehaviour<GossipStoneState> DragBehaviour;

        private string[] ImageNames;
        private bool HoldsImage;
        private string HeldImageName;
        private int ImageIndex = 0;
        private bool RemoveImage;
        private bool isScrollable;
        bool isBroadcastable;
        public bool hoveredOver;

        Size GossipStoneSize;

        public GossipStone(ObjectPoint data, Settings settings)
            : this(settings, data.Name, data.X, data.Y, data.ImageCollection, data.Size, data.isScrollable, data.SizeMode, data.isBroadcastable) { }

        public GossipStone(Settings settings, string name, int x, int y, string[] imageCollection, Size imageSize, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable)
        {
            Settings = settings;

            if (imageCollection == null)
                ImageNames = Settings.DefaultGossipStoneImages;
            else
                ImageNames = imageCollection;

            Name = name;
            GossipStoneSize = imageSize;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                this.SizeMode = (PictureBoxSizeMode)SizeMode;
                this.Size = GossipStoneSize;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<GossipStoneState>(this, Settings);
            DragBehaviour = new DraggableElementBehaviour<GossipStoneState>(this, Settings);

            this.BackColor = Color.Transparent;
            this.Location = new Point(x, y);
            this.TabStop = false;
            this.AllowDrop = true;
            this.isScrollable = isScrollable;
            this.isBroadcastable = isBroadcastable;



            this.MouseUp += DragBehaviour.Mouse_ClickUp;
            this.MouseDown += ProgressBehaviour.Mouse_ClickDown;
            this.MouseDown += DragBehaviour.Mouse_ClickDown;
            this.MouseMove += Mouse_Move;
            this.DragEnter += Mouse_DragEnter;
            this.DragDrop += Mouse_DragDrop;
            this.MouseWheel += Mouse_Wheel;
            this.MouseEnter += Panel_MouseEnter;
            this.MouseLeave += Panel_MouseLeave;
        }

        // both of these functions are for when the stone is in a WOTH panel, so that it can be scrolled without the whole WOTH panle scrolling as well
        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            this.hoveredOver = true;
        }

        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            this.hoveredOver = false;
        }

        private void Mouse_Wheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0 && this.isScrollable == true)
            {
                RemoveImage = true;
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                ImageIndex += Settings.InvertScrollWheel ? scrolls : -scrolls;
                if (ImageIndex < 0) ImageIndex = 0;
                else if (ImageIndex >= ImageNames.Length) ImageIndex = ImageNames.Length - 1;
                UpdateImage();
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).Mouse_Wheel(sender, e);
                };
            }
        }

        private void Mouse_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void Mouse_DragDrop(object sender, DragEventArgs e)
        {
            ImageIndex = 0;
            HoldsImage = true;
            var dropContent = (DragDropContent)e.Data.GetData(typeof(DragDropContent));
            HeldImageName = dropContent.ImageName;
            UpdateImage();
            DragBehaviour.SaveChanges();
        }

        public void Mouse_ClickUp(object sender, MouseEventArgs e)
        {
            DragBehaviour.Mouse_ClickUp(sender, e);
        }

        public void Mouse_Move(object sender, MouseEventArgs e)
        {
            if (HoldsImage)
            {
                DragBehaviour.Mouse_Move(sender, e);
            }
        }

        private void UpdateImage()
        {
            if (HoldsImage)
            {
                Image = Image.FromFile(@"Resources/" + HeldImageName);
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).HeldImageName = HeldImageName;
                    ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).HoldsImage = true;
                    ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
                }
            }
            else
            {
                Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                    ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
                }
            }
        }

        public GossipStoneState GetState()
        {
            return new GossipStoneState()
            {
                HoldsImage = HoldsImage,
                HeldImageName = HeldImageName,
                ImageIndex = ImageIndex,
            };
        }

        public GossipStoneState GetStateBroadcast()
        {
            return new GossipStoneState()
            {
                HoldsImage = HoldsImage,
                HeldImageName = HeldImageName,
                ImageIndex = ImageIndex,
            };
        }

        public void SetState(GossipStoneState state)
        {
            HoldsImage = state.HoldsImage;
            HeldImageName = state.HeldImageName;
            ImageIndex = state.ImageIndex;
        }

        public void IncrementState()
        {
            RemoveImage = true;
            if (ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
            UpdateImage();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).IncrementState();
            };
        }

        public void DecrementState()
        {
            RemoveImage = true;
            if (ImageIndex > 0) ImageIndex -= 1;
            UpdateImage();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).DecrementState();
            };
        }

        public void ResetState()
        {
            RemoveImage = true;
            ImageIndex = 0;
            UpdateImage();
        }

        public void StartDragDrop()
        {
            HoldsImage = false;
            UpdateImage();
            var dropContent = new DragDropContent(false, HeldImageName);
            DoDragDrop(dropContent, DragDropEffects.Copy);
            SaveChanges();
        }

        public void SaveChanges()
        {
            if (RemoveImage)
            {
                HoldsImage = false;
                RemoveImage = false;
                UpdateImage();
            }
        }

        public void CancelChanges() { }
    }
}
