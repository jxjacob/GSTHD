using System;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GSTHD
{
    public static class AttachToEmulators
    {
        public static Tuple<uint, int, uint> getGameVerificationInfo(string gameFile)
        {
            string pathto = Application.StartupPath + "/Autotrackers/" + gameFile;

            string[] lines = System.IO.File.ReadAllLines(pathto);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts[0] == "game_verification")
                {
                    // Address, Bytes, Value
                    return Tuple.Create((uint)Convert.ToInt32(parts[1], 16), int.Parse(parts[2]), (uint)Convert.ToInt32(parts[3], 16));
                }
            }
            MessageBox.Show("Could not attach to emulator. Could not find \"game_verification\" entry within " + gameFile, "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        public static Tuple<Process, uint> attachToProject64(Form1 baseForm, bool doOffsetScan = false)
        {
            Process target = null;
            try
            {
                target = Process.GetProcessesByName("project64")[0];
            }
            catch (Exception)
            {
                return null;
            }



            uint romAddrStart = 0;
            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);
            if (doOffsetScan)
            {
                Debug.WriteLine("going in");
                // bruteforce different pj64 offsets
                for (uint potOff = 0xDFD00000; potOff < 0xE01FFFFF; potOff += 16)
                {

                    // checks the emu for the verification bits
                    if (gameInfo.Item2 == 8)
                    {
                        uint addr = Memory.Int8AddrFix(gameInfo.Item1);
                        if (Memory.ReadInt8(target, potOff + addr) == gameInfo.Item3)
                        {
                            Debug.WriteLine(potOff.ToString("X"));
                            romAddrStart = potOff;
                            break;
                        }
                    }
                    else if (gameInfo.Item2 == 16)
                    {
                        uint addr = Memory.Int16AddrFix(gameInfo.Item1);
                        if (Memory.ReadInt16(target, potOff + addr) == gameInfo.Item3)
                        {
                            Debug.WriteLine(potOff.ToString("X"));
                            romAddrStart = potOff;
                            break;
                        }
                    }
                    else if (gameInfo.Item2 == 32)
                    {
                        if (Memory.ReadInt32(target, potOff + gameInfo.Item1) == gameInfo.Item3)
                        {
                            Debug.WriteLine(potOff.ToString("X"));
                            romAddrStart = potOff;
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Incorrect bytes set for verification.\nMust be either 8, 16, or 32", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                Debug.WriteLine("Done");
                return Tuple.Create(target, romAddrStart);
            }

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        romAddrStart = 0xDFE40000;
                        break;
                    case 1:
                        romAddrStart = 0xDFE70000;
                        break;
                    case 2:
                        romAddrStart = 0xDFFB0000;
                        break;
                    default:
                        Debug.WriteLine("wasnt those 3 addresses");
                        return null;

                }

                int gamecheck = 0;
                try
                {
                    if (gameInfo.Item2 == 8)
                    {
                        uint addr = Memory.Int8AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt8(target, romAddrStart + addr);
                    }
                    else if (gameInfo.Item2 == 16)
                    {
                        uint addr = Memory.Int16AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt16(target, romAddrStart + addr);
                    }
                    else if (gameInfo.Item2 == 32)
                    {
                        gamecheck = Memory.ReadInt32(target, romAddrStart + gameInfo.Item1);
                    }
                    else
                    {
                        MessageBox.Show("Incorrect bytes set for verification.\nMust be either 8, 16, or 32", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("yeah bud shits fucked");
                    MessageBox.Show(e.Message, "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (gamecheck == gameInfo.Item3)
                {
                    Debug.WriteLine("verifyably pj64");
                    return Tuple.Create(target, romAddrStart);
                }

            }
            return null;
        }


        public static Tuple<Process, uint> attachToBizhawk(Form1 baseForm)
        {
            Process target = null;
            try
            {
                target = Process.GetProcessesByName("emuhawk")[0];
            }
            catch (Exception)
            {
                return null;
            }
            Debug.WriteLine("found hawk");

            uint romAddrStart = 0;
            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);


            Int64 addressDLL = 0;
            foreach (ProcessModule mo in target.Modules)
            {
                if (mo.ModuleName.ToLower() == "mupen64plus.dll")
                {
                    addressDLL = mo.BaseAddress.ToInt64();
                    break;
                }
            }

            if (addressDLL == 0)
            {
                return null;
            }
            Debug.WriteLine("found dll");

            //Dim attemptOffset As Int64 = 0
            //for (int i = 0; i < 2; i++){
            //    switch (i)
            //    {
            //        case 0:
            //            romAddrStart = 0x658E0;
            //            break;
            //        case 1:
            //            romAddrStart = 0x658D0;
            //            break;
            //        default:
            //            return null;
            //    }

            //i'm too lazy to find common addresses, so i'm just gonna do a light bruteforce
            for (uint potOff = 0x5A000; potOff < 0x5658DF; potOff += 16)
            {
                romAddrStart = potOff;



                int gamecheck = 0;
                try
                {
                    if (gameInfo.Item2 == 8)
                    {
                        uint addr = Memory.Int8AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt8(target, romAddrStart + addr);
                    }
                    else if (gameInfo.Item2 == 16)
                    {
                        uint addr = Memory.Int16AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt16(target, romAddrStart + addr);
                    }
                    else if (gameInfo.Item2 == 32)
                    {
                        gamecheck = Memory.ReadInt32(target, (uint)(addressDLL + romAddrStart + gameInfo.Item1));
                    }
                    else
                    {
                        MessageBox.Show("Incorrect bytes set for verification.\nMust be either 8, 16, or 32", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("yeah bud shits fucked");
                    MessageBox.Show(e.Message, "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                
                if (gamecheck == gameInfo.Item3)
                {
                    
                    Debug.WriteLine("verifyably bizhawk");
                    return Tuple.Create(target, (uint)(addressDLL + romAddrStart));
                }

            }





            return null;
        }


        public static Tuple<Process, ulong> attachToRMG(Form1 baseForm)
        {
            Process target = null;
            try
            {
                target = Process.GetProcessesByName("rmg")[0];
            }
            catch (Exception)
            {
                return null;
            }
            Debug.WriteLine("trans rights");

            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);


            ulong addressDLL = 0;
            foreach (ProcessModule mo in target.Modules)
            {
                if (mo.ModuleName.ToLower() == "mupen64plus.dll")
                {
                    addressDLL = (ulong)mo.BaseAddress.ToInt64();
                    break;
                }
            }

            if (addressDLL == 0)
            {
                return null;
            }
            Debug.WriteLine("found dll at 0x" + addressDLL.ToString("X"));

            for (uint potOff = 0x29C15D8; potOff < 0x2FC15D8; potOff += 16)
            {
                ulong romAddrStart = addressDLL + potOff;


                // read the address to find the address of the starting point in the rom
                ulong readAddress = Memory.ReadInt64(target, (romAddrStart));

                if (gameInfo.Item2 == 8)
                {
                    var addr = Memory.Int8AddrFix(readAddress + 0x80000000 + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt8(target, addr);
                    if ((wherethefuck & 0xff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress + 0x80000000));

                    }
                }
                else if (gameInfo.Item2 == 16)
                {
                    var addr = Memory.Int16AddrFix(readAddress + 0x80000000 + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt16(target, addr);
                    if ((wherethefuck & 0xffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress + 0x80000000));

                    }
                }
                else if (gameInfo.Item2 == 32)
                {
                    // use this previously read address to find the game verification data
                    var wherethefuck = Memory.ReadInt32(target, (readAddress + 0x80000000 + gameInfo.Item1));
                    if ((wherethefuck & 0xffffffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress + 0x80000000));

                    }
                }
                else
                {
                    MessageBox.Show("Incorrect bytes set for verification.\nMust be either 8, 16, or 32", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }




            }

            return null;
        }
    }
}
