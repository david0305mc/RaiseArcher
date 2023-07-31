using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private GameObject boomPref;

    public void ShowBoomEffect(Vector2 _pos)
    {
        var boomEffect = Instantiate(boomPref);
        boomEffect.transform.position = _pos;

        ParticleSystem ps = boomEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            if (main.loop)
            {
                ps.gameObject.AddComponent<CFX_AutoStopLoopedEffect>();
                ps.gameObject.AddComponent<CFX_AutoDestructShuriken>();
            }
        }
    }
}
