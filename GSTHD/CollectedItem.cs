using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    public struct CollectedItemState
    {
        public int CollectedItems;
        public bool isMarked;

    }
    class CollectedItem : OrganicImage, ProgressibleElement<CollectedItemState>, DraggableAutocheckElement<CollectedItemState>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<CollectedItemState> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<CollectedItemState> DragBehaviour;

        public string[] ImageNames { get; set; }
        public Label ItemCount { get; set; }
        public Size CollectedItemSize { get; set; }
        public Size CollectedItemCountPosition { get; set; }
        public int CollectedItemMin { get; set; }
        public int CollectedItemMax { get; set; }
        public int CollectedItemDefault { get; set; }
        public int CollectedItems { get; set; }
        public int Step { get; set; }

        public bool isBroadcastable { get; set; }
        public bool hasSlash { get; set; }
        public bool hoveredOver { get; set; }

        public string AutoName { get; set; } = null;
        public string AutoSubName { get; set; } = null;

        delegate void SetStateCallback(CollectedItemState state);
        delegate void UpdateCountCallback();

        public CollectedItem(ObjectPointCollectedItem data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;
            Visible = data.Visible;

            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            Name = data.Name;

            CollectedItemMin = data.CountMin;
            CollectedItemMax = data.CountMax.HasValue ? data.CountMax.Value : 100;
            CollectedItemDefault = data.DefaultValue;
            CollectedItems = Math.Clamp(CollectedItemDefault, CollectedItemMin, CollectedItemMax);
            Step = data.Step == 0 ? 1 : data.Step;
            CollectedItemSize = data.Size;
            isBroadcastable = data.isBroadcastable && !isBroadcast;
            hasSlash = data.hasSlash;

            this.AutoName = data.AutoName;
            this.AutoSubName = data.AutoSubName;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                SizeMode = PictureBoxSizeMode.Zoom;
                Size = CollectedItemSize;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<CollectedItemState>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<CollectedItemState>(this, Settings);

            Location = new Point(data.X, data.Y);
            CollectedItemCountPosition = data.CountPosition.IsEmpty ? new Size(0, -7) : data.CountPosition;

            if (data.BackGroundColor != Color.Transparent) BackColor = data.BackGroundColor;
            TabStop = false;


            ItemCount = new Label
            {
                Visible = data.Visible,
                BackColor = data.BackColor,
                BorderStyle = BorderStyle.None,
                Text = CollectedItems.ToString(),
                Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                ForeColor = data.LabelColor,
                AutoSize = true,
                TextAlign = ContentAlignment.BottomLeft,
                Height = 40,
                Width = 50,
                Location = new Point(0, (CollectedItemSize.Height) - CollectedItemCountPosition.Height*2),
            };

            if (hasSlash)
            {
                ItemCount.Text += " /";
            }

            if (!isBroadcast)
            {
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseUp += DragBehaviour.Mouse_ClickUp;
                MouseDown += DragBehaviour.Mouse_ClickDown;
                MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                MouseWheel += Mouse_Wheel;
                MouseWheel += DragBehaviour.Mouse_Wheel;
                this.MouseEnter += Panel_MouseEnter;
                this.MouseLeave += Panel_MouseLeave;
                ItemCount.MouseDown += ProgressBehaviour.Mouse_ClickDown; // must add these lines because MouseDown/Up on PictureBox won't fire when hovering above Label
                ItemCount.MouseDown += DragBehaviour.Mouse_ClickDown;
                ItemCount.MouseUp += DragBehaviour.Mouse_ClickUp;
                ItemCount.MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                ItemCount.MouseEnter += Panel_MouseEnter;
                ItemCount.MouseLeave += Panel_MouseLeave;
                // ItemCount.MouseWheel += Click_MouseWheel; // must NOT add this line because both MouseWheels would fire when hovering above both PictureBox and Label
            }

            Controls.Add(ItemCount);
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
                } else if (scrolls < 0)
                {
                    for (int i = 0; i > scrolls; i--) DecrementState();
                }
            }
        }

        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = null;
            Image = Image.FromFile(@"Resources/" + ImageNames[Math.Clamp(CollectedItems, 0, ImageNames.Length-1)]);
            if (IsHandleCreated) { Invalidate(); }
        }

        public void UpdateCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateCountCallback(UpdateCount));
                return;

            } else
            {
                ItemCount.Text = CollectedItems.ToString();
                if (hasSlash) ItemCount.Text += " /";
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((CollectedItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).CollectedItems = CollectedItems;
                    ((CollectedItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isMarked = isMarked;
                    ((CollectedItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateCount();
                }
                UpdateImage();
            }
        }

        public CollectedItemState GetState()
        {
            return new CollectedItemState
            {
                CollectedItems = CollectedItems,
                isMarked = isMarked,
            };
        }

        // legacy for autotracker
        public void SetState(int state)
        {
            Invoke(new SetStateCallback(SetState), new object[] { new CollectedItemState { CollectedItems = state, isMarked = isMarked } });
        }

        public void SetState(CollectedItemState state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetStateCallback(SetState), new object[] {state});
                return;
            } else
            {
                CollectedItems = state.CollectedItems;
                isMarked = state.isMarked;
                UpdateCount();
                DragBehaviour.SaveChanges();
            }
        }

        public void IncrementState()
        {
            CollectedItems += Step;
            if (CollectedItems > CollectedItemMax) CollectedItems = (this.Settings.WraparoundItems) ? CollectedItemMin : CollectedItemMax;
            else if (CollectedItems < CollectedItemMin) CollectedItems = (this.Settings.WraparoundItems) ? CollectedItemMax : CollectedItemMin;
            UpdateCount();
        }

        public void DecrementState()
        {
            CollectedItems -= Step;
            if (CollectedItems < CollectedItemMin) CollectedItems = (this.Settings.WraparoundItems) ? CollectedItemMax : CollectedItemMin;
            else if (CollectedItems > CollectedItemMax) CollectedItems = (this.Settings.WraparoundItems) ? CollectedItemMin : CollectedItemMax;
            UpdateCount();
        }

        public void ResetState()
        {
            CollectedItems = CollectedItemDefault;
            isMarked = false;
            UpdateCount();
        }
        public void ToggleCheck()
        {
            isMarked = !isMarked;
            UpdateCount();
        }
        public void SetColor(Color color)
        {
            ItemCount.ForeColor = color;
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

        public void StartDragDrop()
        {

            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[(ImageNames.Length == 1) ? 0 : Math.Clamp(CollectedItems, 1, ImageNames.Length - 1)], marked:isMarked);
            DoDragDrop(dropContent, DragDropEffects.Copy);

        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
