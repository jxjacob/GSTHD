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
    class CollectedItem : OrganicImage, ProgressibleElement<int>, DraggableAutocheckElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<int> DragBehaviour;

        private string[] ImageNames;
        private Label ItemCount;
        private Size CollectedItemSize;
        private Size CollectedItemCountPosition;
        private readonly int CollectedItemMin;
        private readonly int CollectedItemMax;
        public readonly int CollectedItemDefault;
        public int CollectedItems;
        private readonly int Step;

        private bool isBroadcastable;
        public bool hasSlash;
        public bool hoveredOver;

        public string AutoName = null;
        public string AutoSubName = null;

        delegate void SetStateCallback(int state);
        delegate void UpdateCountCallback();

        public CollectedItem(ObjectPointCollectedItem data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;

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

            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<int>(this, Settings);

            Location = new Point(data.X, data.Y);
            CollectedItemCountPosition = data.CountPosition.IsEmpty ? new Size(0, -7) : data.CountPosition;

            if (data.BackGroundColor != Color.Transparent) BackColor = data.BackGroundColor;
            TabStop = false;


            ItemCount = new Label
            {
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
                CollectedItems += Step * (Settings.InvertScrollWheel ? scrolls : -scrolls);
                if (CollectedItems < CollectedItemMin) CollectedItems = CollectedItemMin;
                else if (CollectedItems > CollectedItemMax) CollectedItems = CollectedItemMax;
                UpdateCount();
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

        public int GetState()
        {
            return CollectedItems;
        }

        public void SetState(int state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetStateCallback(SetState), new object[] {state});
                return;
            } else
            {
                CollectedItems = state;
                UpdateCount();
                DragBehaviour.SaveChanges();
            }
        }

        public void IncrementState()
        {
            CollectedItems += Step;
            if (CollectedItems > CollectedItemMax) CollectedItems = CollectedItemMax;
            if (CollectedItems < CollectedItemMin) CollectedItems = CollectedItemMin;
            UpdateCount();
        }

        public void DecrementState()
        {
            CollectedItems -= Step;
            if (CollectedItems > CollectedItemMax) CollectedItems = CollectedItemMax;
            if (CollectedItems < CollectedItemMin) CollectedItems = CollectedItemMin;
            UpdateCount();
        }

        public void ResetState()
        {
            CollectedItems = CollectedItemDefault;
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
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[Math.Clamp(CollectedItems, 1, ImageNames.Length - 1)], marked:isMarked);
            DoDragDrop(dropContent, DragDropEffects.Copy);

        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
