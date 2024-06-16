using GSTHD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<SongMarkerState> ProgressBehaviour;
        private readonly DraggableElementBehaviour<SongMarkerState> DragBehaviour;

        private string[] ImageNames;
        private bool HoldsImage;
        private string HeldImageName;
        private int ImageIndex = 0;

        private bool RemoveImage;
        bool isBroadcastable;
        bool isOnBroadcast;

        public Song Song;

        delegate void SetStateCallback(SongMarkerState state);

        public SongMarker(Song song, Settings settings, string[] imageCollection, bool isBroadcast = false)
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

            Visible = true;

            if (ImageNames.Length > 0)
            {
                Name = Song.Name + "_SongMarker";
                UpdateImage();
                SizeMode = PictureBoxSizeMode.StretchImage;
                Size = new Size(Image.Width, Image.Height);

                //if (data.DragAndDropImageName != string.Empty)
                //    SongMarker.Tag = data.DragAndDropImageName;
                //else
                //    SongMarker.Tag = ImageNames[1];

            }
        }

        public void UpdateFromSettings()
        { 
            MouseDown -= ProgressBehaviour.Mouse_ClickDown;
            MouseDown -= Mouse_MiddleClickDown;
            MouseUp -= DragBehaviour.Mouse_ClickUp;
            MouseDown -= DragBehaviour.Mouse_ClickDown;
            MouseMove -= Mouse_Move;
            DragEnter -= Mouse_DragEnter;
            DragDrop -= Mouse_DragDrop;

            if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.None)
            {
                Visible = false;
            }
            else
            {
                Visible = true;

                if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.CheckOnly)
                {
                    MouseDown += ProgressBehaviour.Mouse_ClickDown;
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropOnly)
                {
                    MouseDown += Mouse_MiddleClickDown;
                    MouseUp += DragBehaviour.Mouse_ClickUp;
                    DragDrop += Mouse_DragDrop;
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropAndCheck)
                {
                    MouseDown += ProgressBehaviour.Mouse_ClickDown;
                    MouseUp += DragBehaviour.Mouse_ClickUp;
                    DragDrop += Mouse_DragDrop;
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DragAndDrop)
                {
                    MouseDown += Mouse_MiddleClickDown;
                    MouseUp += DragBehaviour.Mouse_ClickUp;
                    MouseDown += DragBehaviour.Mouse_ClickDown;
                    MouseMove += Mouse_Move;
                    DragEnter += Mouse_DragEnter;
                    DragDrop += Mouse_DragDrop;
                }
                else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.Full)
                {
                    MouseDown += ProgressBehaviour.Mouse_ClickDown;
                    MouseUp += DragBehaviour.Mouse_ClickUp;
                    MouseDown += DragBehaviour.Mouse_ClickDown;
                    MouseMove += Mouse_Move;
                    DragEnter += Mouse_DragEnter;
                    DragDrop += Mouse_DragDrop;
                }
            }
        }

        public void UpdateImage()
        {
            if (HoldsImage)
            {
                if (Image != null) Image.Dispose();
                Image = null;
                Image = Image.FromFile(@"Resources/" + HeldImageName);
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((SongMarker)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).HeldImageName = HeldImageName;
                    ((SongMarker)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).HoldsImage = true;
                    ((SongMarker)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
                }
            }
            else
            {
                if (Image != null) Image.Dispose();
                Image = null;
                Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((SongMarker)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).HoldsImage = false;
                    ((SongMarker)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                    ((SongMarker)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
                }
            }
            if (IsHandleCreated) { Invalidate(); }
        }

        public void Mouse_MiddleClickDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle:
                    ProgressBehaviour.Mouse_MiddleClickDown(sender, e);
                    break;
            }
        }

        private void Mouse_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            if (HoldsImage)
            {
                DragBehaviour.Mouse_Move(sender, e);
            }
        }

        public void Mouse_DragDrop(object sender, DragEventArgs e)
        {
            ImageIndex = 0;
            HoldsImage = true;
            var dropContent = (DragDropContent)e.Data.GetData(typeof(DragDropContent));
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
            if (this.InvokeRequired)
            {
                Invoke(new SetStateCallback(SetState), new SongMarkerState[] { state });
                return;
            } else
            {
                HoldsImage = state.HoldsImage;
                HeldImageName = state.HeldImageName;
                isMarked = state.isMarked;
                ImageIndex = Math.Clamp(state.ImageIndex, 0, ImageNames.Length);
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
            IncrementMarked(Settings.MarkMode == Settings.MarkModeOption.Cycle);
            UpdateImage();
        }

        public void StartDragDrop()
        {
            HoldsImage = false;
            UpdateImage();
            var dropContent = new DragDropContent(false, HeldImageName, marked: isMarked);
            DoDragDrop(dropContent, DragDropEffects.Copy);
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

    public class Song : OrganicImage, UpdatableFromSettings, ProgressibleElement<int>, DraggableAutocheckElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;
        private readonly DraggableAutocheckElementBehaviour<int> DragBehaviour;

        private readonly string DragPictureName;

        public string[] ImageNames;
        string ActiveImageName;
        public bool isBroadcastable;
        private bool isOnBroadcast;
        public string AutoName = null;

        private int ImageIndex = 0;

        public SongMarker SongMarker;

        Size SongSize;

        delegate void SetStateCallback(int state);

        public Song(ObjectPointSong data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;
            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, settings);
            DragBehaviour = new DraggableAutocheckElementBehaviour<int>(this, settings);

            Visible = data.Visible;

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
            
            BackColor = Color.Transparent;
            Location = new Point(data.X, data.Y);
            TabStop = false;
            AllowDrop = true;

            if (ImageNames.Length > 0)
            {
                UpdateImage();
                SizeMode = PictureBoxSizeMode.Zoom;

                if (string.IsNullOrEmpty(data.DragAndDropImageName))
                    DragPictureName = ImageNames[1];
                else
                    DragPictureName = data.DragAndDropImageName;
            }

            SongMarker = new SongMarker(this, settings, data.TinyImageCollection, isBroadcast)
            {
                BackColor = Color.Transparent,
                TabStop = false,
                AllowDrop = false,
            };

            Size = new Size(SongSize.Width, SongSize.Height + SongMarker.Height * 5 / 6);
            Controls.Add(SongMarker);
            SongMarker.BringToFront();

            if (!isOnBroadcast)
            {
                MouseUp += DragBehaviour.Mouse_ClickUp;
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseDown += DragBehaviour.Mouse_ClickDown;
                DragEnter += Mouse_DragEnter;

            }
            UpdateFromSettings();

        }

        public void UpdateFromSettings()
        {
            if (!isOnBroadcast)
            {
                SongMarker.DragEnter -= Mouse_DragEnter;
                DragDrop -= Mouse_DragDrop;
                DragDrop -= Mouse_DragDrop_WithMoveLocationToSong;
                DragDrop -= SongMarker.Mouse_DragDrop;
                MouseMove -= Mouse_Move;
                MouseMove -= Mouse_Move_WithMoveLocationToSong;
                SongMarker.MouseMove -= Mouse_Move;
                SongMarker.MouseMove -= Mouse_Move_WithMoveLocationToSong;
            }
            
            if (Settings.SongMarkerBehaviour != Settings.SongMarkerBehaviourOption.None)
            {
                if (ImageNames.Length > 0)
                {
                    SongMarker.Location = new Point(
                        (SongSize.Width - SongMarker.Width) / 2,
                        SongSize.Height - SongMarker.Height / 6
                    );
                }
                if (!isOnBroadcast)
                {
                    if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.CheckOnly)
                    {
                        MouseMove += Mouse_Move;
                        SongMarker.MouseMove += Mouse_Move;
                    }
                    else if (Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropOnly
                        || Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DropAndCheck
                        || Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.DragAndDrop
                        || Settings.SongMarkerBehaviour == Settings.SongMarkerBehaviourOption.Full)
                    {
                        SongMarker.DragEnter += Mouse_DragEnter;
                        DragDrop += SongMarker.Mouse_DragDrop;

                        if (Settings.MoveLocationToSong)
                        {
                            DragDrop += Mouse_DragDrop_WithMoveLocationToSong;
                            MouseMove += Mouse_Move_WithMoveLocationToSong;
                            SongMarker.MouseMove += Mouse_Move_WithMoveLocationToSong;
                        }
                        else
                        {
                            DragDrop += Mouse_DragDrop;
                            MouseMove += Mouse_Move;
                            SongMarker.MouseMove += Mouse_Move;
                        }
                    }
                }
                
            }

            if (!isOnBroadcast)
            {
                SongMarker.UpdateFromSettings();
            }
            
        }

        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = null;
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((Song)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                ((Song)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isMarked = isMarked;
                ((Song)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
            }
            if (IsHandleCreated) { Invalidate(); }
        }

        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            DragBehaviour.Mouse_Move_WithAutocheck(sender, e);
        }

        private void Mouse_Move_WithMoveLocationToSong(object sender, MouseEventArgs e)
        {
            DragBehaviour.Mouse_Move(sender, e);
        }

        private void Mouse_DragDrop(object sender, DragEventArgs e) { }

        private void Mouse_DragDrop_WithMoveLocationToSong(object sender, DragEventArgs e)
        {
            var dropContent = (DragDropContent)e.Data.GetData(typeof(DragDropContent));
            if (dropContent.IsAutocheck)
            {
                IncrementState();
                DragBehaviour.SaveChanges();
            }
        }

        private void Mouse_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        public int GetState()
        {
            return ImageIndex;
        }

        public void SetState(int state)
        {
            if (this.InvokeRequired)
            {
                Invoke(new SetStateCallback(SetState), new object[] { state });
                return;
            } else
            {
                ImageIndex = Math.Clamp(state, 0, ImageNames.Length);
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
            IncrementMarked(Settings.MarkMode == Settings.MarkModeOption.Cycle);
            UpdateImage();
        }

        public void StartDragDrop()
        {
            var dropContent = new DragDropContent(DragBehaviour.AutocheckDragDrop, DragPictureName, marked: isMarked);
            DoDragDrop(dropContent, DragDropEffects.Copy);
        }

        public void SaveChanges() { }
        public void CancelChanges() { }
    }
}
