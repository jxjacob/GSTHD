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

        List<string> ListImageName = new List<string>();

        Size GuaranteddHintSize;

        public GuaranteedHint(ObjectPoint data, Settings settings)
        {
            Settings = settings;

            if(data.ImageCollection != null)
                ListImageName = data.ImageCollection.ToList();

            GuaranteddHintSize = data.Size;

            if (data.BackColor != Color.Transparent) BackColor = data.BackColor;
            if (ListImageName.Count > 0)
            {
                this.Name = ListImageName[0];
                this.Image = Image.FromFile(@"Resources/" + ListImageName[0]);
                this.SizeMode = (PictureBoxSizeMode)data.SizeMode;
                this.Size = GuaranteddHintSize;
            }            
            this.Location = new Point(data.X, data.Y);
            this.TabStop = false;
            this.AllowDrop = false;


            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, Settings);
            this.MouseDown += ProgressBehaviour.Mouse_ClickDown;
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
            if (IsHandleCreated) { Invalidate(); }
        }
    }
}
