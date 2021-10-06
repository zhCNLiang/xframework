using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using XLib;

public enum BundleName
{
    Explicit,
    Path,
    Directory,
    TopDirectory
}

[Serializable]
public class PackAsset
{
    [Tooltip("搜索路径")]
    public string SearchPath;
    [Tooltip("递归搜索")]
    public bool Recursive;
    [Tooltip("搜索通配符，多个之间用逗号隔开")]
    public string SearchPattern = "*.*";
    [Tooltip("Bundle命名")]
    public BundleName BundleName = BundleName.Explicit;
    [Tooltip("指定Bundle名字")]
    public string ExplicitName;

    public List<string> GetAssets()
    {
        var assets = new List<string>();

        var patterns = SearchPattern.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        if (Directory.Exists(SearchPath))
        {
            foreach(var pattern in patterns)
            {
                var files = Directory.GetFiles(SearchPath, pattern, Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach(var file in files)
                {
                    if (Directory.Exists(file)) continue;

                    var asset = GameUtility.GetOSPath(file);
                    if (!asset.StartsWith("Assets/")) continue;

                    var ext = Path.GetExtension(file).ToLower();
                    if (ext == ".meta" || ext == ".dll" || ext == ".cs")
                        continue;

                    assets.Add(asset);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Search Path {SearchPath} Does not Exit.");
        }

        return assets;
    }
}

public class PackRule : ScriptableObject
{
    [Header("是否开启Bundle模式")]
    public bool UseBundleMode;
    [Header("使用MD5命名打包")]
    public bool UseMd5Name = true;
    [Header("打包资源清单")]
    public List<PackAsset> PackAssets;
    private Dictionary<string, string> asset2Bundle = new Dictionary<string, string>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        Assets.IsBundleMode = UseBundleMode;
    }
#endif

    public List<AssetBundleBuild> GetBundleBuilds()
    {
        var builds = new List<AssetBundleBuild>();

        asset2Bundle.Clear();
        foreach(var packAsset in PackAssets)
        {
            ApplyRule(packAsset);
        }

        var bundleAssets = new Dictionary<string, List<string>>();
        foreach(var item in asset2Bundle)
        {
            var asset = item.Key;
            var bundleName = item.Value;
            if (!bundleAssets.TryGetValue(bundleName, out var assets))
            {
                assets = new List<string>();
                bundleAssets.Add(bundleName, assets);
            }
            assets.Add(asset);
        }

        foreach(var item in bundleAssets)
        {
            var bundleName = item.Key;
            var assets = item.Value;
            builds.Add(new AssetBundleBuild() {
                assetBundleName = bundleName,
                assetNames = assets.ToArray(),
                assetBundleVariant = Assets.Extension
            });
        }

        return builds;
    }

    private void ApplyRule(PackAsset packAsset)
    {
        var assets = packAsset.GetAssets();
        var bundleName = packAsset.BundleName;
        switch(bundleName)
        {
            case BundleName.Explicit:
            {
                if (string.IsNullOrEmpty(packAsset.ExplicitName))
                {
                    Debug.LogError($"跳过[{packAsset.SearchPath}]打包，必须为 Explicit Bundle 指定 Name");
                    break;
                }
                foreach (var asset in assets) asset2Bundle[asset] = RenameBundle(packAsset.ExplicitName);
                break;
            }
            case BundleName.Path:
            {
                foreach (var asset in assets) asset2Bundle[asset] = RenameBundle(asset);
                break;
            }
            case BundleName.Directory:
            {
                foreach (var asset in assets) asset2Bundle[asset] = RenameBundle(Path.GetDirectoryName(asset));
                break;
            }
            case BundleName.TopDirectory:
            {
                var startIndex = packAsset.SearchPath.Length;
                foreach (var asset in assets)
                {
                    var dir = GameUtility.GetOSPath(Path.GetDirectoryName(asset));

                    if (!string.IsNullOrEmpty(dir))
                    {
                        if (!dir.Equals(packAsset.SearchPath))
                        {
                            var pos = dir.IndexOf("/", startIndex + 1, StringComparison.Ordinal);
                            if (pos != -1) dir = dir.Substring(0, pos);
                        }
                    }

                    asset2Bundle[asset] = RenameBundle(dir);
                }
                break;
            }
            default:
                break;
        }
    }

    private string RenameBundle(string name)
    {
        return UseMd5Name ? GameUtility.CalcMD5(name) : name;
    }
}
