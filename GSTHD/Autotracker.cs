using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Net;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Net.NetworkInformation;
using System.Timers;
using System.IO;

namespace GSTHD
{
    public class TrackedAddress
    {
        public string name;
        public uint address;
        public uint pointerAddress = 0;
        public int numBytes;
        public int currentValue = 0;
        public uint targetValue;
        public int bitmask = 0;
        public int offset;
        public string type;
        public string group;
        public OrganicImage targetControl;
        public int dk64_id = -1;
        public bool enabled = false;
        public bool locked = false;
        public int version = 0;
        public int usesPointer = 0;
    }
    public class TrackedGroup
    {
        public string name;
        public int runningvalue = 0;
        public int currentValue;
        public int count = 0;
        public int countMax;
        public bool isDouble = false;
        public int left_dk64_id = -1;
        public int right_dk64_id = -1;
        public OrganicImage targetControl;
        public int version = 0;
    }

    public class AlreadyRead
    {
        public int value;
        public uint address;
        public int bits;
    }
    public class Autotracker : UpdatableFromSettings
    {
        private Process emulator;
        private IntPtr emuHandle;
        private uint offset;
        private ulong offset64;

        private List<TrackedAddress> trackedAddresses = new List<TrackedAddress>();
        private List<TrackedGroup> trackedGroups = new List<TrackedGroup>();
        private List<AlreadyRead> alreadyReads = new List<AlreadyRead>();

        private uint desiredGameAddr;
        private int desiredGameBytes;
        private uint desiredGameValue;

        private uint desiredGameStateAddr;
        private int desiredGameStateBytes;
        private uint desiredGameStateValue;

        private uint currentMapAddr;
        private int currentMapBytes;
        private int currentMapValue;
        private SpoilerPanel spoilerPanel;

        private uint currentSongAddr;
        private int currentSongBytes;
        private NowPlayingPanel songPanel;
        private uint currentMapTimerAddr;
        private string currentSongGame;
        private string currentSongTitle;

        private List<uint> globalPointers = new List<uint>();
        private List<uint> globalPointerBaseAddr = new List<uint>();
        private List<int> globalPointerBaseBytes = new List<int>();

        private System.Timers.Timer timer;
        private Form1 form;

        private int timeout;
        private bool is64 = false;
        private bool LZTracking = false;
        private bool SongTracking = false;
        private bool PointerTracking = false;

        private int internalRandoVersion = 0;

