using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Text))]
public class LocalizationText : MonoBehaviour
{
    [SerializeField]
    private string m_Key = string.Empty;

    [SerializeField]
    private LocalizationLanguage m_Language;

    private Text m_Text;

    void Awake()
    {
        m_Text = GetComponent<Text>();
        m_Text.text = LocalizationService.Instance.GetValue(m_Key);

#if UNITY_EDITOR
        LocalizationService.Instance.NotifyReloadEvent += OnLanguageServiceReload;
#endif
    }

#if UNITY_EDITOR
    void OnDestroy()
    {
        LocalizationService.Instance.NotifyReloadEvent -= OnLanguageServiceReload;
    }

    void OnLanguageServiceReload()
    {
        m_Text.text = LocalizationService.Instance.GetValue(m_Key);
    }
#endif
}