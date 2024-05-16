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

        public static Tuple<Process, uint> attachToProject64(Form1 baseForm)
        {
            Process target;
            try
            {
                target = Process.GetProcessesByName("project64")[0];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nCould not find process \"project64\" on your machine.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);

            for (uint potOff = 0xDFD00000; potOff < 0xE01FFFFF; potOff += 16)
            {
                int gamecheck;
                try
                {
                    if (gameInfo.Item2 == 8)
                    {
                        uint addr = Memory.Int8AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt8(target.Handle, potOff + addr);
                    }
                    else if (gameInfo.Item2 == 16)
                    {
                        uint addr = Memory.Int16AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt16(target.Handle, potOff + addr);
                    }
                    else if (gameInfo.Item2 == 32)
                    {
                        gamecheck = Memory.ReadInt32(target.Handle, potOff + gameInfo.Item1);
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
                    return null;
                }
                //MessageBox.Show(gamecheck.ToString("X"), "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return null;
                if (gamecheck == gameInfo.Item3)
                {
                    Debug.WriteLine("verifyably pj64");
                    return Tuple.Create(target, potOff);
                }

            }
            //MessageBox.Show("Could not find the correct PJ64 offset\nJXJacob hasn't figured out how to solve this one so you might be out of luck.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }


        public static Tuple<Process, uint> attachToBizhawk(Form1 baseForm)
        {
            Process target = null;
            Debug.WriteLine("start hawk");
            try
            {
                target = Process.GetProcessesByName("emuhawk")[0];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nCould not find process \"emuhawk\" on your machine.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // fuck it we ball
                addressDLL = 2024407040;
                Debug.WriteLine("guessing its at " + addressDLL);
            } else
            {
                Debug.WriteLine("found dll at " + addressDLL);
            }
            

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
                        gamecheck = Memory.ReadInt8(target.Handle, (uint)(addressDLL + romAddrStart + addr));
                    }
                    else if (gameInfo.Item2 == 16)
                    {
                        uint addr = Memory.Int16AddrFix(gameInfo.Item1);
                        gamecheck = Memory.ReadInt16(target.Handle, (uint)(addressDLL + romAddrStart + addr));
                    }
                    else if (gameInfo.Item2 == 32)
                    {
                        gamecheck = Memory.ReadInt32(target.Handle, (uint)(addressDLL + romAddrStart + gameInfo.Item1));
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
                    return null;
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nCould not find process \"rmg\" on your machine.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Could not find mupen64plus loaded within RMG.\nPlease reinstall RMG or reset it to its default settings.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("found dll at 0x" + addressDLL.ToString("X"));

            for (uint potOff = 0x29C15D8; potOff < 0x2FC15D8; potOff += 16)
            {
                ulong romAddrStart = addressDLL + potOff;


                // read the address to find the address of the starting point in the rom
                ulong readAddress = Memory.ReadInt64(target.Handle, (romAddrStart));

                if (gameInfo.Item2 == 8)
                {
                    var addr = Memory.Int8AddrFix(readAddress + 0x80000000 + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt8(target.Handle, addr);
                    if ((wherethefuck & 0xff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress + 0x80000000));

                    }
                }
                else if (gameInfo.Item2 == 16)
                {
                    var addr = Memory.Int16AddrFix(readAddress + 0x80000000 + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt16(target.Handle, addr);
                    if ((wherethefuck & 0xffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress + 0x80000000));

                    }
                }
                else if (gameInfo.Item2 == 32)
                {
                    // use this previously read address to find the game verification data
                    var wherethefuck = Memory.ReadInt32(target.Handle, (readAddress + 0x80000000 + gameInfo.Item1));
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

            //MessageBox.Show("Could not find the correct RMG offset\nJXJacob hasn't figured out how to solve this one so you might be out of luck.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }


        public static Tuple<Process, ulong> attachToSimple64(Form1 baseForm)
        {
            Process target = null;
            try
            {
                target = Process.GetProcessesByName("simple64-gui")[0];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nCould not find process \"simple64-gui\" on your machine.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("shoutouts to simpleflips");

            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);


            ulong addressDLL = 0;
            foreach (ProcessModule mo in target.Modules)
            {
                //Debug.WriteLine($"{mo.ModuleName} - {mo.BaseAddress.ToInt64():X}");
                if (mo.ModuleName.ToLower() == "libmupen64plus.dll")
                {
                    addressDLL = (ulong)mo.BaseAddress.ToInt64();
                    break;
                }
            }

            if (addressDLL == 0)
            {
                MessageBox.Show("Could not find libmupen64plus loaded within simple64.\nPlease reinstall simple64 or reset it to its default settings.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("found dll at 0x" + addressDLL.ToString("X"));

            for (uint potOff = 0x1380000; potOff < 0x29C95D8; potOff += 16)
            {
                // this is honest to christ a bruteforce. with biz and RMG i had a reference for getting them to work, this was a fuckin guess
                //     ToT had an option for muper64plus (which simple64 is based on), but this is a near-completely different approach
                ulong romAddrStart = addressDLL + potOff;


                // read the address to find the address of the starting point in the rom
                ulong readAddress = Memory.ReadInt64(target.Handle, (romAddrStart));

                if (gameInfo.Item2 == 8)
                {
                    var addr = Memory.Int8AddrFix(readAddress + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt8(target.Handle, addr);
                    if ((wherethefuck & 0xff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress));

                    }
                }
                else if (gameInfo.Item2 == 16)
                {
                    var addr = Memory.Int16AddrFix(readAddress + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt16(target.Handle, addr);
                    if ((wherethefuck & 0xffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress));

                    }
                }
                else if (gameInfo.Item2 == 32)
                {
                    // use this previously read address to find the game verification data
                    var wherethefuck = Memory.ReadInt32(target.Handle, (readAddress + gameInfo.Item1));
                    if ((wherethefuck & 0xffffffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, (readAddress));

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



        public static Tuple<Process, ulong> attachToParallel(Form1 baseForm)
        {
            Process target = null;
            try
            {
                target = Process.GetProcessesByName("retroarch")[0];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nCould not find process \"retroarch\" on your machine.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("fuck retroarch tbh");

            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);

            bool ismp64p = false;
            ulong addressDLL = 0;
            foreach (ProcessModule mo in target.Modules)
            {
                if (mo.ModuleName.ToLower() == "parallel_n64_next_libretro.dll")
                {
                    addressDLL = (ulong)mo.BaseAddress.ToInt64();
                    break;
                } else if (mo.ModuleName.ToLower() == "mupen64plus_next_libretro.dll")
                {
                    addressDLL = (ulong)mo.BaseAddress.ToInt64();
                    ismp64p |= true;
                    break;
                }
            }

            if (addressDLL == 0)
            {
                MessageBox.Show("Could not find either the ParaLLel or muper64plus plugins loaded within Parallel Launcher.\nPlease switch to either ParaLLel or GLideN64 graphics plugins to resolve this issue.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            Debug.WriteLine("found dll at 0x" + addressDLL.ToString("X"));

            for (uint potOff = 0x845000; potOff < 0xD56000; potOff += 16)
            {
                ulong romAddrStart = addressDLL + potOff;


                // read the address to find the address of the starting point in the rom
                ulong readAddress = Memory.ReadInt64(target.Handle, (romAddrStart));
                if (ismp64p)
                {
                    // why is retroarch like this
                    readAddress = Memory.ReadInt64(target.Handle, (readAddress + 4) & readAddress);
                    readAddress += 0x80000000;
                }

                if (gameInfo.Item2 == 8)
                {
                    var addr = Memory.Int8AddrFix(readAddress + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt8(target.Handle, addr);
                    if ((wherethefuck & 0xff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, readAddress);

                    }
                }
                else if (gameInfo.Item2 == 16)
                {
                    var addr = Memory.Int16AddrFix(readAddress + gameInfo.Item1);
                    var wherethefuck = Memory.ReadInt16(target.Handle, addr);
                    if ((wherethefuck & 0xffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, readAddress);

                    }
                }
                else if (gameInfo.Item2 == 32)
                {
                    // use this previously read address to find the game verification data
                    var wherethefuck = Memory.ReadInt32(target.Handle, (readAddress + gameInfo.Item1));
                    //if (wherethefuck != 0 && wherethefuck != -954194860) Debug.WriteLine($"{wherethefuck} -- {potOff}");
                    if ((wherethefuck & 0xffffffff) == gameInfo.Item3)
                    {
                        return Tuple.Create(target, readAddress);

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
