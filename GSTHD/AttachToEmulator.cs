using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GSTHD
{
    public static class AttachToEmulators
    {
        //Contains all the functions to attack to each emulator

        //    Public Function attachToProject64(Optional doOffsetScan As Boolean = False) As Process
        public static Tuple<Process, uint> attachToProject64(bool doOffsetScan = false)
        {
            //Dim target As Process = Nothing
            Process target = null;

            //            ' Try to attach to the first instance of project64
            //            Try
            //                target = Process.GetProcessesByName("project64")(0)
            //            Catch ex As Exception
            //                Return Nothing
            //            End Try
            try
            {
                target = Process.GetProcessesByName("project64")[0];
            } catch(Exception e)
            {
                return null;
            }


            //            If doOffsetScan Then
            //                ' So some people's pj64 has a different offset, this will help determine it
            //                For i = &HDFD00000 To &HE01FFFFF Step 16
            //                    If Memory.ReadInt32(target, i + &H11A5EC) = 1514490948 Then
            //                        .rtbAddLine(Hex(i))
            //                    End If
            //                Next
            //                .rtbAddLine("Done")
            //                Return target
            //            End If


            uint romAddrStart = 0;
            if (doOffsetScan)
            {
                Debug.WriteLine("going in");
                // account for different pj64 offsets(?)
                for (uint potOff = 0xDFD00000; potOff < 0xE01FFFFF; potOff += 16)
                {
                    // 1146048075
                    // 0x5A454C44 is ZELD at 0x8011A5EC
                    // 0x454C4441 is ELDA
                    if (Memory.ReadInt32(target, potOff + 0x759260) == 0x444F4E4B)
                    {
                        Debug.WriteLine(potOff.ToString("X"));
                        romAddrStart = potOff;
                        break;
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

                int dk64check = 0;

                try
                {
                    dk64check = Memory.ReadInt32(target, romAddrStart + 0x759260);
                }
                catch (Exception)
                {
                    Debug.WriteLine("yeah bud shits fucked");
                }

                if (dk64check == 0x444F4E4B)
                {
                    Debug.WriteLine("verifyably pj64");
                    return Tuple.Create(target, romAddrStart);
                }

            }
            return null;
        }


        

        //            ' I have found 3 different addresses when connecting to project 64
        //            For i = 0 To 3
        //                Select Case i
        //                    Case 0
        //                        .romAddrStart = &HDFE40000UI
        //                    Case 1
        //                        .romAddrStart = &HDFE70000UI
        //                    Case 2
        //                        .romAddrStart = &HDFFB0000UI
        //                    Case Else
        //                        Return Nothing
        //                End Select

        //                ' Try to read what should be the first part of the ZELDAZ check
        //                Dim ootCheck As Integer = 0

        //                Try
        //                    ootCheck = Memory.ReadInt32(target, .romAddrStart + &H11A5EC)
        //                Catch ex As Exception
        //                    MessageBox.Show("quickRead Problem: " & vbCrLf & ex.Message & vbCrLf & (.romAddrStart + &H11A5EC).ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        //                End Try

        //                ' If it matches, set emulator variable and leave the FOR LOOP
        //                If ootCheck = 1514490948 Then
        //                    .emulator = "project64"
        //                    Exit For
        //                End If
        //            Next

        //            ' Return the process
        //            Return target
        //        End With
        //    End Function
    }
}
