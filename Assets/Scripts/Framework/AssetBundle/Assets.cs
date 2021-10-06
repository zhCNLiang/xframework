using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace XLib
{
    public class Assets : MonoBehaviour
    {
#if UNITY_EDITOR
        public static bool IsBundleMode = false;
#endif
        private static Dictionary<string, BundleRequest> cacheBundleRequest = new Dictionary<string, BundleRequest>();
        private static List<BundleRequest> loadingBundleRequest = new List<BundleRequest>();
        private static List<BundleRequest> waitLoadBundleRequest = new List<BundleRequest>();
        private static List<string> unUseBundleRequest = new List<string>();
        private static Dictionary<string, AssetRequest> cacheAssetRequest = new Dictionary<string, AssetRequest>();
        private static List<AssetRequest> loadingAssetRequest = new List<AssetRequest>();
        private static List<AssetRequest> waitLoadAssetRequest = new List<AssetRequest>();
        private static List<string> unUseAssetRequest = new List<string>();
        private static Dictionary<string, SceneRequest> cacheSceneRequest = new Dictionary<string, SceneRequest>();
        private static List<SceneRequest> loadingSceneRequest = new List<SceneRequest>();
        private static List<SceneRequest> waitLoadSceneRequest = new List<SceneRequest>();
        private static List<string> unUseSceneRequest = new List<string>();

        public static Manifest manifest { get; private set;}
        public static readonly string Extension = "unity3d";
        public static readonly string version = "version.json";
        private static readonly string MainfestAsset = "manifest" + "." + Extension;
        private static readonly int MAX_LOAD_BUNDLE_NUM = 5;

        private static string _datePath;
        public static string dataPath {
            get {
                if (string.IsNullOrEmpty(_datePath))
                    _datePath = Path.Combine(Application.streamingAssetsPath, GameUtility.GetPlatformName());
                return _datePath;
            }
        }

        private static string _updatePath;
        public static string updatePath {
            get {
                if (string.IsNullOrEmpty(_updatePath))
                {
                    _updatePath = Path.Combine(Application.persistentDataPath, "DLC");
                }
                return _updatePath;
            }
        }

        private HashSet<AssetRequest> assetRequestLiveInScene;

        public static bool isInitialize { get; private set;}

        public static IEnumerator Initialize()
        {
            if (isInitialize)
                yield break;

            Assets assets = GameObject.FindObjectOfType<Assets>();
            if (null == assets)
            {
                GameObject go = new GameObject("Assets", new Type[] { typeof(Assets) });
                DontDestroyOnLoad(go);
            }

            manifest = Manifest.LoadApkManifest(MainfestAsset);
            manifest?.Initialize();

            GameUtility.CreateFolder(updatePath);

            isInitialize = true;
        }

        public static string GetPriorityPath(string path)
        {
            string priorityPath = Path.Combine(updatePath, path);

            if (File.Exists(priorityPath))
            {
                return priorityPath;
            }

            priorityPath = Path.Combine(dataPath, path);

            return priorityPath;
        }

        public static void UpdateManifest(Manifest newManifest)
        {
            manifest = newManifest;
        }

        internal static string[] GetAssetBundleDepends(string bundleName)
        {
            return manifest.GetBundleDepends(bundleName);
        }

        internal static string GetBundleHashName(string bundleName)
        {
            return manifest.GetAssetBundle(bundleName);
        }

        public static bool ExistBundle(string bundleName)
        {
            return manifest.ExistBundle(bundleName) || MainfestAsset.Equals(bundleName);
        }

        internal static BundleRequest LoadBundleAsync(string bundleName)
        {
            return LoadBundle(bundleName, true);
        }

        internal static BundleRequest LoadBundle(string bundleName)
        {
            return LoadBundle(bundleName, false);
        }

        private static BundleRequest LoadBundle(string bundleName, bool bAsync)
        {
            BundleRequest request;
            if (!cacheBundleRequest.TryGetValue(bundleName, out request))
            {
                request = bAsync ? new BundleRequestAsync(bundleName) : new BundleRequest(bundleName);
                cacheBundleRequest.Add(request.name, request);
            }

            request.Retain();
            if (bAsync)
                waitLoadBundleRequest.Add(request);
            else
                request.Load();

            return request;
        }

        public static AssetRequest LoadAssetAsync(string assetPath, Type type)
        {
            return LoadAsset(assetPath, type, true);
        }

        public static AssetRequest LoadAsset(string assetPath, Type type)
        {
            return LoadAsset(assetPath, type, false);
        }

        public static void UnLoadAsset(AssetRequest assetRequest)
        {
            assetRequest?.Release();
        }

        private static AssetRequest LoadAsset(string assetPath, Type type, bool bAsync)
        {
            AssetRequest request;
            if (!cacheAssetRequest.TryGetValue(assetPath, out request))
            {
                if (assetPath.StartsWith("http://")
                    || assetPath.StartsWith("https://"))
                    request = new WebRequestAsync(assetPath, type);
                else
                    request = bAsync ? new AssetRequestAsync(assetPath, type) : new AssetRequest(assetPath, type);
                cacheAssetRequest.Add(request.assetPath, request);
            }

            request.Retain();
            if (bAsync)
                waitLoadAssetRequest.Add(request);
            else
                request.Load();

            return request;
        }

        public void KeepAliveInScene(AssetRequest assetRequest)
        {
            if (!assetRequestLiveInScene.Contains(assetRequest))
            {
                assetRequest.Retain();
                assetRequestLiveInScene.Add(assetRequest);
            }
        }

        public void ClearAliveInScene(AssetRequest assetRequest)
        {
            if (assetRequestLiveInScene.Contains(assetRequest))
            {
                assetRequest.Release();
                assetRequestLiveInScene.Remove(assetRequest);
            }
        }

        public static SceneRequest LoadSceneAsync(string assetPath, LoadSceneMode loadSceneMode)
        {
            return LoadScene(assetPath, loadSceneMode, true);
        }

        public static SceneRequest LoadScene(string assetPath, LoadSceneMode loadSceneMode)
        {
            return LoadScene(assetPath, loadSceneMode, false);
        }

        private static SceneRequest LoadScene(string assetPath, LoadSceneMode loadSceneMode, bool bAsync)
        {
            SceneRequest request;
            if (!cacheSceneRequest.TryGetValue(assetPath, out request))
            {
                request = bAsync ? new SceneRequestAsync(assetPath, loadSceneMode) : new SceneRequest(assetPath, loadSceneMode);
                cacheSceneRequest.Add(assetPath, request);
            }

            request.Retain();
            if (bAsync)
                waitLoadSceneRequest.Add(request);
            else
                request.Load();

            return request;
        }

        public static void UnloadScene(SceneRequest sceneRequest)
        {
            sceneRequest.Release();
        }

        private void Update()
        {
            // bundle load
            foreach (var item in cacheBundleRequest)
            {
                var bundleRequest = item.Value;
                if (bundleRequest.IsUnUse())
                {
                    unUseBundleRequest.Add(item.Key);
                    waitLoadBundleRequest.Remove(bundleRequest);
                    loadingBundleRequest.Remove(bundleRequest);
                    bundleRequest.UnLoad();
                }
            }

            foreach (var bundleKey in unUseBundleRequest)
            {
                cacheBundleRequest.Remove(bundleKey);
            }
            unUseBundleRequest.Clear();

            for (int i = 0; i < waitLoadBundleRequest.Count; i++)
            {
                if (loadingBundleRequest.Count >= MAX_LOAD_BUNDLE_NUM) break;

                BundleRequest bundleRequest = waitLoadBundleRequest[i];
                bundleRequest.Load();
                loadingBundleRequest.Add(bundleRequest);
                waitLoadBundleRequest.RemoveAt(i);
                i--;
            }

            for (int i = 0; i < loadingBundleRequest.Count; i++)
            {
                BundleRequest bundleRequest = loadingBundleRequest[i];
                if (!bundleRequest.Update())
                {
                    loadingBundleRequest.RemoveAt(i);
                    i--;
                }
            }

            // asset load
            foreach (var item in cacheAssetRequest)
            {
                var assetRequest = item.Value;
                if (assetRequest.IsUnUse())
                {
                    unUseAssetRequest.Add(item.Key);
                    waitLoadAssetRequest.Remove(assetRequest);
                    loadingAssetRequest.Remove(assetRequest);
                    assetRequest.UnLoad();
                }
            }

            foreach (var assetKey in unUseAssetRequest)
            {
                cacheAssetRequest.Remove(assetKey);
            }
            unUseAssetRequest.Clear();

            for (int i = 0; i < waitLoadAssetRequest.Count; i++)
            {
                AssetRequest assetRequest = waitLoadAssetRequest[i];
                assetRequest.Load();
                loadingAssetRequest.Add(assetRequest);
                waitLoadAssetRequest.RemoveAt(i);
                i--;
            }

            for (int i = 0; i < loadingAssetRequest.Count; i++)
            {
                AssetRequest assetRequest = loadingAssetRequest[i];
                if (!assetRequest.Update())
                {
                    loadingAssetRequest.RemoveAt(i);
                    i--;
                }
            }

            // scene load
            foreach (var item in cacheSceneRequest)
            {
                var sceneRequest = item.Value;
                if (sceneRequest.IsUnUse())
                {
                    unUseSceneRequest.Add(item.Key);
                    waitLoadSceneRequest.Remove(sceneRequest);
                    loadingSceneRequest.Remove(sceneRequest);
                    sceneRequest.UnLoad();
                }
            }

            foreach (var assetKey in unUseSceneRequest)
            {
                cacheSceneRequest.Remove(assetKey);
            }
            unUseSceneRequest.Clear();

            for (int i = 0; i < waitLoadSceneRequest.Count; i++)
            {
                SceneRequest sceneRequest = waitLoadSceneRequest[i];
                sceneRequest.Load();
                loadingSceneRequest.Add(sceneRequest);
                waitLoadSceneRequest.RemoveAt(i);
                i--;
            }

            for (int i = 0; i < loadingSceneRequest.Count; i++)
            {
                SceneRequest scenenRequest = loadingSceneRequest[i];
                if (!scenenRequest.Update())
                {
                    loadingSceneRequest.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}