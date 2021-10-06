using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLib;
using System.IO;
using UnityEngine.SceneManagement;

public class GameApp : MonoBehaviour
{
    [Header("更新界面")] [SerializeField] private string updatePanel;
    [Header("游戏Scene场景入口")] [SerializeField] private string gameScene;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        Logger.Info?.Output("Game App Start");
            
        yield return Assets.Initialize();

#if UNITY_EDITOR
        if (Assets.IsBundleMode)
        {
#endif
            var assetRequest = Assets.LoadAssetAsync(updatePanel, typeof(GameObject));
            yield return assetRequest;
            var panel = Instantiate(assetRequest.asset as GameObject);
            panel.name = Path.GetFileNameWithoutExtension(updatePanel);

            var updater = panel.GetComponent<UpdatePanel>();
            yield return updater.StartUpdate();
#if UNITY_EDITOR
        }
#endif
        yield return Assets.LoadSceneAsync(gameScene, LoadSceneMode.Single);

        XLuaManager.Instance.InitLuaEnv();
    }
}
