using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorMenu 
{
    [MenuItem("Tools/GenerateTableCode")]
    public static void GenerateTableCode()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }
        
        DataManager.GenDatatable();
        DataManager.GenConfigTable();
        DataManager.GenTableEnum();
        Debug.Log("GenerateTableCode");
    }
}
