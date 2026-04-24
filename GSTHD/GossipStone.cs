using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Size = System.Drawing.Size;

namespace GSTHD
{
    public struct GossipStoneState
    {
        public bool HoldsImage;
        public List<string> HeldImages;
        public int ImageIndex;
        public MarkedImageIndex isMarked;

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
            return $"{HoldsImage},{exported},{ImageIndex},{(int)isMarked}"; 
        }
    }

    public class GossipStone : OrganicImage, ProgressibleElement<GossipStoneState>, DraggableElement<GossipStoneState>, UpdatableFromSettings, IAlternatableObject
    {
        private readonly Settings Settings;
        private MainWindow f1;
        private readonly ProgressibleElementBehaviour<GossipStoneState> ProgressBehaviour;
        private readonly DraggableElementBehaviour<GossipStoneState> DragBehaviour;

        public string[] ImageNames { get; set; }
        public bool HoldsImage;
        public List<string> HeldImages { get; set; } = new List<string>();
        private int CycleIndex { get; set; } = 0;
        private bool canCycle { get; set; } = false;
        private int ImageIndex = 0;
        private bool RemoveImage;
        public bool isScrollable;
        public bool isBroadcastable { get; set; }
        public bool hoveredOver;

        private bool isCyling = false;

        Size GossipStoneSize;


        delegate void IncrementCallbacK();

        public GossipStone(ObjectPoint data, Settings settings, bool isOnBroadcast = false)
            : this(settings, data.Visible, data.Name, data.X, data.Y, data.ImageCollection, data.Size, data.isScrollable, data.SizeMode, data.isBroadcastable, data.CanCycle, isOnBroadcast, data.isMarkable) { }

        public GossipStone(Settings settings, bool visible, string name, int x, int y, string[] imageCollection, Size imageSize, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool CanCycle = false, bool isOnBroadcast = false, bool isMarkable = true) : base((Uri?)null)
        {
            Settings = settings;
            IsVisible = visible;

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
                TrueWidth = GossipStoneSize.Width;
                TrueHeight = GossipStoneSize.Height;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<GossipStoneState>(this, Settings);
            DragBehaviour = new DraggableElementBehaviour<GossipStoneState>(this, Settings);

            
            Canvas.SetTop(this, y);
            Canvas.SetLeft(this, x);
            this.IsTabStop = false;
            DragDrop.SetAllowDrop(this, true);
            this.isScrollable = isScrollable;
            this.canCycle = CanCycle;
            this.isBroadcastable = !isOnBroadcast && isBroadcastable;
            


            if (!isOnBroadcast)
            {
                this.PointerReleased += DragBehaviour.Mouse_ClickUp;
                this.PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                this.PointerPressed += DragBehaviour.Mouse_ClickDown;
                this.PointerMoved += Mouse_Move;
                DragDrop.AddDragEnterHandler(this, Mouse_DragEnter);
                DragDrop.AddDropHandler(this,  Mouse_DragDrop);
                this.PointerWheelChanged += Mouse_Wheel;
                this.PointerEntered += Panel_MouseEnter;
                this.PointerExited += Panel_MouseLeave;
            }

        }

        public void TryToKill()
        {
            if (isCyling && HeldImages.Count > 1)
            {
                f1.RemoveCycling(this);
            }
        }

        private static MainWindow FindF1(StyledElement c)
        {
            while (true)
            {
                if (c == null) return null;
                if (c is MainWindow findF1)
                {
                    return findF1;
                }
                else
                {
                    c = c.Parent;
                }
            }
        }

        // both of these functions are for when the stone is in a WOTH panel, so that it can be scrolled without the whole WOTH panle scrolling as well
        private void Panel_MouseEnter(object? sender, EventArgs e)
        {
            this.hoveredOver = true;
        }

        private void Panel_MouseLeave(object? sender, EventArgs e)
        {
            this.hoveredOver = false;
        }

        private void Mouse_Wheel(object? sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y != 0 && this.isScrollable == true)
            {
                //TODO: replace 1 with something more reasonable? whatever SystemInformation.MouseWheelScrollDelta used to be
                var scrolls = e.Delta.Y / 1;
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

        private void Mouse_DragEnter(object? sender, DragEventArgs e)
        {
            // if (e.DataTransfer.Formats.Contains.(Settings.GSTCustomFormat))
            e.DragEffects = !e.DataTransfer.Formats.Contains(Settings.GSTCustomFormat) ? DragDropEffects.None : DragDropEffects.Copy;
        }

        private void Mouse_DragDrop(object? sender, DragEventArgs e)
        {
            ImageIndex = 0;
            HoldsImage = true;
            RemoveImage = false;
            var inpu = e.DataTransfer.GetItems(Settings.GSTCustomFormat).First().TryGetText();
            var parts = inpu.Split('|');
            var dropContent = new DragDropContent(bool.Parse(parts[0]), parts[1], int.Parse(parts[2]), Enum.Parse<MarkedImageIndex>(parts[3]));
            if (canCycle || Settings.ForceGossipCycles)
            {
                if (!HeldImages.Contains(dropContent.ImageName))
                {
                    HeldImages.Add(dropContent.ImageName);
                    isMarked = (!Settings.StoneOverrideCheckMark) ? (MarkedImageIndex)System.Math.Max((int)dropContent.isMarked, (int)isMarked) : isMarked;
                }
            }
            else
            {
                HeldImages.Clear();
                HeldImages.Add(dropContent.ImageName);
                isMarked = (!Settings.StoneOverrideCheckMark) ? (MarkedImageIndex)System.Math.Max((int)dropContent.isMarked, (int)isMarked) : isMarked;
            }
            UpdateImage();
            DragBehaviour.SaveChanges();
        }

        public void Mouse_ClickUp(object sender, PointerReleasedEventArgs e)
        {
            DragBehaviour.Mouse_ClickUp(sender, e);
        }

        public void Mouse_Move(object? sender, PointerEventArgs e)
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
                // if (Image != null) Image.Dispose();
                // Image = null;
                try
                {
                    Source = "Resources/" + HeldImages[CycleIndex];
                    Debug.WriteLine(Source);
                } catch (ArgumentOutOfRangeException)
                {
                    CycleIndex = 0;
                    Source = "Resources/" + HeldImages[CycleIndex];
                }
                if (isBroadcastable && BroadcastManager.Window!= null)
                {
                    var remotewindow = (GossipStone)BroadcastManager.GetControl(Name)!;
                    remotewindow.HeldImages = HeldImages;
                    remotewindow.HoldsImage = true;
                    remotewindow.CycleIndex = CycleIndex;
                    remotewindow.isMarked = isMarked;
                    remotewindow.UpdateImage();
                }
            }
            else
            {
                // if (Image != null) Image.Dispose();
                // Image = null;
                Source = "Resources/" + ImageNames[ImageIndex];
                if (isBroadcastable && BroadcastManager.Window!= null)
                {
                    var remotewindow = (GossipStone)BroadcastManager.GetControl(Name)!;
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
            InvalidateVisual();
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
            ImageIndex = GSTMath.Clamp(state.ImageIndex, 0, ImageNames.Length);
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
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(IncrementCycle);
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
                HeldImages.Remove(HeldImages[CycleIndex]);
                if (CycleIndex >= HeldImages.Count)
                {
                    CycleIndex = 0;
                }
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    var remotewindow = (GossipStone)BroadcastManager.GetControl(Name)!;
                    remotewindow.CycleIndex = 0;
                    remotewindow.HeldImages = HeldImages;
                }
            } else
            {
                RemoveImage = true;
                HeldImages.Clear();
                HoldsImage = false;
                isMarked = 0;
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    var remotewindow = (GossipStone)BroadcastManager.GetControl(Name)!;
                    remotewindow.RemoveImage = true;
                    remotewindow.HeldImages.Clear();
                    remotewindow.isMarked = 0;
                    remotewindow.HoldsImage = false;
                }
                ImageIndex = 0;
            }
            UpdateImage();
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }

        public void StartDragDrop(PointerEventArgs e)
        {
            var dropContent = new DragDropContent(false, HeldImages[CycleIndex], marked: isMarked);
            var dataTransferItem =  new DataTransferItem();
            dataTransferItem.Set(Settings.GSTCustomFormat, dropContent.ToString());
            var dataTransfer = new DataTransfer();
            dataTransfer.Add(dataTransferItem);
            DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
            HeldImages.Remove(dropContent.ImageName);
            if (HeldImages.Count == 0) { HoldsImage = false; isMarked = MarkedImageIndex.none; }
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
                isMarked = MarkedImageIndex.none;
                HeldImages.Clear();
                RemoveImage = false;
                UpdateImage();
            }
        }

        public void CancelChanges() { }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            //var point = (ObjectPoint)ogPoint;
            switch (name)
            {
                case "":
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Gossip Stone Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            InvalidateVisual();
            UpdateImage();
        }
    }
}
