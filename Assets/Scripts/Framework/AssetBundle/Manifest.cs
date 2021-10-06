using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using XLib;
using System.IO;

[Serializable]
public class BundleRef
{
    public string name;
    public long len;
    public string hash;
    public int[] deps;
}

[Serializable]
public class AssetRef
{
    public string path;
    public int bundle;
}

public class Manifest : ScriptableObject
{
    [SerializeField]
    private float appVersion;
    [SerializeField]
    private int resVersion;

    [SerializeField]
    private List<BundleRef> bundleRefs;
    [SerializeField]
    private List<AssetRef> assetRefs;

    private Dictionary<string, BundleRef> asset2Bundle = new Dictionary<string, BundleRef>();
    private Dictionary<string, BundleRef> bundle2Map = new Dictionary<string, BundleRef>();

    public static Manifest LoadApkManifest(string name)
    {
        Manifest apkManifest = null;
#if UNITY_EDITOR
        if (Assets.IsBundleMode)
        {
#endif
            var assetPath = Path.Combine(Assets.dataPath, name);
            var apkManifestBundle = AssetBundle.LoadFromFile(assetPath);
            apkManifest = apkManifestBundle.LoadAsset<Manifest>("Manifest");
            apkManifestBundle.Unload(false);

            assetPath = Path.Combine(Assets.updatePath, name);
            if (File.Exists(assetPath))
            {
                var updateManifestBundle = AssetBundle.LoadFromFile(assetPath);
                var updateManifest = updateManifestBundle.LoadAsset<Manifest>("Manifest");
                updateManifestBundle.Unload(false);

                if (updateManifest.IsNewVersion(apkManifest))
                    apkManifest = updateManifest;
                else
                    GameUtility.DeleteFolder(Assets.updatePath);
            }
#if UNITY_EDITOR
        }
#endif
        return apkManifest;
    }

    public void Initialize()
    {
        foreach (var assetRef in assetRefs)
        {
            asset2Bundle[assetRef.path] = bundleRefs[assetRef.bundle];
        }

        foreach(var bundle in bundleRefs)
        {
            bundle2Map.Add(bundle.name, bundle);
        }
    }

    public bool IsNewVersion(Manifest other)
    {
        if (appVersion > other.appVersion)
            return true;

        if (resVersion > other.resVersion)
            return true;

        return false;
    }

    public bool IsNewVersion(Version ver)
    {
        if (appVersion > ver.appVersion)
            return true;

        if (resVersion > ver.resVersion)
            return true;

        return false;
    }

    public List<BundleRef> GetDiffBundles(Manifest oldManifest)
    {
        var diffBundleRefs = new List<BundleRef>();

        var bNext = false;
        foreach(var bundleRef in bundleRefs)
        {
            bNext = false;
            foreach(var oldBundleRef in oldManifest.bundleRefs)
            {
                if (oldBundleRef.name.Equals(bundleRef.name))
                {
                    if (!oldBundleRef.hash.Equals(bundleRef.hash))
                    {
                        diffBundleRefs.Add(bundleRef);
                    }
                    bNext = true;
                    break;
                }
            }
            if (bNext) continue;
            diffBundleRefs.Add(bundleRef);
        }

        return diffBundleRefs;
    }

#if UNITY_EDITOR
    public List<BundleRef> BundleRefs { set { bundleRefs = value; } }
    public List<AssetRef> AssetRefs { set { assetRefs = value; }}

    public void WriteVersion(float appVer, int resVer)
    {
        appVersion = appVer;
        resVersion = resVer;
    }
#endif

    public string GetAssetBundle(string path)
    {
        if (asset2Bundle.TryGetValue(path, out var bundleRef))
        {
            return bundleRef.name;
        }
        return string.Empty;
    }

    public long GetBundleSize(string bundle)
    {
        if (bundle2Map.TryGetValue(bundle, out var bundleRef))
        {
            return bundleRef.len;
        }
        return 0;
    }

    public string GetBundleHash(string bundle)
    {
        if (bundle2Map.TryGetValue(bundle, out var bundleRef))
        {
            return bundleRef.hash;
        }
        return string.Empty;
    }

    public bool ExistBundle(string bundle)
    {
        return bundle2Map.ContainsKey(bundle);
    }

    public string[] GetBundleDepends(string bundle)
    {
        if (bundle2Map.TryGetValue(bundle, out var bundleRef))
        {
            var deps = bundleRef.deps;
            if (deps != null)
            {
                string[] depends = new string[deps.Length];
                var i = 0;
                foreach (var dep in deps)
                {
                    depends[i++] = bundleRefs[dep].name;
                }
                return depends;
            }
        }
        return null;
    }
}
