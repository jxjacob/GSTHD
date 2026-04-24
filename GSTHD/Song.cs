using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Linq;
using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace GSTHD
{
    public struct SongMarkerState
    {
        public bool HoldsImage;
        public string HeldImageName;
        public int ImageIndex;
        public MarkedImageIndex isMarked;
    }

    public struct SongState
    {
        public int ImageIndex;
        public MarkedImageIndex isMarked;
        public SongMarkerState MarkerState;

        public override string ToString() => $"{ImageIndex},{(int)isMarked},{MarkerState.HoldsImage},{MarkerState.HeldImageName},{MarkerState.ImageIndex},{(int)MarkerState.isMarked}";
    }

    public class SongMarker : OrganicImage, UpdatableFromSettings, ProgressibleElement<SongMarkerState>, DraggableElement<SongMarkerState>
    {
        protected override Type StyleKeyOverride => typeof(AdvancedImage);
        
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<SongMarkerState> ProgressBehaviour;
        private readonly DraggableElementBehaviour<SongMarkerState> DragBehaviour;

        public string[] ImageNames;
        private bool HoldsImage;
        private string HeldImageName;
        private int ImageIndex = 0;

        private bool RemoveImage;
        bool isBroadcastable;
        bool isOnBroadcast;

        Song Song;

        delegate void SetStateCallback(SongMarkerState state);

        public SongMarker(Song song, Settings settings, string[] imageCollection, bool isBroadcast = false) : base((Uri?)null)
        {
            Song = song;
            Settings = settings;
            isBroadcastable = song.isBroadcastable && !isBroadcast;
            isOnBroadcast = isBroadcast;
            ProgressBehaviour = new ProgressibleElementBehaviour<SongMarkerState>(this, settings);
            DragBehaviour = new DraggableElementBehaviour<SongMarkerState>(this, settings);

            if (imageCollection == null)
            {
                ImageNames = Settings.DefaultSongMarkerImages;
            }
            else
            {
                ImageNames = imageCollection;
            }

            IsVisible = true;

            if (ImageNames.Length > 0)
            {
                Name = Song.Name + "_SongMarker";
                SizeMode = PictureBoxSizeMode.StretchImage;
                // this is fuckin stupid, but apparently the old way was it just used the loaded image size which is SO MUCH WORSE
                TrueWidth = Song.SongSize.Width/2;
                TrueHeight =  Song.SongSize.Height/2;
                UpdateImage();

                //if (data.DragAndDropImageName != string.Empty)
                //    SongMarker.Tag = data.DragAndDropImageName;
                //else
                //    SongMarker.Tag = ImageNames[1];

            }
        }

        public void UpdateFromSettings()
        { 
            PointerPressed -= ProgressBehaviour.Mouse_ClickDown;
            PointerPressed -= Mouse_MiddleClickDown;
            PointerReleased -= DragBehaviour.Mouse_ClickUp;
            PointerPressed -= DragBehaviour.Mouse_ClickDown;
            PointerMoved -= Mouse_Move;
            DragDrop.RemoveDropHandler(this, Mouse_DragDrop);
            DragDrop.RemoveDragEnterHandler(this, Mouse_DragEnter);

            if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.None)
            {
                IsVisible = false;
            }
            else
            {
                IsVisible = true;

                if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.CheckOnly)
                {
                    PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropOnly)
                {
                    PointerPressed += Mouse_MiddleClickDown;
                    PointerReleased += DragBehaviour.Mouse_ClickUp;
                    DragDrop.AddDragEnterHandler(this, Mouse_DragEnter);
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropAndCheck)
                {
                    PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                    PointerReleased += DragBehaviour.Mouse_ClickUp;
                    DragDrop.AddDragEnterHandler(this, Mouse_DragEnter);
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DragAndDrop)
                {
                    PointerPressed += Mouse_MiddleClickDown;
                    PointerReleased += DragBehaviour.Mouse_ClickUp;
                    PointerPressed += DragBehaviour.Mouse_ClickDown;
                    PointerMoved += Mouse_Move;
                    DragDrop.AddDragEnterHandler(this,  Mouse_DragEnter);
                    DragDrop.AddDropHandler(this, Mouse_DragDrop);
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.Full)
                {
                    PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                    PointerReleased += DragBehaviour.Mouse_ClickUp;
                    PointerPressed += DragBehaviour.Mouse_ClickDown;
                    PointerMoved += Mouse_Move;
                    DragDrop.AddDragEnterHandler(this,  Mouse_DragEnter);
                    DragDrop.AddDropHandler(this, Mouse_DragDrop);
                }
            }
        }

        public void UpdateImage()
        {
            if (HoldsImage)
            {
                // if (Image != null) Image.Dispose();
                // Image = null;
                Source = "Resources/" + HeldImageName;
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    var sm = BroadcastManager.GetControls(Name);
                    if (sm.Count() > 0)
                    {
                        SongMarker smm = (SongMarker)sm.First();
                        smm.HeldImageName = HeldImageName;
                        smm.HoldsImage = true;
                        smm.UpdateImage();
                    }
                }
            }
            else
            {
                // if (Image != null) Image.Dispose();
                // Image = null;
                Source = @"Resources/" + ImageNames[ImageIndex];
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    var sm = BroadcastManager.GetControls(Name);
                    if (sm.Count() > 0)
                    {
                        SongMarker smm = (SongMarker)sm.First();
                        smm.HoldsImage = false;
                        smm.ImageIndex = ImageIndex;
                        smm.UpdateImage();
                    }
                }
            }
            InvalidateVisual();
        }

        public void Mouse_MiddleClickDown(object? sender, PointerPressedEventArgs e)
        {
            if (e.Properties.IsMiddleButtonPressed)
            {
                ProgressBehaviour.Mouse_MiddleClickDown(sender, e);
            }
        }

        private void Mouse_DragEnter(object? sender, DragEventArgs e)
        {
            e.DragEffects = !e.DataTransfer.Formats.Contains(Settings.GSTCustomFormat) ? DragDropEffects.None : DragDropEffects.Copy;
        }

        private void Mouse_Move(object? sender, PointerEventArgs e)
        {
            if (HoldsImage)
            {
                DragBehaviour.Mouse_Move(sender, e);
            }
        }

        public void Mouse_DragDrop(object? sender, DragEventArgs e)
        {
            ImageIndex = 0;
            HoldsImage = true;
            var inpu = e.DataTransfer.GetItems(Settings.GSTCustomFormat).First().TryGetText();
            var parts = inpu.Split('|');
            var dropContent = new DragDropContent(bool.Parse(parts[0]), parts[1], int.Parse(parts[2]), Enum.Parse<MarkedImageIndex>(parts[3]));
            HeldImageName = dropContent.ImageName;
            UpdateImage();
            DragBehaviour.SaveChanges();
        }

        public SongMarkerState GetState()
        {
            return new SongMarkerState()
            {
                HoldsImage = HoldsImage,
                HeldImageName = HeldImageName,
                ImageIndex = ImageIndex,
                isMarked = isMarked,
            };
        }

        public void SetState(SongMarkerState state)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState( state ));
            } else
            {
                HoldsImage = state.HoldsImage;
                HeldImageName = state.HeldImageName;
                isMarked = state.isMarked;
                ImageIndex = GSTMath.Clamp(state.ImageIndex, 0, ImageNames.Length);
                UpdateImage();
                DragBehaviour.SaveChanges();
            }
            
        }

        public void IncrementState()
        {
            RemoveImage = true;
            if (ImageIndex < ImageNames.Length - 1) ImageIndex += 1;
            else if (Settings.WraparoundItems) ImageIndex = 0;
            UpdateImage();
        }

        public void DecrementState()
        {
            RemoveImage = true;
            if (ImageIndex > 0) ImageIndex -= 1;
            else if (Settings.WraparoundItems) ImageIndex = ImageNames.Length - 1;
            UpdateImage();
        }

        public void ResetState()
        {
            RemoveImage = true;
            ImageIndex = 0;
            isMarked = 0;
            UpdateImage();
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }

        public void StartDragDrop(PointerEventArgs e)
        {
            HoldsImage = false;
            UpdateImage();
            var dropContent = new DragDropContent(false, HeldImageName, marked: isMarked);
            var dataTransferItem =  new DataTransferItem();
            dataTransferItem.Set(Settings.GSTCustomFormat, dropContent.ToString());
            var dataTransfer = new DataTransfer();
            dataTransfer.Add(dataTransferItem);
            DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
            SaveChanges();
        }

        public void SaveChanges()
        {
            if (RemoveImage)
            {
                HoldsImage = false;
                RemoveImage = false;
                UpdateImage();
            }
        }

        public void CancelChanges() { }
    }

    public class Song : OrganicImage, UpdatableFromSettings, ProgressibleElement<int>, DraggableAutocheckElement<int>, IAlternatableObject
    {
        protected override Type StyleKeyOverride => typeof(AdvancedImage);
        
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<int> DragBehaviour;

        private readonly string DragPictureName;

        public string[] ImageNames { get; set; }
        string ActiveImageName;
        public bool isBroadcastable { get; set; }
        private bool isOnBroadcast;
        public string AutoName { get; set; } = null;
        public string LinkedItem { get; set; } = null;

        private int ImageIndex = 0;

        public SongMarker SongMarker;

        public Size SongSize;

        delegate void SetStateCallback(int state);

        public Song(ObjectPointSong data, Settings settings, bool isBroadcast = false) : base((Uri?)null)
        {
            Settings = settings;
            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<int>(this, settings);

            IsVisible = data.Visible;

            if (data.ImageCollection != null)
            {
                ImageNames = data.ImageCollection;
                ActiveImageName = data.ActiveSongImage;
            }

            Name = data.Name;
            SongSize = data.Size;
            isBroadcastable = data.isBroadcastable && !isBroadcast;
            isOnBroadcast = isBroadcast;
            AutoName = data.AutoName;
            
            // BackColor = Color.Transparent;
            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            IsTabStop = false;
            DragDrop.SetAllowDrop(this, true);
            isMarkable = data.isMarkable;
            LinkedItem = data.LinkedItem;

            if (ImageNames.Length > 0)
            {
                SizeMode = PictureBoxSizeMode.Normal;
                UpdateImage();

                if (string.IsNullOrEmpty(data.DragAndDropImageName))
                    DragPictureName = ImageNames[1];
                else
                    DragPictureName = data.DragAndDropImageName;
            }

            SongMarker = new SongMarker(this, settings, data.TinyImageCollection, isBroadcast)
            {
                // BackColor = Color.Transparent,
                IsTabStop = false,
            };
            DragDrop.SetAllowDrop(SongMarker, true);

            TrueWidth = SongSize.Width;
            TrueHeight = (int)(SongSize.Height + SongMarker.TrueHeight * 5 / 6);
            
            // Children.Add(SongMarker);
            SongMarker.ZIndex += 2;

            if (!isOnBroadcast)
            {
                PointerReleased += DragBehaviour.Mouse_ClickUp;
                PointerPressed += ProgressBehaviour.Mouse_ClickDown;
                PointerPressed += DragBehaviour.Mouse_ClickDown;
                DragDrop.AddDragEnterHandler(this, Mouse_DragEnter);

            }
            UpdateFromSettings();

        }

        public void UpdateFromSettings()
        {
            if (!isOnBroadcast)
            {
                DragDrop.RemoveDragEnterHandler(SongMarker, Mouse_DragEnter);
                DragDrop.RemoveDropHandler(this, Mouse_DragDrop);
                DragDrop.RemoveDropHandler(this, Mouse_DragDrop_WithMoveLocationToSong);
                DragDrop.RemoveDropHandler(this, SongMarker.Mouse_DragDrop);
                PointerMoved -= Mouse_Move;
                PointerMoved -= Mouse_Move_WithMoveLocationToSong;
                SongMarker.PointerMoved -= Mouse_Move;
                SongMarker.PointerMoved -= Mouse_Move_WithMoveLocationToSong;
            }
            
            if (Settings.SongMarkerBehaviour != Settings.SongMarkerBehaviourOption.None)
            {
                if (ImageNames.Length > 0)
                {
                    //TODO: adjust this likely
                    Canvas.SetTop(SongMarker, Canvas.GetTop(this) + SongSize.Height - SongMarker.TrueHeight / 6);
                    Canvas.SetLeft(SongMarker, Canvas.GetLeft(this) + (SongSize.Width - SongMarker.TrueWidth) / 2);
                }
                if (!isOnBroadcast)
                {
                    if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.CheckOnly)
                    {
                        PointerMoved += Mouse_Move;
                        SongMarker.PointerMoved += Mouse_Move;
                    }
                    else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropOnly
                        || Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropAndCheck
                        || Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DragAndDrop
                        || Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.Full)
                    {
                        DragDrop.AddDragEnterHandler(SongMarker, Mouse_DragEnter);
                        DragDrop.AddDropHandler(this, SongMarker.Mouse_DragDrop);

                        if (Settings.MoveLocationToSong)
                        {
                            DragDrop.AddDropHandler(this, Mouse_DragDrop_WithMoveLocationToSong);
                            PointerMoved += Mouse_Move_WithMoveLocationToSong;
                            SongMarker.PointerMoved += Mouse_Move_WithMoveLocationToSong;
                        }
                        else
                        {
                            DragDrop.AddDragEnterHandler(this, Mouse_DragDrop);
                            PointerMoved += Mouse_Move;
                            SongMarker.PointerMoved += Mouse_Move;
                        }
                    }
                }
                
            }

            if (!isOnBroadcast)
            {
                SongMarker.UpdateFromSettings();
            }
            
        }

        public void UpdateImage(bool isRemote=false)
        {
            // if (Image != null) Image.Dispose();
            // Image = null;
            Source = @"Resources/" + ImageNames[ImageIndex];
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                var so = BroadcastManager.GetControl(Name);
                if (so is Song soo)
                {
                    soo.ImageIndex = ImageIndex;
                    soo.isMarked = isMarked;
                    soo.UpdateImage(true);
                }
                else if (so is Item soi)
                {
                    soi.ImageIndex = ImageIndex;
                    soi.isMarked = isMarked;
                    soi.UpdateImage(true);
                }
            }
            if (LinkedItem != null && LinkedItem != "" && !isRemote && Parent != null)
            {
                Song ite = (Song)((Panel)Parent).Children.First(x => x.Name == LinkedItem);
                ite.ImageIndex = ImageIndex;
                ite.isMarked = isMarked;
                ite.UpdateImage(true);
            }
            InvalidateVisual();
        }

        public void SetMarkerLocation()
        {
            Canvas.SetTop(SongMarker, Canvas.GetTop(this) + SongSize.Height - SongMarker.TrueHeight / 6);
            Canvas.SetLeft(SongMarker, Canvas.GetLeft(this) + (SongSize.Width - SongMarker.TrueWidth) / 2);
        }

        private void Mouse_Move(object? sender, PointerEventArgs e)
        {
            DragBehaviour.Mouse_Move_WithAutocheck(sender, e);
        }

        private void Mouse_Move_WithMoveLocationToSong(object? sender, PointerEventArgs e)
        {
            DragBehaviour.Mouse_Move(sender, e);
        }

        private void Mouse_DragDrop(object? sender, DragEventArgs e) { }

        private void Mouse_DragDrop_WithMoveLocationToSong(object sender, DragEventArgs e)
        {
            var inpu = e.DataTransfer.GetItems(Settings.GSTCustomFormat).First().TryGetText();
            var parts = inpu.Split('|');
            var dropContent = new DragDropContent(bool.Parse(parts[0]), parts[1], int.Parse(parts[2]), Enum.Parse<MarkedImageIndex>(parts[3]));
            if (dropContent.IsAutocheck)
            {
                IncrementState();
                DragBehaviour.SaveChanges();
            }
        }

        private void Mouse_DragEnter(object? sender, DragEventArgs e)
        {
            e.DragEffects = !e.DataTransfer.Formats.Contains(Settings.GSTCustomFormat) ? DragDropEffects.None : DragDropEffects.Copy;
        }

        public int GetState()
        {
            return ImageIndex;
        }

        public void SetState(int state)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState(state));
            } else
            {
                ImageIndex = GSTMath.Clamp(state, 0, ImageNames.Length);
                UpdateImage();
                DragBehaviour.SaveChanges();
            }
            
        }

        public SongState GetWholeState()
        {
            return new SongState()
            {
                ImageIndex = ImageIndex,
                isMarked = isMarked,
                MarkerState = SongMarker.GetState(),
            };
        }

        public void SetWholeState(SongState state)
        {
            ImageIndex = state.ImageIndex;
            isMarked = state.isMarked;
            SongMarkerState temp = new SongMarkerState()
            {
                HoldsImage = state.MarkerState.HoldsImage,
                HeldImageName = state.MarkerState.HeldImageName,
                ImageIndex = state.MarkerState.ImageIndex,
                isMarked = state.MarkerState.isMarked,
            };
            SongMarker.SetState(temp);
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
        }

        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
            UpdateImage();
        }

        public void StartDragDrop(PointerEventArgs e)
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, DragPictureName, marked: isMarked);
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
            SongMarker.IsVisible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            //var point = (ObjectPoint)ogPoint;
            switch (name)
            {
                case "Size":
                    // size is handled differently and is done here instead of in the main file
                    // format: `1,2`
                    var newvalues = value.ToString().Split(',');
                    SongSize = new Size(this.SongSize.Width + int.Parse(newvalues[0]) * mult, this.SongSize.Height + int.Parse(newvalues[1]) * mult);
                    TrueWidth = SongSize.Width;
                    TrueHeight = (SongSize.Height + SongMarker.TrueHeight * 5 / 6);
                    break;
                case "DragAndDropImageName":
                    //this thing does nothing, ignore it
                    break;
                case "TinyImageCollection":
                    if (mult > 0) SongMarker.ImageNames = ((JArray)value).ToObject<string[]>();
                    else SongMarker.ImageNames = (string[])ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Song Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            InvalidateVisual();
            UpdateImage();
            SetMarkerLocation();
        }
    }
}
