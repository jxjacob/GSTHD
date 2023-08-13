using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Windows.Forms;

namespace GSTHD
{
    public struct GossipStoneState
    {
        public bool HoldsImage;
        public List<string> HeldImages;
        public int ImageIndex;


        public override string ToString() {
            // for thing in heldimage
            string exported = "";
            if (HoldsImage)
            {
                foreach (string image in HeldImages)
                {
                    if (exported.Length > 0)
                    {
                        exported += "|";
                    }
                    exported += image;
                }
            }
            // put in the name and then |
            // write that new string to the line below
            return $"{HoldsImage},{exported},{ImageIndex}"; 
        }
    }

    public class GossipStone : PictureBox, ProgressibleElement<GossipStoneState>, DraggableElement<GossipStoneState>, UpdatableFromSettings
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<GossipStoneState> ProgressBehaviour;
        private readonly DraggableElementBehaviour<GossipStoneState> DragBehaviour;

        private string[] ImageNames;
        private bool HoldsImage;
        private List<string> HeldImages = new List<string>();
        private int CycleIndex = 0;
        private bool canCycle = false;
        private int ImageIndex = 0;
        private bool RemoveImage;
        private bool isScrollable;
        bool isBroadcastable;
        public bool hoveredOver;

        Size GossipStoneSize;

        private System.Threading.Timer CyclingTimer;

        delegate void UpdateImageCallbacK();

        public GossipStone(ObjectPoint data, Settings settings, bool isOnBroadcast = false)
            : this(settings, data.Name, data.X, data.Y, data.ImageCollection, data.Size, data.isScrollable, data.SizeMode, data.isBroadcastable, data.CanCycle, isOnBroadcast) { }

        public GossipStone(Settings settings, string name, int x, int y, string[] imageCollection, Size imageSize, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool CanCycle = false, bool isOnBroadcast = false)
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
            this.canCycle = CanCycle;
            this.isBroadcastable = isBroadcastable;


            if (!isOnBroadcast)
            {
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

        }

