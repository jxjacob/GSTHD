using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace GSTHD
{
    class NowPlayingPanel : Canvas, IAlternatableObject
    {
        private LabelOneTimeThing displayedSongGame;

        private LabelOneTimeThing displayedSongTitle;

        private bool isBroadcastable = false;

        delegate void SetNamesCallback(string game, string title);

        public NowPlayingPanel(ObjectPanelNowPlaying data, bool isOnBroadcast = false)
        {
            Name = data.Name;
            SetTop(this, data.Y);
            SetLeft(this, data.X);
            IsTabStop = false;
            IsVisible = data.Visible;
            Width = data.Width;
            Height = data.Height;
            Background = new SolidColorBrush(new Color(data.BackColor.A, data.BackColor.R, data.BackColor.G, data.BackColor.B));


            displayedSongTitle = new LabelOneTimeThing()
            {
                Name = Guid.NewGuid().ToString(),
                Text = "Please Autotrack a DK64R 4.0+ Seed".ToUpper(),
                FontFamily = data.TitleFontName,
                FontSize = data.TitleFontSize * 1.4,
                Foreground = new SolidColorBrush(new Color(data.TitleFontColor.A, data.TitleFontColor.R,
                    data.TitleFontColor.G, data.TitleFontColor.B)),
                Width = data.Width - data.TitlePositionX,
                Height = (int)(data.TitleFontSize * 1.3),
                TextAlignment = LegacyConverters.ConvetTA(ContentAlignment.MiddleLeft),
                TextTrimming = TextTrimming.CharacterEllipsis,
                //AutoSize = true,
            };
            SetTop(displayedSongTitle, data.TitlePositionY+3);
            SetLeft(displayedSongTitle, data.TitlePositionX+3);
            FontStyleFix(displayedSongTitle, data.TitleFontStyle);
            Children.Add(displayedSongTitle);

            displayedSongGame = new LabelOneTimeThing()
            {
                Name = Guid.NewGuid().ToString(),
                Text = "To begin song tracking:".ToUpper(),
                FontFamily = data.GameFontName,
                FontSize = data.GameFontSize * 1.4,
                Foreground = new SolidColorBrush(new Color(data.GameFontColor.A, data.GameFontColor.R,
                    data.GameFontColor.G, data.GameFontColor.B)),
                Width = data.Width - data.GamePositionX,
                Height = (int)(data.GameFontSize * 1.3),
                TextAlignment = LegacyConverters.ConvetTA(ContentAlignment.MiddleLeft),
                TextTrimming = TextTrimming.CharacterEllipsis,
                //AutoSize = true,
            };
            SetTop(displayedSongGame, data.GamePositionY+3);
            SetLeft(displayedSongGame, data.GamePositionX+3);
            FontStyleFix(displayedSongGame, data.GameFontStyle);
            Children.Add(displayedSongGame);




            isBroadcastable = data.isBroadcastable && !isOnBroadcast;

        }


        public void SetNames(string Game, string Title)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetNames(Game, Title));
            }
            else
            {
                displayedSongGame.Text = Game;
                displayedSongTitle.Text = Title;
                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    ((NowPlayingPanel)BroadcastManager.GetControl(Name)!).SetNames(Game, Title);
                }

            }

        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }
        
        private void FontStyleFix(LabelOneTimeThing label, FontStyle style)
        {
            // sucks that this is manual but what you gonna do?
            switch (style)
            {
                case FontStyle.Bold:
                    label.FontWeight = FontWeight.Bold;
                    label.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
                case FontStyle.Italic:
                    label.FontWeight = FontWeight.Normal;
                    label.FontStyle = Avalonia.Media.FontStyle.Italic;
                    break;
                case FontStyle.Regular:
                    label.FontWeight = FontWeight.Normal;
                    label.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
            }
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            // switch (name)
            // {
            //     case "Width":
            //         this.Width += (mult * int.Parse(value.ToString()));
            //         displayedSongTitle.Width = this.Width - displayedSongTitle.Location.X;
            //         displayedSongGame.Width = this.Width - displayedSongGame.Location.X;
            //         break;
            //     case "TitlePositionX":
            //         displayedSongTitle.Location = new Point(displayedSongTitle.Location.X + (mult*int.Parse(value.ToString())), displayedSongTitle.Location.Y);
            //         displayedSongTitle.Width = this.Width - displayedSongTitle.Location.X;
            //         break;
            //     case "TitlePositionY":
            //         displayedSongTitle.Location = new Point(displayedSongTitle.Location.X, displayedSongTitle.Location.Y + (mult * int.Parse(value.ToString())));
            //         break;
            //     case "TitleFontName":
            //         if (mult > 0) displayedSongTitle.Font = new Font(value.ToString(), displayedSongTitle.Font.Size, displayedSongTitle.Font.Style);
            //         else displayedSongTitle.Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), displayedSongTitle.Font.Size, displayedSongTitle.Font.Style);
            //         break;
            //     case "TitleFontSize":
            //         displayedSongTitle.Font = new Font(displayedSongTitle.Font.Name, displayedSongTitle.Font.Size + (mult * int.Parse(value.ToString())), displayedSongTitle.Font.Style);
            //         break;
            //     case "TitleFontStyle":
            //         if (mult > 0) displayedSongTitle.Font = new Font(displayedSongTitle.Font.FontFamily, displayedSongTitle.Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
            //         else displayedSongTitle.Font = new Font(displayedSongTitle.Font.FontFamily, displayedSongTitle.Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
            //         break;
            //     case "TitleFontColor":
            //         if (mult > 0) displayedSongTitle.ForeColor = Color.FromName(value.ToString());
            //         else displayedSongTitle.ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //     case "GamePositionX":
            //         displayedSongGame.Location = new Point(displayedSongGame.Location.X + (mult * int.Parse(value.ToString())), displayedSongGame.Location.Y);
            //         displayedSongGame.Width = this.Width - displayedSongGame.Location.X;
            //         break;
            //     case "GamePositionY":
            //         displayedSongGame.Location = new Point(displayedSongGame.Location.X, displayedSongGame.Location.Y + (mult * int.Parse(value.ToString())));
            //         break;
            //     case "GameFontName":
            //         if (mult > 0) displayedSongGame.Font = new Font(value.ToString(), displayedSongGame.Font.Size, displayedSongGame.Font.Style);
            //         else displayedSongGame.Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), displayedSongGame.Font.Size, displayedSongGame.Font.Style);
            //         break;
            //     case "GameFontSize":
            //         displayedSongGame.Font = new Font(displayedSongGame.Font.Name, displayedSongGame.Font.Size + (mult * int.Parse(value.ToString())), displayedSongGame.Font.Style);
            //         break;
            //     case "GameFontStyle":
            //         if (mult > 0) displayedSongGame.Font = new Font(displayedSongGame.Font.FontFamily, displayedSongGame.Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
            //         else displayedSongGame.Font = new Font(displayedSongGame.Font.FontFamily, displayedSongGame.Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
            //         break;
            //     case "GameFontColor":
            //         if (mult > 0) displayedSongGame.ForeColor = Color.FromName(value.ToString());
            //         else displayedSongGame.ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //     default:
            //         throw new NotImplementedException($"Could not perform NowPlayingPanel Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            // }
        }

        public void ConfirmAlternates() { }

    }
}
