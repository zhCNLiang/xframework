using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace XLib
{
    public enum LoadState
    {
        Init,
        LoadBundle,
        LoadAsset,
        Loaded,
        UnLoad,
    }

    public class Request<T> : IEnumerator where T : class
    {
        public Action<T> onComplete;
        protected int refCount { get; set; }
        public virtual float progress { get; }
        public bool isDone { get { return loadState == LoadState.Loaded || loadState == LoadState.UnLoad; } }
        public string error { get; protected set; }
        protected LoadState loadState = LoadState.Init;
        public LoadState state
        {
            get { return loadState; }
            set
            {
                loadState = value;
                if (loadState == LoadState.Loaded)
                {
                    try
                    {
                        onComplete?.Invoke(this as T);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {

        }

        public virtual void Load()
        {

        }

        public virtual void UnLoad()
        {

        }

        public virtual bool Update()
        {
            return false;
        }

        public void Retain()
        {
            refCount++;
        }

        public void Release()
        {
            refCount--;
        }

        public bool IsUnUse()
        {
            return refCount <= 0;
        }
    }

    public class BundleRequest : Request<BundleRequest>
    {
        public AssetBundle asset { get; protected set; }
        public string name { get; protected set; }

        public override float progress
        {
            get
            {
                return state == LoadState.Loaded ? 1f : 0f;
            }
        }

        public BundleRequest(string bundleName)
        {
            name = bundleName;
        }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

            var bundlePath = Assets.GetPriorityPath(name);
            asset = AssetBundle.LoadFromFile(bundlePath);
            state = LoadState.Loaded;
        }

        public override void UnLoad()
        {
            asset?.Unload(true);
            asset = null;
            onComplete = null;
            state = LoadState.UnLoad;
        }
    }

    public class BundleRequestAsync : BundleRequest
    {
        private AssetBundleCreateRequest request;

        public override float progress
        {
            get
            {
                if (state == LoadState.Loaded) return 1f;
                if (state == LoadState.Init || state == LoadState.UnLoad) return 0f;

                return request != null ? request.progress : 0f;
            }
        }

        public BundleRequestAsync(string bundleName)
            : base(bundleName) { }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

            var bundlePath = Assets.GetPriorityPath(name);
            request = AssetBundle.LoadFromFileAsync(bundlePath);
            state = LoadState.LoadBundle;
        }

        public override void UnLoad()
        {
            request = null;
            base.UnLoad();
        }

        public override bool Update()
        {
            if (request != null)
            {
                if (request.isDone)
                {
                    asset = request.assetBundle;
                    state = LoadState.Loaded;
                    return false;
                }
            }

            return true;
        }
    }

    public class AssetRequest : Request<AssetRequest>
    {
        public UnityEngine.Object asset { get; protected set; }
        public string assetPath { get; protected set; }
        public string name { get; protected set; }
        public string bundleName { get; protected set; }
        public Type type { get; protected set; }
        protected BundleRequest bundleRequest;
        protected List<BundleRequest> dependBundles;

        public override float progress
        {
            get
            {
                return state == LoadState.Loaded ? 1f : 0f;
            }
        }

        public AssetRequest(string assetPath, Type type)
        {
            this.assetPath = GameUtility.GetOSPath(assetPath);
            name = Path.GetFileName(assetPath);
            this.type = type;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
                bundleName = Assets.GetBundleHashName(this.assetPath);
            }
#endif
        }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
#endif
                bundleRequest = Assets.LoadBundle(bundleName);
                var depends = Assets.GetAssetBundleDepends(bundleName);
                if (depends != null)
                {
                    dependBundles = new List<BundleRequest>();
                    foreach (var dep in depends)
                    {
                        dependBundles.Add(Assets.LoadBundle(dep));
                    }
                }
                asset = bundleRequest?.asset?.LoadAsset(name, type);
#if UNITY_EDITOR
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
            }
#endif

            state = LoadState.Loaded;
        }

        public override void UnLoad()
        {
            if (dependBundles != null)
            {
                foreach (var dependBundle in dependBundles)
                {
                    dependBundle.Release();
                }
                dependBundles = null;
            }

            if (bundleRequest != null)
            {
                bundleRequest.Release();
                bundleRequest = null;
            }

            onComplete = null;
            asset = null;

            state = LoadState.UnLoad;
        }
    }

    public class AssetRequestAsync : AssetRequest
    {
        private AssetBundleRequest request;
        public override float progress
        {
            get
            {
                if (state == LoadState.Loaded) return 1;
                if (state == LoadState.Init || state == LoadState.UnLoad) return 0;

                if (state == LoadState.LoadAsset)
                {
                    return request.progress * 0.7f + 0.3f;
                }

                if (state == LoadState.LoadBundle)
                {
                    int count = dependBundles != null ? dependBundles.Count + 1 : 1;
                    float value = bundleRequest.progress;
                    if (dependBundles != null)
                    {
                        foreach (var dependBundle in dependBundles)
                        {
                            value += dependBundle.progress;
                        }
                    }

                    float progress = value / count * 0.3f;
                    return progress;
                }

                return 0;
            }
        }

        public AssetRequestAsync(string assetPath, Type type)
            : base(assetPath, type) { }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
#endif
                bundleRequest = Assets.LoadBundleAsync(bundleName);
                var depends = Assets.GetAssetBundleDepends(bundleName);
                if (depends != null)
                {
                    dependBundles = new List<BundleRequest>();
                    foreach (var dep in depends)
                    {
                        dependBundles.Add(Assets.LoadBundleAsync(dep));
                    }
                }

                state = LoadState.LoadBundle;
#if UNITY_EDITOR
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                state = LoadState.LoadAsset;
            }
