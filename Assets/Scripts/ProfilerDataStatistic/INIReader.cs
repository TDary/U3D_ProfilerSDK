using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

public static class INIReader {
    public static string inipath = "";
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    public static string ReadInivalue(string Section, string Key) {
        StringBuilder temp = new StringBuilder(500);
        GetPrivateProfileString(Section, Key, "", temp, 500, inipath);
        return temp.ToString();
    }

    public static string ReadInivalue(string Section, string Key, string iniPath) {
        StringBuilder temp = new StringBuilder(500);
        GetPrivateProfileString(Section, Key, "", temp, 500, iniPath);
        return temp.ToString();
    }

    public static bool ExistINIFile(string iniPath) {
        return File.Exists(iniPath);
    }
}

