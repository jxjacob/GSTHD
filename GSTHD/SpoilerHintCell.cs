using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Activities.Expressions;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GSTHD
{
    public struct SpoilerCellState
    {
        public int totalPoints;
        public int currentPoints;

        public int totalWOTHS;
        public int currentWOTHS;

        public List<int> foundItems;
        public List<PotionTypes> potionsList;

        public override string ToString()
        {
            return $"{totalPoints},{currentPoints},{totalWOTHS},{currentWOTHS},{foundItems},{potionsList}";
        }
    }



    public class SpoilerCell : Panel, UpdatableFromSettings
    {
        Settings Settings;

        public string levelName;
        public List<int> foundItems = new List<int>();
        public List<PotionTypes> potionsList;
        public int levelOrder;
        public int levelID;

        private PictureBox levelNumberImage;
        private Item unknownLevelNumberImage;
        private PictureBox levelImage;

        private int totalPoints;
        public int currentPoints = 0;
        private Color pointColour;
        private Label pointLabel;

        private int totalWOTHS;
        public int currentWOTHS = 0;
        private Color wothColour;
        private Label wothLabel;

        private Color emptyColour;

        public bool ayoJetpac = false;
        public bool noPotions = false;

        public int topRowHeight;
        public int WorldNumWidth;
        public int WorldNumHeight;
        public int PotionWidth;
        public int PotionHeight;

        public bool MinimalMode = false;
        public bool isBroadcastable = false;

        public string[] levelList = { "japes", "aztec", "factory", "galleon", "forest", "caves", "castle", "helm", "isles" };

        delegate void UpdatePointsCallback();
        delegate void SetStateCallback(SpoilerCellState state);


        public SpoilerCell(Settings settings, int width, int height, int x, int y, int points, int woths, List<PotionTypes> potions, int topRowHeight, int WorldNumWidth, int WorldNumHeight, int PotionWidth, int PotionHeight, string name, string levelname, int levelnum, int levelorder, Color backColor, bool isMinimal, bool isBroadcastable=false)
        {
            // when getting created, get the spoiler numebrs from the parent panel
            Settings = settings;

            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);

            // just concerns defining the shape of the panel
            this.BackColor = backColor;
            this.Location = new Point(x, y);
            this.Name = name;
            this.Size = new Size(width, height);

            // if level order is given, make it the readable/base-1 version, otherwise just store it as -1
            this.levelOrder = (levelorder >= 0) ? levelorder + 1 : -1;
            // level num is basically the level ID (japes is 0, aztec is 1, etc)
            this.levelID = levelnum;
            this.levelName = levelname;
            this.totalPoints = points;
            this.totalWOTHS = woths;
            this.potionsList = potions;
            this.MinimalMode = isMinimal;

            this.topRowHeight = topRowHeight;
            this.WorldNumWidth = WorldNumWidth;
            this.WorldNumHeight = WorldNumHeight;
            this.PotionHeight = PotionHeight;
            this.PotionWidth = PotionWidth;

            this.isBroadcastable = isBroadcastable;

            if (totalPoints > 0) noPotions = true;

            int shownnumbers = 1;
            if (totalPoints >= 0)
            {
                pointLabel = new Label
                {
                    Name = Guid.NewGuid().ToString(),
                    Text = totalPoints.ToString(),
                    Font = new Font(new FontFamily("Calibri"), 9, FontStyle.Bold),
                    ForeColor = pointColour,
                    //BackColor = Color.Red,
                    Width = 20,
                    Height = WorldNumHeight,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(width - (shownnumbers * 15) - 2, -2)
                };
                shownnumbers++;
                if (totalPoints == 0) pointLabel.ForeColor = emptyColour;
                Controls.Add(pointLabel);
            }
            if (totalWOTHS >= 0)
            {
                wothLabel = new Label
                {
                    Name = Guid.NewGuid().ToString(),
                    Text = totalWOTHS.ToString(),
                    Font = new Font(new FontFamily("Calibri"), 9, FontStyle.Bold),
                    ForeColor = wothColour,
                    //BackColor = Color.Yellow,
                    Width = 15,
                    Height = WorldNumHeight,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(width - (shownnumbers * 15)-2, -2)
                };
                Controls.Add(wothLabel);
            }
            
            if (levelOrder > 0 && levelOrder < 9)
            {
                // put in the static image
                levelNumberImage = new PictureBox
                {
                    Image = Image.FromFile($"Resources/dk64/{levelOrder}.png"),
                    Width = WorldNumWidth,
                    Height = WorldNumHeight,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(0, 0),
                };
                Controls.Add(levelNumberImage);
            } else
            {
                // put in the item
            }

            levelImage = new PictureBox
            {
                Image = Image.FromFile($"Resources/dk64/{levelList[levelID]}.png"),
                Width = 58,
                Height = WorldNumHeight - 2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(18, 1),
            };
            Controls.Add(levelImage);

            Debug.WriteLine(levelOrder + " " + Name + " " + Location + " " + totalPoints + " " + totalWOTHS + " -- " + string.Join(", ", potionsList.ToArray()));
        }


        // then on the second row, it displays all the items that are in the order
            // THIS SECOND ROW IS GONNA HAVE TO BE A NEW PANEL CLASS SO IT CAN EASILY HAVE ITEMS DRAGGED INTO IT
            // can have items dragged into it for those who dont autotrack
            // ALSO gonna need to remove the old potions incase shit dies
            // also means that OG items need to pass on their dk64 code when dragged and just ignored by everything else
            // which sounds miserable lmao
            // ALSO gonna have to support vial hints and autoreplacing them with the correct item
            // essentially gonna maintain a poitions list and a moves list; when a move is being added, remove a corresponding potion from the potions list
            // when an item is added, it reduces the value by the specified points
            // will need to either dynamically resize or respace when theres too many potions
            // for the isles cell, i might try a workaround that hides your starting moves (update cycle 1), since that artificially inflates that sections potions count and fucks with visibility

        // also gonna need to define a new label version for the counts
        public void UpdateFromSettings()
        {
            // probably do something with being able to choose whether to display the icons
            // also settings for 3 colours: woth number, point number, and completed number
            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);
            UpdatePoints();
        }

        public void UpdatePoints()
        {

            if (this.InvokeRequired)
            {
                Invoke(new UpdatePointsCallback(UpdatePoints));
                return;
            }
            else
            {

                // set the points label
                if (totalPoints >= 0)
                {
                    pointLabel.Text = (totalPoints - currentPoints).ToString();
                    if (pointLabel.Text == "0") pointLabel.ForeColor = emptyColour;
                    else pointLabel.ForeColor = pointColour;
                    Debug.WriteLine($"Update to {levelName}: Points={pointLabel.Text}");
                }
                // also update WOTHS count, which dont decrement but do need to be fixed theres a crankyadd
                if (totalWOTHS >= 0)
                {
                    wothLabel.Text = totalWOTHS.ToString();
                    wothLabel.ForeColor = wothColour;
                }
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    if (((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(Name.Split('_')[0], true)[0]).spoilerLoaded)
                    {
                        ((SpoilerCell)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetState(GetState());
                    }
                }


            }

        }

        public void UpdatePotions()
        {
            // display all potions left, then display all aquired moves afterwards
        }

        public void AddNewItem(DK64_Item dk_id, int pointValue, bool isStarting)
        {
            Debug.WriteLine($"adding {dk_id.name} (ID: {dk_id.item_id}) to {this.levelName}");

            foundItems.Add(dk_id.item_id);
            //TODO: the part with the potions and the display
            // also if id = 8 and isstarting is true, make these incoming items invisible (if settings permit)
            if (pointValue >= 0)
            {
                currentPoints += pointValue;
                UpdatePoints();
            }
        }

        public void AddCrankys(int crPoints, int crWOTHS, List<PotionTypes> crPotions)
        {
            totalPoints += crPoints;
            totalWOTHS += crWOTHS;
            potionsList = potionsList.Concat(crPotions).ToList();
            if (crPoints > 0 || crPotions.Count > 0)
            //{
            //    ayoJetpac = true;
            //    // put in the static image
            //    levelNumberImage = new PictureBox
            //    {
            //        Image = Image.FromFile($"Resources/dk64/rwsprite.png"),
            //        Height = 18,
            //        Width = this.Height,
            //        SizeMode = PictureBoxSizeMode.Zoom,
            //        Location = new Point(-1, 0),
            //    };
            //    Controls.Add(levelNumberImage);
            //}
            UpdatePoints();
            UpdatePotions();
        }


        public SpoilerCellState GetState()
        {
            return new SpoilerCellState()
            {
                totalPoints = totalPoints,
                currentPoints = currentPoints,
                totalWOTHS = totalWOTHS,
                currentWOTHS = currentWOTHS,
                foundItems = foundItems,
                potionsList = potionsList,
        };
    }

        public void SetState(SpoilerCellState state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetStateCallback(SetState), new object[] { state });
                return;
            }
            else
            {
                Debug.WriteLine("setting state");
                totalPoints = state.totalPoints;
                currentPoints = state.currentPoints;
                totalWOTHS = state.totalWOTHS;
                currentWOTHS = state.currentWOTHS;
                foundItems = state.foundItems;
                potionsList = state.potionsList;
                UpdatePoints();
                UpdatePotions();
            }
        }
    }
}

