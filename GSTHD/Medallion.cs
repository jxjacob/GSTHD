using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Color = Avalonia.Media.Color;

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

        public int DefaultDungeonIndex;
        private int DungeonIndex;

        public TextBlock SelectedDungeon;

        public bool isBroadcastable { get; set; }
        public string AutoName = null;

        delegate void SetStateCallback(MedallionState state);

        public Medallion(ObjectPointMedallion data, Settings settings, System.Drawing.Color? bgColor, bool isBroadcast = false) : base((Uri?)null)
        {
            Settings = settings;
            IsVisible = data.Visible;

            ImageNames = data.ImageCollection ?? [];

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
                TrueWidth = data.Size.Width;
                TrueHeight = data.Size.Height;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<MedallionState>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<MedallionState>(this, Settings);

            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            IsTabStop = false;
            // this.AllowDrop = true;

            SelectedDungeon = new TextBlock
            {
                FontFamily = data.Label.FontName,
                FontSize = data.Label.FontSize.Value*1.4,
                Text = DungeonNames[DefaultDungeonIndex],
                Background = new SolidColorBrush(Color.FromArgb(bgColor.Value.A, bgColor.Value.R,  bgColor.Value.G, bgColor.Value.B)),
                Foreground = new SolidColorBrush(Color.FromRgb(255,255,255)),
                // AutoSize = true,
                TextAlignment = TextAlignment.Center,
                IsVisible = data.Visible,
                Width = TrueWidth+6,
                Padding = new Thickness(1, 1, 1, -1),
            };
            FontStyleFix(data.Label.FontStyle.Value);
            SelectedDungeon.ZIndex += 2;
            SetSelectedDungeonLocation();

            if (!isBroadcast)
            {
                PointerReleased += DragBehaviour.Mouse_ClickUp;
                PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                PointerPressed += DragBehaviour.Mouse_ClickDown;
                PointerMoved += DragBehaviour.Mouse_Move_WithAutocheck;
                PointerWheelChanged += DragBehaviour.Mouse_Wheel;
                PointerWheelChanged += Mouse_Wheel;


                SelectedDungeon.PointerReleased += DragBehaviour.Mouse_ClickUp;
                SelectedDungeon.PointerPressed += DungeonLabel_MouseDown;
                SelectedDungeon.PointerPressed += DragBehaviour.Mouse_ClickDown;
                SelectedDungeon.PointerMoved += DragBehaviour.Mouse_Move_WithAutocheck;
                SelectedDungeon.PointerWheelChanged += DragBehaviour.Mouse_Wheel;
                SelectedDungeon.PointerWheelChanged += Mouse_Wheel;

                UpdateFromSettings();
            }
            
        }

        public void UpdateFromSettings()
        {

        }

        public void SetSelectedDungeonLocation()
        {
            Canvas.SetTop(SelectedDungeon, (int)(Canvas.GetTop(this) + TrueHeight * 0.75));
            Canvas.SetLeft(SelectedDungeon, Canvas.GetLeft(this) - 3);
        }

        private void Mouse_Wheel(object sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y != 0)
            {
                var scrolls = e.Delta.Y / 1;
                SetDungeonName((Settings.InvertScrollWheel ? (int)scrolls : (int)-scrolls));
            }
        }

        private void DungeonLabel_MouseDown(object sender, PointerEventArgs e)
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
                DungeonIndex = GSTMath.EMod(newIndex, DungeonNames.Length);
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
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                Medallion me = (Medallion)BroadcastManager.GetControl(Name)!;
                me.SelectedDungeon.Text = DungeonNames[DungeonIndex];
                me.SetSelectedDungeonLocation();
            }

        }


        public void UpdateImage()
        {
            
            Source = "Resources/" + ImageNames[ImageIndex];
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                Medallion me = (Medallion)BroadcastManager.GetControl(Name)!;
                me.ImageIndex = ImageIndex;
                me.isMarked = isMarked;
                me.UpdateImage();
            }
            InvalidateVisual();
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
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState(state));
            } else
            {
                ImageIndex = GSTMath.Clamp(state.ImageIndex, 0, ImageNames.Length);
                DungeonIndex = state.DungeonIndex;
                SelectedDungeon.Text = DungeonNames[DungeonIndex];
                isMarked = state.isMarked;
                UpdateImage();
                SetSelectedDungeonLocation();
                DragBehaviour.SaveChanges();
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    Medallion me = (Medallion)BroadcastManager.GetControl(Name)!;
                    me.SelectedDungeon.Text = DungeonNames[DungeonIndex];
                    me.SetSelectedDungeonLocation();
                }
            }
            
        }

        public void SetImageState(int state)
        {
            ImageIndex = GSTMath.Clamp(state, 0, ImageNames.Length);
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
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                Medallion me = (Medallion)BroadcastManager.GetControl(Name)!;
                me.SelectedDungeon.Text = DungeonNames[DungeonIndex];
                me.SetSelectedDungeonLocation();
            }
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }

        public void StartDragDrop(PointerEventArgs e)
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[1], marked: isMarked);
            var dataTransferItem =  new DataTransferItem();
            dataTransferItem.Set(Settings.GSTCustomFormat, dropContent.ToString());
            var dataTransfer = new DataTransfer();
            dataTransfer.Add(dataTransferItem);
            DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
        }

        public void SaveChanges() { }
        public void CancelChanges() { }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            SelectedDungeon.IsVisible = visible;
        }
        
        private void FontStyleFix(FontStyle style)
        {
            // sucks that this is manual but what you gonna do?
            switch (style)
            {
                case GSTHD.FontStyle.Bold:
                    SelectedDungeon.FontWeight = FontWeight.Bold;
                    SelectedDungeon.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
                case GSTHD.FontStyle.Italic:
                    SelectedDungeon.FontWeight = FontWeight.Normal;
                    SelectedDungeon.FontStyle = Avalonia.Media.FontStyle.Italic;
                    break;
                case GSTHD.FontStyle.Regular:
                    SelectedDungeon.FontWeight = FontWeight.Normal;
                    SelectedDungeon.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
            }
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
            InvalidateVisual();
            UpdateImage();
            SetSelectedDungeonLocation();
        }
    }
}
