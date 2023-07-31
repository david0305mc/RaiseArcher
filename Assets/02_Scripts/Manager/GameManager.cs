using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private GameObject boomPref;
    [SerializeField] private EnemyObj skeletonPref;
    [SerializeField] private Transform enemySpawnPos;
    [SerializeField] private Transform worldRoot;

    private void Start()
    {
        UpdateSpawn().Forget();
    }

    private async UniTask UpdateSpawn()
    {
        while (true)
        {
            await UniTask.Delay(1000);
            SpwanEnemy();
        }
    }

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

    private void SpwanEnemy()
    {
        var obj = Lean.Pool.LeanPool.Spawn<EnemyObj>(skeletonPref, worldRoot);
        obj.transform.position = enemySpawnPos.position;
    }

}
