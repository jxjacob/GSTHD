using System;
using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Color = Avalonia.Media.Color;

namespace GSTHD
{
    public struct DoubleItemState
    {
        public int ImageIndex;
        public MarkedImageIndex isMarked;
    }
    public class DoubleItem : OrganicImage, IAlternatableObject, ProgressibleElement<DoubleItemState>
    {
        protected override Type StyleKeyOverride => typeof(AdvancedImage);
        
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<DoubleItemState> ProgressBehaviour;

        public string[] ImageNames { get; set; }
        public int ImageIndex { get; set; } = 0;

        public bool isMouseDown { get; set; } = false;
        public bool isColoredLeft { get; set; } = false;
        public bool isColoredRight { get; set; } = false;
        public int left_id { get; set; }
        public int right_id { get; set; }
        // purely used for handling broadcast marking logic
        public MarkedImageIndex leftMark { get; set; } = 0;
        public MarkedImageIndex rightMark { get; set; } = 0;

        public bool isBroadcastable { get; set; }
        public string AutoName { get; set; } = null;

        delegate void SetStateCallback(DoubleItemState state);

        private DoubleItemState LastState { get; set; }

        protected Avalonia.Point DragStartPoint;

        public DoubleItem(ObjectPoint data, Settings settings, bool isBroadcast = false) : base((Uri?)null)
        {
            Settings = settings;
            IsVisible = data.Visible;

            if (data.ImageCollection == null)
                ImageNames = [];
            else
                ImageNames = data.ImageCollection;

            Name = data.Name;
            TrueWidth = data.Size.Width;
            TrueHeight = data.Size.Height;
            left_id = data.LeftDK64_ID;
            right_id = data.RightDK64_ID;
            BackColor = new Color(data.BackColor.A, data.BackColor.R, data.BackColor.G, data.BackColor.B);

            if (ImageNames.Length > 0)
            {
                SizeMode = PictureBoxSizeMode.Zoom;
                UpdateImage();
            }

            isBroadcastable = !isBroadcast && data.isBroadcastable;
            AutoName = data.AutoName;
            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            IsTabStop = false;
            // this.AllowDrop = false;
            isMarkable = data.isMarkable;

            ProgressBehaviour = new ProgressibleElementBehaviour<DoubleItemState>(this, Settings);

            if (!isBroadcast)
            {
                PointerMoved += Click_MouseMove;
                PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                PointerPressed += Click_MouseDown;
            }
        }

        private void Click_MouseDown(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount != 1)
                isMouseDown = false;
            else
            {
                isMouseDown = true;
                DragStartPoint = e.GetPosition(null);
            }
        }

