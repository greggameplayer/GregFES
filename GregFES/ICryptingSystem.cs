using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace GregFES
{
    public interface ICryptingSystem
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr destination, int Length);

        public abstract void FileEncrypt(string inputFile, string password, type typeValue);

        public abstract void FileDecrypt(string inputFile, string outputFile, string password);

        public abstract type FileDecryptTry(string inputFile);
    }

    public enum type
    {
        None = 0,
        Password,
        File
    }
}
