using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

public class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private List<Transform> slotLists;
    [SerializeField] private GameObject boomPref;
    [SerializeField] private EnemyObj skeletonPref;
    [SerializeField] private TankObj tankPref;
    [SerializeField] private Transform enemySpawnPos;
    [SerializeField] private Transform worldRoot;

    private Dictionary<int, EnemyObj> enemyObjDic = new Dictionary<int, EnemyObj>();
    private List<TankObj> tankLists = new List<TankObj>();

    private void Start()
    {
        InitObjects();
        UpdateEnemySpawn().Forget();
        SpawnTanks();
    }
    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
    }

    public void InitObjects()
    { 
        
    }

    private void SpawnTanks()
    {
        Enumerable.Range(0, 8).ToList().ForEach(i => {
            var tankObj = Lean.Pool.LeanPool.Spawn(tankPref, slotLists[i]);
            tankLists.Add(tankObj);
        });
        
    }

    private async UniTask UpdateEnemySpawn()
    {
        while (true)
        {
            await UniTask.Delay(100);
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
        var enemyData = UserData.Instance.AddEnemy(UserData.Instance.GenerateUID());
        var obj = Lean.Pool.LeanPool.Spawn(skeletonPref, worldRoot);
        obj.SetData(enemyData.uid, ()=> {
            RemoveEnemy(enemyData.uid);
        });
        var randomePos = Random.insideUnitCircle * 2f;
        obj.transform.position = enemySpawnPos.position + new Vector3(randomePos.x, randomePos.y, 0);
        enemyObjDic.Add(enemyData.uid, obj);
    }

    public void RemoveEnemy(int uid)
    {
        if (UserData.Instance.EnemyDataDic.ContainsKey(uid))
        {
            Lean.Pool.LeanPool.Despawn(enemyObjDic[uid]);
            UserData.Instance.RemoveEnemy(uid);
            enemyObjDic.Remove(uid);
        }
    }

    public EnemyObj GetRandomeEnemy()
    {
        var enemyUID = UserData.Instance.GetRandomEnemy();
        if(enemyUID > 0)
            return enemyObjDic[enemyUID];
        return null;

    }

}
