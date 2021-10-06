using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Utilities
{
    public static class LocalStorage
    {
        public static readonly string characters
        = "0123456789"
        + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        + "abcdefghijklmnopqrstuvwxyz";

        public static string localKey;

        static LocalStorage()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(characters[15]);
            sb.Append(characters[13]);
            sb.Append(characters[15]);
            sb.Append(characters[47]);
            sb.Append(characters[9]);
            sb.Append(characters[11]);
            sb.Append(characters[46]);
            sb.Append(characters[10]);
            sb.Append(characters[18]);
            sb.Append(characters[27]);
            sb.Append(characters[12]);
            sb.Append(characters[40]);
            sb.Append(characters[21]);
            sb.Append(characters[38]);
            sb.Append(characters[54]);
            sb.Append(characters[36]);
            sb.Append(characters[9]);
            sb.Append(characters[41]);
            sb.Append(characters[34]);
            sb.Append(characters[54]);
            sb.Append(characters[22]);
            sb.Append(characters[38]);
            sb.Append(characters[41]);
            sb.Append(characters[24]);
            sb.Append(characters[42]);
            sb.Append(characters[13]);
            sb.Append(characters[31]);
            sb.Append(characters[38]);
            sb.Append(characters[17]);
            sb.Append(characters[4]);
            sb.Append(characters[21]);
            sb.Append(characters[54]);

            localKey = sb.ToString();
        }

        public static void SetInt(string key, int value)
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            var eValue = AESHelper.AesEncrypt(value.ToString(), localKey);
            PlayerPrefs.SetString(eKey, eValue);
            PlayerPrefs.Save();
        }

        public static int GetInt(string key, int @default = 0)
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            if (!PlayerPrefs.HasKey(eKey))
            {
                return @default;
            }
            var value = PlayerPrefs.GetString(eKey);
            value = AESHelper.AesDecrypt(value, localKey);
            return int.TryParse(value, out var result) ? result : @default;
        }

        public static void SetFloat(string key, float value)
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            var eValue = AESHelper.AesEncrypt(value.ToString(), localKey);
            PlayerPrefs.SetString(eKey, eValue);
            PlayerPrefs.Save();
        }

        public static float GetFloat(string key, float @default = 0f)
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            if (!PlayerPrefs.HasKey(eKey))
            {
                return @default;
            }
            var value = PlayerPrefs.GetString(eKey);
            value = AESHelper.AesDecrypt(value, localKey);
            return float.TryParse(value, out var result) ? result : @default;
        }

        public static void SetString(string key, string value)
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            var eValue = AESHelper.AesEncrypt(value, localKey);
            PlayerPrefs.SetString(eKey, eValue);
            PlayerPrefs.Save();
        }

        public static string GetString(string key, string @default = "")
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            if (!PlayerPrefs.HasKey(eKey))
            {
                return @default;
            }
            var value = PlayerPrefs.GetString(eKey);
            value = AESHelper.AesDecrypt(value, localKey);
            return string.IsNullOrEmpty(value) ? @default : value;
        }

        public static void DeleteKey(string key)
        {
            var eKey = AESHelper.AesEncrypt(key, localKey);
            if (!PlayerPrefs.HasKey(eKey))
            {
                return;
            }
            PlayerPrefs.DeleteKey(eKey);
            PlayerPrefs.Save();
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}