using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum LocalizationLanguage
{
    zhHans,
    zhHK,
    zhHant,
    en,
    jp,
}

public class LocalizationService : Singleton<LocalizationService>
{
    public delegate void NotifyReload();
    public event NotifyReload NotifyReloadEvent;
    private LocalizationLanguage m_Language = LocalizationLanguage.zhHans;
    private Dictionary<string, string> m_LocalizationTextMap = new Dictionary<string, string>();
    private string GetLocalizationFilePath(LocalizationLanguage language)
    {
        var filePath = string.Format("{0}/Localization_{1}.bytes", Application.streamingAssetsPath, language.ToString());
        return filePath;
    }

    protected override void Init()
    {
        base.Init();
        LoadLocalizationText();
    }

    protected override void UnInit()
    {
        base.UnInit();
    }

    public void Reload()
    {
        NotifyReloadEvent?.Invoke();
    }

    private bool LoadLocalizationText()
    {
        m_LocalizationTextMap.Clear();

        var filePath = GetLocalizationFilePath(m_Language);
        if (!File.Exists(filePath))
        {
            Debug.LogError("can not find file at " + filePath);
            return false;
        }

        using(var fs = new FileStream(filePath, FileMode.Open))
        {
            using(var br = new BinaryReader(fs))
            {
                var cnt = br.ReadInt32();
                for(int i=0; i < cnt; i++)
                {
                    var key = br.ReadString();
                    var value = br.ReadString();
                    m_LocalizationTextMap[key] = value;
                }
            }
        }

        return true;
    }

    public LocalizationLanguage Language
    {
        get {
            return m_Language;
        }

        set {
            if (value != m_Language)
            {
                m_Language = value;
                LoadLocalizationText();
            }
        }
    }

    public bool IsSupport(LocalizationLanguage language)
    {
        var filePath = GetLocalizationFilePath(language);
        if (!File.Exists(filePath))
        {
            return false;
        }
        return true;
    }

    public bool HasKey(string key)
    {
        return m_LocalizationTextMap.ContainsKey(key);
    }

    public string GetValue(string key)
    {
        if (m_LocalizationTextMap.TryGetValue(key, out var localizationText))
        {
            return localizationText;
        }
        return string.Empty;
    }
}
