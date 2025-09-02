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
using System.Text.RegularExpressions;

namespace GSTHD
{
    class SpoilerPanel : Panel, UpdatableFromSettings, IAlternatableObject
    {
        Settings Settings;

        private Label DefaultLabel;

        public string whereSpoiler;

        private Dictionary<string, int> pointspread;
        public List<SpoilerCell> cells = new List<SpoilerCell>();
        public List<int> startingItems = new List<int>();
        public List<int> startingNotHintable = new List<int>();
        public List<int> foundATItems = new List<int>();
        public Dictionary<int, DK64_Item> DK64Items;
        public Dictionary<int, int> DK64Maps;

        public Dictionary<string, string> spoilerData;
        public Dictionary<string, object> mainSettings;

        public List<int> levelOrder = new List<int>();
        public List<int> kroolOrder = new List<int>();
        public List<int> helmOrder = new List<int>();
        public string randoVersion = string.Empty;

        private Color CellBackColor;
        public Color storedBackColor { get; set; }

        public int cellWidth;
        public int cellHeight;

        public int numRows;
        public int RowPadding { get; set; }
        public int numCols;
        public int ColPadding { get; set; }

        public int DataRowHeight { get; set; } = 20;
        public int topRowPadding { get; set; }
        public int WorldNumWidth { get; set; }
        public int WorldNumHeight { get; set; }
        public int WorldLabelWidth { get; set; }
        public int PotionWidth { get; set; }
        public int PotionHeight { get; set; }
        public string CellFontName { get; set; }
        public int CellFontSize { get; set; }
        public FontStyle CellFontStyle { get; set; }
        public int LabelSpacing { get; set; }
        public int CellLabelWidth { get; set; }

        public bool WriteByRow { get; set; }
        public bool pointsMode;

        public int lastKnownMap = -1;
        public int howManySlams = 0;
        private int startingWOTHS = -1;

        public bool ExtendFinalCell { get; set; } = false;
        public bool MinimalMode { get; set; } = false;
        public bool spoilerLoaded = false;
        public bool isBroadcastable { get; set; }

        private bool isOnBroadcast = false;
        public bool isMarkable = true;
        private bool hasStartingInLog = false;

        private static readonly Regex unspacer = new Regex(@"\s+");

        delegate void AddFromATCallback(int currentMap, int dk_id, int howMany, MarkedImageIndex marked);
        delegate void AddFromDragCallback(int currentMap, int dk_id, MarkedImageIndex marked, bool isFaded);

        public SpoilerPanel(ObjectPanelSpoiler data, Settings settings, bool isOnBroadcast=false)
        {
            Settings = settings;
            Visible = data.Visible;

            // just concerns defining the shape of the panel
            CellBackColor = data.CellBackColor;
            storedBackColor = data.BackColor;
            this.BackColor = data.DefaultColor;
            this.Location = new Point(data.X, data.Y);
            this.Name = data.Name;
            this.Size = new Size(data.Width, data.Height);
            this.WriteByRow = data.WriteByRow;
            this.numRows = data.Rows;
            this.RowPadding = data.RowPadding;
            this.numCols = data.Columns;
            this.ColPadding = data.ColPadding;

            this.topRowPadding = data.DataRowPadding;
            this.WorldNumWidth = data.WorldNumWidth;
            this.WorldNumHeight = data.WorldNumHeight;
            this.WorldLabelWidth = data.WorldLabelWidth;
            this.PotionHeight = data.PotionHeight;
            this.PotionWidth = data.PotionWidth;
            this.CellFontName = data.FontName;
            this.CellFontSize = data.FontSize;
            this.CellFontStyle = data.FontStyle;
            this.LabelSpacing = data.LabelSpacing;
            this.CellLabelWidth = data.LabelWidth;

            this.ExtendFinalCell = data.ExtendFinalCell;

            this.MinimalMode = data.isMinimal;
            if (!MinimalMode)
            {
                DataRowHeight = data.DataRowHeight;
            }
            this.isBroadcastable = data.isBroadcastable && !isOnBroadcast;
            this.isOnBroadcast = isOnBroadcast;
            this.isMarkable = data.isMarkable;

            // label for when a spoiler isnt loaded yet
            DefaultLabel = new Label
            {
                Name = Guid.NewGuid().ToString(),
                Text = "Please open a compatible DK64 Spoiler Log",
                Font = new Font(new FontFamily(CellFontName), CellFontSize),
                ForeColor = Color.White,
                BackColor = Color.Black,
                Width = data.Width,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(0, 0)
            };
            Controls.Add(DefaultLabel);

            

            // dropping is always allowed due to broadcast view goofs im too lazy to solve
            this.DragEnter += Mouse_DragEnter;
            this.DragDrop += Mouse_DragDrop;
            this.AllowDrop = true;
        }

