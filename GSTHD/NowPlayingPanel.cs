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
        private Label displayedSongGame;

        private Label displayedSongTitle;

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


            displayedSongTitle = new Label()
            {
                Name = Guid.NewGuid().ToString(),
                Text = "Please Autotrack a DK64R 4.0+ Seed".ToUpper(),
                Font = new Font(data.TitleFontName, data.TitleFontSize, data.TitleFontStyle),
                ForeColor = data.TitleFontColor,
                Width = data.Width,
                Height = (int)(data.TitleFontSize*1.3),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(data.TitlePositionX, data.TitlePositionY),
                AutoEllipsis = true,
                //AutoSize = true,
            };
            Controls.Add(displayedSongTitle);

            displayedSongGame = new Label()
            {
                Name = Guid.NewGuid().ToString(),
                Text = "To begin song tracking".ToUpper(),
                Font = new Font(data.GameFontName, data.GameFontSize, data.GameFontStyle),
                ForeColor = data.GameFontColor,
                Width = data.Width,
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
            var point = (ObjectPoint)ogPoint;
            switch (name)
            {
                case "":
                    break;
                default:
                    throw new NotImplementedException($"Could not perform NowPlayingPanel Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }
    }

    
}
