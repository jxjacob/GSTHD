﻿using System;
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

namespace GSTHD
{
    public class TrackedAddress
    {
        public string name;
        public uint address;
        public int numBytes;
        public int currentValue = 0;
        public uint targetValue;
        public int bitmask = 0;
        public int offset;
        public string type;
        public string group;
        public OrganicImage targetControl;
        public int dk64_id = -1;
    }
    public class TrackedGroup
    {
        public string name;
        public int runningvalue = 0;
        public int currentValue;
        public int count = 0;
        public int countMax;
        public bool isDouble = false;
        public OrganicImage targetControl;
    }
    public class Autotracker : UpdatableFromSettings
    {
        private Process emulator;
        private IntPtr emuHandle;
        private uint offset;
        private ulong offset64;

        private List<TrackedAddress> trackedAddresses = new List<TrackedAddress>();
        private List<TrackedGroup> trackedGroups = new List<TrackedGroup>();

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

        private System.Timers.Timer timer;
        private Form1 form;

        private int timeout;
        private bool is64 = false;
        private bool LZTracking = false;

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
                Debug.WriteLine("Beginning timer with " + trackedAddresses.Count + " addresses and " + trackedGroups.Count + " groups");
                //timer = new System.Threading.Timer(MainTracker, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                timer = new System.Timers.Timer(1000);
                timer.Elapsed += MainTracker;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
        }

