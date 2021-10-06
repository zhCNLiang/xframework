using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class Download : DownloadHandlerScript
{
    public string url { get; set; }
    public long len { get; set; }
    public string hash { get; set; }
    public string path { get; set; }
    public Action<Download> completed { get; set; }
    public string err { get; set; }

    FileStream fileStream;
    private long pos;

    UnityWebRequest request;
    private bool isRunning;

    private string temporaryPath;
    private string tempPath {
        get {
            if (string.IsNullOrEmpty(temporaryPath))
                temporaryPath = Path.ChangeExtension(path, ".temp");
            return temporaryPath;
        }
    }

    private string verifiedPath;
    private string verifyPath
    {
        get {
            if (string.IsNullOrEmpty(verifiedPath))
                verifiedPath = Path.ChangeExtension(path, ".verify");
            return verifiedPath;
        }
    }

    public float progress
    {
        get {
            return (float)pos / len;
        }
    }

    public bool CheckDownloaded()
    {
        if (File.Exists(verifyPath))
        {
            return true;
        }
        return false;
    }
    
    private bool CheckDownloadValid()
    {
        using(var fs = File.Open(tempPath, FileMode.OpenOrCreate, FileAccess.Read))
        {
            if (fs.Length == len && hash.Equals(GameUtility.CalcMD5(fs)))
            {
                return true;
            }
        }
        return false;
    }

    public void Start()
    {
        if (isRunning)
        {
            return;
        }

        fileStream = File.Open(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        pos = fileStream.Length;
        if (pos < len)
        {
            fileStream.Flush(true);
            request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Range", "bytes=" + pos + "-");
            request.downloadHandler = this;
            request.SendWebRequest();
            isRunning = true;
        }
        else
        {
            CompleteContent();
        }
    }

    public void Stop()
    {
        isRunning = false;
        fileStream?.Dispose();
        fileStream = null;
        request?.Abort();
        request?.Dispose();
        request = null;
    }

    public bool Update()
    {
        if (!isRunning)
        {
            return false;
        }

        if (!request.isDone)
        {
            return true;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            err = request.error;
        }

        CompleteContent();

        return false;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        fileStream.Write(data, 0, dataLength);
        pos += dataLength;
        return isRunning;
    }

    protected override void CompleteContent()
    {
        Stop();

        if (string.IsNullOrEmpty(err))
        {
            if (CheckDownloadValid())
            {
                GameUtility.ReName(tempPath, Path.GetFileName(verifyPath));
            }
            else
            {
                err = $"download file {tempPath} is no valid";
            }
        }

        completed?.Invoke(this);
    }

    public void UpdateFile()
    {
        if (File.Exists(verifyPath))
        {
            GameUtility.DeleteFile(path);
            GameUtility.ReName(verifyPath, Path.GetFileName(path));
        }
    }
}
