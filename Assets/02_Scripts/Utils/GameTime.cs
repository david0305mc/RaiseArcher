using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTime 
{
    private static long initGameTime = 0;
    private static float initClientStartupTime = 0;

    public static void Init(double _gameTime)
    {
        initGameTime = Convert.ToInt64(_gameTime);
        initClientStartupTime = Time.realtimeSinceStartup;
        //Debug.LogFormat("[GameTime/Set] {0}", Get(DateTimeKind.Utc));
    }

    public static long Get()
    {
        if (initGameTime == 0)
        {
            Init((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
        }

        return initGameTime + Convert.ToInt64(Time.realtimeSinceStartup - initClientStartupTime);
    }

}
