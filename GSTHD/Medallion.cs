using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GSTHD
{
    public struct MedallionState
    {
        public int DungeonIndex;
        public int ImageIndex;
        public MarkedImageIndex isMarked;

        public override string ToString() => $"{DungeonIndex},{ImageIndex},{(int)isMarked}";
    }

    public class Medallion : OrganicImage, ProgressibleElement<MedallionState>, DraggableAutocheckElement<MedallionState>, IAlternatableObject
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<MedallionState> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<MedallionState> DragBehaviour;

        public string[] ImageNames {  get; set; }
        private string[] DungeonNames;
        private bool Wraparound;
        private int ImageIndex = 0;

        private int DefaultDungeonIndex;
        private int DungeonIndex;

        public Label SelectedDungeon;

        public bool isBroadcastable { get; set; }
        public string AutoName = null;

        delegate void SetStateCallback(MedallionState state);

        public Medallion(ObjectPointMedallion data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;
            Visible = data.Visible;

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
            isMarkable = data.isMarkable;

            Name = data.Name;

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

            SelectedDungeon = new Label
            {
                Font = new Font(new FontFamily(data.Label.FontName), data.Label.FontSize.Value, data.Label.FontStyle.Value),
                Text = DungeonNames[DefaultDungeonIndex],
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                AutoSize = true,
                Visible = data.Visible,
                Location = new Point(Location.X + Size.Width / 2, (int)(Location.Y + Size.Height * 0.75))
            };

            if (!isBroadcast)
            {
                MouseUp += DragBehaviour.Mouse_ClickUp;
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseDown += DragBehaviour.Mouse_ClickDown;
                MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                MouseWheel += DragBehaviour.Mouse_Wheel;
                MouseWheel += Mouse_Wheel;


                SelectedDungeon.MouseUp += DragBehaviour.Mouse_ClickUp;
                SelectedDungeon.MouseDown += DungeonLabel_MouseDown;
                SelectedDungeon.MouseDown += DragBehaviour.Mouse_ClickDown;
                SelectedDungeon.MouseMove += DragBehaviour.Mouse_Move_WithAutocheck;
                SelectedDungeon.MouseWheel += DragBehaviour.Mouse_Wheel;
                SelectedDungeon.MouseWheel += Mouse_Wheel;

                UpdateFromSettings();
            }
            
        }

        public void UpdateFromSettings()
        {

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
                SetDungeonName((Settings.InvertScrollWheel ? scrolls : -scrolls));
            }
        }

        private void DungeonLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton)) SetDungeonName(1);
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton)) SetDungeonName(-1);
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton)) SetDungeonName(-DungeonIndex);
        }


        private void SetDungeonName(int direction)
        {
            if (Settings.WraparoundDungeonNames)
            {
                //EMod
                var newIndex = DungeonIndex + direction;
                DungeonIndex = Math.EMod(newIndex, DungeonNames.Length);
            }
            else
            {
                DungeonIndex += direction;
                if (DungeonIndex < 0) DungeonIndex = 0;
                else if (DungeonIndex >= DungeonNames.Length) DungeonIndex = DungeonNames.Length - 1;
            }
            SelectedDungeon.Text = DungeonNames[DungeonIndex];
            SetSelectedDungeonLocation();
            DragBehaviour.SaveChanges();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SelectedDungeon.Text = DungeonNames[DungeonIndex];
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetSelectedDungeonLocation();
            }

        }


        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = null;
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                Medallion me = (Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0];
                me.ImageIndex = ImageIndex;
                me.isMarked = isMarked;
                me.UpdateImage();
            }
            if (IsHandleCreated) { Invalidate(); }
        }

        public MedallionState GetState()
        {
            return new MedallionState()
            {
                DungeonIndex = DungeonIndex,
                ImageIndex = ImageIndex,
                isMarked = isMarked,
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
                ImageIndex = Math.Clamp(state.ImageIndex, 0, ImageNames.Length);
                DungeonIndex = state.DungeonIndex;
                SelectedDungeon.Text = DungeonNames[DungeonIndex];
                isMarked = state.isMarked;
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
            ImageIndex = Math.Clamp(state, 0, ImageNames.Length);
            UpdateImage();
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
            DungeonIndex = DefaultDungeonIndex;
            SelectedDungeon.Text = DungeonNames[DungeonIndex];
            SetSelectedDungeonLocation();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SelectedDungeon.Text = DungeonNames[DungeonIndex];
                ((Medallion)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetSelectedDungeonLocation();
            }
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[1], marked: isMarked);
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
            //var point = (ObjectPoint)ogPoint;
            switch (name)
            {
                case "":
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Medallion Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            Invalidate();
            UpdateImage();
            SetSelectedDungeonLocation();
        }
    }
}
