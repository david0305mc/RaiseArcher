using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class UtilExtension 
{
    public static Transform FirstChildOrDefault(this Transform parent, Func<Transform, bool> query)
    {
        if (parent.childCount == 0)
        {
            return null;
        }

        Transform result = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (query(child))
            {
                return child;
            }
            result = FirstChildOrDefault(child, query);
        }

        return result;
    }


    public static void SetSprite(this Image s, string path)
    {
        if (s == null)
            return;

        if (string.IsNullOrEmpty(path))
        {
            s.sprite = null;
            return;
        }
        s.sprite = Utill.Load<Sprite>(path);
    }

    public static void SetActiveRecursively(this GameObject obj, bool state)
    {
        obj.SetActive(state);
        foreach (Transform child in obj.transform)
        {
            SetActiveRecursively(child.gameObject, state);
        }
    }

    public static void Clear(this CancellationTokenSource cts)
    {
        cts.Cancel();
        cts.Dispose();
    }
}
