using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GSTHD
{
    class GuaranteedHint : OrganicImage, ProgressibleElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;

        public List<string> ListImageName { get; set; } = new List<string>();
        public bool isBroadcastable { get; set; } = false;

        public Size GuaranteddHintSize { get; set; }

        public GuaranteedHint(ObjectPoint data, Settings settings, bool isOnBroadcast)
        {
            Name = data.Name;
            Settings = settings;
            Visible = data.Visible;

            if (data.ImageCollection != null)
                ListImageName = data.ImageCollection.ToList();

            GuaranteddHintSize = data.Size;

            if (data.BackColor != Color.Transparent) BackColor = data.BackColor;
            if (ListImageName.Count > 0)
            {
                this.Image = Image.FromFile(@"Resources/" + ListImageName[0]);
                this.SizeMode = (PictureBoxSizeMode)data.SizeMode;
                this.Size = GuaranteddHintSize;
            }            
            this.Location = new Point(data.X, data.Y);
            this.TabStop = false;
            this.AllowDrop = false;
            this.isBroadcastable = data.isBroadcastable;

            if (!isOnBroadcast)
            {
                ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, Settings);
                this.MouseDown += ProgressBehaviour.Mouse_ClickDown;
            }
        }

        public void UpdateImage()
        {
            if (IsHandleCreated) { Invalidate(); }
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((GuaranteedHint)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).isMarked = isMarked;
                ((GuaranteedHint)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateImage();
            }
        }

        // this is so fucking unneccesary just to allow you to checkmark a static image lmao
        public void IncrementState()
        {
            //blanky
        }
        public void DecrementState()
        {
            //blanky
        }
        public void ResetState()
        {
            //blanky
        }
        public void ToggleCheck()
        {
            isMarked = !isMarked;
            UpdateImage();
        }
    }
}
