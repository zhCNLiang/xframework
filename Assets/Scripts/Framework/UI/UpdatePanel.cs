using System.Collections;
using UnityEngine;
using XLib;
using System.IO;
using UnityEngine.UI;

public class UpdatePanel : MonoBehaviour
{
    [Header("更新地址")] [SerializeField] private string updateUrl;

    [SerializeField] private GameObject messageBox;
    [SerializeField] private Text messageTitle;
    [SerializeField] private Text messageContent;
    [SerializeField] private Button messageButton;
    [SerializeField] private Slider updateSlider;
    [SerializeField] private Text updateText;

    private Manifest remoteManifest;

    private Downloader downloader;

    private bool bNeedUpdate;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    public IEnumerator StartUpdate()
    {
        yield return RequestRemoteVersion();
        DestroyImmediate(gameObject, true);
    }

    IEnumerator RequestRemoteVersion()
    {
        var assetRequest = Assets.LoadAssetAsync(string.Format("{0}/{1}", updateUrl, Assets.version), typeof(TextAsset)) as WebRequestAsync;
        yield return assetRequest;

        var textAsset = assetRequest.asset as TextAsset;
        if (textAsset != null)
        {
            var ver = JsonUtility.FromJson<Version>(textAsset.text);
            if (!Assets.manifest.IsNewVersion(ver))
            {
                yield return RequestRemoteManifest(ver);
            }
        }
    }

    IEnumerator RequestRemoteManifest(Version ver)
    {
        var assetRequest = Assets.LoadAssetAsync(string.Format("{0}/{1}", updateUrl, ver.manifest.name), typeof(AssetBundle));
        yield return assetRequest;
        var bundle = assetRequest.asset as AssetBundle;
        var bundleRequest = bundle.LoadAssetAsync<Manifest>("Manifest");
        yield return bundleRequest;
        remoteManifest = bundleRequest.asset as Manifest;
        remoteManifest.Initialize();

        Debug.Log("updateManifest");

        yield return CheckAndDownload(ver);
    }

    IEnumerator CheckAndDownload(Version ver)
    {
        if (remoteManifest == null)
            yield break;

        if (remoteManifest.IsNewVersion(Assets.manifest))
        {
            bNeedUpdate = true;
            downloader = new Downloader();

            var diffBundles = remoteManifest.GetDiffBundles(Assets.manifest);
            foreach(var bundleRef in diffBundles)
            {
                downloader.AddDownload(new Download()
                {
                    url = string.Format("{0}/{1}", updateUrl, bundleRef.name),
                    len = bundleRef.len,
                    hash = bundleRef.hash,
                    path = string.Format("{0}/{1}", Assets.updatePath, bundleRef.name)
                });
            }

            downloader.AddDownload(new Download()
            {
                url = string.Format("{0}/{1}", updateUrl, ver.manifest.name),
                len = ver.manifest.len,
                hash = ver.manifest.hash,
                path = string.Format("{0}/{1}", Assets.updatePath, ver.manifest.name)
            });

            downloader.StartDownload();
        }

        yield return WaitDownloadFinish();
    }

    IEnumerator WaitDownloadFinish()
    {
        if (downloader == null)
            yield break;
        yield return new WaitUntil( () => downloader.CheckDownloadAll() );

        if (!downloader.CheckAllDownloadValid())
            yield break;

        downloader.UpdateAllDownloadFiles();

        Assets.UpdateManifest(remoteManifest);

        CleanOldDownloadDLC();
    }

    void CleanOldDownloadDLC()
    {
        foreach(var file in Directory.GetFiles(Assets.updatePath, "*.*"))
        {
            if (file.EndsWith(Assets.Extension))
            {
                var bundleName = Path.GetFileName(file);
                if (Assets.ExistBundle(bundleName))
                    continue;
            }
            GameUtility.DeleteFile(file);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (downloader != null)
        {
            updateSlider.value = (float)downloader.progress;
            var downSize = GameUtility.FormatFileSize(downloader.downSize);
            var totalSize = GameUtility.FormatFileSize(downloader.totalSize);
            updateText.text = $"{downSize}/{totalSize}({downloader.downCount}/{downloader.totalCount})";
            downloader.Update();
        }
    }
}
