using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GSTHD
{
    public static class Memory
    {
        // Public Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As UIntPtr, <[In](), Out()> ByVal buffer As Byte(), ByVal size As UInt32, ByRef lpNumberOfBytesRead As IntPtr) As Int32
        [DllImport("kernel32", SetLastError = true)]
        public static extern int ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, Byte[] buffer, UInt32 size, IntPtr lpNumberOfBytesRead);
        //    End Function

        //    <DllImport("kernel32.dll")> _
        //    Public Function WriteProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As UIntPtr, <[In] (), Out() > ByVal buffer As Byte(), ByVal size As UInt32, ByRef lpNumberOfBytesWritten As IntPtr) As Int32
        //    End Function

        //    'Public Function WriteInt16(ByVal P As Process, ByVal memAdr As Int32, ByVal value As Integer) As Boolean
        //    '   Return WriteBytes(P, memAdr, BitConverter.GetBytes(value), 2)
        //    'End Function

        //    Public Function WriteInt16(ByVal P As Process, ByVal memAdr As UInteger, ByVal value As Int16) As Boolean
        //        Return WriteBytes(P, memAdr, BitConverter.GetBytes(value), 2)
        //    End Function

        //    Public Function WriteInt32(ByVal P As Process, ByVal memAdr As UInteger, ByVal value As Integer) As Boolean
        //        Return WriteBytes(P, memAdr, BitConverter.GetBytes(value), 4)
        //    End Function

        //    Public Function ReadInt8(ByVal P As Process, ByVal memAdr As UInteger) As Integer
        //        Return ReadBytes(P, memAdr, 1)(0)
        //    End Function
        public static int ReadInt8(Process P, uint memAdr)
        {
            return ReadBytes(P, memAdr, 1)[0];
        }

        //    Public Function ReadInt16(ByVal P As Process, ByVal memAdr As UInteger) As Integer
        //        Return BitConverter.ToInt16(ReadBytes(P, memAdr, 2), 0)
        //    End Function
        public static int ReadInt16(Process P, uint memAdr)
        {
            return BitConverter.ToInt16(ReadBytes(P, memAdr, 2), 0);
        }

        //    Public Function ReadInt32(ByVal P As Process, ByVal memAdr As UInteger) As Integer
        //        Return BitConverter.ToInt32(ReadBytes(P, memAdr, 4), 0)
        //    End Function
        public static int ReadInt32(Process P, uint memAdr)
        {
            return BitConverter.ToInt32(ReadBytes(P, memAdr, 4), 0);
        }

        //    Public Function ReadUInt32(ByVal P As Process, ByVal memAdr As UInteger) As UInteger
        //        Dim ptrBytesRead As Byte
        //        Dim buffer As Byte() = New Byte(3) { }
        //ReadProcessMemory(P.Handle, New UIntPtr(memAdr), buffer, 4, ptrBytesRead)
        //        Return BitConverter.ToUInt32(buffer, 0)
        //    End Function
        public static UInt32 ReadUInt32(Process P, uint memAdr)
        {
            IntPtr ptrBytesRead = new IntPtr(0);
            byte[] buffer = new byte[3];
            ReadProcessMemory(P.Handle, (UIntPtr)memAdr, buffer, 4, ptrBytesRead);
            return BitConverter.ToUInt32(ReadBytes(P, memAdr, 4), 0);
        }

        //    'Private Function ReadByte(ByVal P As Process, ByVal memAdr As Integer, ByVal bytesToRead As UInteger) As Byte()
        //    'Dim ptrBytesRead As Byte
        //    'Dim buffer(0) As Byte
        //    'ReadProcessMemory(P.Handle, New IntPtr(memAdr), buffer, bytesToRead, ptrBytesRead)
        //    'Return buffer
        //    'End Function

        //    Private Function ReadBytes(ByVal P As Process, ByVal memAdr As UInteger, ByVal bytesToRead As UInteger) As Byte()
        //        Dim ptrBytesRead As Byte
        //        Dim buffer As Byte() = New Byte(bytesToRead - 1) { }
        //ReadProcessMemory(P.Handle, New UIntPtr(memAdr), buffer, bytesToRead, ptrBytesRead)
        //        Return buffer
        //    End Function
        private static byte[] ReadBytes(Process P, uint memAdr, uint bytesToRead)
        {
            IntPtr ptrBytesRead = new IntPtr(0);
            byte[] buffer = new byte[bytesToRead];
            ReadProcessMemory(P.Handle, new UIntPtr(memAdr), buffer, bytesToRead, ptrBytesRead);
            return buffer;
        }

        //    Private Function WriteBytes(ByVal P As Process, ByVal memAdr As UInteger, ByVal bytes As Byte(), ByVal length As UInteger) As Boolean
        //        Dim bytesWritten As IntPtr
        //        Dim result As Integer = WriteProcessMemory(P.Handle, New UIntPtr(memAdr), bytes, length, bytesWritten)
        //        Return result<> 0
        //    End Function




    }

}

//Module Memory

//End Module