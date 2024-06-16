using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace GSTHD
{
    public struct DoubleItemState
    {
        public int ImageIndex;
        public MarkedImageIndex isMarked;
    }
    public class DoubleItem : OrganicImage, IAlternatableObject, ProgressibleElement<DoubleItemState>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<DoubleItemState> ProgressBehaviour;

        public string[] ImageNames { get; set; }
        public int ImageIndex { get; set; } = 0;

        public bool isMouseDown { get; set; } = false;
        public bool isColoredLeft { get; set; } = false;
        public bool isColoredRight { get; set; } = false;
        public int left_id { get; set; }
        public int right_id { get; set; }
        public Size DoubleItemSize { get; set; }

        // purely used for handling broadcast marking logic
        public MarkedImageIndex leftMark { get; set; } = 0;
        public MarkedImageIndex rightMark { get; set; } = 0;

        public bool isBroadcastable { get; set; }
        public string AutoName { get; set; } = null;

        delegate void SetStateCallback(DoubleItemState state);

        public DoubleItem(ObjectPoint data, Settings settings, bool isBroadcast = false)
        {
            Settings = settings;
            Visible = data.Visible;

            if (data.ImageCollection == null)
                ImageNames = new string[0];
            else
                ImageNames = data.ImageCollection;

            DoubleItemSize = data.Size;
            this.Name = data.Name;
            this.Size = DoubleItemSize;
            this.left_id = data.LeftDK64_ID;
            this.right_id = data.RightDK64_ID;

            if (ImageNames.Length > 0)
            {
                this.Image = Image.FromFile(@"Resources/" + ImageNames[0]);
                this.SizeMode = PictureBoxSizeMode.Zoom;
                
            }

            this.isBroadcastable = data.isBroadcastable;
            this.AutoName = data.AutoName;
            this.Location = new Point(data.X, data.Y);
            this.TabStop = false;
            this.AllowDrop = false;

            ProgressBehaviour = new ProgressibleElementBehaviour<DoubleItemState>(this, Settings);

            if (!isBroadcast)
            {
                this.MouseMove += this.Click_MouseMove;
                MouseDown += ProgressBehaviour.Mouse_ClickDown;
                MouseDown += this.Click_MouseDown;
            }
        }

        private void Click_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 1)
                isMouseDown = false;
            else isMouseDown = true;
        }

        private void Click_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton) && isMouseDown)
            {
                // TODO change that bool to DragBehaviour.AutocheckDragDrop
                var dropContent = new DragDropContent(false, ImageNames[4], left_id, isMarked);
                DecrementLeftState();
                this.DoDragDrop(dropContent, DragDropEffects.Copy);
                isMouseDown = false;
            }
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton) && isMouseDown)
            {
                var dropContent = new DragDropContent(false, ImageNames[5], right_id, isMarked);
                DecrementRightState();
                this.DoDragDrop(dropContent, DragDropEffects.Copy);
                isMouseDown = false;
            }
        }

        public void UpdateImage()
        {
            if (Image != null) Image.Dispose();
            Image = null;
            Image = Image.FromFile(@"Resources/" + ImageNames[ImageIndex]);
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ImageIndex = ImageIndex;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isColoredLeft = isColoredLeft;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isColoredRight = isColoredRight;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isMarked = isMarked;
                ((DoubleItem)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
            };
            if (IsHandleCreated) { Invalidate(); }
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
            if (isColoredLeft) DecrementLeftState(); else IncrementLeftState();
        }

        public void SetLeftMark(MarkedImageIndex mark)
        {
            leftMark = mark;
            isMarked = (MarkedImageIndex)System.Math.Max((int)leftMark, (int)rightMark);
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
            if (isColoredRight) DecrementRightState(); else IncrementRightState();
        }

        public void SetRightMark(MarkedImageIndex mark)
        {
            rightMark = mark;
            isMarked = (MarkedImageIndex)System.Math.Max((int)leftMark, (int)rightMark);
        }


        public void ToggleCheck()
        {
            IncrementMarked(Settings.MarkMode == Settings.MarkModeOption.Cycle);
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
            Invoke(new SetStateCallback(SetState), new object[] { new DoubleItemState { ImageIndex = state, isMarked=isMarked } });
        }

        public void SetState(DoubleItemState state)
        {
            if (this.InvokeRequired)
            {
                Invoke(new SetStateCallback(SetState), new object[] { state });
                return;
            } else
            {
                isMarked = state.isMarked;
                if ((state.ImageIndex & 1) == 1) { IncrementLeftState(); }
                if ((state.ImageIndex & 2) == 2) { IncrementRightState(); }
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
            Visible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (ObjectPointCollectedItem)ogPoint;
            switch (name)
            {
                case "":
                    break;
                default:
                    throw new NotImplementedException($"Could not perform DoubleItem Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }
    }
}
