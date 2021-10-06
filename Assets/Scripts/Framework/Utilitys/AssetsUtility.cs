using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLib;

public static class AssetsUtility
{
    #region async load asset api
    public class AssetLoader
    {
        public AssetRequest request { get; private set;}
        private Action<AssetRequest> loadedAction;
        public AssetLoader(AssetRequest request, Action<AssetRequest> loadedAction = null)
        {
            this.request = request;
            this.loadedAction = loadedAction;
        }

        public void Retain()
        {
            request.Retain();
        }

        public void Release()
        {
            request.Release();
        }

        public void Stop()
        {
            if (loadedAction != null)
                request.onComplete -= loadedAction;
            loadedAction = null;
        }
    }

    public static AssetLoader LoadAssetAsync<T>(string assetPath, Action<T> onComplete, bool bAutoRelease = true)
        where T : UnityEngine.Object
    {
        Action<AssetRequest> loaded = (AssetRequest request) => {
            var asset = request.asset as T;
            if (bAutoRelease) request.Release();
            onComplete?.Invoke(asset);
        };
        var assetRequest = Assets.LoadAssetAsync(assetPath, typeof(T));
        assetRequest.onComplete += loaded;

        var loader = new AssetLoader(assetRequest, loaded);
        return loader;
    }

    public static AssetLoader LoadPrefabAsync(string assetPath, Action<GameObject> onComplete, bool bAutoRelease = true)
    {
        return LoadAssetAsync(assetPath, onComplete, bAutoRelease);
    }

    public static AssetLoader LoadSpriteAsync(string assetPath, Action<Sprite> onComplete, bool bAutoRelease = true)
    {
        return LoadAssetAsync(assetPath, onComplete, bAutoRelease);
    }

    public static AssetLoader LoadTextureAsync(string assetPath, Action<Texture> onComplete, bool bAutoRelease = true)
    {
        return LoadAssetAsync(assetPath, onComplete, bAutoRelease);
    }

    public static AssetLoader LoadTextAssetAsync(string assetPath, Action<TextAsset> onComplete, bool bAutoRelease = true)
    {
        return LoadAssetAsync(assetPath, onComplete, bAutoRelease);
    }
#endregion

#region sync load asset api
    public static AssetLoader LoadAsset<T>(string assetPath, bool bAutoRelease = true)
        where T : UnityEngine.Object
    {
        var assetRequest = Assets.LoadAsset(assetPath, typeof(T));
        if (bAutoRelease) assetRequest.Release();

        var loader = new AssetLoader(assetRequest);
        return loader;
    }

    public static AssetLoader LoadPrefab(string assetPath, bool bAutoRelease = true)
    {
        return LoadAsset<GameObject>(assetPath, bAutoRelease);
    }

    public static AssetLoader LoadSprite(string assetPath, bool bAutoRelease = true)
    {
        return LoadAsset<Sprite>(assetPath, bAutoRelease);
    }

    public static AssetLoader LoadTexture(string assetPath, bool bAutoRelease = true)
    {
        return LoadAsset<Texture>(assetPath, bAutoRelease);
    }

    public static AssetLoader LoadTextAsset(string assetPath, bool bAutoRelease = true)
    {
        return LoadAsset<TextAsset>(assetPath, bAutoRelease);
    }
#endregion
}