        // both of these functions are for when the stone is in a WOTH panel, so that it can be scrolled without the whole WOTH panle scrolling as well
        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            this.hoveredOver = true;
            if (HeldImages.Count > 1 && CyclingTimer != null)
            {
                CyclingTimer.Change(-1, -1);
            }
        }

        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            this.hoveredOver = false;
            if (HeldImages.Count > 1)
            {
                CyclingTimer.Change(TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
            }
        }

        private void Mouse_Wheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0 && this.isScrollable == true)
            {
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                int whichway = Settings.InvertScrollWheel ? scrolls : -scrolls;
                if (whichway > 0)
                {
                    IncrementState();
                } else if (whichway < 0)
                {
                    DecrementState();
                }
                //if (ImageIndex < 0) ImageIndex = 0;
                //else if (ImageIndex >= ImageNames.Length) ImageIndex = ImageNames.Length - 1;
                //UpdateImage();
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
            RemoveImage = false;
            var dropContent = (DragDropContent)e.Data.GetData(typeof(DragDropContent));
            if (canCycle || Settings.ForceGossipCycles)
            {
                if (!HeldImages.Contains(dropContent.ImageName))
                {
                    HeldImages.Add(dropContent.ImageName);
                }
                if (HeldImages.Count > 1 && CyclingTimer == null)
                {
                    CyclingTimer = new System.Threading.Timer(IncrementCycle, null, TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
                }
            }
            else
            {
                HeldImages.Clear();
                HeldImages.Add(dropContent.ImageName);
            }
            UpdateImage();
            DragBehaviour.SaveChanges();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                remotewindow.HoldsImage = HoldsImage;
                remotewindow.HeldImages = HeldImages;
                remotewindow.CycleIndex = 0;
                remotewindow.UpdateImage();
                if (HeldImages.Count > 1 && remotewindow.CyclingTimer == null)
                {
                    remotewindow.CyclingTimer = new System.Threading.Timer(remotewindow.IncrementCycle, null, TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
                }
            
            }
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

        public void UpdateImage()
        {
            if (HoldsImage)
            {
                if (Image != null) Image.Dispose();
                Image = null;
                Image = Image.FromFile(@"Resources/" + HeldImages[CycleIndex]);
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.HeldImages = HeldImages;
                    remotewindow.HoldsImage = true;
                    remotewindow.UpdateImage();
                }
                if (HeldImages.Count > 1 && CyclingTimer == null)
                {
                    CyclingTimer = new System.Threading.Timer(IncrementCycle, null, TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
                }
            }
            else
            {
                if (Image != null) Image.Dispose();
                Image = null;
                Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.HoldsImage = false;
                    remotewindow.ImageIndex = ImageIndex;
                    remotewindow.UpdateImage();
                }
            }
        }

        public GossipStoneState GetState()
        {
            return new GossipStoneState()
            {
                HoldsImage = HoldsImage,
                HeldImages = HeldImages,
                ImageIndex = ImageIndex,
            };
        }

        public GossipStoneState GetStateBroadcast()
        {
            return new GossipStoneState()
            {
                HoldsImage = HoldsImage,
                HeldImages = HeldImages,
                ImageIndex = ImageIndex,
            };
        }

        public void SetState(GossipStoneState state)
        {
            HoldsImage = state.HoldsImage;
            HeldImages = state.HeldImages;
            ImageIndex = state.ImageIndex;
            UpdateImage();
            DragBehaviour.SaveChanges();
        }

        public void IncrementState()
        {
            if ((Settings.OverrideHeldImage && HoldsImage) || !HoldsImage)
            {
                RemoveImage = true;
                HoldsImage = false;
                HeldImages.Clear();
                if (ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
                UpdateImage();
            }
        }

        public void DecrementState()
        {
            if ((Settings.OverrideHeldImage && HoldsImage) || !HoldsImage)
            {
                RemoveImage = true;
                HoldsImage = false;
                HeldImages.Clear();
                if (ImageIndex > 0) ImageIndex -= 1;
                UpdateImage();
            }
        }

        public void IncrementCycle(object state)
        {
            CycleIndex++;
            if (CycleIndex == HeldImages.Count)
            {
                CycleIndex=0;
            }
            try
            {
                Invoke(new UpdateImageCallbacK(UpdateImage));
            } catch (Exception e)
            {

            }
        }

        public void ResetState()
        {
            // when hovering over an image, using middle click will delete that entry in the list
            if (HeldImages.Count > 1 && hoveredOver)
            {
                var temp = HeldImages[CycleIndex];
                HeldImages.Remove(HeldImages[CycleIndex]);
                if (CycleIndex >= HeldImages.Count)
                {
                    CycleIndex = 0;
                }
                if (HeldImages.Count <= 1)
                {
                    if (CyclingTimer != null) CyclingTimer.Dispose();
                    CyclingTimer = null;
                    CycleIndex = 0;
                }
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.CycleIndex = 0;
                    remotewindow.HeldImages = HeldImages;
                    if (HeldImages.Count <= 1)
                    {
                        if (remotewindow.CyclingTimer != null) remotewindow.CyclingTimer.Dispose();
                        remotewindow.CyclingTimer = null;
                        remotewindow.CycleIndex = 0;
                    }
                }
            } else
            {
                RemoveImage = true;
                HeldImages.Clear();
                HoldsImage = false;
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.RemoveImage = true;
                    remotewindow.HeldImages.Clear();
                    remotewindow.HoldsImage = false;
                }
                ImageIndex = 0;
            }
            UpdateImage();
        }

        public void StartDragDrop()
        {
            if (HeldImages.Count > 1)
            {
                //TODO: finish this thought lmao
            } else
            {
                HoldsImage = false;
            }
            UpdateImage();
            var dropContent = new DragDropContent(false, HeldImages[CycleIndex]);
            DoDragDrop(dropContent, DragDropEffects.Copy);
            SaveChanges();
        }

        public void NukeTimer()
        {
            if (CyclingTimer != null) CyclingTimer.Dispose();
            CyclingTimer = null;
        }

        public void UpdateFromSettings()
        {
            if (HeldImages.Count > 1)
            {
                CyclingTimer.Change(TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
            }
        }

        public void SaveChanges()
        {
            if (RemoveImage)
            {
                HoldsImage = false;
                HeldImages.Clear();
                RemoveImage = false;
                UpdateImage();
            }
        }

        public void CancelChanges() { }
    }
}
