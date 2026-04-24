using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Color = System.Drawing.Color;

namespace GSTHD
{
    public struct SpoilerCellState
    {
        public int totalPoints;
        public int currentPoints;

        public int totalWOTHS;
        public int startingWOTHS;

        public MarkedImageIndex levelNumMarked;
        public int levelNumIndex;
        public MarkedImageIndex levelLabelMarked;

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
            return $"{currentPoints},{startingWOTHS},{levelNumIndex},{(int)levelNumMarked},{(int)levelLabelMarked}\n{itemstring}\n{displaystring}";
        }
    }

    public class CellPictureBox : OrganicImage, ProgressibleElement<int>
    {
        private readonly ProgressibleElementBehaviour<int> ProgressBehaviour;

        public SpoilerCell hostCell;

        public int dk_id;
        public int internal_id;

        private Settings settings;


        public CellPictureBox(Settings settings, bool isOnBroadcast, bool isMarkable) : base((Uri?)null)
        {
            this.settings = settings;
            this.isMarkable = isMarkable;
            ProgressBehaviour = new ProgressibleElementBehaviour<int>(this, settings);
            if (!isOnBroadcast) PointerPressed += ProgressBehaviour.Mouse_ClickDown;
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
                hostCell.Children.Remove(this);
            }
        }

        public void ToggleCheck()
        {
            IncrementMarked(settings.EnabledMarks);
            hostCell.TellMarked(internal_id, isMarked);
            InvalidateVisual();
        }

        public void ToggleFade()
        {
            if (isFaded)
            {
                // restore fadedness
                isFaded = false;
            } else
            {
                // make faded
                isFaded = true;
            }

            InvalidateVisual();
            hostCell.TellFaded(internal_id, isFaded);
        }
    }

    public class CellDisplay
    {
        public int potionType;
        public int item_id;
        public bool isStarting;
        public bool isFaded = false;
        public MarkedImageIndex isMarked = 0;
        public int internal_id;

        public override string ToString()
        {
            return $"{potionType}\t{item_id}\t{isStarting}\t{isFaded}\t{(int)isMarked}";
        }
    }

    public class SpoilerCell : Canvas, UpdatableFromSettings
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
        private int runningInternalCount = 0;

        private GuaranteedHint levelNumberImage;
        private Item unknownLevelNumberImage;
        private GuaranteedHint levelImage;

        private int totalPoints;
        public int currentPoints = 0;
        private Color pointColour;
        private TextBlock? pointLabel;

        private int totalWOTHS;
        public int startingWOTHS = 0;
        private Color wothColour;
        private TextBlock? wothLabel;
        public int markedWOTHS = 0;
        public int markedStartingWOTHS = 0;

        private Color emptyColour;
        private Color kindaEmptyColour;

        public string fontName;
        public int fontSize;
        public FontStyle fontStyle;
        public int labelSpacing;
        public int labelWidth;

        public bool ayoJetpac = false;
        public bool noPotions = false;
        public bool startingisExcluded = false;

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
        public bool isMarkable = true;

        public string[] levelList = { "japes", "aztec", "factory", "galleon", "forest", "caves", "castle", "helm", "isles" };
        public string[] potionImageList = { "dk64/potion_shared.png", "dk64/potion_dk.png", "dk64/potion_diddy.png", "dk64/potion_lanky.png", "dk64/potion_tiny.png", "dk64/potion_chunky.png", "dk64/ButWhereWasDK.png", "dk64/key_unknown.png" };

        delegate void UpdatePointsCallback();
        delegate void UpdatePotionsCallback();
        delegate void SetStateCallback(SpoilerCellState state);


        public SpoilerCell(Settings settings, int width, int height, int x, int y, int points, int woths, int startingwotths, List<PotionTypes> potions, int topRowHeight, int topRowPadding, int WorldNumWidth, int WorldNumHeight, int WorldLabelWidth, int PotionWidth, int PotionHeight, string name, string levelname, int levelnum, int levelorder, string cellFontName, int cellFontSize, FontStyle cellFontStyle, int labelSpacing, int labelWidth, Color backColor, bool isMinimal, Dictionary<string, int> spread, Dictionary<int, DK64_Item> dkitems, bool isBroadcastable = false, bool isOnBroadcast = false, bool isMarkable = true, bool startingisExcluded = false)
        {
            // when getting created, get the spoiler numebrs from the parent panel
            Settings = settings;

            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);
            kindaEmptyColour = Color.FromKnownColor(Settings.SpoilerKindaEmptyColour);

            // just concerns defining the shape of the panel
            this.Background = LegacyConverters.ConvetToBrush(backColor);
            SetTop(this, y);
            SetLeft(this, x);
            this.Name = name;
            Width = width;
            Height = height;

            // if level order is given, make it the readable/base-1 version, otherwise just store it as -1
            this.levelOrder = (levelorder >= 0) ? levelorder + 1 : -1;
            // level num is basically the level ID (japes is 0, aztec is 1, etc)
            this.levelID = levelnum;
            this.levelName = levelname;
            this.totalPoints = points;
            this.totalWOTHS = woths;
            this.startingWOTHS = startingwotths;
            this.potionsList = potions;
            potionsList.Sort();
            this.pointspread = spread;
            this.DK64Items = dkitems;
            this.MinimalMode = isMinimal;
            this.startingisExcluded = startingisExcluded;

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
            this.isMarkable = isMarkable;

            DragDrop.AddDragEnterHandler(this, Mouse_DragEnter);
            DragDrop.AddDropHandler(this,  Mouse_DragDrop);
            DragDrop.SetAllowDrop(this, true);


            if (totalPoints >= 0) noPotions = true;

            int shownnumbers = 1;
            if (totalPoints >= 0)
            {
                pointLabel = new TextBlock
                {
                    Name = Guid.NewGuid().ToString(),
                    Text = totalPoints.ToString(),
                    FontFamily = fontName,
                    FontSize = fontSize*1.4,
                    Foreground = LegacyConverters.ConvetToBrush(pointColour),
                    //BackColor = Color.Red,
                    Width = labelWidth,
                    Height = WorldNumHeight,
                    // AutoSize = false,
                    TextAlignment = TextAlignment.Right,
                    // Anchor = AnchorStyles.Right,
                };
                FontStyleFix(pointLabel, fontStyle);
                Canvas.SetLeft(pointLabel, width);
                shownnumbers++;
                Children.Add(pointLabel);
            }
            if (totalWOTHS >= 0 || startingWOTHS != -1)
            {
                wothLabel = new TextBlock
                {
                    Name = Guid.NewGuid().ToString(),
                    Text = totalWOTHS.ToString(),
                    FontFamily = fontName,
                    FontSize = fontSize*1.4,
                    Foreground = LegacyConverters.ConvetToBrush(wothColour),
                    //BackColor = Color.Yellow,
                    Width = labelWidth,
                    Height = WorldNumHeight,
                    // AutoSize = false,
                    TextAlignment = TextAlignment.Right,
                };
                FontStyleFix(wothLabel, fontStyle);
                Canvas.SetLeft(wothLabel, width);
                Children.Add(wothLabel);
            }
            
            if (levelOrder is > 0 and < 9)
            {
                // put in the static image
                ObjectPoint temp1 = new ObjectPoint()
                {
                    Name = $"{name}_levelImageNumber",
                    ImageCollection = [$"dk64/{levelOrder}.png"],
                    Size = new Size(WorldNumWidth, WorldNumHeight),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    X = 0,
                    Y = 0,
                    Visible = true,
                    isBroadcastable = this.isBroadcastable
                };
                levelNumberImage = new GuaranteedHint(temp1, settings, isOnBroadcast, true);
                Children.Add(levelNumberImage);
            } else if (levelOrder < 0)
            {
                // put in the item
                ObjectPoint temp2 = new ObjectPoint()
                {
                    Name = $"{name}_unknownLevel",
                    X = 0, Y = 0,
                    Visible = true,
                    Size = new Size(WorldNumWidth, WorldNumHeight),
                    ImageCollection = ["dk64/unknownnum.png", "dk64/1.png", "dk64/2.png", "dk64/3.png", "dk64/4.png", "dk64/5.png", "dk64/6.png", "dk64/7.png", "dk64/8.png"
                    ],
                    isBroadcastable = this.isBroadcastable,
                };
                unknownLevelNumberImage = new Item(temp2, settings, isOnBroadcast, true);
                Children.Add(unknownLevelNumberImage);
            }

            ObjectPoint temp3 = new ObjectPoint()
            {
                Name = $"{name}_levelImageName",
                ImageCollection = [$"dk64/{levelList[levelID]}.png"],
                Size = new Size(WorldLabelWidth, WorldNumHeight - 2),
                SizeMode = PictureBoxSizeMode.Zoom,
                X = (!MinimalMode && levelOrder == 9) ? -6 : WorldNumWidth,
                Y = 1,
                Visible = true,
                isBroadcastable = this.isBroadcastable,
            };
            levelImage = new GuaranteedHint(temp3, settings, isOnBroadcast, true);
            Children.Add(levelImage);

            InitializeDisplayList();
            UpdateVisuals();

        }

        private void Mouse_DragEnter(object sender, DragEventArgs e)
        {
            e.DragEffects = !e.DataTransfer.Formats.Contains(Settings.GSTCustomFormat) ? DragDropEffects.None : DragDropEffects.Copy;
        }

        private void Mouse_DragDrop(object sender, DragEventArgs e)
        {
            // TODO: makes sure the thign being dragged into is an actual item
            try
            {
                var inpu = e.DataTransfer.GetItems(Settings.GSTCustomFormat).First().TryGetText();
                var parts = inpu.Split('|');
                var dropContent = new DragDropContent(bool.Parse(parts[0]), parts[1], int.Parse(parts[2]), Enum.Parse<MarkedImageIndex>(parts[3]));

                // ignore if its not a real item
                if (dropContent.dk_id != -1)
                {
                    // get the parent panel to handle it (incase its a starting move and needs to be handled differently)
                    ((SpoilerPanel)(this.Parent)).AddFromDrag(this.levelID, dropContent.dk_id, dropContent.isMarked, !dropContent.IsAutocheck);
                    
                    // lookup the actual dk item
                    //DK64_Item item = DK64Items[dropContent.dk_id];
                    //int sentPoints = (noPotions) ? pointspread[item.itemType] : -2;
                    ////Debug.WriteLine($"{item}, {dropContent.IsAutocheck}");
                    //AddNewItem(item, sentPoints, false, 1, !dropContent.IsAutocheck, dropContent.isMarked);
                }
            } catch { }

        }


        // also gonna need to define a new TextBlock version for the counts
        public void UpdateFromSettings()
        {
            // probably do something with being able to choose whether to display the icons
            pointColour = Color.FromKnownColor(Settings.SpoilerPointColour);
            wothColour = Color.FromKnownColor(Settings.SpoilerWOTHColour);
            emptyColour = Color.FromKnownColor(Settings.SpoilerEmptyColour);
            kindaEmptyColour = Color.FromKnownColor(Settings.SpoilerKindaEmptyColour);
            UpdateVisuals();
            if (isBroadcastable && BroadcastManager.Window != null)
            {
                ((SpoilerCell)BroadcastManager.GetControl(Name)!).UpdateFromSettings();
            }

        }

        public void InitializeDisplayList()
        {
            displayList = new List<CellDisplay>();
            if (!noPotions)
            {
                foreach (PotionTypes pot in potionsList)
                {
                    displayList.Add(new CellDisplay { potionType = (int)pot, isStarting = false, item_id = -1, isFaded = false, internal_id = runningInternalCount });
                    runningInternalCount++;
                }
            }
        }

        public bool AddToDisplayList(DK64_Item item, bool starting, bool faded, MarkedImageIndex marked)
        {
            for (int i = 0; i < displayList.Count; i++)
            {
                if (starting && startingisExcluded) continue;
                if (displayList[i].potionType == (int)item.potionType && displayList[i].item_id == -1)
                {
                    // new move into existing potion
                    displayList[i].item_id = item.item_id;
                    displayList[i].isStarting = starting;
                    displayList[i].isFaded = faded;
                    if (marked != MarkedImageIndex.need_override) displayList[i].isMarked = marked;
                    return false;
                } else if ((displayList[i].potionType == (int)item.potionType || displayList[i].potionType == -1) && displayList[i].item_id == item.item_id && displayList[i].isFaded && !faded)
                {
                    // new move into existing faded slot (dupe preventing)
                    displayList[i].isFaded = false;
                    if (marked != MarkedImageIndex.need_override) displayList[i].isMarked = marked;
                    return false;
                }
            }
            displayList.Add(new CellDisplay { potionType = -1, isStarting = starting, item_id = item.item_id, isFaded = faded, isMarked = (marked != MarkedImageIndex.need_override) ? marked : 0, internal_id = runningInternalCount });
            runningInternalCount++;
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
                        // if a starting move is deleted by accident, add the points back to account for it
                        if (displayList[i].isStarting) currentPoints += (noPotions) ? pointspread[DK64Items[id].itemType] : 0;
                        displayList.RemoveAt(i);
                        //Debug.WriteLine($"removed {id}"); 
                        return;
                    }
                }
            }
        }

        public void TellFaded(int internal_id, bool isFaded)
        {
            displayList.First(x => x.internal_id == internal_id).isFaded = isFaded;
            UpdateVisuals();
            //foreach (CellDisplay display in displayList)
            //{
            //    if (display.item_id == dk_id && display.isFaded == !isFaded)
            //    {
            //        display.isFaded = isFaded;
            //        UpdateVisuals();
            //        break;
            //    }
            //}
        }

        public void TellMarked(int internal_id, MarkedImageIndex isMarked)
        {
            displayList.First(x => x.internal_id == internal_id).isMarked = isMarked;
            UpdateVisuals();
            //foreach (CellDisplay display in displayList)
            //{
            //    if (display.item_id == dk_id && display.isMarked != isMarked)
            //    {
            //        display.isMarked = isMarked;
            //        UpdateVisuals();
            //        break;
            //    }
            //}
        }

        public void CountMarks()
        {
            markedWOTHS = 0;
            markedStartingWOTHS = 0;
            if (!Settings.CellCountWothMarks) return;
            foreach (CellDisplay display in displayList) 
            { 
                if (display.isMarked == MarkedImageIndex.mark_check)
                {
                    if (display.isStarting)
                    {
                        markedStartingWOTHS++;
                    } else
                    {
                        markedWOTHS++;
                    }
                }
            }
        }

        private void UpdatePoints()
        {

            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(UpdatePoints);
            }
            else
            {

                // set the points TextBlock
                if (totalPoints >= 0)
                {
                    pointLabel.Text = (totalPoints - currentPoints).ToString();
                    if (pointLabel.Text == "0" && !isThereAnyFaded()) { pointLabel.Foreground = LegacyConverters.ConvetToBrush(emptyColour); }
                    else if (pointLabel.Text == "0") { pointLabel.Foreground = LegacyConverters.ConvetToBrush(kindaEmptyColour); }
                    else pointLabel.Foreground = LegacyConverters.ConvetToBrush(pointColour);
                    //Debug.WriteLine($"Update to {levelName}: Points={pointLabel.Text}");
                }
                // also update WOTHS count, which dont decrement but do need to be fixed theres a crankyadd
                if (totalWOTHS >= 0 || startingWOTHS != -1)
                {
                    CountMarks();
                    if (startingWOTHS != -1 && !Settings.HideStarting)
                    {
                        wothLabel.Text = (totalWOTHS - markedWOTHS).ToString() + "+" + (startingWOTHS - markedStartingWOTHS).ToString();
                    } else
                    {
                        wothLabel.Text = (totalWOTHS - markedWOTHS).ToString();
                    }
                    wothLabel.Foreground = LegacyConverters.ConvetToBrush(wothColour);
                }

                AdjustPointLocations();

                if (isBroadcastable && BroadcastManager.Window != null)
                {
                    SpoilerPanel sp = ((SpoilerPanel)BroadcastManager.GetControl(Name.Split('_')[0]));
                    if (sp.spoilerLoaded)
                    {
                        ((SpoilerCell)sp.Children.First(x => string.Equals(x.Name, Name, StringComparison.CurrentCultureIgnoreCase))).SetState(GetState());
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
                var ts = TextShaper.Current;
                var typeface = new Typeface(pointLabel.FontFamily);
                ShapedBuffer shaped = ts.ShapeText(pointLabel.Text, new TextShaperOptions(typeface.GlyphTypeface, pointLabel.FontSize));
                var pointMeasure = new ShapedTextRun(shaped, new GenericTextRunProperties(typeface, pointLabel.FontSize)).Size.Width;
                pointLabel.Width = System.Math.Max(labelWidth, pointMeasure);
                pointWidth = (int)pointLabel.Width;
                Canvas.SetLeft(pointLabel, this.Width - (System.Math.Max(labelSpacing, pointWidth)) - 2 - this.topRowPadding);
            }

            if (wothLabel != null)
            {
                var ts = TextShaper.Current;
                var typeface = new Typeface(wothLabel.FontFamily);
                ShapedBuffer shaped = ts.ShapeText(wothLabel.Text, new TextShaperOptions(typeface.GlyphTypeface, wothLabel.FontSize));
                var pointMeasure = new ShapedTextRun(shaped, new GenericTextRunProperties(typeface, wothLabel.FontSize)).Size.Width;
                
                int pointVis = (pointLabel != null) ? 1 : 0;
                wothLabel.Width = System.Math.Max(labelWidth, pointMeasure);
                SetLeft(wothLabel, this.Width - (System.Math.Max(labelSpacing, wothLabel.Width)) - pointVis*(pointWidth) - 2 - this.topRowPadding);
            }
        }

        private void UpdatePotions()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(UpdatePotions);
            }
            else
            {
                if (!MinimalMode)
                {
                    foreach (var t in displayedPotions)
                    {
                        Children.Remove(t);
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
                        displayablePotsWidth = (int)(Width / usedPotWidth);
                        int displayablePotsHeight = (bottomRowHeight / usedPotHeight);
                        if (thingstodisplay > displayablePotsWidth)
                        {
                            int neededrows = (int)System.Math.Ceiling((double)thingstodisplay/(double)displayablePotsWidth);
                            if (neededrows > displayablePotsHeight)
                            {
                                //Debug.WriteLine("needs resizing");
                                int newpotwidth = (int)(Width / (displayablePotsWidth + 1));
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

                        CellPictureBox newPot = new CellPictureBox(Settings, isOnBroadcast, isMarkable)
                        {
                            TrueHeight = usedPotHeight,
                            TrueWidth = usedPotWidth,
                            //SizeMode = PictureBoxSizeMode.Zoom,
                            Source = "Resources/" + toDisplay,
                            hostCell = this,
                            dk_id = pot.item_id,
                            isFaded = pot.isFaded,
                            Background = this.Background,
                            isMarked = pot.isMarked,
                            internal_id = pot.internal_id
                        };
                        Canvas.SetTop(newPot, newY);
                        Canvas.SetLeft(newPot, newX);

                        //Debug.WriteLine($"{thingsdisplayed}:   x:{newX} y:{newY} w:{newPot.Width} h:{newPot.Height}");
                        displayedPotions.Add(newPot);
                        Children.Add(newPot);
                        thingsdisplayed++;
                    }

                    // no need to do a broadcast view check, as updatepoints does a setstate
                } else
                {
                    foreach (var t in displayedPotions)
                    {
                        Children.Remove(t);
                    }

                    displayedPotions.Clear();
                }

            }
        }

        public void AddNewItem(DK64_Item dk_id, int pointValue, bool isStarting, int howMany, bool isFaded = false, MarkedImageIndex MarkedIndex = 0)
        {
            
            for (int i = 0; i < howMany; i++)
            {
                if (!isFaded) foundItems.Add(dk_id.item_id);
                bool result = AddToDisplayList(dk_id, isStarting, isFaded, (!Settings.CellOverrideCheckMark) ? ((MarkedIndex > MarkedImageIndex.none) ? MarkedIndex : MarkedImageIndex.need_override) : MarkedImageIndex.need_override );
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
            this.WorldNumWidth = WorldNumWidth;
            this.WorldNumHeight = WorldNumHeight;
            this.WorldLabelWidth = WorldLabelWidth;
            this.PotionWidth = PotionWidth;
            this.PotionHeight = PotionHeight;
            this.labelSpacing = CellLabelSpacing;
            this.labelWidth = CellLabelWidth;
            this.Background = LegacyConverters.ConvetToBrush(CellBackColor);
            this.MinimalMode = MinimalMode;
            this.bottomRowHeight = cellHeight - topRowHeight;
            this.topRowPadding = (levelOrder == 9 && !MinimalMode) ? 0 : topRowPadding;
            if (pointLabel != null)
            {
                pointLabel.FontFamily = CellFontName;
                pointLabel.FontSize = CellFontSize;
                FontStyleFix(pointLabel, CellFontStyle);
                pointLabel.Width = labelWidth;
                pointLabel.Height = WorldNumHeight;
            }
            if (wothLabel != null)
            {
                wothLabel.FontFamily = CellFontName;
                wothLabel.FontSize = CellFontSize;
                FontStyleFix(wothLabel, CellFontStyle);
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
                startingWOTHS = startingWOTHS,
                foundItems = foundItems,
                potionsList = potionsList,
                displayList = displayList,
                levelLabelMarked = levelImage.isMarked,
                levelNumMarked = (levelNumberImage != null) ? levelNumberImage.isMarked : (unknownLevelNumberImage != null) ? unknownLevelNumberImage.isMarked : 0,
                levelNumIndex = (unknownLevelNumberImage != null) ? unknownLevelNumberImage.GetState().ImageIndex : 0
            };
        }

        public void SetState(SpoilerCellState state)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => SetState(state));
            }
            else
            {
                //Debug.WriteLine("setting state");
                totalPoints = state.totalPoints;
                currentPoints = state.currentPoints;
                totalWOTHS = state.totalWOTHS;
                startingWOTHS = state.startingWOTHS;
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
            startingWOTHS = int.Parse(firstPart[1]);
            if (levelNumberImage != null)
            {
                levelNumberImage.isMarked = (MarkedImageIndex)int.Parse(firstPart[3]);
            } else if (unknownLevelNumberImage != null)
            {
                unknownLevelNumberImage.isMarked = (MarkedImageIndex)int.Parse(firstPart[3]);
                unknownLevelNumberImage.SetState(int.Parse(firstPart[2]));
            }
            levelImage.isMarked = (MarkedImageIndex)int.Parse(firstPart[4]);

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
                    newdl.Add(new CellDisplay() { potionType = int.Parse(inner[0]) , item_id = int.Parse(inner[1]) , isStarting = bool.Parse(inner[2]), isFaded = bool.Parse(inner[3]), isMarked = (MarkedImageIndex)int.Parse(inner[4]) });
                }
                displayList = newdl;
            }

            UpdateVisuals();
        }
        
        private void FontStyleFix(TextBlock inp, FontStyle style)
        {
            // sucks that this is manual but what you gonna do?
            switch (style)
            {
                case GSTHD.FontStyle.Bold:
                    inp.FontWeight = FontWeight.Bold;
                    inp.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
                case GSTHD.FontStyle.Italic:
                    inp.FontWeight = FontWeight.Normal;
                    inp.FontStyle = Avalonia.Media.FontStyle.Italic;
                    break;
                case GSTHD.FontStyle.Regular:
                    inp.FontWeight = FontWeight.Normal;
                    inp.FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
            }
        }

        public void UpdateVisuals()
        {
            // hotkey for both points and potions so i dont have the *chance* to forget them both
            UpdatePoints();
            UpdatePotions();
        }
    }
}

