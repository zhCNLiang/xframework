using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Downloader
{
    public double progress { get { return totalSize > 0 ? (double)downSize / totalSize : 0f; } }
    public long totalSize { get; private set; }
    public long downSize { get; private set; }
    public long totalCount { get; private set; }
    public long downCount { get; private set; }

    private readonly int MAX_DOWNLOAD_COUNT = 10;

    private List<Download> downloads = new List<Download>();
    private List<Download> waitdownloads = new List<Download>();
    private List<Download> downloading = new List<Download>();

    private bool isStartDownload;

    public void AddDownload(Download download)
    {
        downloads.Add(download);
    }

    public void StartDownload()
    {
        for (int i = 0; i < downloads.Count; i++)
        {
            var download = downloads[i];
            totalSize += download.len;
            totalCount++;
            if (download.CheckDownloaded())
            {
                Logger.Trace?.Output($"file is download : {download.path}");
                downSize += download.len;
                downCount++;
            }
            else
            {
                waitdownloads.Add(download);
            }
        }
        isStartDownload = true;
    }

    public bool CheckDownloadAll()
    {
        return waitdownloads.Count == 0 && downloading.Count == 0;
    }

    public bool CheckAllDownloadValid()
    {
        foreach(var download in downloads)
        {
            if (!download.CheckDownloaded())
                return false;
        }
        return true;
    }

    public void UpdateAllDownloadFiles()
    {
        foreach(var download in downloads)
        {
            download.UpdateFile();
        }
    }

    public void Update()
    {
        if (!isStartDownload)
            return;

        for (int i = 0; i < waitdownloads.Count; i++)
        {
            if (downloading.Count < MAX_DOWNLOAD_COUNT)
            {
                var download = waitdownloads[i];
                download.Start();
                downloading.Add(download);
                waitdownloads.RemoveAt(i);
                i--;
            }
        }

        for(int i = 0 ; i < downloading.Count; i++)
        {
            var download = downloading[i];
            if (!download.Update())
            {
                downCount++;
                downSize += download.len;
                downloading.RemoveAt(i);
                i--;
                continue;
            }
        }
    }
}
