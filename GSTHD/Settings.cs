using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace GSTHD
{
    public class AltSettings
    {
        public string LayoutName { get; set; }
        public Dictionary<string, string> Changes { get; set; } = new Dictionary<string, string>();
    }
    public class Settings
    {
        private const string SettingsFileName = @"settings.json";

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SongMarkerBehaviourOption
        {
            None,
            CheckOnly,
            DropOnly,
            DropAndCheck,
            DragAndDrop,
            Full,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DragButtonOption
        {
            None,
            Left,
            Middle,
            Right,
            LeftAndRight,
            Control,
            Shift,
            Alt
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ExtraActionModButton
        {
            None,
            Left,
            Middle,
            Right,
            DoubleLeft,
            MouseButton1,
            MouseButton2,
            Control,
            Shift,
            Alt
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public enum BasicActionButtonOption
        {
            None,
            Left,
            Middle,
            Right,
            MouseButton1,
            MouseButton2,
            Control,
            Shift,
            Alt
        }


        [JsonConverter(typeof(StringEnumConverter))]
        public enum SelectEmulatorOption
        {
            Project64,
            Project64_4,
            Bizhawk,
            RMG,
            simple64,
            parallel,
            retroarch
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SpoilerOrderOption
        {
            Numerical,
            Chronological
        }


        [JsonConverter(typeof(StringEnumConverter))]
        public enum MarkModeOption
        {
            None,
            Checkmark,
            X,
            Question,
            Star
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SongFileWriteOption
        {
            Disabled,
            Single,
            Multi
        }

        private const SongMarkerBehaviourOption DefaultSongMarkerBehaviour = SongMarkerBehaviourOption.DropAndCheck;

        public bool ShowMenuBar { get; set; } = true;
        public string ActiveLayout { get; set; } = "Layouts\\dk64.json";
        public string ActiveLayoutBroadcastFile {  get; set; } = string.Empty;
        public string ActivePlaces { get; set; } = "dk64_places.json";
        public string ActiveSometimesHints { get; set; } = "sometimes_hints.json";
        public bool InvertScrollWheel { get; set; } = false;
        public bool WraparoundDungeonNames { get; set; } = true;
        public bool WraparoundItems { get; set; } = false;
        public bool HintPathAutofill { get; set; } = false;
        public bool HintPathAutofillAggressive { get; set; } = false;
        public DragButtonOption DragButton { get; set; } = DragButtonOption.Left;
        public DragButtonOption AutocheckDragButton { get; set; } = DragButtonOption.LeftAndRight;
        public BasicActionButtonOption IncrementActionButton { get; set; } = BasicActionButtonOption.Left;
        public BasicActionButtonOption DecrementActionButton { get; set; } = BasicActionButtonOption.Right;
        public BasicActionButtonOption ResetActionButton { get; set; } = BasicActionButtonOption.Middle;
        public ExtraActionModButton ExtraActionButton { get; set; } = ExtraActionModButton.Shift;
        public SelectEmulatorOption SelectEmulator { get; set; } = SelectEmulatorOption.Project64;
        public SpoilerOrderOption SpoilerOrder { get; set; } = SpoilerOrderOption.Numerical;
        public List<MarkModeOption> EnabledMarks { get; set; }
        public int MinDragThreshold { get; set; } = 6;
        public bool MoveLocationToSong { get; set; } = false;
        // public bool AutoCheckSongs { get; set; } = false;
        public SongMarkerBehaviourOption SongMarkerBehaviour { get; set; } = DefaultSongMarkerBehaviour;
        public string[] DefaultSongMarkerImages { get; set; } = new string[0];
        public string[] DefaultGossipStoneImages { get; set; } = new string[0];
        public string[] DefaultPathGoalImages { get; set; } = new string[0];
        public int DefaultPathGoalCount { get; set; } = 0;
        public int DefaultWothGossipStoneCount { get; set; } = 4;
        public string[] DefaultWothColors { get; set; } = new string[]
        {
            "White",
            "Orange",
            "Crimson",
        };
        public string[] DefaultBarrenColors { get; set; } = new string[]
        {
            "White",
            "Gold",
        };
        public int DefaultWothColorIndex { get; set; } = 0;
        public bool EnableDuplicateWoth { get; set; } = true;
        public bool EnableLastWoth { get; set; } = false;
        public bool EnableBarrenColors { get; set; } = true;
        public bool ForceGossipCycles { get; set; } = false;
        public bool OverrideHeldImage { get; set; } = false;
        public bool StoneOverrideCheckMark { get; set; } = false;
        public bool CellOverrideCheckMark { get; set; } = false;
        public bool CellCountWothMarks { get; set; } = false;
        public double GossipCycleTime { get; set; } = 1;
        public bool EnableAutosave { get; set; } = true;
        public bool DeleteOldAutosaves { get; set; } = true;
        public bool EnableSongTracking { get; set; } = false;
        public SongFileWriteOption WriteSongDataToFile { get; set; } = SongFileWriteOption.Disabled;
        public bool SubtractItems { get; set; } = true;
        public bool HideStarting {  get; set; } = true;
        public KnownColor LastWothColor { get; set; } = KnownColor.BlueViolet;
        public KnownColor SpoilerPointColour { get; set; } = KnownColor.White;
        public KnownColor SpoilerWOTHColour { get; set; } = KnownColor.ForestGreen;
        public KnownColor SpoilerEmptyColour { get; set; } = KnownColor.Teal;
        public KnownColor SpoilerKindaEmptyColour { get; set; } = KnownColor.GreenYellow;
        public List<AltSettings> AlternateSettings { get; set; } = new List<AltSettings>();


        public MedallionLabel DefaultDungeonNames { get; set; } = new MedallionLabel()
        {
            TextCollection = new string[] { "????", "FREE", "DEKU", "DC", "JABU", "FOREST", "FIRE", "WATER", "SHADOW", "SPIRIT" },
            DefaultValue = 0,
            Wraparound = true,
            FontName = "Consolas",
            FontSize = 8,
            FontStyle = FontStyle.Bold,
        };

        public static SongMarkerBehaviourOption GetSongMarkerBehaviour(string value)
        {
            if (string.IsNullOrEmpty(value))
                return DefaultSongMarkerBehaviour;

            return (SongMarkerBehaviourOption) Enum.Parse(typeof(SongMarkerBehaviourOption), value);
        }

        public static Settings Read()
        {
            if (File.Exists(SettingsFileName))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFileName));
            else
            {
                var settings = new Settings();
                settings.Write();
                return settings;
            }
        }

        public void AddAltSetting(string groupname, string settingname, bool check)
        {
            if (!AlternateSettings.Where(x => x.LayoutName == ActiveLayout).Any())
            {
                // there isnt a setting for this layout yet, go add it
                AlternateSettings.Add(new AltSettings { LayoutName = ActiveLayout });
            }
            var words = Regex.Split((groupname != null) ? groupname : "", @"_\:_").ToList();
            if (words.Count == 1) words.Add("");
            AltSettings thealt = AlternateSettings.Where(x => x.LayoutName == ActiveLayout).First();
            if (words[1] == string.Empty && (groupname?.Contains("_:_") == true || groupname?.Contains("_::_") == true) || words[0] == "")
            {
                // add as SETTINGNAME : CHECK
                if (groupname?.Contains("_::_") == true)
                {
                    // toggles in collection
                    if (thealt.Changes.ContainsKey(groupname + settingname))
                    {
                        if (check) thealt.Changes[groupname + settingname] = check.ToString();
                        else thealt.Changes.Remove(groupname + settingname);
                    } else
                    {
                        if (check) thealt.Changes.Add(groupname + settingname, check.ToString());
                    }
                } else
                {
                    //plain old toggles
                    if (thealt.Changes.ContainsKey(settingname))
                    {
                        if (check) thealt.Changes[settingname] = check.ToString();
                        else thealt.Changes.Remove(settingname);
                    } else
                    {
                        if (check) thealt.Changes.Add(settingname, check.ToString());
                    }

                }

            } else
            {
                // add as GROUPNAME : SETTINGNAME
                if (thealt.Changes.ContainsKey(groupname))
                {
                    if (check) thealt.Changes[groupname] = settingname;
                    else thealt.Changes.Remove(groupname);
                }
                else
                {
                    if (check) thealt.Changes.Add(groupname, settingname);
                }
            }
            Write();
        }

        public void Write()
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsFileName, str);
        }
    }
}
