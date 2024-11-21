using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Firebase.Auth;

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
    [MenuItem("Tools/SignOutFirebaseAuth")]
    public static void RemoveFirebaseCache()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        // 로그아웃 처리
        
        auth.CurrentUser.DeleteAsync();

        Debug.Log("Authentication cache cleared.");
    }

}