        //32-bit version
        public Autotracker(Process theProgram, uint foundOffset, ref Form1 theForm)
        {
            emulator = theProgram;
            emuHandle = emulator.Handle;
            offset = foundOffset;
            form = theForm;
            form.CurrentLayout.ListUpdatables.Add(this);

            if (form.CurrentLayout.App_Settings.AutotrackingGame != null)
            {
                SetGameStateTargets(form.CurrentLayout.App_Settings.AutotrackingGame);
                CalibrateTracks();
                Debug.WriteLine("Autotracking22 at " + offset.ToString("X"));
                Debug.WriteLine("Beginning timer with " + trackedAddresses.Count + " addresses and " + trackedGroups.Count + " groups");
                //timer = new System.Threading.Timer(MainTracker, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                timer = new System.Timers.Timer(1000);
                timer.Elapsed += MainTracker;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
        }

        //64-bit version
        public Autotracker(Process theProgram, ulong foundOffset, Form1 theForm)
        {
            emulator = theProgram;
            emuHandle = emulator.Handle;
            offset64 = foundOffset;
            form = theForm;
            form.CurrentLayout.ListUpdatables.Add(this);
            is64 = true;

            if (form.CurrentLayout.App_Settings.AutotrackingGame != null)
            {
                SetGameStateTargets(form.CurrentLayout.App_Settings.AutotrackingGame);
                CalibrateTracks();
                Debug.WriteLine("Autotracking " + emulator.ProcessName + " at " + offset64.ToString("X"));
                Debug.WriteLine("Beginning timer with " + trackedAddresses.Count + " addresses and " + trackedGroups.Count + " groups");
                //timer = new System.Threading.Timer(MainTracker, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                timer = new System.Timers.Timer(1000);
                timer.Elapsed += MainTracker;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
        }

        public void CalibrateTracks()
        {
            // go through each address or group and store the controls that they are going to update so i can skip that godawful foreach controls for every single damn update
            // also removes addresses without a counterpart on the layout

            var tac = new List<TrackedAddress>();

            foreach (TrackedAddress ta in trackedAddresses)
            {
                if (ta.group != "")
                {
                    TrackedGroup result = trackedGroups.Find(x => x.name == ta.group);
                    if (result.targetControl != null)
                    {
                        //Debug.WriteLine("group " + result.name + " already has a targetcontrol, skipping");
                        ta.enabled = result.targetControl.Visible;
                        continue;
                    }
                    foreach (Control thing in form.Controls[0].Controls)
                    {
                        if (thing is CollectedItem ci)
                        {
                            if (ci.AutoName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }
                            else if (ci.AutoSubName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                        else if (thing is DoubleItem di)
                        {
                            if (di.AutoName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }
                        }
                        else if (thing is Item it)
                        {
                            if (it.AutoName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                    }
                    if (result.targetControl == null)
                    {
                        Debug.WriteLine("group " + result.name + " doesnt have a thing on the tracker");
                        tac.Add(ta);
                    }
                } else
                {
                    foreach (Control thing in form.Controls[0].Controls)
                    {
                        if (ta.type == "item" && thing is Item it)
                        {
                            if (it.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                        else if (ta.type == "item" && thing is Medallion md)
                        {
                            if (md.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                        else if (ta.type == "item" && thing is Song sg)
                        {
                            if (sg.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                        else if (ta.type == "collectable" && thing is CollectedItem ci)
                        {
                            if (ci.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                        else if (ta.type == "collectable" && thing is Item it2)
                        {
                            if (it2.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                ta.enabled = thing.Visible;
                                break;
                            }

                        }
                    }
                    if (ta.targetControl == null)
                    {
                        Debug.WriteLine("item " + ta.name + " doesnt have a thing on the tracker");
                        tac.Add(ta);
                    }
                }

            }

            foreach( var thing in tac)
            {
                trackedAddresses.Remove(thing);
            }

            // if the currentmapaddr is set at all, look for a spoilerhint panel
            if (currentMapAddr != 0)
            {
                foreach (Control thing in form.Controls[0].Controls)
                {
                    if (thing is SpoilerPanel panel)
                    {
                        LZTracking = true;
                        spoilerPanel = panel;
                        break;
                    }
                }
            }

            if (currentSongAddr != 0 && currentMapTimerAddr != 0)
            {
                SongTracking = true;
                foreach (Control thing in form.Controls[0].Controls)
                {
                    if (thing is NowPlayingPanel panel)
                    {
                        songPanel = panel;
                        break;
                    }
                }
            }
        }

        private void UpdateCurrentMap()
        {

            //currentMapValue = GoRead(currentMapAddr, currentMapBytes);
            int temp = GoRead(currentMapAddr, currentMapBytes);
            if (spoilerPanel.spoilerLoaded && spoilerPanel.DK64Maps.ContainsKey(temp))
            {
                currentMapValue = temp;

            }

        }

        private void UpdateNowPlaying()
        {
            // if maptimer is below 0, then we're gonna get garbage data lmao
            if (GoRead(currentMapTimerAddr, 32) <= 0) return;

            // read text pointer address
            int pointer = GoRead(currentSongAddr, currentSongBytes);
            List<byte> songGame = new List<byte>();
            List<byte> songTitle = new List<byte>();
            uint readbytes = 0;
            int stage = 0;
            bool needoverwrite = false;

            while (stage < 2)
            {
                // read value at pointer (subtracting 0x80000000, since that is implicity added in the GoRead Function
                int rv = GoRead((uint)pointer - 0x80000000 + readbytes, currentSongBytes);
                // convert data to bytes, then flip it (it comes up backwards)
                byte[] cr = BitConverter.GetBytes(rv);
                Array.Reverse(cr);
                foreach(byte b in cr)
                {
                    // the format is song game, a single 0x00 byte, then the song title, then another 0x00
                    // therefore, read until we find a 0x00, advance the stage, read until another 0x00, then stop looking
                    if (b == 0) stage++;
                    if (b > 127 || (b < 30 && b!=0))
                    {
                        //broken characters
                        if (stage == 0) { songGame.Clear(); songTitle.Clear(); needoverwrite = true; }
                        if (stage == 1) { songTitle = songGame; needoverwrite = true; }
                        stage = 2; break;
                    }
                    if (b != 0)
                    {
                        if (stage == 0) songGame.Add(b);
                        if (stage == 1) songTitle.Add(b);
                    }
                }
                // look 4 bytes over on next loop
                readbytes += 4;
                // infinite loop safeguard; max displayed characters for each is 32-ish
                if (readbytes > 260) stage = 2;
            }
            string gamename = Encoding.ASCII.GetString(songGame.ToArray());
            string titlename = Encoding.ASCII.GetString(songTitle.ToArray());

            if (needoverwrite) gamename = "DONKEY KONG 64";
            if (gamename.Length > 0 && titlename.Length == 0){
                titlename = gamename;
                gamename = "DONKEY KONG 64";
            }
            // send data to tracker object
            // also dont send blanks lol
            if (songGame.Count > 0 || songTitle.Count > 0)
            {
                // if its the same stuff as before, skip writing the stuff
                if (gamename == currentSongGame && currentSongTitle == titlename) return;
                if (songPanel != null)
                {
                    if (songPanel.Visible) songPanel.SetNames(gamename, titlename);
                }
                if (form.Settings.WriteSongDataToFile != Settings.SongFileWriteOption.Disabled) WriteSongData(gamename, titlename);
                currentSongGame = gamename;
                currentSongTitle = titlename;
            }
        }

        private void UpdatePointers()
        {
            
                for (int i = 0; i < globalPointers.Count; i++)
                {
                    globalPointers[i] = (uint)GoRead(globalPointerBaseAddr[i], globalPointerBaseBytes[i]) - 0x80000000;
                    //Debug.WriteLine($"updating point {i} to {globalPointers[i]}");
                }
        }

        private void WriteSongData(string game, string title)
        {
            if (form.Settings.WriteSongDataToFile == Settings.SongFileWriteOption.Multi)
            {
                File.WriteAllText(@"autotrack_song_title.txt", $"{title}");
                File.WriteAllText(@"autotrack_song_game.txt", $"{game}");
            } else
            {
                File.WriteAllText(@"autotrack_songs.txt", $"{game}\n{title}");
            }
        }

        private bool LockCalc(TrackedAddress inp)
        {
            return (inp.usesPointer > 0) ? false : inp.locked;
        }

        private void MainTracker(object state, ElapsedEventArgs e)
        {
            FlushGroups();
            alreadyReads.Clear();
            if (LZTracking) UpdateCurrentMap();
            if (SongTracking && form.Settings.EnableSongTracking) UpdateNowPlaying();
            if (PointerTracking) UpdatePointers();
            foreach (var ta in trackedAddresses)
            {
                if (VerifyGameState())
                {
                    if (!ta.enabled) continue;
                    if (ta.group != "")
                    {
                        TrackedGroup result = trackedGroups.Find(x => x.name.Equals(ta.group));
                        if (result != null)
                        {
                            //Debug.WriteLine("looking up group " + result.name);
                            // read the address, add it to result.
                            //int dt = GoRead(ta.address, ta.numBytes);
                            //Debug.WriteLine(ta.name + ": " + dt);
                            //Debug.WriteLine(" ");
                            if (ta.bitmask!= 0)
                            {
                                if (!LockCalc(ta))
                                {
                                    if ((GoRead(ta.address, ta.numBytes, ta.usesPointer) & ta.bitmask) == ta.bitmask)
                                    {
                                        if (result.isDouble)
                                        {
                                            if (ta.type == "doubleitem_l")
                                            {
                                                result.runningvalue ^= 1;
                                            }
                                            else
                                            {
                                                result.runningvalue ^= 2;
                                            }
                                        }
                                        else
                                        {
                                            result.runningvalue++;
                                            ta.locked = true;
                                        }
                                    }
                                } else
                                {
                                    if (result.isDouble)
                                    {
                                        if (ta.type == "doubleitem_l")
                                        {
                                            result.runningvalue ^= 1;
                                        }
                                        else
                                        {
                                            result.runningvalue ^= 2;
                                        }
                                    }
                                    else
                                    {
                                        result.runningvalue++;
                                    }
                                }
                                
                            } else
                            {
                                if (!LockCalc(ta))
                                {
                                    ta.currentValue = GoRead(ta.address, ta.numBytes, ta.usesPointer);
                                }
                                result.runningvalue += ta.currentValue;
                            }

                            result.count++;
                            if (result.count == result.countMax)
                            {
                                UTGrouped(result, false);
                            }
                        }
                    } else
                    {
                        if (!LockCalc(ta)) UTSingle(ta, GoRead(ta.address, ta.numBytes, ta.usesPointer));
                    }
                }
                else
                {
                    break;
                }

            }
        }

        public void AttemptSpoilerUpdate(int dk_id, MarkedImageIndex OGMarkedIndex, int readValue=1)
        {
            if (LZTracking && dk_id != -1)
            {
                spoilerPanel.AddFromAT(currentMapValue, dk_id, readValue, OGMarkedIndex);
            }
        }

        public int GoRead(uint addr, int numOfBits, int usesPointer = 0)
        {
            // save on reading from memory if we've already read that this cycle (really only matters with console)
            AlreadyRead result = alreadyReads.Find(x => x.address.Equals(addr) && x.bits.Equals(numOfBits));
            if (result != null)
            {
                return result.value;
            }
            if (usesPointer > 0)
            {
                addr += globalPointers[usesPointer -1];
            }
            if (!is64)
            {
                switch (numOfBits)
                {
                    case 8:
                        var value8 = Memory.ReadInt8(emuHandle, offset + Memory.Int8AddrFix(addr));
                        alreadyReads.Add(new AlreadyRead { value = value8, address = addr, bits = numOfBits });
                        return value8;
                    case 16:
                        var value16 = Memory.ReadInt16(emuHandle, offset + Memory.Int16AddrFix(addr));
                        alreadyReads.Add(new AlreadyRead { value = value16, address = addr, bits = numOfBits });
                        return value16;
                    case 32:
                        var value32 = Memory.ReadInt32(emuHandle, offset + addr);
                        alreadyReads.Add(new AlreadyRead { value = value32, address = addr, bits = numOfBits });
                        return value32;
                    default:
                        Debug.WriteLine("could not define how many bits to read");
                        return 0;
                }
            } else
            {
                switch (numOfBits)
                {
                    case 8:
                        var value8 = Memory.ReadInt8(emuHandle, offset64 + Memory.Int8AddrFix(addr));
                        alreadyReads.Add(new AlreadyRead { value = value8, address = addr, bits = numOfBits });
                        return value8;
                    case 16:
                        var value16 = Memory.ReadInt16(emuHandle, offset64 + Memory.Int16AddrFix(addr));
                        alreadyReads.Add(new AlreadyRead { value = value16, address = addr, bits = numOfBits });
                        return value16;
                    case 32:
                        var value32 = Memory.ReadInt32(emuHandle, offset64 + addr);
                        alreadyReads.Add(new AlreadyRead { value = value32, address = addr, bits = numOfBits });
                        return value32;
                    default:
                        Debug.WriteLine("could not define how many bits to read");
                        return 0;
                }
            }
            
        }

        private void UTSingle(TrackedAddress ta, int theRead)
        {
            if (ta.currentValue != theRead)
            {
                if (ta.targetControl is Item it)
                {
                    UpdateTrackerItem(it, ta, theRead);
                } 
                else if (ta.targetControl is Medallion md)
                {
                    UpdateTrackerMedallion(md, ta, theRead);
                }
                else if(ta.targetControl is Song sg)
                {
                    UpdateTrackerSong(sg, ta, theRead);
                }
                else if(ta.targetControl is CollectedItem ci)
                {
                    UpdateTrackerCollectable(ci, ta, theRead);
                }

                
                ta.currentValue = theRead;
                if (ta.bitmask != 0 && (ta.currentValue^ta.bitmask) == ta.bitmask) ta.locked = true;
            }
        }

        private void UTGrouped(TrackedGroup tg, bool forceUpdate = false)
        {
            if (tg.currentValue != tg.runningvalue || forceUpdate)
            {
                //Debug.WriteLine(tg.name + ": " + tg.runningvalue);

                if (tg.targetControl is CollectedItem ci)
                {
                    // i could abosutely opimize this bit but i dont think i care lmao
                    if (ci.AutoName == tg.name)
                    {
                        if (ci.AutoSubName != null && form.Settings.SubtractItems)
                        {
                            UpdateTrackerCollectable(ci, new TrackedAddress(), tg.runningvalue - GetSubtractValue(ci.AutoSubName));
                        }
                        else
                        {
                            UpdateTrackerCollectable(ci, new TrackedAddress(), tg.runningvalue);
                        }
                    }
                    else if (ci.AutoSubName == tg.name && form.Settings.SubtractItems)
                    {
                        UpdateTrackerCollectable(ci, new TrackedAddress(), GetSubtractValue(ci.AutoName) - tg.runningvalue);
                    }
                } else if (tg.targetControl is DoubleItem di)
                {
                    UpdateTrackerDoubleItem(di, tg);
                } else if (tg.targetControl is Item it)
                {
                    UpdateTrackerItem(it, tg.runningvalue);
                }

                tg.currentValue = tg.runningvalue;
            }
        }

        private void UpdateTrackerItem(Item theItem, TrackedAddress ta, int theRead)
        {
            if (ta.bitmask != 0)
            {
                var theumuh = theRead & ta.bitmask;
                if (theumuh == ta.bitmask)
                {
                    //Debug.WriteLine($"move {ta.name} - umuh {theumuh} - cv {ta.currentValue}");
                    if (theumuh != (ta.currentValue & ta.bitmask))
                    {
                        //Debug.WriteLine("umuh and cv mismatch, should be a real item");
                        theItem.SetState(1 + ta.offset);
                        AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
                    }
                } else if (theumuh == 0)
                {
                    theItem.SetState(0 + ta.offset);
                }
            }
            else
            {
                theItem.SetState(theRead + ta.offset);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0, theRead);
            }
        }

        private void UpdateTrackerItem(Item theItem, int theRead)
        {
            theItem.SetState(theRead);
        }

        private void UpdateTrackerCollectable(CollectedItem theItem, TrackedAddress ta, int theRead)
        {
            if (ta.bitmask != 0)
            {
                var theumuh = theRead & ta.bitmask;
                if (theumuh != 0)
                {
                    // have a funny feeling that this is supposed to be umuh and not 1
                    int goingIn = 1 + ta.offset;
                    theItem.SetState(goingIn);
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
                }
            }
            else
            {
                int goingIn = theRead + ta.offset;
                theItem.SetState(goingIn);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
            }
        }

        private void UpdateTrackerCollectableSub(CollectedItem theItem, TrackedAddress ta, int theSub)
        {
            int theRead = GetSubtractValue(theItem.AutoName);
            if (ta.bitmask != 0)
            {
                var theumuh = theRead & ta.bitmask;
                if (theumuh != 0)
                {
                    // have a funny feeling that this is supposed to be umuh and not 1
                    int goingIn = 1 + ta.offset - theSub;
                    theItem.SetState(goingIn);
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
                }
            }
            else
            {
                int goingIn = theRead + ta.offset - theSub;
                theItem.SetState(goingIn);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
            }
        }

        private void UpdateTrackerDoubleItem(DoubleItem theItem, TrackedGroup tg)
        {
            theItem.SetState(tg.runningvalue);
            if ((tg.runningvalue & 1) != (tg.currentValue & 1)) AttemptSpoilerUpdate(tg.left_dk64_id, (tg.targetControl != null) ? tg.targetControl.isMarked : 0);
            if ((tg.runningvalue & 2) != (tg.currentValue & 2)) AttemptSpoilerUpdate(tg.right_dk64_id, (tg.targetControl != null) ? tg.targetControl.isMarked : 0);
        }

        private void UpdateTrackerMedallion(Medallion theItem, TrackedAddress ta, int theRead)
        {
            if (ta.bitmask != 0)
            {
                var theumuh = theRead & ta.bitmask;
                if (theumuh == ta.bitmask)
                {
                    theItem.SetImageState(1 + ta.offset);
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
                }
                else if (theumuh == 0)
                {
                    theItem.SetImageState(0 + ta.offset);
                }
            }
            else
            {
                theItem.SetImageState(theRead + ta.offset);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
            }
        }

        private void UpdateTrackerSong(Song theItem, TrackedAddress ta, int theRead)
        {
            if (ta.bitmask != 0)
            {
                var theumuh = theRead & ta.bitmask;
                if (theumuh == ta.bitmask)
                {
                    theItem.SetState(1 + ta.offset);
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
                }
                else if (theumuh == 0)
                {
                    theItem.SetState(0 + ta.offset);
                }
            }
            else
            {
                theItem.SetState(theRead + ta.offset);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : 0);
            }
        }

        public bool VerifyGameState()
        {
            if (VerifyGame())
            {
                if (GoRead(desiredGameStateAddr, desiredGameStateBytes) == desiredGameStateValue)
                {
                    return true;
                }
            }
            Debug.WriteLine("not in adventure mode: " + GoRead(desiredGameAddr, desiredGameBytes) + " is not " + desiredGameStateValue);
            return false;
        }

        public bool VerifyGame()
        {
            if (GoRead(desiredGameAddr, desiredGameBytes) == desiredGameValue)
            {
                timeout = 0;
                return true;
            }
            timeout++;
            if (timeout > 10)
            {
                NukeTimer();
                MessageBox.Show("Lost connection to " + emulator.ProcessName + ". Please reconnect.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine("incorrect game. stopping timer.");
            }
            return false;
        }

        private void FlushGroups()
        {
            foreach (TrackedGroup tg in trackedGroups)
            {
                tg.runningvalue = 0;
                tg.count = 0;
            }
        }

        private void SetGameStateTargets(string gameFile)
        {
            // lookup the game in its csv from the main
            string pathto = Application.StartupPath + "/Autotrackers/" + gameFile;
            List <string> foundGroups = new List<string>();

            string[] lines = System.IO.File.ReadAllLines(pathto);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                // ignore the header row lol
                if (parts[0] == "name") { continue; }

                if (parts[0] == "game_state")
                {
                    desiredGameStateAddr = (uint)Convert.ToInt32(parts[1], 16);
                    desiredGameStateBytes = int.Parse(parts[2]);
                    desiredGameStateValue = (uint)Convert.ToInt32(parts[3], 16);
                    continue;
                }
                else if (parts[0] == "game_verification")
                {
                    desiredGameAddr = (uint)Convert.ToInt32(parts[1], 16);
                    desiredGameBytes = int.Parse(parts[2]);
                    desiredGameValue = (uint)Convert.ToInt32(parts[3], 16);
                    continue;
                } 
                else if (parts[0] == "game_current_map")
                {
                    currentMapAddr = (uint)Convert.ToInt32(parts[1], 16);
                    currentMapBytes = int.Parse(parts[2]);
                    continue;
                }
                else if (parts[0] == "game_current_song_pointer")
                {
                    currentSongAddr = (uint)Convert.ToInt32(parts[1], 16);
                    currentSongBytes = int.Parse(parts[2]);

                    continue;
                }
                else if (parts[0] == "game_map_timer")
                {
                    // used for song display
                    currentMapTimerAddr = (uint)Convert.ToInt32(parts[1], 16);
                    continue;
                }
                else if (parts[0] == "game_rando_version")
                {
                    internalRandoVersion = GoRead((uint)Convert.ToInt32(parts[1], 16), int.Parse(parts[2]));
                    Debug.WriteLine($"rando v{internalRandoVersion}");
                    continue;
                }
                else if (parts[0].StartsWith("game_global_move_pointer"))
                {
                    // used for pointer-ed items
                    globalPointers.Add((uint)GoRead((uint)Convert.ToInt32(parts[1], 16), int.Parse(parts[2])) - 0x80000000);
                    globalPointerBaseAddr.Add((uint)Convert.ToInt32(parts[1], 16));
                    globalPointerBaseBytes.Add(int.Parse(parts[2]));
                    continue;
                }

                // if a system variable is made that isnt one fo the 7 above, its wrong and gets ignored
                if (parts[6] == "system") continue;

                // dont add to thing if theres no address set (for me slowly adding things)
                if (parts[1] != "")
                {
                    TrackedAddress temp = new TrackedAddress();
                    // doing this part first to prevent groups from getting the wrong numbers
                    int requiredversion = 0;
                    if (parts[9] != "")
                    {
                        // prevent new address from being tracked on older versions
                        requiredversion = int.Parse(parts[9]);
                        //Debug.WriteLine($"{temp.name} - {requiredversion} - {internalRandoVersion}");
                        if (requiredversion > internalRandoVersion) continue;
                    }
                    temp.version = requiredversion;
                    temp.name = parts[0];
                    temp.group = parts[7];
                    // for 5.0, probably do a check where if temp.name or temp.group is already found as a TA or TG, then just drop it
                    TrackedAddress ar = trackedAddresses.Find(x => x.name.Equals(temp.name));
                    if (ar != null)
                    {
                        if (ar.version > requiredversion)
                        {
                            Debug.WriteLine("Item " + temp.name + " is being discarded because its version " + requiredversion + " is less than the required " + ar.version);
                            continue;
                        }
                    }
                    TrackedGroup gr = trackedGroups.Find(x => x.name.Equals(temp.group));
                    if (gr != null)
                    {
                        if (gr.version > requiredversion)
                        {
                            Debug.WriteLine("Item " + temp.name + " of group " + temp.group + " is being discarded because its version " + requiredversion + " is less than the required " + gr.version);
                            continue;
                        }
                    }
                    temp.address = (uint)Convert.ToInt32(parts[1], 16);
                    temp.numBytes = int.Parse(parts[2]);
                    if (parts[3] != "")
                    {
                        temp.targetValue = (uint)Convert.ToInt32(parts[3], 16);
                    }
                    else
                    {
                        temp.targetValue = 0;
                    }
                    if (parts[4] != "")
                    {
                        temp.bitmask = int.Parse(parts[4]);
                    }
                    else
                    {
                        temp.bitmask = 0;
                    }
                    if (parts[5] != "")
                    {
                        temp.offset = int.Parse(parts[5]);
                    }
                    else
                    {
                        temp.offset = 0;
                    }
                    temp.type = parts[6];
                    //Debug.WriteLine(temp.name + " :: " + temp.bitmask);
                    if (parts[8] != "")
                    {
                        temp.dk64_id = int.Parse(parts[8]);
                    } else
                    {
                        temp.dk64_id = -1;
                    }

                    if (parts[10] != "")
                    {
                        if (parts[10] == "TRUE")
                        {
                            temp.usesPointer = 1;
                        } else
                        {
                            temp.usesPointer = int.Parse(parts[10]);
                        }
                        PointerTracking = true;
                    } else
                    {
                        temp.usesPointer = 0;
                    }


                    if (temp.group != "")
                    {
                        if (!foundGroups.Contains(temp.group))
                        {
                            bool isD = (temp.type == "doubleitem_l" || temp.type == "doubleitem_r");
                            foundGroups.Add(temp.group);
                            if (isD)
                            {
                                if (temp.type == "doubleitem_l") trackedGroups.Add(new TrackedGroup() { name = temp.group, countMax = 1, isDouble = true, left_dk64_id = temp.dk64_id, version = requiredversion });
                                if (temp.type == "doubleitem_r") trackedGroups.Add(new TrackedGroup() { name = temp.group, countMax = 1, isDouble = true, right_dk64_id = temp.dk64_id, version = requiredversion });
                            }
                            else
                            {
                                trackedGroups.Add(new TrackedGroup() { name = temp.group, countMax = 1, version = requiredversion });
                            }
                        }
                        else
                        {
                            TrackedGroup result = trackedGroups.Find(x => x.name.Equals(temp.group));
                            result.countMax++;
                            if (temp.type == "doubleitem_l") result.left_dk64_id = temp.dk64_id;
                            if (temp.type == "doubleitem_r") result.right_dk64_id = temp.dk64_id;
                        }
                    }
                    trackedAddresses.Add(temp);
                }
            }
        }

        private int GetSubtractValue(string name)
        {
            if (name == null) return 0;
            var subAddress = trackedAddresses.Find(x => x.name == name);
            var subGroup = trackedGroups.Find(x => x.name == name);
            if (subAddress != null) return subAddress.currentValue;
            else if (subGroup != null) return subGroup.currentValue;
            else return 0;
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        private void StartTimer()
        {
            timer.Start();
        }

        public void NukeTimer()
        {
            if (timer != null)
            {
                timer.Elapsed -= MainTracker;
                timer.Stop();
                timer.Close();
            }
            timer = null;
            form.SetMenuAutotrackerCheck(false);
        }

        public void UpdateFromSettings()
        {
            if (timer != null)
            {
                foreach (var tg in trackedGroups)
                {
                    UTGrouped(tg, true);
                }
            }
        }
    }
}
