using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using OfficeOpenXml;
using System.Text;
using UnityEditor;

public static class LanguageTool
{
    [MenuItem("Tools/LanguageExport")]
    public static void ExportLanguage()
    {
        var fileName = Application.dataPath.Replace("Assets", "") + "Tools/Language.xlsx";
        var fileInfo = new FileInfo(fileName);
        ExcelPackage.LicenseContext = LicenseContext.Commercial;
        using (var ePkg = new ExcelPackage(fileInfo))
        {
            var ws = ePkg.Workbook.Worksheets[0];

            HashSet<string> rKeys = new HashSet<string>();

            var nStartColumn = ws.Dimension.Start.Column;
            var nEndColumn = ws.Dimension.End.Column;
            var nStartRow = ws.Dimension.Start.Row;
            var nEndRow = ws.Dimension.End.Row;

            for(int col = nStartColumn + 1; col <= nEndColumn; col++)
            {
                rKeys.Clear();

                var lan = ws.GetValue<string>(nStartRow, col);

                fileName = string.Format("{0}/Localization_{1}.bytes", Application.streamingAssetsPath, lan);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                var fs = new FileStream(fileName, FileMode.Create);
                var bw = new BinaryWriter(fs, Encoding.UTF8);
                bw.Write(rKeys.Count);

                for(int row = nStartRow + 1; row <= nEndRow; row ++)
                {
                    var strKey = ws.GetValue<string>(row, nStartColumn);
                    if (rKeys.Contains(strKey))
                    {
                        Debug.LogError(string.Format("duplicate key {0} at cell({1}, {2})", strKey, row, col));
                        continue;
                    }

                    var strValue = ws.GetValue<string>(row, col);
                    if (null == strValue)
                    {
                        continue;
                    }

                    strValue = strValue.Replace("\"", "\\\"");

                    rKeys.Add(strKey);

                    bw.Write(strKey);
                    bw.Write(strValue);
                }

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write(rKeys.Count);
                bw.Flush();
                bw.Close();
                bw.Dispose();

                AssetDatabase.Refresh();
            }

            LocalizationService.Instance.Reload();

            EditorUtility.DisplayDialog("Language Export", "Language Export is finish!", "confirm");
        }
    }

    [MenuItem("Tools/Language2Text")]
    public static void Language2Text()
    {
        var filePath = EditorUtility.OpenFilePanel("Language Bin File", "", "bytes");
        if (string.IsNullOrEmpty(filePath))
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("{");
        using(var fs = new FileStream(filePath, FileMode.Open))
        {
            using(var br = new BinaryReader(fs))
            {
                var cnt = br.ReadInt32();
                for(int i=0; i < cnt; i++)
                {
                    var key = br.ReadString();
                    var value = br.ReadString();
                    
                    if (i + 1 < cnt)
                    {
                    }
                    sb.Append(string.Format("\t\"{0}\" : \"{1}\"", key, value));
                    sb.Append(i + 1 < cnt ? ",\n" : "\n");
                }
            }
        }
        sb.Append("}");

        var file = filePath.Replace(".bytes", ".json");
        File.WriteAllText(file, sb.ToString());

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Language2Text", "Language2Text is finish!", "confirm");
    }
}
