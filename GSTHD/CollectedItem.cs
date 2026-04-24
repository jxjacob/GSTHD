using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Color = Avalonia.Media.Color;
using Size = System.Drawing.Size;

namespace GSTHD
{
    public struct CollectedItemState
    {
        public int CollectedItems;
        public MarkedImageIndex isMarked;

    }
    class CollectedItem : OrganicImage, IAlternatableObject, ProgressibleElement<CollectedItemState>, DraggableAutocheckElement<CollectedItemState>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<CollectedItemState> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<CollectedItemState> DragBehaviour;

        public string[] ImageNames { get; set; }
        public TextBlock ItemCount { get; set; }
        public Size CollectedItemSize { get; set; }
        public Size CollectedItemCountPosition { get; set; }
        public int CountMin { get; set; }
        public int CountMax { get; set; }
        public int DefaultValue { get; set; }
        public int CollectedItems { get; set; }
        public int Step { get; set; }

        public bool isBroadcastable { get; set; }
        public bool hasSlash { get; set; }
        public bool hoveredOver { get; set; }

        public string AutoName { get; set; } = null;
        public string AutoSubName { get; set; } = null;

        #if DEBUG
                public bool MinimumCap = false;
        #else
                public bool MinimumCap = true;
        #endif

        delegate void SetStateCallback(CollectedItemState state);
        delegate void UpdateCountCallback();

        public CollectedItem(ObjectPointCollectedItem data, Settings settings, bool isBroadcast = false) : base((Uri?)null)
        {
            Settings = settings;
            IsVisible = data.Visible;

            ImageNames = data.ImageCollection ?? [];

            Name = data.Name;

            CountMin = data.CountMin;
            CountMax = data.CountMax ?? 100;
            DefaultValue = data.DefaultValue;
            CollectedItems = GSTMath.Clamp(DefaultValue, CountMin, CountMax);
            Step = data.Step == 0 ? 1 : data.Step;
            CollectedItemSize = data.Size;
            isBroadcastable = data.isBroadcastable && !isBroadcast;
            hasSlash = data.hasSlash;

            AutoName = data.AutoName;
            AutoSubName = data.AutoSubName;
            isMarkable = data.isMarkable;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                SizeMode = PictureBoxSizeMode.Zoom;
                TrueWidth = CollectedItemSize.Width;
                TrueHeight = CollectedItemSize.Height;
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<CollectedItemState>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<CollectedItemState>(this, Settings);

            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            CollectedItemCountPosition = data.CountPosition.IsEmpty ? new Size(0, -7) : data.CountPosition;

            BackColor = new Color(data.BackGroundColor.A, data.BackGroundColor.R, data.BackGroundColor.G, data.BackGroundColor.B);
            IsTabStop = false;


            ItemCount = new TextBlock
            {
                IsVisible = data.Visible,
                Background = new SolidColorBrush(new Color(data.BackColor.A, data.BackColor.R, data.BackColor.G, data.BackColor.B)),
                // BorderStyle = BorderStyle.None,
                Text = CollectedItems.ToString(),
                FontFamily = data.LabelFontName,
                FontSize = data.LabelFontSize * 1.4,
                Foreground = new SolidColorBrush(new Color(data.LabelColor.A, data.LabelColor.R, data.LabelColor.G, data.LabelColor.B)),
                // AutoSize = true,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(3, 1, 3, -1),
                // Height = 40,
                // Width = 50,
            };
            FontStyleFix(data.LabelFontStyle);
            SetCounterPosition();

            if (hasSlash)
            {
                ItemCount.Text += " /";
            }

            if (!isBroadcast)
            {
                PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                PointerReleased += DragBehaviour.Mouse_ClickUp;
                PointerPressed += DragBehaviour.Mouse_ClickDown;
                PointerMoved += DragBehaviour.Mouse_Move_WithAutocheck;
                PointerWheelChanged += Mouse_Wheel;
                PointerWheelChanged += DragBehaviour.Mouse_Wheel;
                PointerEntered += Panel_MouseEnter;
                PointerExited += Panel_MouseLeave;
                ItemCount.PointerPressed += ProgressBehaviour.Mouse_ClickDown; // must add these lines because MouseDown/Up on PictureBox won't fire when hovering above Label
                ItemCount.PointerPressed += DragBehaviour.Mouse_ClickDown;
                ItemCount.PointerReleased += DragBehaviour.Mouse_ClickUp;
                ItemCount.PointerMoved += DragBehaviour.Mouse_Move_WithAutocheck;
                ItemCount.PointerEntered += Panel_MouseEnter;
                ItemCount.PointerExited += Panel_MouseLeave;
                ItemCount.PointerWheelChanged += Mouse_Wheel; // this line used to be commented out in winforms, now it's required
            }
        }

