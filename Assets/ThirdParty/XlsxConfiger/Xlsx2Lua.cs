using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using OfficeOpenXml;
using System.Text;
using UnityEditor;
using System;

public static class Xlsx2Lua
{
    private static readonly string keySeparator = "&";
    private static readonly string ignoreSeparator = "#";

    [MenuItem("Tools/Xlsx2Lua")]
    public static void ExportToLua()
    {
        var folder = Application.dataPath.Replace("Assets", "Tools/Configs");
        var files = Directory.GetFiles(folder, "*.xlsx");

        var output = Application.dataPath + "/Lua/GameLogic/Config";

        var i = 0f;
        EditorUtility.DisplayProgressBar("Export Lua", "开始导出", 0);
        foreach(var file in files)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            if (filename.Contains(@"~$")) continue;

            EditorUtility.DisplayProgressBar("Export Lua", "开始导出->" + file, i++ / files.Length);
            var fileInfo = new FileInfo(file);
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            using (var ePkg = new ExcelPackage(fileInfo))
            {
                var ws = ePkg.Workbook.Worksheets[0];

                HashSet<string> rKeys = new HashSet<string>();

                var nStartColumn = ws.Dimension.Start.Column;
                var nEndColumn = ws.Dimension.End.Column;
                var nStartRow = ws.Dimension.Start.Row;
                var nEndRow = ws.Dimension.End.Row;

                var keyCol = -1;

                var opMap = new Dictionary<int, string>();
                HashSet<int> ingoreColumn = new HashSet<int>();
                for (int col = nStartColumn; col <= nEndColumn; col++)
                {
                    var op = ws.GetValue<string>(nStartRow + 1, col);
                    op.Trim();
                    if (keyCol < 0 && op.Contains(keySeparator))
                    {
                        op = op.Replace(keySeparator, string.Empty);
                        keyCol = col;
                    }
                    if (op.Contains(ignoreSeparator))
                    {
                        op = op.Replace(ignoreSeparator, string.Empty);
                        ingoreColumn.Add(col);
                    }
                    opMap.Add(col, op);
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("local t = {");
                var level = 0;
                for(int row = nStartRow + 2; row <= nEndRow; row ++)
                {
                    level++;
                    var tab = new string('\t', level);
                    if (keyCol > 0)
                    {
                        var op = opMap[keyCol];
                        var keyValue = GetItemValue(op, ws, row, keyCol);

                        if (op.Equals("float") || op.Equals("int") || op.Equals("double"))
                        {
                            keyValue = keyValue.Replace("\"", string.Empty);
                            keyValue = $"[{keyValue}]";
                        }

                        sb.Append($"{tab}{keyValue} = ");
                        sb.AppendLine("{");
                    }
                    else
                    {
                        sb.Append(tab);
                        sb.AppendLine("{");
                    }

                    for(int col = nStartColumn; col <= nEndColumn; col ++)
                    {
                        if (ingoreColumn.Contains(col)) continue;
                        var keyValue = GetItemValue("string", ws, nStartRow, col);

                        keyValue = keyValue.Replace("\"", string.Empty);

                        var op = opMap[col];
                        var value = GetItemValue(op, ws, row, col);
                        sb.AppendLine($"{tab}\t{keyValue} = {value},");
                    }
                    
                    sb.Append(tab);
                    sb.AppendLine("}");
                }
                sb.AppendLine("}");
                sb.Append("return t");

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                var path = string.Format("{0}/{1}.lua", output, filename);
                File.WriteAllText(path, sb.ToString());
            }
        }

        EditorUtility.DisplayDialog("导出成功！", $"成功导出Lua配置\n[{output}]", "确定");
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private static string GetItemValue(string op, ExcelWorksheet worksheet, int row, int col)
    {
        string ret = string.Empty;
        switch(op)
        {
            case "string":
                ret = string.Format("\"{0}\"", worksheet.GetValue<string>(row, col));
                break;
            case "int":
                ret = string.Format("{0}", worksheet.GetValue<int>(row, col));
                break;
            case "float":
                ret = string.Format("{0}", worksheet.GetValue<float>(row, col));
                break;
            case "double":
                ret = string.Format("{0}", worksheet.GetValue<double>(row, col));
                break;
            case "bool":
                ret = string.Format("{0}", worksheet.GetValue<bool>(row, col));
                break;
        }
        return ret;
    }
}
