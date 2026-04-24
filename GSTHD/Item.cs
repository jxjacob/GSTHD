using System;
using System.Diagnostics;
using System.Linq;
using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace GSTHD
{
    public struct ItemState
    {
        public int ImageIndex;
        public MarkedImageIndex isMarked;

    }
    public class Item : OrganicImage, IAlternatableObject, ProgressibleElement<ItemState>, DraggableAutocheckElement<ItemState>
    {
        protected override Type StyleKeyOverride => typeof(AdvancedImage);
        
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<ItemState> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<ItemState> DragBehaviour;

        public string[] ImageNames { get; set; }
        public int ImageIndex { get; set; } = 0;
        public int DefaultIndex { get; set; } = 0;
        public int DK64_ID { get; set; }
        public string? DragImage { get; set; } = null;

        public bool isBroadcastable { get; set; }
        public string? DoubleBroadcastSide { get; set; }
        public string? DoubleBroadcastName { get; set; }
        public bool isDraggable { get; set; }
        private bool isSpoilerCell { get; set; } = false;
        

        public string? LinkedItem { get; set; } = null;

        public string OuterPathID { get; set; }

        public string? AutoName { get; set; } = null;


        public Item(ObjectPoint data, Settings settings, bool isBroadcast = false, bool isOnSpoilerCell = false) : base((Uri?)null)
        {
            Settings = settings;
            IsVisible = data.Visible;

            if (data.ImageCollection == null)
                ImageNames = [];
            else
                ImageNames = data.ImageCollection;

            Name = data.Name;
            
            BackColor = LegacyConverters.ConvetToColor(data.BackColor);
            isBroadcastable = data.isBroadcastable && !isBroadcast;
            isSpoilerCell = isOnSpoilerCell;

            isDraggable = data.isDraggable;

            AutoName = data.AutoName;

            DragImage = data.DragImage;
            DK64_ID = data.DK64_ID;
            OuterPathID = data.OuterPathID;
            isMarkable = data.isMarkable;
            LinkedItem = data.LinkedItem;

            DefaultIndex = data.DefaultIndex;
            ImageIndex = DefaultIndex;

            if (data.DoubleBroadcastSide != null) DoubleBroadcastSide = data.DoubleBroadcastSide;
            if (data.DoubleBroadcastName != null) DoubleBroadcastName = data.DoubleBroadcastName;

            if (ImageNames.Length > 0)
            {
                SizeMode = data.SizeMode;
                TrueWidth = data.Size.Width;
                TrueHeight = data.Size.Height;
                UpdateImage();
            }

            ProgressBehaviour = new ProgressibleElementBehaviour<ItemState>(this, Settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<ItemState>(this, Settings);
            
            //what a stupid way to position objects
            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            IsTabStop = false;
            DragDrop.SetAllowDrop(this, false);
            
            if (!isBroadcast)
            {
                if (isDraggable)
                {
                    PointerReleased += DragBehaviour.Mouse_ClickUp;
                    PointerPressed += DragBehaviour.Mouse_ClickDown;
                    PointerMoved += DragBehaviour.Mouse_Move_WithAutocheck;
                    PointerWheelChanged += DragBehaviour.Mouse_Wheel;
                }
                PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                PointerWheelChanged += Mouse_Wheel;
            }
        }

        public void Mouse_Wheel(object? sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y != 0)
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

        public void UpdateImage(bool isRemote = false)
        {
            if (ImageIndex >= ImageNames.Length) ImageIndex = ImageNames.Length - 1;
            Source = "Resources/" + ImageNames[ImageIndex];
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                if (DoubleBroadcastName == null || DoubleBroadcastSide == null || DoubleBroadcastName == string.Empty || DoubleBroadcastSide == string.Empty)
                {
                    if (isSpoilerCell)
                    {
                        var NamePieces = Name.Split('_');
                        SpoilerPanel parentpanel = (SpoilerPanel)BroadcastManager.GetControl(NamePieces.First())!;
                        SpoilerCell parentCell = (SpoilerCell)parentpanel.Children.First(x => string.Equals(x.Name, $"{NamePieces[0]}_{NamePieces[1]}", StringComparison.CurrentCultureIgnoreCase));
                        Item ite = (Item)parentCell.Children.First(x => string.Equals(x.Name, Name, StringComparison.CurrentCultureIgnoreCase));
                        ite.ImageIndex = ImageIndex;
                        ite.isMarked = isMarked;
                        ite.UpdateImage();
                    }
                    else
                    {
                        try
                        {
                            Item ite = (Item)BroadcastManager.GetControl(Name)!;
                            ite.ImageIndex = ImageIndex;
                            ite.isMarked = isMarked;
                            ite.UpdateImage();
                        } catch (NullReferenceException)
                        {
                            Debug.WriteLine($"Item {Name} could not be found on Broadcast, skipping...");
                        }
                    }
                }
                else
                {
                    //TODO: make this block cleaner
                    if (DoubleBroadcastSide == "left")
                    {
                        DoubleItem di = (DoubleItem)BroadcastManager.GetControl(DoubleBroadcastName)!;
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
                        DoubleItem di = (DoubleItem)BroadcastManager.GetControl(DoubleBroadcastName)!;
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
            if (!string.IsNullOrEmpty(LinkedItem) && !isRemote && Parent != null)
            {
                Item ite = (Item)((Panel)Parent).Children.First(x => x.Name == LinkedItem);
                ite.ImageIndex = ImageIndex;
                ite.isMarked = isMarked;
                ite.UpdateImage(true);
            }
            InvalidateVisual();
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
            // Invoke(new SetStateCallback(SetState), new object[] { new ItemState { ImageIndex = state, isMarked = isMarked } });
            Dispatcher.UIThread.Post(() => SetState(new ItemState(){ ImageIndex = state, isMarked = isMarked }));
        }

        public void SetState(ItemState state)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState(state));
            }
            else
            {
                ImageIndex = GSTMath.Clamp(state.ImageIndex, 0, ImageNames.Length);
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

        public void StartDragDrop(PointerEventArgs e)
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, (DragImage != null) ? DragImage : ImageNames[(ImageNames.Length == 1) ? 0 : GSTMath.Clamp(ImageIndex, 1, ImageNames.Length - 1)], DK64_ID, isMarked);
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
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            try
            {
                ObjectPoint? point = (ObjectPoint)ogPoint;
            }
            catch
            {
                ObjectPoint? point = null;
            }

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
                case "BackColor":
                    if (mult > 0) BackColor = LegacyConverters.ConvetToColor(System.Drawing.Color.FromName(value.ToString()));
                    else BackColor = LegacyConverters.ConvetToColor((System.Drawing.Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Item Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            InvalidateVisual();
            UpdateImage();
        }
    }
}