        private void CalibrateTracks()
        {
            // go through each address or group and store the controls that they are going to update so i can skip that godawful foreach controls for every single damn update
            // also removes addresses without a counterpart on the layout

            var tac = new List<TrackedAddress>();

            foreach (TrackedAddress ta in trackedAddresses)
            {
                if (ta.group != "")
                {
                    TrackedGroup result = trackedGroups.Find(x => x.name.Equals(ta.group));
                    if (result.targetControl != null)
                    {
                        //Debug.WriteLine("group " + result.name + " already has a targetcontrol, skipping");
                        continue;
                    }
                    foreach (Control thing in form.Controls[0].Controls)
                    {
                        if (thing is CollectedItem ci)
                        {
                            if (ci.AutoName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
                                break;
                            }
                            else if (ci.AutoSubName == result.name && form.Settings.SubtractItems)
                            {
                                result.targetControl = (OrganicImage)thing;
                                break;
                            }

                        }
                        else if (thing is DoubleItem di)
                        {
                            if (di.AutoName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
                                break;
                            }
                        }
                        else if (thing is Item it)
                        {
                            if (it.AutoName == result.name)
                            {
                                result.targetControl = (OrganicImage)thing;
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
                                break;
                            }

                        }
                        else if (ta.type == "item" && thing is Medallion md)
                        {
                            if (md.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                break;
                            }

                        }
                        else if (ta.type == "item" && thing is Song sg)
                        {
                            if (sg.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                break;
                            }

                        }
                        else if (ta.type == "collectable" && thing is CollectedItem ci)
                        {
                            if (ci.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
                                break;
                            }

                        }
                        else if (ta.type == "collectable" && thing is Item it2)
                        {
                            if (it2.AutoName == ta.name)
                            {
                                ta.targetControl = (OrganicImage)thing;
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

        private void MainTracker(object state, ElapsedEventArgs e)
        {
            FlushGroups();
            if (LZTracking) UpdateCurrentMap();
            foreach (var ta in trackedAddresses)
            {
                if (VerifyGameState())
                {
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
                                if ((GoRead(ta.address, ta.numBytes) & ta.bitmask) == ta.bitmask)
                                {
                                    if (result.isDouble) { 
                                        if (ta.type == "doubleitem_l")
                                        {
                                            result.runningvalue ^= 1;
                                        } else
                                        {
                                            result.runningvalue ^= 2;
                                        }
                                    } else
                                    {
                                        result.runningvalue++;
                                    }                                    
                                }
                            } else
                            {
                                result.runningvalue += GoRead(ta.address, ta.numBytes);
                            }

                            result.count++;
                            if (result.count == result.countMax)
                            {
                                UTGrouped(result, false);
                            }
                        }
                    } else
                    {
                        UTSingle(ta, GoRead(ta.address, ta.numBytes));
                    }
                }
                else
                {
                    break;
                }

            }
        }

        public void AttemptSpoilerUpdate(int dk_id, bool isOGMarked, int readValue=1)
        {
            if (LZTracking && dk_id != -1)
            {
                spoilerPanel.AddFromAT(currentMapValue, dk_id, readValue, isOGMarked);
            }
        }

        public int GoRead(uint addr, int numOfBits)
        {
            if (!is64)
            {
                switch (numOfBits)
                {
                    case 8:
                        return Memory.ReadInt8(emuHandle, offset + Memory.Int8AddrFix(addr));
                    case 16:
                        return Memory.ReadInt16(emuHandle, offset + Memory.Int16AddrFix(addr));
                    case 32:
                        return Memory.ReadInt32(emuHandle, offset + addr);
                    default:
                        Debug.WriteLine("could not define how many bits to read");
                        return 0;
                }
            } else
            {
                switch (numOfBits)
                {
                    case 8:
                        return Memory.ReadInt8(emuHandle, offset64 + Memory.Int8AddrFix(addr));
                    case 16:
                        return Memory.ReadInt16(emuHandle, offset64 + Memory.Int16AddrFix(addr));
                    case 32:
                        return Memory.ReadInt32(emuHandle, offset64 + addr);
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
                    UpdateTrackerDoubleItem(di, tg.runningvalue);
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
                        AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
                    }
                } else if (theumuh == 0)
                {
                    theItem.SetState(0 + ta.offset);
                }
            }
            else
            {
                theItem.SetState(theRead + ta.offset);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false, theRead);
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
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
                }
            }
            else
            {
                int goingIn = theRead + ta.offset;
                theItem.SetState(goingIn);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
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
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
                }
            }
            else
            {
                int goingIn = theRead + ta.offset - theSub;
                theItem.SetState(goingIn);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
            }
        }

        private void UpdateTrackerDoubleItem(DoubleItem theItem, int theRead)
        {
            theItem.SetState(theRead);
        }

        private void UpdateTrackerMedallion(Medallion theItem, TrackedAddress ta, int theRead)
        {
            if (ta.bitmask != 0)
            {
                var theumuh = theRead & ta.bitmask;
                if (theumuh == ta.bitmask)
                {
                    theItem.SetImageState(1 + ta.offset);
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
                }
                else if (theumuh == 0)
                {
                    theItem.SetImageState(0 + ta.offset);
                }
            }
            else
            {
                theItem.SetImageState(theRead + ta.offset);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
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
                    AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
                }
                else if (theumuh == 0)
                {
                    theItem.SetState(0 + ta.offset);
                }
            }
            else
            {
                theItem.SetState(theRead + ta.offset);
                AttemptSpoilerUpdate(ta.dk64_id, (ta.targetControl != null) ? ta.targetControl.isMarked : false);
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

                // if a system variable is made that isnt one fo the 3 above, its wrong and gets ignored
                if (parts[6] == "system") continue;

                // dont add to thing if theres no address set (for me slowly adding things)
                if (parts[1] != "")
                {
                    TrackedAddress temp = new TrackedAddress();
                    temp.name = parts[0];
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
                    temp.group = parts[7];
                    if (temp.group != "")
                    {
                        if (!foundGroups.Contains(temp.group))
                        {
                            bool isD = (temp.type == "doubleitem_l" || temp.type == "doubleitem_r") ;
                            foundGroups.Add(temp.group);
                            trackedGroups.Add(new TrackedGroup() { name = temp.group, countMax = 1, isDouble = isD });
                        } else
                        {
                            TrackedGroup result = trackedGroups.Find(x => x.name.Equals(temp.group));
                            result.countMax++;
                        }
                    }
                    //Debug.WriteLine(temp.name + " :: " + temp.bitmask);
                    if (parts[8] != "")
                    {
                        temp.dk64_id = int.Parse(parts[8]);
                    } else
                    {
                        temp.dk64_id = -1;
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
                timer = null;
            }
            timer = null;
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