#endif
        }

        public override void UnLoad()
        {
            request = null;
            base.UnLoad();
        }

        public override bool Update()
        {
            if (state == LoadState.Init) return true;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
#endif
                if (dependBundles != null)
                {
                    foreach (var bundle in dependBundles)
                    {
                        if (!bundle.isDone)
                            return true;
                    }
                }

                if (bundleRequest != null)
                {
                    if (!bundleRequest.isDone)
                        return true;
                }

                if (request != null)
                {
                    if (request.isDone)
                    {
                        asset = request.asset;
                        state = LoadState.Loaded;
                        return false;
                    }
                }
                else
                {
                    request = bundleRequest.asset.LoadAssetAsync(name, type);
                    state = LoadState.LoadAsset;
                }
#if UNITY_EDITOR
            }
            else
            {
                state = LoadState.Loaded;
                return false;
            }
#endif

            return true;
        }
    }

    public class WebRequestAsync : AssetRequestAsync
    {
        private UnityWebRequest www;

        public WebRequestAsync(string assetPath, Type type)
            : base(assetPath, type)
        {

        }

        public override float progress
        {
            get
            {
                if (state == LoadState.Loaded) return 1;
                if (state == LoadState.Init || state == LoadState.UnLoad) return 0;
                return www != null ? www.downloadProgress : 0f;
            }
        }

        private DownloadHandler GetHandler()
        {
            if (type == typeof(TextAsset))
            {
                return new DownloadHandlerBuffer();
            }
            else if (type == typeof(Texture2D) || type == typeof(Sprite))
            {
                return new DownloadHandlerTexture(true);
            }
            else if (type == typeof(AudioClip))
            {
                return new DownloadHandlerAudioClip(assetPath, AudioType.OGGVORBIS);
            }
            else if (type == typeof(AssetBundle))
            {
                return new DownloadHandlerAssetBundle(assetPath, uint.MinValue);
            }

            return null;
        }

        private UnityEngine.Object GetAsset()
        {
            if (type == typeof(TextAsset))
            {
                return new TextAsset(www.downloadHandler.text);
            }
            else if (type == typeof(Texture2D))
            {
                return DownloadHandlerTexture.GetContent(www);
            }
            else if (type == typeof(Sprite))
            {
                var tex2d = DownloadHandlerTexture.GetContent(www);
                var sprite = Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height), Vector2.zero);
                return sprite;
            }
            else if (type == typeof(AudioClip))
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
            else if (type == typeof(AssetBundle))
            {
                return DownloadHandlerAssetBundle.GetContent(www);
            }

            return null;
        }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

            www = UnityWebRequest.Get(assetPath);
            www.downloadHandler = GetHandler();
            www.SendWebRequest();

            state = LoadState.LoadAsset;
        }

        public override void UnLoad()
        {
            www?.Dispose();
            www = null;
            base.UnLoad();
        }

        public override bool Update()
        {
            if (state == LoadState.Init) return true;

            if (www != null && www.isDone)
            {
                if (www.result != UnityWebRequest.Result.Success)
                    error = www.error;
                else
                    asset = GetAsset();
                state = LoadState.Loaded;
                return false;
            }

            return true;
        }
    }

    public class SceneRequest : Request<SceneRequest>
    {
        public string assetPath { get; protected set; }
        public string name { get; protected set; }
        public string bundleName { get; protected set; }
        protected BundleRequest bundleRequest;
        protected LoadSceneMode loadSceneMode = LoadSceneMode.Single;

        public override float progress
        {
            get
            {
                return state == LoadState.Loaded ? 1f : 0f;
            }
        }

        public SceneRequest(string assetPath, LoadSceneMode loadSceneMode)
        {
            this.assetPath = GameUtility.GetOSPath(assetPath);
            this.loadSceneMode = loadSceneMode;

            name = Path.GetFileName(assetPath);

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
                bundleName = Assets.GetBundleHashName(this.assetPath);
            }
#endif
        }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