        public void Mouse_Wheel(object? sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y != 0)
            {
                var scrolls = e.Delta.Y / 1;
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
            Source = @"Resources/" + ImageNames[GSTMath.Clamp(CollectedItems, 0, ImageNames.Length-1)];
            InvalidateVisual();
        }

        public void SetCounterPosition()
        {
            Canvas.SetTop(ItemCount, Canvas.GetTop(this) + (CollectedItemSize.Height) - CollectedItemCountPosition.Height*2);
            Canvas.SetLeft(ItemCount, Canvas.GetLeft(this));
        }

        public void UpdateCount()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(UpdateCount);
            } else
            {
                ItemCount.Text = CollectedItems.ToString();
                if (hasSlash) ItemCount.Text += " /";
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    CollectedItem ci = (CollectedItem)BroadcastManager.GetControl(Name)!;
                    ci.CollectedItems = CollectedItems;
                    ci.isMarked = isMarked;
                    ci.UpdateCount();
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
            // Invoke(new SetStateCallback(SetState), new object[] { new CollectedItemState { CollectedItems = state, isMarked = isMarked } });
            SetState(new CollectedItemState(){ CollectedItems = state, isMarked = isMarked });
        }

        public void SetState(CollectedItemState state)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState(state));
            } else
            {
                CollectedItems = state.CollectedItems;
                if (MinimumCap && CollectedItems < CountMin) CollectedItems = CountMin;
                isMarked = state.isMarked;
                UpdateCount();
                DragBehaviour.SaveChanges();
            }
        }

        public void IncrementState()
        {
            CollectedItems += Step;
            if (CollectedItems > CountMax) CollectedItems = (Settings.WraparoundItems) ? CountMin : CountMax;
            else if (CollectedItems < CountMin) CollectedItems = (Settings.WraparoundItems) ? CountMax : CountMin;
            UpdateCount();
        }

        public void DecrementState()
        {
            CollectedItems -= Step;
            if (CollectedItems < CountMin) CollectedItems = (Settings.WraparoundItems) ? CountMax : CountMin;
            else if (CollectedItems > CountMax) CollectedItems = (Settings.WraparoundItems) ? CountMin : CountMax;
            UpdateCount();
        }

        public void ResetState()
        {
            CollectedItems = DefaultValue;
            isMarked = 0;
            UpdateCount();
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateCount();
        }
        public void SetColor(Color color)
        {
            ItemCount.Foreground =  new SolidColorBrush(new Color(color.A, color.R, color.G, color.B));
        }

        // both of these functions are for when the stone is in a WOTH panel, so that it can be scrolled without the whole WOTH panle scrolling as well
        private void Panel_MouseEnter(object? sender, EventArgs e)
        {
            hoveredOver = true;
        }

        private void Panel_MouseLeave(object? sender, EventArgs e)
        {
            hoveredOver = false;
        }

        public void StartDragDrop(PointerEventArgs e)
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, ImageNames[(ImageNames.Length == 1) ? 0 : GSTMath.Clamp(CollectedItems, 1, ImageNames.Length - 1)], marked:isMarked);
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
            ItemCount.IsVisible = visible;
        }
        
        private void FontStyleFix(FontStyle style)
        {
            // sucks that this is manual but what you gonna do?
            switch (style)
            {
                case GSTHD.FontStyle.Bold:
                    ItemCount.FontWeight = FontWeight.Bold;
                    ItemCount.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
                case GSTHD.FontStyle.Italic:
                    ItemCount.FontWeight = FontWeight.Normal;
                    ItemCount.FontStyle = Avalonia.Media.FontStyle.Italic;
                    break;
                case GSTHD.FontStyle.Regular:
                    ItemCount.FontWeight = FontWeight.Normal;
                    ItemCount.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
            }
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (ObjectPointCollectedItem)ogPoint;
            switch (name)
            {
                case "Size":
                    var newvalues = value.ToString().Split(',');
                    TrueWidth += mult*int.Parse(newvalues[0]);
                    TrueHeight += mult*int.Parse(newvalues[1]);
                    break;
                case "LabelFontName":
                    if (mult > 0) ItemCount.FontFamily = value.ToString();
                    else ItemCount.FontFamily = (string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "LabelFontSize":
                    ItemCount.FontSize += (mult * int.Parse(value.ToString()));
                    break;
                case "LabelFontStyle":
                    if (mult > 0) FontStyleFix(Enum.Parse<FontStyle>(value.ToString()));
                    else FontStyleFix((FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                // TODO: restore
                // case "LabelColor":
                //     if (mult > 0) ItemCount.Foreground = Color.FromName(value.ToString());
                //     else ItemCount.Foreground = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                //     break;
                default:
                    throw new NotImplementedException($"Could not perform CollectedItem Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            InvalidateVisual();
            UpdateImage();
            SetCounterPosition();
        }
    }
}
