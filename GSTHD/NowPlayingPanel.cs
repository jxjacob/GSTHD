using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    class NowPlayingPanel : Panel, IAlternatableObject
    {
        private LabelOneTimeThing displayedSongGame;

        private LabelOneTimeThing displayedSongTitle;

        private bool isBroadcastable = false;

        delegate void SetNamesCallback(string game, string title);

        public NowPlayingPanel(ObjectPanelNowPlaying data, bool isOnBroadcast = false)
        {
            this.Name = data.Name;
            this.Location = new Point(data.X, data.Y);
            this.Visible = data.Visible;
            this.Width = data.Width;
            this.Height = data.Height;
            this.BackColor = data.BackColor;


            displayedSongTitle = new LabelOneTimeThing()
            {
                Name = Guid.NewGuid().ToString(),
                Text = "Please Autotrack a DK64R 4.0+ Seed".ToUpper(),
                Font = new Font(data.TitleFontName, data.TitleFontSize, data.TitleFontStyle),
                ForeColor = data.TitleFontColor,
                Width = data.Width - data.TitlePositionX,
                Height = (int)(data.TitleFontSize*1.3),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(data.TitlePositionX, data.TitlePositionY),
                AutoEllipsis = true,
                //AutoSize = true,
            };
            Controls.Add(displayedSongTitle);

            displayedSongGame = new LabelOneTimeThing()
            {
                Name = Guid.NewGuid().ToString(),
                Text = "To begin song tracking:".ToUpper(),
                Font = new Font(data.GameFontName, data.GameFontSize, data.GameFontStyle),
                ForeColor = data.GameFontColor,
                Width = data.Width - data.GamePositionX,
                Height = (int)(data.GameFontSize * 1.3),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(data.GamePositionX, data.GamePositionY),
                AutoEllipsis = true,
                //AutoSize = true,
            };
            Controls.Add(displayedSongGame);




            isBroadcastable = data.isBroadcastable && !isOnBroadcast;

        }


        public void SetNames(string Game, string Title)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetNamesCallback(SetNames), new object[] { Game, Title });
                return;

            }
            else
            {
                displayedSongGame.Text = Game;
                displayedSongTitle.Text = Title;
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((NowPlayingPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetNames(Game, Title);
                }

            }

        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            switch (name)
            {
                case "Width":
                    this.Width += (mult * int.Parse(value.ToString()));
                    displayedSongTitle.Width = this.Width - displayedSongTitle.Location.X;
                    displayedSongGame.Width = this.Width - displayedSongGame.Location.X;
                    break;
                case "TitlePositionX":
                    displayedSongTitle.Location = new Point(displayedSongTitle.Location.X + (mult*int.Parse(value.ToString())), displayedSongTitle.Location.Y);
                    displayedSongTitle.Width = this.Width - displayedSongTitle.Location.X;
                    break;
                case "TitlePositionY":
                    displayedSongTitle.Location = new Point(displayedSongTitle.Location.X, displayedSongTitle.Location.Y + (mult * int.Parse(value.ToString())));
                    break;
                case "TitleFontName":
                    if (mult > 0) displayedSongTitle.Font = new Font(value.ToString(), displayedSongTitle.Font.Size, displayedSongTitle.Font.Style);
                    else displayedSongTitle.Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), displayedSongTitle.Font.Size, displayedSongTitle.Font.Style);
                    break;
                case "TitleFontSize":
                    displayedSongTitle.Font = new Font(displayedSongTitle.Font.Name, displayedSongTitle.Font.Size + (mult * int.Parse(value.ToString())), displayedSongTitle.Font.Style);
                    break;
                case "TitleFontStyle":
                    if (mult > 0) displayedSongTitle.Font = new Font(displayedSongTitle.Font.FontFamily, displayedSongTitle.Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
                    else displayedSongTitle.Font = new Font(displayedSongTitle.Font.FontFamily, displayedSongTitle.Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                case "TitleFontColor":
                    if (mult > 0) displayedSongTitle.ForeColor = Color.FromName(value.ToString());
                    else displayedSongTitle.ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "GamePositionX":
                    displayedSongGame.Location = new Point(displayedSongGame.Location.X + (mult * int.Parse(value.ToString())), displayedSongGame.Location.Y);
                    displayedSongGame.Width = this.Width - displayedSongGame.Location.X;
                    break;
                case "GamePositionY":
                    displayedSongGame.Location = new Point(displayedSongGame.Location.X, displayedSongGame.Location.Y + (mult * int.Parse(value.ToString())));
                    break;
                case "GameFontName":
                    if (mult > 0) displayedSongGame.Font = new Font(value.ToString(), displayedSongGame.Font.Size, displayedSongGame.Font.Style);
                    else displayedSongGame.Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), displayedSongGame.Font.Size, displayedSongGame.Font.Style);
                    break;
                case "GameFontSize":
                    displayedSongGame.Font = new Font(displayedSongGame.Font.Name, displayedSongGame.Font.Size + (mult * int.Parse(value.ToString())), displayedSongGame.Font.Style);
                    break;
                case "GameFontStyle":
                    if (mult > 0) displayedSongGame.Font = new Font(displayedSongGame.Font.FontFamily, displayedSongGame.Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
                    else displayedSongGame.Font = new Font(displayedSongGame.Font.FontFamily, displayedSongGame.Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                case "GameFontColor":
                    if (mult > 0) displayedSongGame.ForeColor = Color.FromName(value.ToString());
                    else displayedSongGame.ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                default:
                    throw new NotImplementedException($"Could not perform NowPlayingPanel Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates() { }

    }
}
