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
    class SpoilerPanel : Panel, UpdatableFromSettings
    {
        Settings Settings;

        private Label DefaultLabel;

        private Dictionary<string, int> pointspread;
        public List<SpoilerCell> cells = new List<SpoilerCell>();
        public List<int> startingItems = new List<int>();
        public Dictionary<int, DK64_Item> DK64Items;
        public Dictionary<int, int> DK64Maps;

        Dictionary<string, string> spoilerData;
        Dictionary<string, string> mainSettings;

        public List<int> levelOrder = new List<int>();
        public List<int> kroolOrder = new List<int>();
        public List<int> helmOrder = new List<int>();

        private Color cellBackColor;

        public int cellWidth;
        public int cellHeight;

        public int numRows;
        public int rowPadding;
        public int numCols;
        public int colPadding;


        public int topRowHeight = 20;
        public int WorldNumWidth;
        public int WorldNumHeight;
        public int PotionWidth;
        public int PotionHeight;

        public bool writeByRow;
        public bool pointsMode;

        public int lastKnownMap = -1;

        public bool MinimalMode = false;
        public bool spoilerLoaded = false;
        public bool isBroadcastable;

        private static readonly Regex unspacer = new Regex(@"\s+");

        public SpoilerPanel(ObjectPanelSpoiler data, Settings settings, bool isOnBroadcast=false)
        {
            Settings = settings;

            // just concerns defining the shape of the panel
            cellBackColor = data.BackColor;
            this.BackColor = data.DefaultColor;
            this.Location = new Point(data.X, data.Y);
            this.Name = data.Name;
            this.Size = new Size(data.Width, data.Height);
            this.writeByRow = data.WriteByRow;
            this.numRows = data.Rows;
            this.rowPadding = data.RowPadding;
            this.numCols = data.Columns;
            this.colPadding = data.ColPadding;

            this.WorldNumWidth = data.WorldNumWidth;
            this.WorldNumHeight = data.WorldNumHeight;
            this.PotionHeight = data.PotionHeight;
            this.PotionWidth = data.PoitionWidth;

            this.MinimalMode = data.isMinimal;
            if (!MinimalMode)
            {
                topRowHeight = data.DataRowHeight;
            }
            this.isBroadcastable = data.isBroadcastable && !isOnBroadcast;

            // label for when a spoiler isnt loaded yet
            DefaultLabel = new Label
            {
                Name = Guid.NewGuid().ToString(),
                Text = "Please open a compatible DK64 Spoiler Log",
                Font = new Font(new FontFamily("Calibri"), 9),
                ForeColor = Color.White,
                BackColor = Color.Black,
                Width = data.Width,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(0, 0)
            };
            Controls.Add(DefaultLabel);

            if (!isOnBroadcast)
            {
                this.DragEnter += Mouse_DragEnter;
                this.DragDrop += Mouse_DragDrop;
            }
            // dropping is always allowed due to broadcast view goofs im too lazy to solve
            this.AllowDrop = true;
        }

        // the idea is that theres a big panel with a grid of smaller sections, defined by the json
        // the panel receives the commands from the autotracker to make the updates, then sends it to the correct cell
        // needs to support having the json dragged over to it

        // will prob need an external table for classifying a move by type and colour
        // and do lookups for item being added by autotrack or by drag

        private string unspace(string text)
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
            mainSettings = loadedjson.GetValue("Settings").ToObject<Dictionary<string, string>>();

            // dump points from json
            if (spoilerData.ContainsKey("point_spread"))
            {
                pointspread = JsonConvert.DeserializeObject<Dictionary<string, int>>(spoilerData["point_spread"]);
                spoilerData.Remove("point_spread");
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
            string shockstate = mainSettings["Shockwave Shuffle"];
            if (shockstate == "start_with")
            {
                startingItems.Add(34);
                startingItems.Add(35);
            }

            if (parsedStartingInfo.ContainsKey("level_order"))
            {
                levelOrder = parsedStartingInfo["level_order"].ToObject<List<int>>();
                // manually att 8 for helm and 9 for isles
                levelOrder.Add(7);
                levelOrder.Add(8);
            }
            else
            {
                // fill with negative values if level order isnt given
                for (int i = 0; i < 9; i++) { levelOrder.Add(-1); }
            }

            kroolOrder = parsedStartingInfo["krool_order"].ToObject<List<int>>();
            helmOrder = parsedStartingInfo["helm_order"].ToObject<List<int>>();
            // TODO: define some keywords for helm/krool orders autofilling AND PUT IT IN INITIALIZE

            // TODO: REMOVE
            //Debug.WriteLine(String.Join(" , ", startingItems.ToArray()));
            //Debug.WriteLine(String.Join(" , ", levelOrder.ToArray()));
            //Debug.WriteLine(String.Join(" , ", kroolOrder.ToArray()));
            //Debug.WriteLine(String.Join(" , ", helmOrder.ToArray()));

            spoilerData.Remove("starting_info");

            InitializeCells();
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).levelOrder = levelOrder;
                ((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).spoilerData = spoilerData;
                ((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).mainSettings = mainSettings;
                ((SpoilerPanel)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).InitializeCells();
            }
        }

        public void InitializeCells()
        {

            cellWidth = ((Width - (rowPadding * (numRows - 1))) / numRows);
            cellHeight = ((Height - (colPadding * (numCols - 1))) / numCols);
            //Debug.WriteLine($"w: {cellWidth} -- h: {cellHeight}");

            // TODO: alternate method for not only row first, but also 
            foreach (var level in spoilerData)
            {
                var parseddata = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(level.Value);

                if ((string)parseddata["level_name"] == "Cranky's Lab")
                {
                    // add jetpac numbers to Isles
                    cells.Last().AddCrankys((int)parseddata["points"], (int)parseddata["woth_count"], ParsePotions(parseddata["vial_colors"]));
                }
                else
                {
                    int placement = 0;
                    if (Settings.SpoilerOrder == Settings.SpoilerOrderOption.Numerical)
                    {
                        placement = levelOrder[int.Parse(level.Key)];
                    }
                    else
                    {
                        placement = int.Parse(level.Key);
                    }
                    int newX = (placement / numRows) * (cellWidth) + ((placement / numRows) * rowPadding);
                    int newY = (placement % numRows) * (cellHeight) + ((placement % numRows) * colPadding);
                    //Debug.WriteLine($"x: {newX} -- y: {newY}");


                    var newpotions = ParsePotions(parseddata["vial_colors"]);

                    SpoilerCell tempcell = new SpoilerCell(Settings, cellWidth, cellHeight,
                        newX, newY,
                        (int)parseddata["points"], (int)parseddata["woth_count"], newpotions,
                        topRowHeight, WorldNumWidth, WorldNumHeight, PotionWidth, PotionHeight,
                        Name + "_" + unspace((string)parseddata["level_name"]), (string)parseddata["level_name"],
                        int.Parse(level.Key), levelOrder[int.Parse(level.Key)],
                        cellBackColor, MinimalMode, isBroadcastable);
                    cells.Add(tempcell);

                }

                foreach (SpoilerCell cell in cells)
                {
                    Controls.Add(cell);
                }

            }


            this.BackColor = cellBackColor;
            DefaultLabel.Dispose();
            DK64Items = DK64_Items.GenerateDK64Items();
            DK64Maps = DK64_Items.GenerateDK64Maps();
            //Debug.WriteLine(DK64Items[201].ToString());
            spoilerLoaded = true;
        }

        public void ReorderCells()
        {
            Debug.WriteLine("reordering");
            foreach (SpoilerCell cell in cells)
            {
                int placement = 0;
                if (Settings.SpoilerOrder == Settings.SpoilerOrderOption.Numerical)
                {
                    placement = levelOrder[cell.levelID];
                }
                else
                {
                    placement = cell.levelID;
                }
                int newX = (placement / numRows) * (cellWidth) + ((placement / numRows) * rowPadding);
                int newY = (placement % numRows) * (cellHeight) + ((placement % numRows) * colPadding);
                cell.Location = new Point(newX, newY);
            }
        }

        public void AddFromAT(int currentMap, int dk_id)
        {
            if (spoilerLoaded && !startingItems.Contains(dk_id) && spoilerLoaded)
            {
                //Debug.WriteLine($"map: {currentMap} -- item: {dk_id}");
                bool isStarting = false;
                try
                {
                    lastKnownMap = DK64Maps[currentMap];
                    // special menu screen location
                    if (lastKnownMap == -2)
                    {
                        lastKnownMap = 8;
                        isStarting = true;
                    }
                } catch {
                    //Debug.WriteLine($"map: {currentMap} ISNT REAL");
                }
                // convert the map to the table i still need to make
                // then update the corresponding cell

                DK64_Item dkitem = DK64Items[dk_id];
                int addedpoints = -1;
                if (pointsMode) addedpoints = pointspread[dkitem.itemType];
                //Debug.WriteLine($"{addedpoints}");

                if (lastKnownMap >= 0) cells[lastKnownMap].AddNewItem(dkitem, addedpoints, isStarting);
            }
        }

        public void UpdateFromSettings()
        {
            Debug.WriteLine("spoiler hint being updated");
            if (spoilerLoaded) ReorderCells();
            // probably do something with being able to choose whether to display the icons
            // for cell in cells, updatefromsettings
            foreach (SpoilerCell cell in cells)
            {
                cell.UpdateFromSettings();
            }

            // also for whether or not to display by numerical order or chronological
        }
    }
}
