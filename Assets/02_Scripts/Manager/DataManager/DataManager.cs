
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.Networking;

public class Table
{
	public int index;
}

public partial class DataManager : Singleton<DataManager>
{
	public static string[] tableNames =
		{
			"ObjTable",
			"SpriteSheet",
			"SpriteCollection",
		};

	public async UniTask LoadDataAsync()
	{
		foreach (var tableName in tableNames)
		{
#if DEV
			string data = Resources.Load<TextAsset>(Path.Combine("Data", $"{tableName}")).ToString();
#else
			string data = await Utill.LoadFromFileAsync(Path.Combine(LOCAL_CSV_PATH, $"{tableName}.csv"));
#endif

			MethodInfo method = GetType().GetMethod($"Bind{tableName}Data");
            method.Invoke(DataManager.Instance, new object[] { Type.GetType($"DataManager+{tableName}"), data});
        }
	}

	public async UniTask LoadConfigTable()
	{
#if DEV
		string data = Resources.Load<TextAsset>(Path.Combine("Data", "ConfigTable")).ToString();
#else
		string path = Path.Combine(LOCAL_CSV_PATH, CONFIG_TABLE_NAME);
        var data = (await UnityWebRequest.Get(path).SendWebRequest()).downloadHandler.text;
#endif

		List<string[]> rows = CSVSerializer.ParseCSV(data.ToString(), '|');
		rows.RemoveRange(0, 2);
		foreach (var rowItem in rows)
		{
			var field = typeof(ConfigTable).GetField(rowItem[0], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			try
			{
				CSVSerializer.SetValue(ConfigTable.Instance, field, rowItem[2]);
			}
			catch
			{
				Debug.LogWarning($"{rowItem[0]} invalid");
			}
		}
	}
	object CSVDeserialize(string text, Type type, bool hasSkipLine = true)
	{
		
		List<string[]> rows = CSVSerializer.ParseCSV(text, '|');
		if (hasSkipLine)
			rows.RemoveAt(1);

		var ret = CSVSerializer.Deserialize(rows, type);
		return ret;
	}
}

