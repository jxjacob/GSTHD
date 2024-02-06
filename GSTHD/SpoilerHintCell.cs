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
using System.Text.RegularExpressions;

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

        public List<CellDisplay> displayList;

        public override string ToString()
        {
            return $"{totalPoints},{currentPoints},{totalWOTHS},{currentWOTHS},{foundItems},{potionsList},{displayList}";
        }
    }

    public class CellPictureBox : PictureBox, ProgressibleElement<int>
    {
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;

        private readonly SpoilerCell hostCell;

        private readonly int dk_id;

        public CellPictureBox(Settings settings, SpoilerCell hostCell, int dk_id)
        {
            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, settings);
            MouseDown += ProgressBehaviour.Mouse_ClickDown;
            this.hostCell = hostCell;
            this.dk_id = dk_id;
            this.dk_id = dk_id;
        }

        public void IncrementState()
        {
            // purposely empty
        }

        public void DecrementState()
        {
            // purposely empty
        }

        public void ResetState()
        {
            if (dk_id != -1)
            {
                // ping the host cell to get it to remove itself
                hostCell.RemoveItem(dk_id);
            }
        }
    }

    public class CellDisplay
    {
        public int potionType;
        public int item_id;
        public bool isStarting;
    }

    public class SpoilerCell : Panel, UpdatableFromSettings
    {
        Settings Settings;

        public string levelName;
        public int levelOrder;
        public int levelID;

        private Dictionary<string, int> pointspread;
        public Dictionary<int, DK64_Item> DK64Items;

        public List<int> foundItems = new List<int>();
        public List<PotionTypes> potionsList;
        public List<CellDisplay> displayList;
        public List<CellPictureBox> displayedPotions = new List<CellPictureBox>();

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

        public string fontName;
        public int fontSize;
        public FontStyle fontStyle;
        public int labelSpacing;
        public int labelWidth;

        public bool ayoJetpac = false;
        public bool noPotions = false;

        public int topRowHeight;
        public int topRowPadding;
        public int bottomRowHeight;
        public int WorldNumWidth;
        public int WorldNumHeight;
        public int PotionWidth;
        public int PotionHeight;

        public bool MinimalMode = false;
        public bool isBroadcastable = false;

        public string[] levelList = { "japes", "aztec", "factory", "galleon", "forest", "caves", "castle", "helm", "isles" };
        public string[] potionImageList = { "dk64/potion_shared.png", "dk64/potion_dk.png", "dk64/potion_diddy.png", "dk64/potion_lanky.png", "dk64/potion_tiny.png", "dk64/potion_chunky.png", "dk64/ButWhereWasDK.png", "dk64/key_unknown.png" };

        delegate void UpdatePointsCallback();
        delegate void UpdatePotionsCallback();
        delegate void SetStateCallback(SpoilerCellState state);


        public SpoilerCell(Settings settings, int width, int height, int x, int y, int points, int woths, List<PotionTypes> potions, int topRowHeight, int topRowPadding, int WorldNumWidth, int WorldNumHeight, int PotionWidth, int PotionHeight, string name, string levelname, int levelnum, int levelorder, string cellFontName, int cellFontSize, FontStyle cellFontStyle, int labelSpacing, int labelWidth, Color backColor, bool isMinimal, Dictionary<string, int> spread, Dictionary<int, DK64_Item> dkitems, bool isBroadcastable=false)
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
            potionsList.Sort();
            this.pointspread = spread;
            this.DK64Items = dkitems;
            this.MinimalMode = isMinimal;

            this.topRowHeight = topRowHeight;
            // final cell doesnt need padding for a key that doesnt exist
            Debug.WriteLine($"lo: {levelOrder}");
            this.topRowPadding = (levelOrder == 9 && !MinimalMode) ? 0 : topRowPadding;
            this.bottomRowHeight = height - topRowHeight;
            this.PotionHeight = PotionHeight;
            this.PotionWidth = PotionWidth;
            this.WorldNumWidth = WorldNumWidth;
            this.WorldNumHeight = WorldNumHeight;
            this.fontName = cellFontName;
            this.fontSize = cellFontSize;
            this.fontStyle = cellFontStyle;
            this.labelSpacing = labelSpacing;
            this.labelWidth = labelWidth;

            this.isBroadcastable = isBroadcastable;

            this.DragEnter += Mouse_DragEnter;
            this.DragDrop += Mouse_DragDrop;
            this.AllowDrop = true;

            if (totalPoints >= 0) noPotions = true;

            int shownnumbers = 1;
            if (totalPoints >= 0)
            {
                pointLabel = new Label
                {
                    Name = Guid.NewGuid().ToString(),
                    Text = totalPoints.ToString(),
                    Font = new Font(new FontFamily(fontName), fontSize, fontStyle),
                    ForeColor = pointColour,
                    //BackColor = Color.Red,
                    Width = labelWidth,
                    Height = WorldNumHeight,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Right,
                    Location = new Point(width - (shownnumbers * labelSpacing) - 2 - this.topRowPadding, -1)
                };
                Debug.WriteLine($"points {pointLabel.Width}");
                shownnumbers++;
                if (totalPoints == 0) pointLabel.ForeColor = emptyColour;
                Controls.Add(pointLabel);
            }
            if (totalWOTHS >= 0)
            {
                Debug.WriteLine("adding woths");
                wothLabel = new Label
                {
                    Name = Guid.NewGuid().ToString(),
                    Text = totalWOTHS.ToString(),
                    Font = new Font(new FontFamily(fontName), fontSize, fontStyle),
                    ForeColor = wothColour,
                    //BackColor = Color.Yellow,
                    Width = labelWidth,
                    Height = WorldNumHeight,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(width - (shownnumbers * labelSpacing) - 2 - this.topRowPadding, -1)
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
            } else if (levelOrder < 0)
            {
                // put in the item
                ObjectPoint temp = new ObjectPoint()
                {
                    Name = $"{name}_unknownLevel",
                    X = 0, Y = 0,
                    Size = new Size(WorldNumWidth, WorldNumHeight),
                    ImageCollection = new string[] { "dk64/unknownnum.png", "dk64/1.png", "dk64/2.png", "dk64/3.png", "dk64/4.png", "dk64/5.png", "dk64/6.png", "dk64/7.png" },
                    isBroadcastable = true,
                };
                unknownLevelNumberImage = new Item(temp, settings);
                Controls.Add(unknownLevelNumberImage);
            }

            levelImage = new PictureBox
            {
                Image = Image.FromFile($"Resources/dk64/{levelList[levelID]}.png"),
                Width = 58,
                Height = WorldNumHeight - 2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((!MinimalMode && levelOrder == 9) ? -6 : 18, 1),
            };
            Controls.Add(levelImage);

            InitializeDisplayList();
            UpdatePotions();
            UpdatePoints();

            //Debug.WriteLine(levelOrder + " " + Name + " " + Location + " " + totalPoints + " " + totalWOTHS + " -- " + string.Join(", ", potionsList.ToArray()));
        }

        private void Mouse_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void Mouse_DragDrop(object sender, DragEventArgs e)
        {
            // TODO: makes sure the thign being dragged into is an actual item
            try
            {
                DragDropContent dropContent = (DragDropContent)e.Data.GetData(typeof(DragDropContent));

                // ignore if its not a real item
                if (dropContent.dk_id != -1)
                {
                    // lookup the actual dk item
                    DK64_Item item = DK64Items[dropContent.dk_id];
                    int sentPoints = (noPotions) ? pointspread[item.itemType] : -2;
                    //Debug.WriteLine($"{item}, {sentPoints}");
                    AddNewItem(item, sentPoints, false, 1);
                }
            } catch { }

        }


        // also gonna need to define a new label version for the counts
        public void UpdateFromSettings()
        {
            // probably do something with being able to choose whether to display the icons
            // also settings for 3 colours: woth number, point number, and completed number
            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);
            //Debug.WriteLine($"{isBroadcastable} - {emptyColour}");
            UpdatePotions();
            UpdatePoints();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((SpoilerCell)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).UpdateFromSettings();
            }

        }

        public void InitializeDisplayList()
        {
            displayList = new List<CellDisplay>();
            if (!noPotions)
            {
                foreach (PotionTypes pot in potionsList)
                {
                    displayList.Add(new CellDisplay { potionType = (int)pot, isStarting = false, item_id = -1 });
                }
            }
        }

        public void AddToDisplayList(DK64_Item item, bool starting = false)
        {
            for (int i = 0; i < displayList.Count; i++)
            {
                if (displayList[i].potionType == (int)item.potionType && displayList[i].item_id == -1)
                {
                    displayList[i].item_id = item.item_id;
                    displayList[i].isStarting = starting;
                    return;
                }
            }
            displayList.Add(new CellDisplay { potionType = -1, isStarting = starting, item_id = item.item_id });
        }

        public void RemoveFromDisplayList(int id)
        {
            for (int i = displayList.Count-1; i >= 0; i--)
            {
                if (displayList[i].item_id == id)
                {
                    if (displayList[i].potionType != -1)
                    {
                        displayList[i].item_id = -1;
                        displayList[i].isStarting = false;
                        //Debug.WriteLine($"wiped {id}");
                        return;
                    } else
                    {
                        displayList.RemoveAt(i);
                        //Debug.WriteLine($"removed {id}"); 
                        return;
                    }
                }
            }
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
                    //Debug.WriteLine($"Update to {levelName}: Points={pointLabel.Text}");
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
            if (this.InvokeRequired)
            {
                Invoke(new UpdatePotionsCallback(UpdatePotions));
                return;
            }
            else
            {
                if (!MinimalMode)
                {
                    for (int i = 0; i < displayedPotions.Count; i++) {
                        displayedPotions[i].Dispose();
                    }
                    displayedPotions.Clear();
                    // also oging to need to account for the starting moves being potentially hidden
                    int thingstodisplay = displayList.Count;
                    foreach (var thing in displayList)
                    {
                        if (thing.isStarting && Settings.HideStarting)
                        {
                            thingstodisplay--;
                        }
                    }

                    int usedPotWidth = PotionWidth;
                    int usedPotHeight = PotionHeight;
                    // used to determine when to add a new row
                    int displayablePotsWidth = 0;

                    // resizes if neccesary
                    while (true)
                    {
                        displayablePotsWidth = (Width / usedPotWidth);
                        int displayablePotsHeight = (bottomRowHeight / usedPotHeight);
                        if (thingstodisplay > displayablePotsWidth)
                        {
                            int neededrows = (int)System.Math.Ceiling((double)thingstodisplay/(double)displayablePotsWidth);
                            if (neededrows > displayablePotsHeight)
                            {
                                Debug.WriteLine("needs resizing");
                                int newpotwidth = Width / (displayablePotsWidth + 1);
                                double ratio = (double)usedPotWidth / (double)newpotwidth;
                                usedPotWidth = newpotwidth;
                                usedPotHeight = (int)System.Math.Floor((double)usedPotHeight / ratio);
                                Debug.WriteLine($"{bottomRowHeight} - {usedPotWidth} {usedPotHeight}");
                            } else
                            {
                                break;
                            }
                        } else { break; }
                    }

                    int yOffset = 0;
                    // if everything can fit on one line, centre it
                    if (thingstodisplay <= displayablePotsWidth)
                    {
                        yOffset = (bottomRowHeight-usedPotHeight) / 2;
                    } 

                    int thingsdisplayed = 0;
                    foreach (CellDisplay pot in displayList)
                    {
                        if (pot.isStarting && Settings.HideStarting) continue;

                        int newX = (thingsdisplayed % displayablePotsWidth)*usedPotWidth;
                        int newY = (thingsdisplayed / displayablePotsWidth)*usedPotHeight + topRowHeight + yOffset;

                        string toDisplay = (pot.item_id != -1) ? DK64Items[pot.item_id].image : potionImageList[(int)pot.potionType];
                        Debug.WriteLineIf((pot.item_id != -1), $"todisplay = {toDisplay}");

                        CellPictureBox newPot = new CellPictureBox(Settings, this, pot.item_id)
                        {
                            Size = new Size(usedPotWidth, usedPotHeight),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Location = new Point(newX, newY),
                            Image = Image.FromFile(@"Resources/" + toDisplay)
                        };

                        //Debug.WriteLine($"{thingsdisplayed}:   x:{newX} y:{newY} w:{newPot.Width} h:{newPot.Height}");
                        // probably add pot to a displaylist
                        displayedPotions.Add(newPot);
                        Controls.Add(newPot);
                        thingsdisplayed++;
                    }

                    // no need to do a broadcast view check, as updatepoints does a setstate
                }

            }
        }

        public void AddNewItem(DK64_Item dk_id, int pointValue, bool isStarting, int howMany)
        {
            
            for (int i = 0; i < howMany; i++)
            {
                foundItems.Add(dk_id.item_id);
                // also if id = 8 and isstarting is true, make these incoming items invisible (if settings permit)
                if (pointValue != -1) currentPoints += pointValue;
                AddToDisplayList(dk_id, isStarting);
                UpdatePotions();
                UpdatePoints();
            }
            
        }

        public void RemoveItem(int dk_id)
        {
            DK64_Item temp = DK64Items[dk_id];
            currentPoints -= (noPotions) ? pointspread[temp.itemType] : 0;

            RemoveFromDisplayList(dk_id);
            UpdatePotions();
            UpdatePoints();
        }

        public void AddCrankys(int crPoints, int crWOTHS, List<PotionTypes> crPotions)
        {
            totalPoints += crPoints;
            totalWOTHS += crWOTHS;
            potionsList = potionsList.Concat(crPotions).ToList();
            potionsList.Sort();
            if (crPoints > 0 || crPotions.Count > 0) { }
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
                displayList = displayList,
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
                //Debug.WriteLine("setting state");
                totalPoints = state.totalPoints;
                currentPoints = state.currentPoints;
                totalWOTHS = state.totalWOTHS;
                currentWOTHS = state.currentWOTHS;
                foundItems = state.foundItems;
                potionsList = state.potionsList;
                displayList = state.displayList;
                UpdatePoints();
                UpdatePotions();
            }
        }
    }
}

