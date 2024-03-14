using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace GSTHD
{
    public struct GossipStoneState
    {
        public bool HoldsImage;
        public List<string> HeldImages;
        public int ImageIndex;
        public bool isMarked;

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
            return $"{HoldsImage},{exported},{ImageIndex},{isMarked}"; 
        }
    }

    public class GossipStone : OrganicImage, ProgressibleElement<GossipStoneState>, DraggableElement<GossipStoneState>, UpdatableFromSettings
    {
        private readonly Settings Settings;
        private Form1 f1;
        private readonly ProgressibleElementBehaviour<GossipStoneState> ProgressBehaviour;
        private readonly DraggableElementBehaviour<GossipStoneState> DragBehaviour;

        private string[] ImageNames;
        public bool HoldsImage;
        public List<string> HeldImages = new List<string>();
        private int CycleIndex = 0;
        private bool canCycle = false;
        private int ImageIndex = 0;
        private bool RemoveImage;
        private bool isScrollable;
        bool isBroadcastable;
        public bool hoveredOver;

        private bool isCyling = false;

        Size GossipStoneSize;


        delegate void IncrementCallbacK();

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

        public void tryToKill()
        {
            if (isCyling && HeldImages.Count > 1)
            {
                f1.RemoveCycling(this);
            }
        }

        private Form1 FindF1(Control c)
        {
            if (c == null) return null;
            if (c is Form1 f1)
            {
                return f1;
            } else
            {
                return FindF1(c.Parent);
            }
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
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                scrolls = (Settings.InvertScrollWheel ? scrolls : -scrolls);
                if (scrolls > 0)
                {
                    for (int i = 0; i < scrolls; i++) IncrementState();
                }
                else if (scrolls < 0)
                {
                    for (int i = 0; i > scrolls; i--) DecrementState();
                }
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
                    isMarked = (!Settings.StoneOverrideCheckMark) ? dropContent.isMarked || isMarked : isMarked;
                }
            }
            else
            {
                HeldImages.Clear();
                HeldImages.Add(dropContent.ImageName);
                isMarked = (!Settings.StoneOverrideCheckMark) ? dropContent.isMarked || isMarked : isMarked;
            }
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

        public void UpdateImage()
        {
            if (f1 == null)
            {
                this.f1 = FindF1(this.Parent);
            }
            if (HoldsImage)
            {
                if (Image != null) Image.Dispose();
                Image = null;
                try
                {
                    Image = Image.FromFile(@"Resources/" + HeldImages[CycleIndex]);
                } catch (ArgumentOutOfRangeException)
                {
                    CycleIndex = 0;
                    Image = Image.FromFile(@"Resources/" + HeldImages[CycleIndex]);
                }
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.HeldImages = HeldImages;
                    remotewindow.HoldsImage = true;
                    remotewindow.CycleIndex = CycleIndex;
                    remotewindow.isMarked = isMarked;
                    remotewindow.UpdateImage();
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
                    remotewindow.isMarked = isMarked;
                    remotewindow.UpdateImage();
                }
            }
            if (f1 != null)
            {
                // if wasnt cycling but now should be, add to cycling
                if (!isCyling && HeldImages.Count > 1)
                {
                    isCyling = true;
                    f1.AddCycling(this);
                } else if (isCyling && HeldImages.Count < 2)
                {
                    isCyling = false;
                    f1.RemoveCycling(this);
                }
                // if was cycling but now shouldnt be, remove from cycling
            }
            if (IsHandleCreated) { Invalidate(); }
        }

        public GossipStoneState GetState()
        {
            return new GossipStoneState()
            {
                HoldsImage = HoldsImage,
                HeldImages = HeldImages,
                ImageIndex = ImageIndex,
                isMarked = isMarked,
            };
        }

        public void SetState(GossipStoneState state)
        {
            HoldsImage = state.HoldsImage;
            HeldImages = state.HeldImages;
            isMarked = state.isMarked;
            ImageIndex = Math.Clamp(state.ImageIndex, 0, ImageNames.Length);
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
                else if (Settings.WraparoundItems) ImageIndex = 0;
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
                else if (Settings.WraparoundItems) ImageIndex = ImageNames.Length - 1;
                UpdateImage();
            }
        }

        public void IncrementCycle()
        {

            if (InvokeRequired)
            {
                    //TheQueuedThing = IncrementCycle;
                    try
                    {
                        Invoke(new IncrementCallbacK(IncrementCycle));
                    } catch (ObjectDisposedException) { 
                        //blank
                    }   
                
            } else
            {
                if (!hoveredOver && HeldImages.Count > 1)
                {
                    CycleIndex++;
                    if (CycleIndex == HeldImages.Count)
                    {
                        CycleIndex = 0;
                    }
                    UpdateImage();

                }
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
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.CycleIndex = 0;
                    remotewindow.HeldImages = HeldImages;
                }
            } else
            {
                RemoveImage = true;
                HeldImages.Clear();
                HoldsImage = false;
                isMarked = false;
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    var remotewindow = ((GossipStone)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]);
                    remotewindow.RemoveImage = true;
                    remotewindow.HeldImages.Clear();
                    remotewindow.isMarked = false;
                    remotewindow.HoldsImage = false;
                }
                ImageIndex = 0;
            }
            UpdateImage();
        }

        public void ToggleCheck()
        {
            isMarked = !isMarked;
            UpdateImage();
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(false, HeldImages[CycleIndex], marked: isMarked);
            DoDragDrop(dropContent, DragDropEffects.Copy);
            HeldImages.Remove(dropContent.ImageName);
            if (HeldImages.Count == 0) HoldsImage = false;
            UpdateImage();
            SaveChanges();
        }

        public void UpdateFromSettings()
        {
            //pass
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
