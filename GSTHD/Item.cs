using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    public struct ItemState
    {
        public int ImageIndex;
        public MarkedImageIndex isMarked;

    }
    public class Item : OrganicImage, IAlternatableObject, ProgressibleElement<ItemState>, DraggableAutocheckElement<ItemState>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<ItemState> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<ItemState> DragBehaviour;

        public string[] ImageNames { get; set; }
        public int ImageIndex { get; set; } = 0;
        public int DefaultIndex { get; set; } = 0;
        public int DK64_ID { get; set; }
        public string DragImage { get; set; } = null;

        public bool isBroadcastable { get; set; }
        public string DoubleBroadcastSide { get; set; }
        public string DoubleBroadcastName { get; set; }
        public bool isDraggable { get; set; }

        public string LinkedItem { get; set; } = null;

        public string OuterPathID { get; set; }

        public string AutoName { get; set; } = null;

        delegate void SetStateCallback(ItemState state);

        public Item(ObjectPoint data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;
            Visible = data.Visible;

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
            this.OuterPathID = data.OuterPathID;
            this.isMarkable = data.isMarkable;
            LinkedItem = data.LinkedItem;

            this.DefaultIndex = data.DefaultIndex;
            ImageIndex = DefaultIndex;

            if (data.DoubleBroadcastSide != null) this.DoubleBroadcastSide = data.DoubleBroadcastSide;
            if (data.DoubleBroadcastName != null) this.DoubleBroadcastName = data.DoubleBroadcastName;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                SizeMode = data.SizeMode;
                Size = data.Size;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<ItemState>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<ItemState>(this, Settings);

            Location = new Point(data.X, data.Y);
            TabStop = false;
            AllowDrop = false;

            Control thething = this.Parent;
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

        public void UpdateImage(bool isRemote = false)
        {
            if (Image != null) Image.Dispose();
            Image = null;
            if (ImageIndex >= ImageNames.Length) ImageIndex = ImageNames.Length - 1;
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                if (DoubleBroadcastName == null || DoubleBroadcastSide == null || DoubleBroadcastName == string.Empty || DoubleBroadcastSide == string.Empty)
                {
                    try
                    {
                        Item ite = (Item)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0];
                        ite.ImageIndex = ImageIndex;
                        ite.isMarked = isMarked;
                        ite.UpdateImage();
                    } catch (IndexOutOfRangeException)
                    {
                        Debug.WriteLine($"Item {this.Name} could not be found on Broadcast, skipping...");
                    }
                }
                else
                {
                    //TODO: make this block cleaner
                    if (DoubleBroadcastSide == "left")
                    {
                        DoubleItem di = (DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(DoubleBroadcastName, true)[0];
                        di.SetLeftMark(isMarked);
                        if (ImageIndex == 0)
                        {
                            di.DecrementLeftState();
                        }
                        else
                        {
                            di.IncrementLeftState();
                        }

                    }
                    else if (DoubleBroadcastSide == "right")
                    {
                        DoubleItem di = (DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(DoubleBroadcastName, true)[0];
                        di.SetRightMark(isMarked);
                        if (ImageIndex == 0)
                        {
                            di.DecrementRightState();
                        }
                        else
                        {
                            di.IncrementRightState();
                        }
                    }
                }

            }
            if ((LinkedItem != null && LinkedItem != "") && isRemote == false)
            {
                GSTForms f = (GSTForms)this.FindForm();
                if (f != null)
                {
                    Item ite = (Item)f.Controls.Find(LinkedItem, true)[0];
                    ite.ImageIndex = ImageIndex;
                    ite.isMarked = isMarked;
                    ite.UpdateImage(true);

                }
            }
            if (IsHandleCreated) { Invalidate(); }
        }


        public ItemState GetState()
        {
            return new ItemState
            {
                ImageIndex = ImageIndex,
                isMarked = isMarked,
            };
        }


        // legacy for autotracker
        public void SetState(int state)
        {
            Invoke(new SetStateCallback(SetState), new object[] { new ItemState { ImageIndex = state, isMarked = isMarked } });
        }

        public void SetState(ItemState state)
        {
            if (this.InvokeRequired)
            {
                Invoke(new SetStateCallback(SetState), new object[] { state });
                return;
            }
            else
            {
                ImageIndex = Math.Clamp(state.ImageIndex, 0, ImageNames.Length);
                isMarked = state.isMarked;
                VerifyState();
                UpdateImage();
                DragBehaviour.SaveChanges();
            }
        }

        public void IncrementState()
        {
            if (ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
            else if (Settings.WraparoundItems) ImageIndex = 0;
            UpdateImage();
        }

        public void DecrementState()
        {
            if (ImageIndex > 0) ImageIndex -= 1;
            else if (Settings.WraparoundItems) ImageIndex = ImageNames.Length - 1;
            UpdateImage();
        }

        public void ResetState()
        {
            ImageIndex = 0;
            isMarked = 0;
            UpdateImage();
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }

        private void VerifyState()
        {
            if (ImageIndex > ImageNames.Length - 1) ImageIndex = (ImageNames.Length - 1);
            if (ImageIndex < 0) ImageIndex = 0;
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, (DragImage != null) ? DragImage : ImageNames[(ImageNames.Length == 1) ? 0 : Math.Clamp(ImageIndex, 1, ImageNames.Length - 1)], DK64_ID, isMarked);
            DoDragDrop(dropContent, DragDropEffects.Copy);
        }

        public void SaveChanges() { }
        public void CancelChanges() { }

        public void SetVisible(bool visible)
        {
            Visible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (ObjectPoint)ogPoint;
            switch (name)
            {
                case "":
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Item Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            Invalidate();
            UpdateImage();
        }
    }
}
