using UnityEngine;
using System.Text;
using System.IO;
using System;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameUtility
{
    #region 平台相关

    public static string GetPlatformName()
    {
#if UNITY_EDITOR
        #if UNITY_ANDROID
            return "Android";
        #elif UNITY_IOS
            return "iOS"
        #else
            return "Windows";
        #endif
#else
        var platform = Application.platform;
        switch(platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
                break;
            case RuntimePlatform.Android:
                return "Android";
                break;
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
                break;
            default:
                return "Unkown";
        }
#endif
    }

    #endregion

    #region 文件、文件夹操作

    public static void CopyFolder(string srcFolder, string destFolder, Action<int, int, string, string> progress = null, bool overwrite = true)
    {
        if (!Directory.Exists(srcFolder))
        {
            Debug.LogWarning($"目录不存在，拷贝失败！\n路径：{srcFolder}");
            return;
        }

        CreateFolder(destFolder);

        var files = Directory.GetFileSystemEntries(srcFolder, "*.*", SearchOption.AllDirectories);
        var index = 1; var total = files.Length;
        foreach(var file in files)
        {
            var path = file.Replace(srcFolder, destFolder);
            if (File.Exists(file))
            {
                CreateFolder(Path.GetDirectoryName(file));
                File.Copy(file, path, overwrite);
            }
            else
            {
                CreateFolder(path);
            }
            progress?.Invoke(index, total, file, path);
            index++;
        }
    }

    public static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    public static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    public static string GetOSPath(string path)
    {
        return path.Replace("\\", "/");
    }

    public static string GetOSDirRoot(string dir)
    {
        var pos = dir.LastIndexOf("/") + 1;
        if (pos != -1) return dir.Substring(pos, dir.Length - pos);
        return dir;
    }

    public static void ReName(string path, string name)
    {
        var srcFileName = path;
        var destFileName = string.Format("{0}/{1}", Path.GetDirectoryName(path), name);
        File.Move(srcFileName, destFileName);
    }

    public static void WriteAllBytes(string path, byte[] bytes)
    {
        var dir = Path.GetDirectoryName(path);
        CreateFolder(dir);
        File.WriteAllBytes(path, bytes);
    }

    public static void WriteAllText(string path, string contents)
    {
        var dir = Path.GetDirectoryName(path);
        CreateFolder(dir);
        File.WriteAllText(path, contents, Encoding.UTF8);
    }

    #endregion

    #region 算法

    private static readonly MD5 md5 = MD5.Create();
    public static string CalcMD5(FileStream source)
    {
        byte[] bytes = md5.ComputeHash(source);

        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    public static string CalcMD5(string source)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(source);

        bytes = md5.ComputeHash(bytes);

        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    public static string FormatFileSize(long fileSize)
    {
        if (fileSize < 1024)
        {
            return String.Format("{0}B", fileSize);
        }
        else if (fileSize < 1048576)
        {
            return String.Format("{0:.00}K", fileSize / 1024);
        }
        else if (fileSize < 1073741824)
        {
            return String.Format("{0:.00}M", fileSize / 1048576);
        }
        else
        {
            return String.Format("{0:.00}G", fileSize / 1073741824);
        }
    }

    #endregion
}
