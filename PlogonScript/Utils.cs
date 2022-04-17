using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace PlogonScript;

// Thanks to https://stackoverflow.com/a/65886646/4314212
public class Utils
{
    private const int SW_SHOW = 5;

    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    public static bool OpenFolderInExplorer(string folder)
    {
        var info = new SHELLEXECUTEINFO
        {
            cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
            lpVerb = "explore",
            nShow = SW_SHOW,
            lpFile = folder
        };
        return ShellExecuteEx(ref info);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHELLEXECUTEINFO
    {
        public int cbSize;
        private readonly uint fMask;
        private readonly IntPtr hwnd;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpVerb;
        [MarshalAs(UnmanagedType.LPTStr)] public string lpFile;
        [MarshalAs(UnmanagedType.LPTStr)] private readonly string lpParameters;
        [MarshalAs(UnmanagedType.LPTStr)] private readonly string lpDirectory;
        public int nShow;
        private readonly IntPtr hInstApp;
        private readonly IntPtr lpIDList;
        [MarshalAs(UnmanagedType.LPTStr)] private readonly string lpClass;
        private readonly IntPtr hkeyClass;
        private readonly uint dwHotKey;
        private readonly IntPtr hIcon;
        private readonly IntPtr hProcess;
    }
}