        // the idea is that theres a big panel with a grid of smaller sections, defined by the json
        // the panel receives the commands from the autotracker to make the updates, then sends it to the correct cell
        // needs to support having the json dragged over to it

        // will prob need an external table for classifying a move by type and colour
        // and do lookups for item being added by autotrack or by drag

        private string Unspace(string text)
        {
            return unspacer.Replace(text, "");
        }

        private void Mouse_DragEnter(object sender, DragEventArgs e)
        {
            if (!spoilerLoaded) e.Effect = e.AllowedEffect;
        }

        private void Mouse_DragDrop(object sender, DragEventArgs e)
        {
            // makes sure the thign being dragged into is an actual file and not just a random GST item
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files[0].EndsWith(".json")) ImportFromJson(files[0]);
            }
        }

        private List<PotionTypes> ParsePotions(JArray potions)
        {
            List<PotionTypes> result = new List<PotionTypes>();
            foreach (string potion in potions)
            {
                PotionTypes toadd = 0;
                switch (potion)
                {
                    case "Clear Vial":
                    case "Bean":
                        toadd = PotionTypes.Colourless; break;
                    case "Yellow Vial":
                        toadd = PotionTypes.Yellow; break;
                    case "Red Vial":
                        toadd= PotionTypes.Red; break;
                    case "Blue Vial":
                        toadd = PotionTypes.Blue; break;
                    case "Purple Vial":
                        toadd = PotionTypes.Purple; break;
                    case "Green Vial":
                        toadd = PotionTypes.Green; break;
                    case "Kong":
                        toadd = PotionTypes.Kong; break;
                    case "Key":
                        toadd = PotionTypes.Key; break;
                }
                result.Add(toadd);
            }
            return result;
        }

        public void ImportFromJson(string filepath)
        {
            whereSpoiler = filepath;
            //Debug.WriteLine($"Import from file {filepath}");
            JObject loadedjson = JObject.Parse(File.ReadAllText(filepath));
            // check that its a legit json
            if (!loadedjson.ContainsKey("Spoiler Hints Data"))
            {
                MessageBox.Show("Could not find Spoiler Hints Data in the provided json file.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // go to the "Spoiler Hints Data"
            spoilerData = loadedjson.GetValue("Spoiler Hints Data").ToObject<Dictionary<string, string>>();
            mainSettings = loadedjson.GetValue("Settings").ToObject<Dictionary<string, object>>();

            // dump points from json
            if (spoilerData.ContainsKey("point_spread"))
            {
                pointspread = JsonConvert.DeserializeObject<Dictionary<string, int>>(spoilerData["point_spread"]);
                spoilerData.Remove("point_spread");
                //futureproof fairymoves that evaded notice
                if (!pointspread.ContainsKey("fairy_moves"))
                {
                    pointspread.Add("fairy_moves", pointspread["training_moves"]);
                }
                pointsMode = true;
            }
            else
            {
                pointsMode = false;
            }

            var parsedStartingInfo = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(spoilerData["starting_info"]);

            foreach (int i in parsedStartingInfo["starting_kongs"])
            {
                startingItems.Add(i);
            }
            foreach (string i in parsedStartingInfo["starting_keys"])
            {
                //Debug.WriteLine((int)i[i.Length - 1]);
                startingItems.Add(
                    ((int)i[i.Length - 1]) - '0' + 100
                    );
            }
            // grab the Shockwave Shuffle data and add Camera/Shockwave's item IDs if the player statrs with them
            string shockstate = (string)mainSettings["Shockwave Shuffle"];
            if (shockstate == "start_with")
            {
                startingItems.Add(34);
                startingItems.Add(35);
            }
            if (loadedjson.ContainsKey("Randomizer Version"))
            {
                randoVersion = loadedjson.GetValue("Randomizer Version").ToString();
            } else
            {
                randoVersion = "3.x";
            }

            kroolOrder = parsedStartingInfo["krool_order"].ToObject<List<int>>();
            helmOrder = parsedStartingInfo["helm_order"].ToObject<List<int>>();

            // false 3.0 failsafe
            if (kroolOrder[0] > 6) randoVersion = "4.x";

            if (parsedStartingInfo.ContainsKey("starting_moves"))
            {
                hasStartingInLog = true;
                // gen items for the first time
                DK64Items = DK64_Items.GenerateDK64Items();
                List<string> tempstarting = parsedStartingInfo["starting_moves"].ToObject<List<string>>();
                foreach (var move in tempstarting)
                {
                    var tempitem = DK64Items.FirstOrDefault(x => x.Value.name == move);
                    // kongs shouldn't be present in this section, so an ID of 0 here means a failed lookup, which we'll skip to not bug things out further
                    if (tempitem.Key == 0) continue;
                    if (tempitem.Key != 36)
                    {
                        startingItems.Add(tempitem.Key);
                        Debug.WriteLine("adding " + tempitem.Value.name + " to starting moves");
                    } else
                    {
                        // slams are handled seperately, and arent in the startingItems list
                        howManySlams += 1;
                        Debug.WriteLine("adding " + howManySlams + "th SLAM to slamcount");
                    }
                }

                //tempstarting = parsedStartingInfo["starting_moves_not_hintable"].ToObject<List<string>>();
                foreach (var move in tempstarting)
                {
                    if (move == "Camera and Shockwave") continue;
                    var tempitem = DK64Items.FirstOrDefault(x => x.Value.name == move);
                    if (tempitem.Key != 36)
                    {
                        startingNotHintable.Add(tempitem.Key);
                        Debug.WriteLine("adding " + tempitem.Value.name + " to UNHINTABLE starting moves");
                    }
                    //else
                    //{
                    //    // slams are handled seperately, and arent in the startingItems list
                    //    howManySlams += 1;
                    //    Debug.WriteLine("adding " + howManySlams + "th SLAM to slamcount");
                    //}
                }
            }
            // adds certian shopkeeps as starting items for seeds that dont have them in the pool, so they arent erroneously added to Isles upon tracking
            if (loadedjson.ContainsKey("Item Pool"))
            {
                var itempool = loadedjson.GetValue("Item Pool").ToObject<List<string>>();
                if (!itempool.Contains("Cranky")) startingItems.Add(250);
                if (!itempool.Contains("Funky")) startingItems.Add(251);
                if (!itempool.Contains("Candy")) startingItems.Add(252);
                if (!itempool.Contains("Snide")) startingItems.Add(253);
            }
            else if (randoVersion.StartsWith("3"))
            {
                // preventing false positives with 3.x seeds
                startingItems.Add(250);
                startingItems.Add(251);
                startingItems.Add(252);
                startingItems.Add(253);
            }

            

            if (parsedStartingInfo.ContainsKey("starting_moves_woth_count"))
            {
                startingWOTHS = (int)parsedStartingInfo["starting_moves_woth_count"];
            }

            if (parsedStartingInfo.ContainsKey("level_order"))
            {
                levelOrder = parsedStartingInfo["level_order"].ToObject<List<int>>();
                // manually add 8 for helm and 9 for isles
                if (levelOrder.Count == 7) levelOrder.Add(7);
                levelOrder.Add(8);
            }
            else
            {
                // fill with negative values if level order isnt given (then add helm and isle's numbers)
                for (int i = 0; i < 7; i++) { levelOrder.Add(-1); }
                if (randoVersion.StartsWith("3"))
                {
                    levelOrder.Add(7);
                } else
                {
                    levelOrder.Add(-1);
                }
                levelOrder.Add(8);
            }

            // TODO: REMOVE
            //Debug.WriteLine(String.Join(" , ", startingItems.ToArray()));
            //Debug.WriteLine(String.Join(" , ", levelOrder.ToArray()));
            //Debug.WriteLine(String.Join(" , ", kroolOrder.ToArray()));
            //Debug.WriteLine(String.Join(" , ", helmOrder.ToArray()));

            spoilerData.Remove("starting_info");

            InitializeCells();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                SpoilerPanel sp = (SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0];
                sp.levelOrder = levelOrder;
                sp.spoilerData = spoilerData;
                sp.mainSettings = mainSettings;
                sp.randoVersion = randoVersion;
                sp.hasStartingInLog = hasStartingInLog;
                sp.Settings = Settings;
                sp.InitializeCells();
            }
        }

        public void InitializeCells()
        {
            if (DK64Items == null) DK64Items = DK64_Items.GenerateDK64Items();
            DK64Maps = DK64_Items.GenerateDK64Maps();
            cellWidth = ((Width - (RowPadding * System.Math.Max(numRows - 1, 1))) / ((numCols != 1) ? numRows : 1));
            cellHeight = ((Height - (ColPadding * System.Math.Max(numCols - 1, 1))) / ((numCols != 1) ? numCols : numRows));
            //Debug.WriteLine($"w: {cellWidth} -- h: {cellHeight}");

            foreach (var level in spoilerData)
            {
                var parseddata = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(level.Value);

                if ((string)parseddata["level_name"] == "Cranky's Lab")
                {
                    // add jetpac numbers to Isles (legacy feature)
                    cells.Last().AddCrankys((int)parseddata["points"], (int)parseddata["woth_count"], ParsePotions(parseddata["vial_colors"]));
                }
                else
                {
                    int placement = 0;
                    int theNum = 0;
                    if (Settings.SpoilerOrder == Settings.SpoilerOrderOption.Numerical && levelOrder[0] != -1)
                    {
                        // this is the worst code ive ever written
                        foreach (int l in levelOrder)
                        {
                            if (l == int.Parse(level.Key))
                            {
                                break;
                            }
                            placement++;
                        }
                        theNum = placement;
                    }
                    else
                    {
                        placement = int.Parse(level.Key);
                        theNum = levelOrder[int.Parse(level.Key)];
                    }
                    int xmod = (WriteByRow) ? (placement / numRows) : (placement % numRows);
                    int ymod = (WriteByRow) ? (placement % numRows) : (placement / numRows);

                    int newX = xmod * (cellWidth) + (xmod * RowPadding);
                    int newY = ymod * (cellHeight) + (ymod * ColPadding);
                    //Debug.WriteLine($"p: {placement} l: {(string)parseddata["level_name"]} x: {newX} -- y: {newY}");


                    var newpotions = ParsePotions(parseddata["vial_colors"]);

                    int finalWidth = cellWidth;
                    if (ExtendFinalCell && placement == 8)
                    {
                        finalWidth = cellWidth * (numRows-xmod) + RowPadding*(numRows - xmod - 1);
                    }

                    int passedstartingwoths = (int.Parse(level.Key) == 8) ? startingWOTHS : -1;

                    SpoilerCell tempcell = new SpoilerCell(Settings, finalWidth, cellHeight,
                        newX, newY,
                        (int)parseddata["points"], (int)parseddata["woth_count"], passedstartingwoths, newpotions,
                        DataRowHeight, topRowPadding, WorldNumWidth, WorldNumHeight, WorldLabelWidth, PotionWidth, PotionHeight,
                        Name + "_" + Unspace((string)parseddata["level_name"]), (string)parseddata["level_name"],
                        int.Parse(level.Key), theNum,
                        CellFontName, CellFontSize, CellFontStyle, LabelSpacing, CellLabelWidth,
                        CellBackColor, MinimalMode, pointspread, DK64Items, isBroadcastable, isOnBroadcast, isMarkable, startingisExcluded: hasStartingInLog);
                    cells.Add(tempcell);

                }

                foreach (SpoilerCell cell in cells)
                {
                    Controls.Add(cell);
                }

            }

            GSTForms f = (GSTForms)this.FindForm();

            
            for (int i = 0; i < helmOrder.Count; i++)
            {
                var temp = f.Controls[0].Controls.Find($"HelmOrder{i}", false);
                if (temp.Length > 0) { ((Item)temp.First()).SetState(helmOrder[i] + 1); }
                else { break; }
            }
            
            if (randoVersion.StartsWith("3"))
            {
                // 3.x era of krool phases
                for (int i = 0; i < kroolOrder.Count; i++)
                {
                    var temp = f.Controls[0].Controls.Find($"KroolOrder{i}", false);
                    if (temp.Length > 0) { ((Item)temp.First()).SetState(kroolOrder[i] + 1); }
                    else { break; }
                }

            }
            else if (int.Parse(randoVersion.Substring(0,1)) >= 4 )
            {
                for (int i = 0; i < kroolOrder.Count; i++)
                {
                    var temp = f.Controls[0].Controls.Find($"KroolOrder{i}", false);
                    if (temp.Length > 0) { ((Item)temp.First()).SetState(DK64_Items.BossRooms[kroolOrder[i]] ); }
                    else { break; }
                }
            }


            this.BackColor = storedBackColor;
            DefaultLabel.Dispose();
            //Debug.WriteLine(DK64Items[201].ToString());
            spoilerLoaded = true;
        }

        public void ReorderCells()
        {
            Debug.WriteLine("reordering");
            foreach (SpoilerCell cell in cells)
            {
                int placement = 0;
                if (Settings.SpoilerOrder == Settings.SpoilerOrderOption.Numerical && levelOrder[0] != -1)
                {
                    // this is the worst code ive ever written
                    foreach (int l in levelOrder)
                    {
                        if (l == cell.levelID)
                        {
                            break;
                        }
                        placement++;
                    }
                }
                else
                {
                    placement = cell.levelID;
                }

                int xmod = (WriteByRow) ? (placement / numRows) : (placement % numRows);
                int ymod = (WriteByRow) ? (placement % numRows) : (placement / numRows);

                int newX = xmod * (cellWidth) + (xmod * RowPadding);
                int newY = ymod * (cellHeight) + (ymod * ColPadding);

                if (ExtendFinalCell && placement == 8) cell.Width = cellWidth * (numRows - xmod) + RowPadding * (numRows - xmod - 1);
                Debug.WriteLine($"x: {newX}, y: {newY}, w: {cell.Width}, h:{cell.Height}");
                cell.Location = new Point(newX, newY);
            }
        }

        public void SetCells(string input)
        {
            //input is the ENTIRE selection of cell output, splitting on \f
            string[] incells = input.Split('\f');

            foreach (string strcell in incells)
            {
                string[] names = Regex.Split(strcell, @"\:\:\|\:\:");
                //names0 is the actual name of the cell we're fucking with
                //names1 is the rest

                SpoilerCell thisCell = this.cells.Where(x => x.Name == names[0].Trim()).ToList()[0];

                thisCell.SetState(names[1]);
            }
        }


        public void AddFromDrag(int currentMap, int dk_id, MarkedImageIndex marked, bool isFaded)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddFromDragCallback(AddFromDrag), new object[] { currentMap, dk_id, marked, isFaded });
                return;
            }
            else
            {
                bool isStarting = false;
                int addedpoints = -1;
                bool allowit = true;
                DK64_Item dkitem = DK64Items[dk_id];
                if (startingItems.Contains(dk_id))
                {
                    if (currentMap != 8) allowit = false;
                    isStarting = true;
                    if (hasStartingInLog) addedpoints = (pointsMode) ? 0 : -1;
                }
                else
                {
                    addedpoints = (pointsMode) ? pointspread[dkitem.itemType] : -1;
                }
                if (allowit) { 
                    Debug.WriteLine($"dragging a copy of {dk_id} to map {lastKnownMap}, valued at {addedpoints} points");
                    cells[currentMap].AddNewItem(dkitem, addedpoints, isStarting, 1, MarkedIndex: marked, isFaded: isFaded); 
                }
            }

        }

        public void AddFromAT(int currentMap, int dk_id, int howMany, MarkedImageIndex marked)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddFromATCallback(AddFromAT), new object[] { currentMap, dk_id, howMany, marked});
                return;
            } else
            {
                if (spoilerLoaded && dk_id >=0 && !startingNotHintable.Contains(dk_id))
                {
                    bool isStarting = false;
                    int addedpoints = -1;
                    // i hate slams
                    if (dk_id == 36)
                    {
                        if (howMany > howManySlams)
                        {
                            howMany -= howManySlams;
                            howManySlams += howMany;
                            addedpoints = (pointsMode) ? pointspread[DK64Items[36].itemType] : -1;
                        } else
                        {
                            if (hasStartingInLog)
                            {
                                isStarting = true;
                                addedpoints = (pointsMode) ? 0 : -1;
                            }
                            else
                            {
                                howMany = 0;
                            }
                        }
                    } else
                    {
                        howMany = 1;
                    }


                    //Debug.WriteLine($"map: {currentMap} -- item: {dk_id}");
                    
                    try
                    {
                        if (currentMap <= -790)
                        {
                            lastKnownMap = currentMap + 800;
                        } else
                        {
                            lastKnownMap = DK64Maps[currentMap];
                            // special menu screen location
                            if (lastKnownMap == -2)
                            {
                                lastKnownMap = 8;
                                isStarting = true;
                            }
                        }
                    } catch {
                        //Debug.WriteLine($"map: {currentMap} ISNT REAL");
                    }
                    

                    DK64_Item dkitem = DK64Items[dk_id];

                    if (startingItems.Contains(dk_id))
                    {
                        // exclude kongs/keys
                        if (dk_id < 6 || (dk_id > 100 && dk_id < 110)) return;
                        isStarting = true;
                        if (hasStartingInLog) addedpoints = (pointsMode) ? 0 : -1;
                    } else if (dk_id != 36 || !hasStartingInLog)
                    {
                        addedpoints = (pointsMode) ? pointspread[dkitem.itemType] : -1;
                    }
                    // so that the starting moves can be even visible
                    //Debug.WriteLine($"{addedpoints}");

                    if (lastKnownMap >= 0 && !foundATItems.Contains(dk_id) && howMany > 0)
                    {
                        // dupe slams are handled seperately and skip the queue
                        if (dk_id != 36) foundATItems.Add(dk_id);
                        Debug.WriteLine($"adding {howMany} copy of {dk_id} to map {lastKnownMap}, valued at {addedpoints} points");
                        cells[lastKnownMap].AddNewItem(dkitem, addedpoints, isStarting, howMany, MarkedIndex:marked);
                    }
                } else
                {
                    Debug.WriteLine($"ID: {dk_id} is a starting key/kong/camera and has been ignored.");
                }

            }
        }

        public void RemoveSlams(int count)
        {
            // this function is supposed to account for manually removing a slam from a cell, warranting an update to the panel's count
            // however i am using the assumption that the "autotracker is always right" (and therefore always has the correct number of slams) and that "i will do nothing to correct user error" to get away with not actually implementing this
            howManySlams -= count;
        }

        public void Push()
        {
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null && spoilerLoaded)
            {
                SpoilerPanel sp = (SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0];
                sp.levelOrder = levelOrder;
                sp.spoilerData = spoilerData;
                sp.mainSettings = mainSettings;
                sp.randoVersion = randoVersion;
                sp.Settings = Settings;
                sp.InitializeCells();
                foreach (SpoilerCell cell in cells)
                {
                    cell.UpdateVisuals();
                }
            }
        }

        public void UpdateFromSettings()
        {
            //Debug.WriteLine("spoiler hint being updated");
            if (spoilerLoaded)
            {
                ReorderCells();
                if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).ReorderCells();

                }
            }
            // probably do something with being able to choose whether to display the icons
            // for cell in cells, updatefromsettings
            foreach (SpoilerCell cell in cells)
            {
                cell.UpdateFromSettings();
            }

        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (ObjectPanelSpoiler)ogPoint;
            switch (name)
            {
                case "DefaultColor":
                    if (!spoilerLoaded)
                    {
                        if (mult > 0) {
                            // format: `1,2,3`
                            var newrgb = value?.ToString().Split(',');
                            Color tempColor;
                            // if there isnt more than 1 response, assume its a word and not rgb
                            if (newrgb.Length > 1) tempColor = Color.FromArgb(int.Parse(newrgb[0]), int.Parse(newrgb[1]), int.Parse(newrgb[2]));
                            else tempColor = Color.FromName(value.ToString());
                            BackColor = tempColor; 
                        }
                        else BackColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    }
                    break;
                case "CellBackColor":
                    if (mult > 0)
                    {
                        // format: `1,2,3`
                        var newrgb = value?.ToString().Split(',');
                        Color tempColor;
                        // if there isnt more than 1 response, assume its a word and not rgb
                        if (newrgb.Length > 1) tempColor = Color.FromArgb(int.Parse(newrgb[0]), int.Parse(newrgb[1]), int.Parse(newrgb[2]));
                        else tempColor = Color.FromName(value.ToString());
                        CellBackColor = tempColor;
                    }
                    else CellBackColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "Rows":
                    numRows += int.Parse(value.ToString()) * mult;
                    break;
                case "Columns":
                    numCols += int.Parse(value.ToString()) * mult;
                    break;
                case "FontName":
                    if (mult > 0) CellFontName = value.ToString();
                    else CellFontName = (string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "FontSize":
                    if (mult > 0) CellFontSize = int.Parse(value.ToString());
                    else CellFontSize = (int)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "FontStyle":
                    if (mult > 0) CellFontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString());
                    else CellFontStyle = (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "LabelWidth":
                    CellLabelWidth += int.Parse(value.ToString()) * mult;
                    break;
                default:
                    throw new NotImplementedException($"Could not perform PanelSpoiler Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates()
        {
            RefreshCells();
        }
        
        public void RefreshCells()
        {
            if (spoilerLoaded)
            {
                BackColor = storedBackColor;
                // a place where i will go through each cell, and re-set their colours and dimensions and etc
                cellWidth = ((Width - (RowPadding * System.Math.Max(numRows - 1, 1))) / ((numCols != 1) ? numRows : 1));
                cellHeight = ((Height - (ColPadding * System.Math.Max(numCols - 1, 1))) / ((numCols != 1) ? numCols : numRows));
                foreach (SpoilerCell cell in cells)
                {
                    cell.RefreshVisuals(cellWidth, cellHeight,
                        DataRowHeight, topRowPadding,
                        WorldNumWidth, WorldNumHeight, WorldLabelWidth,
                        PotionWidth, PotionHeight,
                        CellFontName, CellFontSize, CellFontStyle,
                        LabelSpacing, CellLabelWidth, CellBackColor,
                        MinimalMode);
                }
                ReorderCells();
            }

        }
    }
}
