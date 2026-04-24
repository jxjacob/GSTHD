using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;



namespace GSTHD
{
    public static partial class Memory
    {
        // const int PROCESS_QUERY_INFORMATION = 0x0400;
        // const int MEM_COMMIT = 0x00001000;
        // const int PAGE_READWRITE = 0x04;
        // const int PROCESS_WM_READ = 0x0010;
        //
        // [DllImport("kernel32.dll")]
        // public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [LibraryImport("kernel32", SetLastError = true)]
        private static partial int ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, Byte[] buffer, UInt32 size, IntPtr lpNumberOfBytesRead);
        
        
        
        [StructLayout(LayoutKind.Sequential)]
        public struct iovec
        {
            public IntPtr iov_base; // Starting address
            public UIntPtr iov_len; // Number of bytes to transfer
        }
        
        [DllImport("libc", SetLastError = true)]
        private static extern unsafe int process_vm_readv(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov, ulong riovcnt, ulong flags);


        // [DllImport("psapi.dll", SetLastError = true)]
        // public static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, UInt32 cb, [MarshalAs(UnmanagedType.U4)] out UInt32 lpcbNeeded);
        //
        // // functions to read memory at certain lengths


        //32-bit functions
        public static int ReadInt8(IntPtr P, uint memAdr)
        {
            return ReadBytes(P, memAdr, 1)[0];
        }
        public static int ReadInt16(IntPtr P, uint memAdr)
        {
            return BitConverter.ToInt16(ReadBytes(P, memAdr, 2), 0);
        }
        public static int ReadInt32(IntPtr P, uint memAdr)
        {
            return BitConverter.ToInt32(ReadBytes(P, memAdr, 4), 0);
        }
        public static UInt32 ReadUInt32(IntPtr P, uint memAdr)
        {
            #if WINDOWS
            IntPtr ptrBytesRead = new IntPtr(0);
            byte[] buffer = new byte[3];
            ReadProcessMemory(P, (UIntPtr)memAdr, buffer, 4, ptrBytesRead);
            return BitConverter.ToUInt32(ReadBytes(P, memAdr, 4), 0);
            #else
            return BitConverter.ToUInt32(ReadBytesLinux(P, (IntPtr)memAdr, 8), 0);
            #endif
        }
        private static byte[] ReadBytes(IntPtr P, uint memAdr, uint bytesToRead)
        {
            #if WINDOWS
            IntPtr ptrBytesRead = new IntPtr(0);
            byte[] buffer = new byte[bytesToRead];
            ReadProcessMemory(P, new UIntPtr(memAdr), buffer, bytesToRead, ptrBytesRead);
            return buffer;
            #else
            return ReadBytesLinux(P, (IntPtr)memAdr, (int)bytesToRead);
            #endif
        }
        
        public static unsafe byte[] ReadBytesLinux(IntPtr P, IntPtr memAdr, int bytesToRead)
        {
            byte[] localBuffer = new byte[bytesToRead];

            fixed (byte* bufferPtr = localBuffer)
            {
                var localIo = new iovec
                {
                    iov_base = (IntPtr)bufferPtr,
                    iov_len = (UIntPtr)bytesToRead
                };
                var remoteIo = new iovec
                {
                    iov_base = memAdr,
                    iov_len = (UIntPtr)bytesToRead
                };
                
            var res = process_vm_readv((int)P, &localIo, 1, &remoteIo, 1, 0);
            }

            return localBuffer;
        }

        
        
        
        
        

        //64-bit functions
        public static int ReadInt8(IntPtr P, ulong memAdr)
        {
            return ReadBytes(P, memAdr, 1)[0];
        }

        public static int ReadInt16(IntPtr P, ulong memAdr)
        {
            return BitConverter.ToInt16(ReadBytes(P, memAdr, 2), 0);
        }

        public static int ReadInt32(IntPtr P, ulong memAdr)
        {
            return BitConverter.ToInt32(ReadBytes(P, memAdr, 4), 0);
        }

        public static ulong ReadInt64(IntPtr P, ulong memAdr)
        {
            return BitConverter.ToUInt64(ReadBytes(P, memAdr, 8), 0);
        }

        private static byte[] ReadBytes(IntPtr P, ulong memAdr, uint bytesToRead)
        {
            #if WINDOWS
            IntPtr ptrBytesRead = new IntPtr(0);
            byte[] buffer = new byte[bytesToRead];
            ReadProcessMemory(P, new UIntPtr(memAdr), buffer, bytesToRead, ptrBytesRead);
            return buffer;
            #else
            return ReadBytesLinux(P, (IntPtr)memAdr, (int)bytesToRead);
            #endif
        }

        // general functions that needed a home somewhere lol

        public static uint Int8AddrFix(uint addr)
        {
            return (addr % 4) switch
            {
                0 => addr + 3,
                1 => addr + 1,
                2 => addr - 1,
                3 => addr - 3,
                _ => addr
            };
        }


        public static uint Int16AddrFix(uint addr)
        {
            return (addr % 4) switch
            {
                2 or 3 => addr - 2,
                0 or 1 => addr + 2,
                _ => addr
            };
        }

        public static ulong Int8AddrFix(ulong addr)
        {
            return (addr % 4) switch
            {
                0 => addr + 3,
                1 => addr + 1,
                2 => addr - 1,
                3 => addr - 3,
                _ => addr
            };
        }


        public static ulong Int16AddrFix(ulong addr)
        {
            return (addr % 4) switch
            {
                2 or 3 => addr - 2,
                0 or 1 => addr + 2,
                _ => addr
            };
        }
    }
}