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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace GSTHD
{
    public struct SpoilerCellState
    {
        public int totalPoints;
        public int currentPoints;

        public int totalWOTHS;
        public int currentWOTHS;

        public bool levelNumMarked;
        public int levelNumIndex;
        public bool levelLabelMarked;

        public List<int> foundItems;
        public List<PotionTypes> potionsList;

        public List<CellDisplay> displayList;

        public override string ToString()
        {
            string itemstring = "";
            foreach (int item in foundItems)
            {
                if (itemstring.Length > 0)
                {
                    itemstring += ",";
                }
                itemstring += item.ToString();
            }

            string displaystring = "";
            foreach (CellDisplay item in displayList)
            {
                if (displaystring.Length > 0)
                {
                    displaystring += ",";
                }
                displaystring += item.ToString();
            }
            return $"{currentPoints},{currentWOTHS},{levelNumIndex},{levelNumMarked},{levelLabelMarked}\n{itemstring}\n{displaystring}";
        }
    }

    public class CellPictureBox : OrganicImage, ProgressibleElement<int>
    {
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;

        public SpoilerCell hostCell;

        public int dk_id;


        public CellPictureBox(Settings settings, bool isOnBroadcast)
        {
            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, settings);
            if (!isOnBroadcast) MouseDown += ProgressBehaviour.Mouse_ClickDown;
        }

        public void IncrementState()
        {
            if (dk_id>=0) ToggleFade();
        }

        public void DecrementState()
        {
            if (dk_id >= 0) ToggleFade();
        }

        public void ResetState()
        {
            if (dk_id != -1)
            {
                // ping the host cell to get it to remove itself
                hostCell.RemoveItem(dk_id);
                this.Dispose();
            }
        }

        public void ToggleCheck()
        {
            isMarked = !isMarked;
            hostCell.TellMarked(dk_id, isMarked);
            Invalidate();
        }

        public void ToggleFade()
        {
            if (isFaded)
            {
                // restore fadedness
                isFaded = false;
                Invalidate();
                hostCell.TellFaded(dk_id, isFaded);
            } else
            {
                // make faded
                isFaded = true;
                Invalidate();
                hostCell.TellFaded(dk_id, isFaded);
            }
        }
    }

    public class CellDisplay
    {
        public int potionType;
        public int item_id;
        public bool isStarting;
        public bool isFaded = false;
        public bool isMarked = false;

        public override string ToString()
        {
            return $"{potionType}\t{item_id}\t{isStarting}\t{isFaded}\t{isMarked}";
        }
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

        private GuaranteedHint levelNumberImage;
        private Item unknownLevelNumberImage;
        private GuaranteedHint levelImage;

        private int totalPoints;
        public int currentPoints = 0;
        private Color pointColour;
        private Label pointLabel;

        private int totalWOTHS;
        public int currentWOTHS = 0;
        private Color wothColour;
        private Label wothLabel;

        private Color emptyColour;
        private Color kindaEmptyColour;

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
        public int WorldLabelWidth;
        public int PotionWidth;
        public int PotionHeight;

        public bool MinimalMode = false;
        public bool isBroadcastable = false;
        private bool isOnBroadcast = false;

        public string[] levelList = { "japes", "aztec", "factory", "galleon", "forest", "caves", "castle", "helm", "isles" };
        public string[] potionImageList = { "dk64/potion_shared.png", "dk64/potion_dk.png", "dk64/potion_diddy.png", "dk64/potion_lanky.png", "dk64/potion_tiny.png", "dk64/potion_chunky.png", "dk64/ButWhereWasDK.png", "dk64/key_unknown.png" };

        delegate void UpdatePointsCallback();
        delegate void UpdatePotionsCallback();
        delegate void SetStateCallback(SpoilerCellState state);


        public SpoilerCell(Settings settings, int width, int height, int x, int y, int points, int woths, List<PotionTypes> potions, int topRowHeight, int topRowPadding, int WorldNumWidth, int WorldNumHeight, int WorldLabelWidth, int PotionWidth, int PotionHeight, string name, string levelname, int levelnum, int levelorder, string cellFontName, int cellFontSize, FontStyle cellFontStyle, int labelSpacing, int labelWidth, Color backColor, bool isMinimal, Dictionary<string, int> spread, Dictionary<int, DK64_Item> dkitems, bool isBroadcastable=false, bool isOnBroadcast=false)
        {
            // when getting created, get the spoiler numebrs from the parent panel
            Settings = settings;

            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);
            kindaEmptyColour = Color.FromKnownColor(Settings.SpoilerKindaEmptyColour);

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
            this.topRowPadding = (levelOrder == 9 && !MinimalMode) ? 0 : topRowPadding;
            this.bottomRowHeight = height - topRowHeight;
            this.PotionHeight = PotionHeight;
            this.PotionWidth = PotionWidth;
            this.WorldNumWidth = WorldNumWidth;
            this.WorldNumHeight = WorldNumHeight;
            this.WorldLabelWidth = WorldLabelWidth;
            this.fontName = cellFontName;
            this.fontSize = cellFontSize;
            this.fontStyle = cellFontStyle;
            this.labelSpacing = labelSpacing;
            this.labelWidth = labelWidth;

            this.isBroadcastable = isBroadcastable && !isOnBroadcast;
            this.isOnBroadcast = isOnBroadcast;

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
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Right,
                    Location = new Point(width, -1)
                };
                shownnumbers++;
                Controls.Add(pointLabel);
            }
            if (totalWOTHS >= 0)
            {
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
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                    Location = new Point(width, -1)
                };
                Controls.Add(wothLabel);
            }
            
            if (levelOrder > 0 && levelOrder < 9)
            {
                // put in the static image
                ObjectPoint temp1 = new ObjectPoint()
                {
                    Name = $"{name}_levelImageNumber",
                    ImageCollection = new string[] { $"dk64/{levelOrder}.png" },
                    Size = new Size(WorldNumWidth, WorldNumHeight),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    X = 0,
                    Y = 0,
                    Visible = true,
                    isBroadcastable = this.isBroadcastable
                };
                levelNumberImage = new GuaranteedHint(temp1, settings, isOnBroadcast);
                Controls.Add(levelNumberImage);
            } else if (levelOrder < 0)
            {
                // put in the item
                ObjectPoint temp2 = new ObjectPoint()
                {
                    Name = $"{name}_unknownLevel",
                    X = 0, Y = 0,
                    Visible = true,
                    Size = new Size(WorldNumWidth, WorldNumHeight),
                    ImageCollection = new string[] { "dk64/unknownnum.png", "dk64/1.png", "dk64/2.png", "dk64/3.png", "dk64/4.png", "dk64/5.png", "dk64/6.png", "dk64/7.png" },
                    isBroadcastable = this.isBroadcastable,
                };
                unknownLevelNumberImage = new Item(temp2, settings);
                Controls.Add(unknownLevelNumberImage);
            }

            ObjectPoint temp3 = new ObjectPoint()
            {
                Name = $"{name}_levelImageName",
                ImageCollection = new string[] { $"dk64/{levelList[levelID]}.png" },
                Size = new Size(WorldLabelWidth, WorldNumHeight - 2),
                SizeMode = PictureBoxSizeMode.Zoom,
                X = (!MinimalMode && levelOrder == 9) ? -6 : WorldNumWidth,
                Y = 1,
                Visible = true,
                isBroadcastable = this.isBroadcastable,
            };
            levelImage = new GuaranteedHint(temp3, settings, isOnBroadcast);
            Controls.Add(levelImage);

            InitializeDisplayList();
            UpdateVisuals();

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
                    //Debug.WriteLine($"{item}, {dropContent.IsAutocheck}");
                    AddNewItem(item, sentPoints, false, 1, !dropContent.IsAutocheck, dropContent.isMarked);
                }
            } catch { }

        }


        // also gonna need to define a new label version for the counts
        public void UpdateFromSettings()
        {
            // probably do something with being able to choose whether to display the icons
            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);
            kindaEmptyColour = Color.FromKnownColor(Settings.SpoilerKindaEmptyColour);
            UpdateVisuals();
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
                    displayList.Add(new CellDisplay { potionType = (int)pot, isStarting = false, item_id = -1, isFaded = false });
                }
            }
        }

        public bool AddToDisplayList(DK64_Item item, bool starting, bool faded, bool marked)
        {
            for (int i = 0; i < displayList.Count; i++)
            {
                if (displayList[i].potionType == (int)item.potionType && displayList[i].item_id == -1)
                {
                    // new move into existing potion
                    displayList[i].item_id = item.item_id;
                    displayList[i].isStarting = starting;
                    displayList[i].isFaded = faded;
                    displayList[i].isMarked = marked;
                    return false;
                } else if ((displayList[i].potionType == (int)item.potionType || displayList[i].potionType == -1) && displayList[i].item_id == item.item_id && displayList[i].isFaded && !faded)
                {
                    // new move into existing faded slot (dupe preventing)
                    displayList[i].isFaded = false;
                    displayList[i].isMarked = marked;
                    return false;
                }
            }
            displayList.Add(new CellDisplay { potionType = -1, isStarting = starting, item_id = item.item_id, isFaded = faded, isMarked = marked});
            return true;
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
                        displayList[i].isFaded = false;
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

        public void TellFaded(int dk_id, bool isFaded)
        {
            foreach (CellDisplay display in displayList)
            {
                if (display.item_id == dk_id && display.isFaded == !isFaded)
                {
                    display.isFaded = isFaded;
                    UpdateVisuals();
                    break;
                }
            }
        }

        public void TellMarked(int dk_id, bool isMarked)
        {
            foreach (CellDisplay display in displayList)
            {
                if (display.item_id == dk_id && display.isMarked == !isMarked)
                {
                    display.isMarked = isMarked;
                    UpdateVisuals();
                    break;
                }
            }
        }

        private void UpdatePoints()
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
                    if (pointLabel.Text == "0" && !isThereAnyFaded()) { pointLabel.ForeColor = emptyColour; }
                    else if (pointLabel.Text == "0") { pointLabel.ForeColor = kindaEmptyColour; }
                    else pointLabel.ForeColor = pointColour;
                    //Debug.WriteLine($"Update to {levelName}: Points={pointLabel.Text}");
                }
                // also update WOTHS count, which dont decrement but do need to be fixed theres a crankyadd
                if (totalWOTHS >= 0)
                {
                    wothLabel.Text = totalWOTHS.ToString();
                    wothLabel.ForeColor = wothColour;
                }

                AdjustPointLocations();

                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    if (((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(Name.Split('_')[0], true)[0]).spoilerLoaded)
                    {
                        ((SpoilerCell)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).SetState(GetState());
                    }
                }


            }

        }

        private bool isThereAnyFaded()
        {
            foreach (CellDisplay thing in displayList)
            {
                if (thing.isFaded) return true;
            }
            return false;
        }

        private void AdjustPointLocations()
        {
            //Debug.WriteLine($"{this.Name}");
            int pointWidth = 0;
            if (pointLabel != null)
            {
                int pointMeasure = TextRenderer.MeasureText(pointLabel.Text, pointLabel.Font).Width;
                pointLabel.Width = System.Math.Max(labelWidth, pointMeasure);
                pointWidth = pointLabel.Width;
                pointLabel.Location = new Point(this.Size.Width - (System.Math.Max(labelSpacing, pointWidth)) - 1 - this.topRowPadding, -1);
                //Debug.WriteLine($"point -- loc: {pointLabel.Location}   width: {pointLabel.Width}    pm: {pointMeasure}    ls: {labelSpacing}");
            }

            if (wothLabel != null)
            {
                int pointVis = (pointLabel != null) ? 1 : 0;
                wothLabel.Width = System.Math.Max(labelWidth, TextRenderer.MeasureText(wothLabel.Text, wothLabel.Font).Width);
                wothLabel.Location = new Point(this.Size.Width - (labelSpacing) - pointVis*(pointWidth) - 1 - this.topRowPadding, -1);
                //Debug.WriteLine($"woth -- loc: {wothLabel.Location}   width: {wothLabel.Width}");
            }
        }

        private void UpdatePotions()
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

                    displayList = displayList.OrderByDescending(i => i.item_id >= 0).ThenBy(i => i.potionType).ToList();

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
                                //Debug.WriteLine("needs resizing");
                                int newpotwidth = Width / (displayablePotsWidth + 1);
                                double ratio = (double)usedPotWidth / (double)newpotwidth;
                                usedPotWidth = newpotwidth;
                                usedPotHeight = (int)System.Math.Floor((double)usedPotHeight / ratio);
                                //Debug.WriteLine($"{bottomRowHeight} - {usedPotWidth} {usedPotHeight}");
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
                        //Debug.WriteLineIf((pot.item_id != -1), $"todisplay = {toDisplay}");

                        CellPictureBox newPot = new CellPictureBox(Settings, isOnBroadcast)
                        {
                            Size = new Size(usedPotWidth, usedPotHeight),
                            //SizeMode = PictureBoxSizeMode.Zoom,
                            Location = new Point(newX, newY),
                            Image = Image.FromFile(@"Resources/" + toDisplay),
                            hostCell = this,
                            dk_id = pot.item_id,
                            isFaded = pot.isFaded,
                            BackColor = this.BackColor,
                            isMarked = pot.isMarked,
                        };

                        //Debug.WriteLine($"{thingsdisplayed}:   x:{newX} y:{newY} w:{newPot.Width} h:{newPot.Height}");
                        displayedPotions.Add(newPot);
                        Controls.Add(newPot);
                        thingsdisplayed++;
                    }

                    // no need to do a broadcast view check, as updatepoints does a setstate
                }

            }
        }

        public void AddNewItem(DK64_Item dk_id, int pointValue, bool isStarting, int howMany, bool isFaded = false, bool isMarked = false)
        {
            
            for (int i = 0; i < howMany; i++)
            {
                if (!isFaded) foundItems.Add(dk_id.item_id);
                bool result = AddToDisplayList(dk_id, isStarting, isFaded, isMarked && !Settings.CellOverrideCheckMark);
                if (result && pointValue != -1) currentPoints += pointValue;
                UpdateVisuals();
            }
            
        }

        public void RemoveItem(int dk_id)
        {
            DK64_Item temp = DK64Items[dk_id];
            currentPoints -= (noPotions) ? pointspread[temp.itemType] : 0;

            RemoveFromDisplayList(dk_id);
            UpdateVisuals();
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
            UpdateVisuals();
        }

        public void RefreshVisuals(int cellWidth, int cellHeight,
                        int topRowHeight, int topRowPadding,
                        int WorldNumWidth, int WorldNumHeight, int WorldLabelWidth,
                        int PotionWidth, int PotionHeight,
                        string CellFontName, int CellFontSize, FontStyle CellFontStyle,
                        int CellLabelSpacing, int CellLabelWidth, Color CellBackColor,
                        bool MinimalMode)
        {
            this.Width = cellWidth;
            this.Height = cellHeight;
            this.topRowHeight = topRowHeight;
            this.topRowPadding = topRowPadding;
            this.WorldNumWidth = WorldNumWidth;
            this.WorldNumHeight = WorldNumHeight;
            this.WorldLabelWidth = WorldLabelWidth;
            this.PotionWidth = PotionWidth;
            this.PotionHeight = PotionHeight;
            this.labelSpacing = CellLabelSpacing;
            this.labelWidth = CellLabelWidth;
            this.BackColor = CellBackColor;
            this.MinimalMode = MinimalMode;
            Font tempfont = new Font(new FontFamily(CellFontName), CellFontSize, CellFontStyle);
            if (pointLabel != null)
            {
                pointLabel.Font = tempfont;
                pointLabel.Width = labelWidth;
                pointLabel.Height = WorldNumHeight;
            }
            if (wothLabel != null)
            {
                wothLabel.Font = tempfont;
                wothLabel.Width = labelWidth;
                wothLabel.Height = WorldNumHeight;
            }
            UpdateVisuals();
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
                levelLabelMarked = levelImage.isMarked,
                levelNumMarked = (levelNumberImage != null) ? levelNumberImage.isMarked : (unknownLevelNumberImage != null) ? unknownLevelNumberImage.isMarked : false,
                levelNumIndex = (unknownLevelNumberImage != null) ? unknownLevelNumberImage.GetState().ImageIndex : 0
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
                levelImage.isMarked = state.levelLabelMarked;
                if (levelNumberImage != null)
                {
                    levelNumberImage.isMarked = state.levelNumMarked;
                } else if (unknownLevelNumberImage != null)
                {
                    unknownLevelNumberImage.isMarked = state.levelNumMarked;
                    unknownLevelNumberImage.SetState(state.levelNumIndex);
                }

                UpdateVisuals();
            }
        }

        public void SetState(string statestring)
        {
            //break up string into sections, recompile data. probably call the real setstate after tbh
            // return $"{currentPoints},{currentWOTHS},{levelNumIndex},{levelNumMarked},{levelLabelMarked}\n{itemstring}\n{displaystring}";

            string[] parts = statestring.Split('\n');
            string[] firstPart = parts[0].Split(',');
            //fp0 = currentpoints
            //fp1 = currentwoths
            currentPoints = int.Parse(firstPart[0]);
            currentWOTHS = int.Parse(firstPart[1]);
            if (levelNumberImage != null)
            {
                levelNumberImage.isMarked = bool.Parse(firstPart[3]);
            } else if (unknownLevelNumberImage != null)
            {
                unknownLevelNumberImage.isMarked = bool.Parse(firstPart[3]);
                unknownLevelNumberImage.SetState(int.Parse(firstPart[2]));
            }
            levelImage.isMarked = bool.Parse(firstPart[4]);

            //p1 = founditems
            if (parts[1].Length > 0)
            {
                foundItems = parts[1].Split(',').Select(int.Parse).ToList();
            }
            //p2 = displaylist
            if (parts[2].Length > 0)
            {
                string[] dl = parts[2].Split(',');

                List<CellDisplay> newdl = new List<CellDisplay>();
                foreach (string part in dl)
                {
                    string[] inner = part.Split('\t');
                    //i0 = pottype (int)
                    //i1 = itemid (int)
                    //i2 = isstarting (bool
                    //i3 = fiaded (bool
                    //i4 = marked (bool
                    newdl.Add(new CellDisplay() { potionType = int.Parse(inner[0]) , item_id = int.Parse(inner[1]) , isStarting = bool.Parse(inner[2]), isFaded = bool.Parse(inner[3]), isMarked = bool.Parse(inner[4]) });
                }
                displayList = newdl;
            }

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            // hotkey for both points and potions so i dont have the *chance* to forget them both
            UpdatePoints();
            UpdatePotions();
        }
    }
}

