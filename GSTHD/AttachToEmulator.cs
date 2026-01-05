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
    public class EmulatorInfo
    {
        public Settings.SelectEmulatorOption ID;

        public string ReadableEmulatorName;
        public string ProcessName;

        public bool FindDLL;
        public string DLLName;
        public bool AdditionalLookup;

        public uint LowerOffsetRange;
        public uint UpperOffsetRange;
        public uint RangeStep;

        public uint ExtraOffset;

        public EmulatorInfo(Settings.SelectEmulatorOption option, string readable, string process, bool lookup, string dll, bool additional, uint lower, uint upper, uint step = 16, uint extraOffset = 0)
        {
            ID = option;
            ReadableEmulatorName = readable;
            ProcessName = process;
            FindDLL = lookup;
            DLLName = dll;
            AdditionalLookup = additional;
            LowerOffsetRange = lower;
            UpperOffsetRange = upper;
            RangeStep = step;
            ExtraOffset = extraOffset;
        }
    }
    public static class AttachToEmulators
    {
        private static readonly Dictionary<Settings.SelectEmulatorOption, EmulatorInfo> EmulatorConfigs = new Dictionary<Settings.SelectEmulatorOption, EmulatorInfo>
        {
            { Settings.SelectEmulatorOption.Project64, new EmulatorInfo(Settings.SelectEmulatorOption.Project64, "Project64", "project64", false, null, false, 0xDFD00000, 0xE01FFFFF) },
            { Settings.SelectEmulatorOption.Project64_4, new EmulatorInfo(Settings.SelectEmulatorOption.Project64_4, "Project64", "project64", false, null, false, 0xFDD00000, 0xFE1FFFFF) },
            { Settings.SelectEmulatorOption.Bizhawk, new EmulatorInfo(Settings.SelectEmulatorOption.Bizhawk, "Bizhawk", "emuhawk", true, "mupen64plus.dll", false, 0x5A000, 0x5658DF)},
            { Settings.SelectEmulatorOption.RMG, new EmulatorInfo(Settings.SelectEmulatorOption.RMG, "Rosalie's Mupen GUI", "rmg" , true, "mupen64plus.dll", true, 0x29C15D8, 0x2FC15D8, extraOffset:0x80000000) },
            { Settings.SelectEmulatorOption.simple64, new EmulatorInfo(Settings.SelectEmulatorOption.simple64, "simple64", "simple64-gui", true, "libmupen64plus.dll", true, 0x1380000, 0x29C95D8) },
            { Settings.SelectEmulatorOption.parallel, new EmulatorInfo(Settings.SelectEmulatorOption.parallel, "Parallel Launcher", "retroarch", true, "parallel_n64_next_libretro.dll", true, 0x845000, 0xD56000) },
            { Settings.SelectEmulatorOption.retroarch, new EmulatorInfo(Settings.SelectEmulatorOption.retroarch, "RetroArch", "retroarch", true, "mupen64plus_next_libretro.dll", true, 0, 0xFFFFFF, step:4) }
        };


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

        public static Tuple<Process, ulong> attachToEmulator(Form1 baseForm, Settings.SelectEmulatorOption emu)
        {
            EmulatorInfo EmuInfo = EmulatorConfigs[emu];


            Process target;
            try
            {
                target = Process.GetProcessesByName(EmuInfo.ProcessName)[0];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nCould not find process \"" + EmuInfo.ProcessName + "\" on your machine.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var gameInfo = getGameVerificationInfo(baseForm.CurrentLayout.App_Settings.AutotrackingGame);


            ulong addressDLL = 0;
            if (EmuInfo.FindDLL)
            {

                foreach (ProcessModule mo in target.Modules)
                {
                    if (mo.ModuleName.ToLower() == EmuInfo.DLLName)
                    {
                        addressDLL = (ulong)mo.BaseAddress.ToInt64();
                        break;
                    }
                }

                if (addressDLL == 0 && EmuInfo.ID == Settings.SelectEmulatorOption.Bizhawk)
                {
                    // bizhawk-only "fuck it we ball"
                    addressDLL = 2024407040;
                    Debug.WriteLine("guessing its at " + addressDLL.ToString("X"));
                }
                else if (addressDLL == 0)
                {
                    MessageBox.Show("Could not find " + EmuInfo.DLLName + " loaded within " + EmuInfo.ReadableEmulatorName + ".\nPlease reinstall " + EmuInfo.ReadableEmulatorName + " or reset it to its default settings.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                Debug.WriteLine("found dll at 0x" + addressDLL.ToString("X"));


            }

            progresswindow pw = new progresswindow("Connecting to " + EmuInfo.ReadableEmulatorName + "...\nAttempting to find the correct memory offset...", (int)(EmuInfo.UpperOffsetRange - EmuInfo.LowerOffsetRange));
            pw.Show();
            pw.Refresh();
            bool hasseennonzero = false;


            for (uint potOff = EmuInfo.LowerOffsetRange; potOff < EmuInfo.UpperOffsetRange; potOff += EmuInfo.RangeStep)
            {
                pw.incBar(EmuInfo.RangeStep);

                ulong readAddress = 0;
                if (EmuInfo.AdditionalLookup)
                {
                    ulong romAddrStart = addressDLL + potOff;
                    //read the address to find the address of the starting point in the rom
                    readAddress = Memory.ReadInt64(target.Handle, (romAddrStart));
                    if (readAddress != 0) hasseennonzero = true;
                    //if (EmuInfo.ID == Settings.SelectEmulatorOption.parallel)
                    //{
                    //    // uncomment this if you need to re-add mupen64plus_next_libretro.dll to parallel
                    //    readAddress = Memory.ReadInt64(target.Handle, (addressDLL + potOff + 4) & readAddress);
                    //    readAddress += 0x80000000;
                    //}
                    //if (readAddress != 0) hasseennonzero = true;
                }
                else
                {
                    readAddress = addressDLL + potOff;
                }



                if (gameInfo.Item2 == 8)
                {
                    var addr = Memory.Int8AddrFix(readAddress + EmuInfo.ExtraOffset + gameInfo.Item1);
                    var testValue = Memory.ReadInt8(target.Handle, addr);
                    if (testValue != 0) hasseennonzero = true;
                    if (testValue == gameInfo.Item3)
                    {
                        pw.Close();
                        return Tuple.Create(target, (readAddress + EmuInfo.ExtraOffset));

                    }
                }
                else if (gameInfo.Item2 == 16)
                {
                    var addr = Memory.Int16AddrFix(readAddress + EmuInfo.ExtraOffset + gameInfo.Item1);
                    var testValue = Memory.ReadInt16(target.Handle, addr);
                    if (testValue != 0) hasseennonzero = true;
                    if (testValue == gameInfo.Item3)
                    {
                        pw.Close();
                        return Tuple.Create(target, (readAddress + EmuInfo.ExtraOffset));

                    }
                }
                else if (gameInfo.Item2 == 32)
                {
                    // use this previously read address to find the game verification data
                    var testValue = Memory.ReadInt32(target.Handle, (readAddress + EmuInfo.ExtraOffset + gameInfo.Item1));
                    //Debug.WriteLine($"{testValue.ToString("X")} -- {(readAddress + EmuInfo.ExtraOffset + gameInfo.Item1).ToString("X")}");
                    if (testValue != 0) hasseennonzero = true;
                    if (testValue == gameInfo.Item3)
                    {
                        pw.Close();
                        return Tuple.Create(target, (readAddress + EmuInfo.ExtraOffset));

                    }
                }
                else
                {
                    pw.Close();
                    MessageBox.Show("Incorrect bytes set for game_verification.\nPlease confirm the value of game_verification within your autotracking csv is either 8, 16, or 32", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }



            }


            pw.Close();
            if (!hasseennonzero)
            {
                if (EmuInfo.ID == Settings.SelectEmulatorOption.Bizhawk && Environment.Is64BitProcess)
                {
                    MessageBox.Show("Could not read any data from Bizhawk; potentially due to a version mismatch between GSTHD and Bizhawk.\nSwitch to the GSTHD_32.exe program, and if the problem persists afterwards, contact JXJacob directly for further help.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else
                {
                    MessageBox.Show("Could not read any data from " + EmuInfo.ReadableEmulatorName + "; and therefore something has probably gone horribly wrong.\nRe-install " + EmuInfo.ReadableEmulatorName + ", and if the problem persists afterwards, contact JXJacob directly for further help.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            return null;
        }

       
    }
}