#endif
                bundleRequest = Assets.LoadBundle(bundleName);
                SceneManager.LoadScene(assetPath, loadSceneMode);
#if UNITY_EDITOR
            }
            else
            {
                EditorSceneManager.LoadSceneInPlayMode(assetPath, new LoadSceneParameters(loadSceneMode));
            }
#endif

            state = LoadState.Loaded;
        }

        public override void UnLoad()
        {
            if (bundleRequest != null)
            {
                bundleRequest.Release();
                bundleRequest = null;
            }

            SceneManager.UnloadSceneAsync(name);

            state = LoadState.UnLoad;
        }
    }

    public class SceneRequestAsync : SceneRequest
    {
        private AsyncOperation asyncOperation;

        public override float progress
        {
            get
            {
                if (state == LoadState.Loaded) return 1;
                if (state == LoadState.Init || state == LoadState.UnLoad) return 0;
                return asyncOperation != null ? asyncOperation.progress : 0f;
            }
        }

        public SceneRequestAsync(string assetPath, LoadSceneMode loadSceneMode)
            : base(assetPath, loadSceneMode)
        {
            
        }

        public override void Load()
        {
            if (loadState != LoadState.Init)
                return;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
#endif
                bundleRequest = Assets.LoadBundleAsync(bundleName);
#if UNITY_EDITOR
            }
            else
            {
                asyncOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(assetPath, new LoadSceneParameters(loadSceneMode));       
            }
#endif

            state = LoadState.LoadBundle;
        }

        public override void UnLoad()
        {
            asyncOperation = null;
            base.UnLoad();
        }

        public override bool Update()
        {
            if (state == LoadState.Init) return true;

#if UNITY_EDITOR
            if (Assets.IsBundleMode)
            {
#endif
                if (bundleRequest != null)
                {
                    if (!bundleRequest.isDone)
                        return true;
                }

                if (asyncOperation != null)
                {
                    if (asyncOperation.isDone)
                    {
                        state = LoadState.Loaded;
                        return false;
                    }
                }
                else
                {
                    asyncOperation = SceneManager.LoadSceneAsync(assetPath, loadSceneMode);
                    state = LoadState.LoadAsset;
                }
#if UNITY_EDITOR
            }
            else
            {
                state = LoadState.Loaded;
                return false;
            }
#endif

            return true;
        }
    }

    public class ManifestRequest
    {
        private string name;
        public Manifest asset;

        public ManifestRequest(string name)
        {
            this.name = name;
        }

        public void Load()
        {
            var assetPath = Path.Combine(Assets.dataPath, name);
            var apkManifestBundle = AssetBundle.LoadFromFile(assetPath);
            var apkManifest = apkManifestBundle.LoadAsset<Manifest>("Manifest");

            asset = apkManifest;

            assetPath = Path.Combine(Assets.updatePath, name);
            if (File.Exists(assetPath))
            {
                var updateManifestBundle = AssetBundle.LoadFromFile(assetPath);
                var updateManifest = updateManifestBundle.LoadAsset<Manifest>("Manifest");

                if (updateManifest.IsNewVersion(apkManifest))
                {
                    asset = updateManifest;
                    updateManifestBundle.Unload(false);
                    apkManifestBundle.Unload(true);
                    apkManifestBundle = null;
                    apkManifest = null;
                }
                else
                {
                    apkManifestBundle.Unload(false);
                    updateManifestBundle.Unload(true);
                    updateManifestBundle = null;
                    updateManifest = null;
                    GameUtility.DeleteFolder(Assets.updatePath);
                }
            }

            asset.Initialize();
        }
    }
}