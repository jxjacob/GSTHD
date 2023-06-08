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

namespace GSTHD
{
    public class TrackedAddress
    {
        public string name;
        public uint address;
        public int numBytes;
        public int currentValue = 0;
        public uint targetValue;
        public string type;
        public string group;
    }
    public class TrackedGroup
    {
        public string name;
        public int runningvalue = 0;
        public int currentValue;
        public int count = 0;
        public int countMax;
    }
    public class Autotracker
    {
        private Process emulator;
        private uint offset;

        private List<TrackedAddress> trackedAddresses = new List<TrackedAddress>();
        private List<TrackedGroup> trackedGroups = new List<TrackedGroup>();

        private uint desiredGameAddr;
        private int desiredGameBytes;
        private uint desiredGameValue;

        private uint desiredGameStateAddr;
        private int desiredGameStateBytes;
        private uint desiredGameStateValue;

        private System.Threading.Timer timer;
        private Form1 form;

        public Autotracker(Process theProgram, uint foundOffset, Form1 theForm)
        {
            emulator = theProgram;
            offset = foundOffset;
            form = theForm;

            if (form.CurrentLayout.App_Settings.AutotrackingGame != null)
            {
                SetGameStateTargets(form.CurrentLayout.App_Settings.AutotrackingGame);

                Debug.WriteLine("Beginning timer with " + trackedAddresses.Count + " addresses and " + trackedGroups.Count + " groups");
                timer = new System.Threading.Timer(MainTracker, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }

        private void MainTracker(object state)
        {
            FlushGroups();
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
                            result.runningvalue += GoRead(ta.address, ta.numBytes);
                            result.count++;
                            if (result.count == result.countMax)
                            {
                                UTGrouped(result);
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

        public int GoRead(uint addr, int numOfBits)
        {
            //Debug.WriteLine("reading " + numOfBits + " bits at address " + addr);
            // address fixing (endian bullshit)
            if (numOfBits == 8)
            {
                switch (addr % 4)
                {
                    case 0:
                        addr += 3;
                        break;
                    case 1:
                        addr += 1;
                        break;
                    case 2:
                        addr -= 1;
                        break;
                    case 3:
                        addr -= 3;
                        break;
                }
            } else if (numOfBits == 16)
            {
                // there is definitely a better way of implementing this with a smaller modulo but idc
                switch (addr % 16) {
                    case 2:
                    case 3:
                    case 6:
                    case 7:
                    case 10:
                    case 11:
                    case 14:
                    case 15:
                        addr -= 2;
                        break;
                    case 0:
                    case 1:
                    case 4:
                    case 5:
                    case 8:
                    case 9:
                    case 12:
                    case 13:
                        addr += 2;
                        break;
                }
            }


            switch (numOfBits)
            {
                case 8:
                    return Memory.ReadInt8(emulator, offset + addr);
                case 16:
                    return Memory.ReadInt16(emulator, offset + addr);
                case 32:
                    return Memory.ReadInt32(emulator, offset + addr);
                default:
                    Debug.WriteLine("could not define how many bits to read");
                    return 0;
            }
        }

        private void UTSingle(TrackedAddress ta, int theRead)
        {
            if (ta.currentValue != theRead)
            {
                Debug.WriteLine(ta.name + ": " + theRead);

                // i can't fucking stand this method
                foreach (Control thing in form.Controls[0].Controls)
                {
                    if (ta.type == "item" && thing is Item)
                    {
                        if (((Item)thing).AutoName == ta.name)
                        {
                            UpdateTrackerItem((Item)thing, theRead);
                        }

                    } else if (ta.type == "collectable" && thing is CollectedItem)
                    {
                        if (((CollectedItem)thing).AutoName == ta.name)
                        {
                            UpdateTrackerCollectable((CollectedItem)thing, theRead);
                        }

                    }
                }
                ta.currentValue = theRead;
            }
        }

        private void UTGrouped(TrackedGroup tg)
        {
            if (tg.currentValue != tg.runningvalue)
            {
                Debug.WriteLine(tg.name + ": " + tg.runningvalue);

                // i can't fucking stand this method
                foreach (Control thing in form.Controls[0].Controls)
                {
                    // cant think of a use case for a merged group but for single items so um uh
                    if (thing is CollectedItem)
                    {
                        if (((CollectedItem)thing).AutoName == tg.name)
                        {
                            UpdateTrackerCollectable((CollectedItem)thing, tg.runningvalue);
                        }

                    }
                }
                tg.currentValue = tg.runningvalue;
            }
        }

        private void UpdateTrackerItem(Item theItem, int theRead)
        {
            if (theItem.AutoBitmask != 0)
            {
                var theumuh = theRead & theItem.AutoBitmask;
                if (theumuh != 0)
                {
                    theItem.SetState(1 + theItem.AutoOffset);
                    //break;
                }
            }
            else
            {
                theItem.SetState(theRead + theItem.AutoOffset);
            }
        }

        private void UpdateTrackerCollectable(CollectedItem theItem, int theRead)
        {
            if (theItem.AutoBitmask != 0)
            {
                var theumuh = theRead & theItem.AutoBitmask;
                if (theumuh != 0)
                {
                    theItem.SetState(1 + theItem.AutoOffset);
                    //break;
                }
            }
            else
            {
                theItem.SetState(theRead + theItem.AutoOffset);
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
            Debug.WriteLine("not in adventure mode: " + GoRead(desiredGameAddr, desiredGameBytes) + " is not " + desiredGameValue);
            return false;
        }

        public bool VerifyGame()
        {
            if (GoRead(desiredGameAddr, desiredGameBytes) == desiredGameValue)
            {
                return true;
            }
            Debug.WriteLine("incorrect game. stopping timer.");
            StopTimer();
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
                    temp.type = parts[4];
                    temp.group = parts[5];
                    if (temp.group != "")
                    {
                        if (!foundGroups.Contains(temp.group))
                        {
                            foundGroups.Add(temp.group);
                            trackedGroups.Add(new TrackedGroup() { name = temp.group, countMax = 1 });
                        } else
                        {
                            TrackedGroup result = trackedGroups.Find(x => x.name.Equals(temp.group));
                            result.countMax++;
                        }
                    }
                    trackedAddresses.Add(temp);
                }
            }
        }

        public void StopTimer()
        {
            timer.Change(-1, -1);
        }

        private void StartTimer()
        {
            timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void NukeTimer()
        {
            timer.Dispose();
        }
    }
}
