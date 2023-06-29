using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    class CollectedItem : PictureBox, ProgressibleElement<int>, DraggableAutocheckElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<int> DragBehaviour;

        private string[] ImageNames;
        private int ImageIndex = 0;
        private Label ItemCount;
        private Size CollectedItemSize;
        private Size CollectedItemCountPosition;
        private readonly int CollectedItemMin;
        private readonly int CollectedItemMax;
        public readonly int CollectedItemDefault;
        private int CollectedItems;
        private readonly int Step;

        private bool isBroadcastable;

        public string AutoName = null;

        delegate void SetStateCallback(int state);

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
            CollectedItems = System.Math.Min(System.Math.Max(CollectedItemMin, CollectedItemDefault), CollectedItemMax);
            Step = data.Step == 0 ? 1 : data.Step;
            CollectedItemSize = data.Size;
            isBroadcastable = data.isBroadcastable && !isBroadcast;

            this.AutoName = data.AutoName;

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
            BackColor = Color.Transparent;
            TabStop = false;


            ItemCount = new Label
            {
                BackColor = Color.Black,
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

            if (!isBroadcast)
            {
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseUp += DragBehaviour.Mouse_ClickUp;
                MouseDown += DragBehaviour.Mouse_ClickDown;
                MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                MouseWheel += Mouse_Wheel;
                MouseWheel += DragBehaviour.Mouse_Wheel;
                ItemCount.MouseDown += ProgressBehaviour.Mouse_ClickDown; // must add these lines because MouseDown/Up on PictureBox won't fire when hovering above Label
                ItemCount.MouseDown += DragBehaviour.Mouse_ClickDown;
                ItemCount.MouseUp += DragBehaviour.Mouse_ClickUp;
                ItemCount.MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
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
            Image = Image.FromFile(@"Resources/" + ImageNames[System.Math.Max(System.Math.Min(CollectedItems, ImageNames.Length - 1), 0)]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((CollectedItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
            }
        }

        public void UpdateCount()
        {
            ItemCount.Text = CollectedItems.ToString();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((CollectedItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).CollectedItems = CollectedItems;
                ((CollectedItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateCount();
            }
            UpdateImage();
        }

        public int GetState()
        {
            return CollectedItems;
        }

        public void SetState(int state)
        {
            if (this.InvokeRequired)
            {
                SetStateCallback d = new SetStateCallback(SetState);
                this.Invoke(d, new object[] {state});
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

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[System.Math.Min(System.Math.Max(1, CollectedItems), ImageNames.Length - 1)]);
            DoDragDrop(dropContent, DragDropEffects.Copy);

        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
