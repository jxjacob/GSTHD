using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Windows.Forms;

namespace GSTHD
{
    public struct MedallionState
    {
        public int DungeonIndex;
        public int ImageIndex;

        public override string ToString() => $"{DungeonIndex},{ImageIndex}";
    }

    public class Medallion : PictureBox, UpdatableFromSettings, ProgressibleElement<MedallionState>, DraggableAutocheckElement<MedallionState>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<MedallionState> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<MedallionState> DragBehaviour;

        private string[] ImageNames;
        private string[] DungeonNames;
        private bool Wraparound;
        private int ImageIndex = 0;

        private int DefaultDungeonIndex;
        private int DungeonIndex;

        public Label SelectedDungeon;

        private bool isBroadcastable;
        public string AutoName = null;

        delegate void SetStateCallback(MedallionState state);

        public Medallion(ObjectPointMedallion data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;

            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            if (data.Label == null)
                data.Label = Settings.DefaultDungeonNames;
            else
            {
                if (data.Label.TextCollection == null)
                    data.Label.TextCollection = Settings.DefaultDungeonNames.TextCollection;
                if (!data.Label.DefaultValue.HasValue)
                    data.Label.DefaultValue = Settings.DefaultDungeonNames.DefaultValue;
                if (!data.Label.Wraparound.HasValue)
                    data.Label.Wraparound = Settings.DefaultDungeonNames.Wraparound;
                if (data.Label.FontName == null)
                    data.Label.FontName = Settings.DefaultDungeonNames.FontName;
                if (!data.Label.FontSize.HasValue)
                    data.Label.FontSize = Settings.DefaultDungeonNames.FontSize;
                if (!data.Label.FontStyle.HasValue)
                    data.Label.FontStyle = Settings.DefaultDungeonNames.FontStyle;
            }

            DungeonNames = data.Label.TextCollection;
            DefaultDungeonIndex = data.Label.DefaultValue.Value;
            DungeonIndex = DefaultDungeonIndex;
            Wraparound = data.Label.Wraparound.Value;
            isBroadcastable = data.isBroadcastable && !isBroadcast;
            AutoName = data.AutoName;

            Name = data.Name;
            BackColor = Color.Transparent;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                SizeMode = PictureBoxSizeMode.StretchImage;
                Size = data.Size;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<MedallionState>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<MedallionState>(this, Settings);

            Location = new Point(data.X, data.Y);
            TabStop = false;
            AllowDrop = false;
            if (!isBroadcast)
            {
                MouseUp += DragBehaviour.Mouse_ClickUp;
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseDown += DragBehaviour.Mouse_ClickDown;
                MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                MouseWheel += DragBehaviour.Mouse_Wheel;
            }
            

            SelectedDungeon = new Label
            {
                Font = new Font(new FontFamily(data.Label.FontName), data.Label.FontSize.Value, data.Label.FontStyle.Value),
                Text = DungeonNames[DefaultDungeonIndex],
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                AutoSize = true,
            };

            SelectedDungeon.Location = new Point(Location.X + Size.Width / 2, (int)(Location.Y + Size.Height * 0.75));

            if (!isBroadcast)
            {
                SelectedDungeon.MouseUp += DragBehaviour.Mouse_ClickUp;
                SelectedDungeon.MouseDown += ProgressBehaviour.Mouse_ClickDown;
                SelectedDungeon.MouseDown += DragBehaviour.Mouse_ClickDown;
                SelectedDungeon.MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                SelectedDungeon.MouseWheel += DragBehaviour.Mouse_Wheel;

                UpdateFromSettings();
            }
            
        }

        public void UpdateFromSettings()
        {
            MouseWheel -= Mouse_Wheel;
            MouseWheel -= Mouse_Wheel_WithWraparound;
            SelectedDungeon.MouseWheel -= Mouse_Wheel;
            SelectedDungeon.MouseWheel -= Mouse_Wheel_WithWraparound;

            if (Settings.WraparoundDungeonNames)
            {
                MouseWheel += Mouse_Wheel_WithWraparound;
                SelectedDungeon.MouseWheel += Mouse_Wheel_WithWraparound;
            }
            else
            {
                MouseWheel += Mouse_Wheel;
                SelectedDungeon.MouseWheel += Mouse_Wheel;
            }
        }

        public void SetSelectedDungeonLocation()
        {
            SelectedDungeon.Location = new Point(Location.X + Size.Width / 2 - SelectedDungeon.Width / 2, (int)(Location.Y + Size.Height * 0.75));
        }

        private void Mouse_Wheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                DungeonIndex += (Settings.InvertScrollWheel ? scrolls : -scrolls);
                if (DungeonIndex < 0) DungeonIndex = 0;
                else if (DungeonIndex >= DungeonNames.Length) DungeonIndex = DungeonNames.Length - 1;
                SelectedDungeon.Text = DungeonNames[DungeonIndex];
                SetSelectedDungeonLocation();
                DragBehaviour.SaveChanges();
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SelectedDungeon.Text = DungeonNames[DungeonIndex];
                    ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetSelectedDungeonLocation();
                }
            }
        }

        private void Mouse_Wheel_WithWraparound(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var scrolls = e.Delta / SystemInformation.MouseWheelScrollDelta;
                var newIndex = DungeonIndex + (Settings.InvertScrollWheel ? scrolls : -scrolls);
                DungeonIndex = Math.EMod(newIndex, DungeonNames.Length);
                SelectedDungeon.Text = DungeonNames[DungeonIndex];
                SetSelectedDungeonLocation();
                DragBehaviour.SaveChanges();
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SelectedDungeon.Text = DungeonNames[DungeonIndex];
                    ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetSelectedDungeonLocation();
                }
            }
        }

        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = null;
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
            }
        }

        public MedallionState GetState()
        {
            return new MedallionState()
            {
                DungeonIndex = DungeonIndex,
                ImageIndex = ImageIndex,
            };
        }

        public void SetState(MedallionState state)
        {
            if (this.InvokeRequired)
            {
                Invoke(new SetStateCallback(SetState), new MedallionState[] { state });
                return;
            } else
            {
                ImageIndex = state.ImageIndex;
                DungeonIndex = state.DungeonIndex;
                SelectedDungeon.Text = DungeonNames[DungeonIndex];
                UpdateImage();
                SetSelectedDungeonLocation();
                DragBehaviour.SaveChanges();
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SelectedDungeon.Text = DungeonNames[DungeonIndex];
                    ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetSelectedDungeonLocation();
                }
            }
            
        }

        public void SetImageState(int state)
        {
            ImageIndex = System.Math.Min(ImageNames.Length, state);
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
            DungeonIndex = DefaultDungeonIndex;
            SelectedDungeon.Text = DungeonNames[DungeonIndex];
            SetSelectedDungeonLocation();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SelectedDungeon.Text = DungeonNames[DungeonIndex];
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetSelectedDungeonLocation();
            }
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[1]);
            DoDragDrop(dropContent, DragDropEffects.Copy);
        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
