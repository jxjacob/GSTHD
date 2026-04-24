using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Avalonia.Controls;
using Color = Avalonia.Media.Color;

namespace GSTHD
{
    class GuaranteedHint : OrganicImage, IAlternatableObject, ProgressibleElement<int>
    {
        private readonly Settings Settings;
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;

        public List<string> ListImageName { get; set; } = new List<string>();
        public bool isBroadcastable { get; set; } = false;
        private bool isSpoilerCell { get; set; } = false;

        public Size GuaranteddHintSize { get; set; }

        public GuaranteedHint(ObjectPoint data, Settings settings, bool isOnBroadcast, bool isOnSpoilerCell) : base((Uri?)null)
        {
            Name = data.Name;
            Settings = settings;
            IsVisible = data.Visible;

            if (data.ImageCollection != null)
                ListImageName = data.ImageCollection.ToList();

            GuaranteddHintSize = data.Size;

            BackColor = new Color(data.BackColor.A, data.BackColor.R, data.BackColor.G, data.BackColor.B);
            if (ListImageName.Count > 0)
            {
                Source = @"Resources/" + ListImageName[0];
                this.SizeMode = (PictureBoxSizeMode)data.SizeMode;
                TrueWidth = data.Size.Width;
                TrueHeight = data.Size.Height;
            }            
            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            this.IsTabStop = false;
            // this.AllowDrop = false;
            this.isBroadcastable = !isOnBroadcast && data.isBroadcastable;
            isSpoilerCell = isOnSpoilerCell;

            if (!isOnBroadcast)
            {
                ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, Settings);
                this.PointerPressed += ProgressBehaviour.Mouse_ClickDown;
            }
        }

        public void UpdateImage()
        {
            InvalidateVisual();
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                if (isSpoilerCell)
                {
                    // this is so stupid lmao
                    var NamePieces = Name.Split('_');
                    SpoilerPanel parentpanel = (SpoilerPanel)BroadcastManager.GetControl(NamePieces.First())!;
                    SpoilerCell parentCell = (SpoilerCell)parentpanel.Children.First(x => string.Equals(x.Name, $"{NamePieces[0]}_{NamePieces[1]}", StringComparison.CurrentCultureIgnoreCase));
                    GuaranteedHint gh = (GuaranteedHint)parentCell.Children.First(x => string.Equals(x.Name, Name, StringComparison.CurrentCultureIgnoreCase));
                    gh.isMarked = isMarked;
                    gh.UpdateImage();
                }
                else
                {
                    GuaranteedHint gh = (GuaranteedHint)BroadcastManager.GetControl(Name)!;
                    gh.isMarked = isMarked;
                    gh.UpdateImage();
                }
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
            isMarked = 0;
            UpdateImage();
        }
        public void ToggleCheck()
        {
            IncrementMarked(Settings.EnabledMarks);
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
                case "ImageCollection":
                    ListImageName = ((JArray)value).ToObject<string[]>().ToList();
                    break;
                default:
                    throw new NotImplementedException($"Could not perform GuaranteedHint Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            InvalidateVisual();
            UpdateImage();
        }
    }
}
