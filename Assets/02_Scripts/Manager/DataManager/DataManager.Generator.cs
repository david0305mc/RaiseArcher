using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

public partial class DataManager
{
    static readonly string DATATABLE_DEF_PATH = "Assets/02_Scripts/Manager/DataManager/DataManager.Data.cs";
    static readonly string CONFIG_TABLE_DEF_PATH = "Assets/02_Scripts/Manager/DataManager/ConfigTable.cs";
    static readonly string TABLE_ENUM_DEF_PATH = "Assets/02_Scripts/Manager/DataManager/EnumTable.cs";

    //static readonly string LOCAL_CSV_PATH = $"{Application.dataPath}/../Cache/dev";
    static readonly string LOCAL_CSV_PATH = $"{Application.dataPath}/Resources/Data";

    static readonly string CONFIG_TABLE_NAME = "ConfigTable.csv";
    static readonly string ENUM_TABLE_NAME = "EnumTable.csv";

    public static void GenDatatable()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("#pragma warning disable 114\n");
        sb.Append("using System;\n");
        sb.Append("using System.Collections;\n");
        sb.Append("using System.Collections.Generic;\n");
        sb.Append("using System.Linq;\n");

        sb.AppendLine("public partial class DataManager {");
        GenTableData(sb);

        sb.Append("};");
        WriteCode(DATATABLE_DEF_PATH, sb.ToString());
    }

    private static void GenTableData(StringBuilder sb)
    {
        foreach (var tableName in DataManager.tableNames)
        {
            var data = Utill.LoadFromFile(Path.Combine(LOCAL_CSV_PATH, $"{tableName}.csv"));
            List<string[]> rows = CSVSerializer.ParseCSV(data, '|');

            string tableNameUpper = $"{tableName[0].ToString().ToUpper()}{tableName.Substring(1).ToLower() }";
            string arrayName = $"{tableNameUpper}Array";
            string dicName = $"{ tableNameUpper }Dic";

            sb.AppendFormat("\tpublic partial class {0} {{\n", tableName);

            Enumerable.Range(0, rows[0].Length).ToList().ForEach(i =>
            {
                var type = rows[1][i];
                if (!(type == "int" || type == "long" || type == "float" || type == "string"))
                {
                    type = type.ToUpper();
                }
                sb.AppendFormat($"\t\tpublic {type} {rows[0][i].ToLower()};\n");
            });

            sb.Append("\t};\n");
            sb.Append($"\tpublic {tableName}[] {arrayName} {{ get; private set; }}\n");
            sb.Append($"\tpublic Dictionary<int, {tableName}> {dicName} {{ get; private set; }}\n");

            sb.Append($"\tpublic void Bind{tableName}Data(Type type, string text){{\n");
            sb.Append("\t\tvar deserializaedData = CSVDeserialize(text, type);\n");
            sb.Append($"\t\tGetType().GetProperty(nameof({arrayName})).SetValue(this, deserializaedData, null);\n");
            sb.Append($"\t\t{dicName} = {arrayName}.ToDictionary(i => i.id);\n");
            sb.Append("\t}\n");

            sb.Append($"\tpublic {tableName} Get{tableName}Data(int _id){{\n");
            sb.Append($"\t\tif ({dicName}.TryGetValue(_id, out {tableName} value)){{\n");
            sb.Append($"\t\t\treturn value;\n");
            sb.Append($"\t\t}}\n");

            string errorMsg = $"table doesnt contain id {{_id}}";
            sb.Append($"\t\tUnityEngine.Debug.LogError($\"{errorMsg}\");\n");
            sb.Append("\t\treturn null;\n");
            sb.Append("\t}\n");
        }
    }

    public static void GenConfigTable()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("#pragma warning disable 114\n");
        sb.Append("using System;\n");
        sb.Append("using System.Collections;\n");
        sb.Append("using System.Collections.Generic;\n");
        sb.Append("public class ConfigTable : Singleton<ConfigTable>{\n");

        GenConfigTableData(sb);
        sb.Append("};");

        WriteCode(CONFIG_TABLE_DEF_PATH, sb.ToString());
    }

    private static void GenConfigTableData(StringBuilder sb)
    {
        sb.AppendLine();
        var data = Utill.LoadFromFile(Path.Combine(LOCAL_CSV_PATH, CONFIG_TABLE_NAME));
        List<string[]> rows = CSVSerializer.ParseCSV(data, '|');

        Enumerable.Range(2, rows.Count - 2).ToList().ForEach(i =>
        {
            var name = rows[i][0];
            var type = rows[i][1];
            sb.AppendLine($"\tpublic {type} {name};");
        });

        sb.AppendLine("\tpublic void LoadConfig(Dictionary<string, Dictionary<string, object>> rowList)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tforeach (var rowItem in rowList)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tvar field = typeof(ConfigTable).GetField(rowItem.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);");
        sb.AppendLine("\t\t\tfield.SetValue(this, rowItem.Value[\"value\"]);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
    }

    public static void WriteCode(string filePath, string content)
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            try
            {
                writer.WriteLine("{0}", content);
                Debug.LogWarningFormat("File {0} generated", filePath);
            }
            catch (System.Exception ex)
            {
                string msg = " threw:\n" + ex.ToString();
                Debug.LogError(msg);
            }
        }
    }
    public static void GenTableEnum()
    {
        StringBuilder sb = new StringBuilder();
        GenTableEnum(sb);
        WriteCode(TABLE_ENUM_DEF_PATH, sb.ToString());
    }
    private static void GenTableEnum(StringBuilder sb)
    {
        var data = Utill.LoadFromFile(Path.Combine(LOCAL_CSV_PATH, ENUM_TABLE_NAME));
        List<string[]> rows = CSVSerializer.ParseCSV(data, '|');

        HashSet<string> keySet = new HashSet<string>();

        Enumerable.Range(2, rows.Count - 2).ToList().ForEach(i =>
        {
            string enumType = rows[i][0].ToString().ToUpper();
            if (!keySet.Contains(enumType))
            {
                if (keySet.Count > 0)
                    sb.AppendFormat("}}\n");
                sb.AppendFormat("public enum {0} \n{{ \n", enumType);
                keySet.Add(enumType);
            }

            sb.AppendFormat("\t{0, -28} = {1, -10}", rows[i][1].ToString().ToUpper(), rows[i][2] + ",");
            sb.AppendFormat("\t// {0}", rows[i][3]);
            sb.AppendLine();
        });
        sb.AppendFormat("}}");
    }
}