        private void Click_MouseMove(object? sender, PointerEventArgs e)
        {
            var pos = e.GetPosition(null);
            if (Math.Abs(DragStartPoint.X - pos.X) >= Settings.MinDragThreshold || Math.Abs(DragStartPoint.Y - pos.Y) >= Settings.MinDragThreshold)
            {
                if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton) && isMouseDown)
                {
                    // TODO change that bool to DragBehaviour.AutocheckDragDrop
                    var dropContent = new DragDropContent(false, ImageNames[4], left_id, isMarked);
                    SetState(LastState);
                    var dataTransferItem =  new DataTransferItem();
                    dataTransferItem.Set(Settings.GSTCustomFormat, dropContent.ToString());
                    var dataTransfer = new DataTransfer();
                    dataTransfer.Add(dataTransferItem);
                    DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
                    isMouseDown = false;
                }
                if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton) && isMouseDown)
                {
                    var dropContent = new DragDropContent(false, ImageNames[5], right_id, isMarked);
                    SetState(LastState);
                    var dataTransferItem =  new DataTransferItem();
                    dataTransferItem.Set(Settings.GSTCustomFormat, dropContent.ToString());
                    var dataTransfer = new DataTransfer();
                    dataTransfer.Add(dataTransferItem);
                    DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
                    isMouseDown = false;
                }

            }
        }

        public void UpdateImage()
        {
            Source = "Resources/" + ImageNames[ImageIndex];
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                DoubleItem di = (DoubleItem)BroadcastManager.GetControl(Name)!;
                di.ImageIndex = ImageIndex;
                di.isColoredLeft = isColoredLeft;
                di.isColoredRight = isColoredRight;
                di.isMarked = isMarked;
                di.UpdateImage();
            };
            InvalidateVisual();
        }

        public void IncrementLeftState()
        {
            if (!isColoredLeft)
            {
                isColoredLeft = true;
                if (!isColoredRight) ImageIndex = 1; else ImageIndex = 3;
            }
            UpdateImage();
        }
        public void DecrementLeftState()
        {
            if (isColoredLeft)
            {
                isColoredLeft = false;
                if (!isColoredRight) ImageIndex = 0; else ImageIndex = 2;
            }
            UpdateImage();
        }
        public void IncrementState()
        {
            LastState = GetState();
            if (isColoredLeft) DecrementLeftState(); else IncrementLeftState();
        }

        public void SetLeftMark(MarkedImageIndex mark)
        {
            leftMark = mark;
            isMarked = (MarkedImageIndex)Math.Max((int)leftMark, (int)rightMark);
        }



        public void IncrementRightState()
        {
            if (!isColoredRight)
            {
                isColoredRight = true;
                if (!isColoredLeft) ImageIndex = 2; else ImageIndex = 3;
            }
            UpdateImage();
        }
        public void DecrementRightState()
        {
            if (isColoredRight)
            {
                isColoredRight = false;
                if (!isColoredLeft) ImageIndex = 0; else ImageIndex = 1;
            }
            UpdateImage();
        }
        public void DecrementState()
        {
            LastState = GetState();
            if (isColoredRight) DecrementRightState(); else IncrementRightState();
        }

        public void SetRightMark(MarkedImageIndex mark)
        {
            rightMark = mark;
            isMarked = (MarkedImageIndex)Math.Max((int)leftMark, (int)rightMark);
        }


        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }


        public DoubleItemState GetState()
        {
            int run = 0;
            if (isColoredLeft) { run ^= 1; }
            if (isColoredRight) { run ^= 2; }
            return new DoubleItemState
            {
                ImageIndex = run,
                isMarked = isMarked
            };
        }

        // legacy for autotracker
        public void SetState(int state)
        {
            // Invoke(new SetStateCallback(SetState), new object[] { new DoubleItemState { ImageIndex = state, isMarked=isMarked } });
            SetState(new DoubleItemState { ImageIndex = state, isMarked=isMarked });
        }

        public void SetState(DoubleItemState state)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState(state));
            } else
            {
                isMarked = state.isMarked;
                if ((state.ImageIndex & 1) == 1) { IncrementLeftState(); } else { DecrementLeftState(); }
                if ((state.ImageIndex & 2) == 2) { IncrementRightState(); } else { DecrementRightState(); }
            }
        }

        public void ResetState()
        {
            ImageIndex = 0;
            isMarked = 0;
            isColoredRight = false;
            isColoredLeft = false;
            UpdateImage();

        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (ObjectPoint)ogPoint;
            switch (name)
            {
                case "X":
                    Canvas.SetLeft(this, Canvas.GetLeft(this) + mult*int.Parse(value.ToString()));
                    break;
                case "Y":
                    Canvas.SetTop(this, Canvas.GetTop(this) + mult*int.Parse(value.ToString()));
                    break;
                case "Size":
                    var newvalues = value.ToString().Split(',');
                    TrueWidth += mult*int.Parse(newvalues[0]);
                    TrueHeight += mult*int.Parse(newvalues[1]);
                    break;
                default:
                    throw new NotImplementedException($"Could not perform DoubleItem Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            InvalidateVisual();
            UpdateImage();
        }
    }
}
