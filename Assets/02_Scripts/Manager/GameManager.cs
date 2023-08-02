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

    private Camera mainCam;
    private Dictionary<int, EnemyObj> enemyObjDic = new Dictionary<int, EnemyObj>();
    private List<TankObj> tankLists = new List<TankObj>();


    //public bool HasEnemyObj(int uid) => enemyObjDic.ContainsKey(uid);
    //public EnemyObj EnemyObj(int uid) => {
    //     retu enemyObjDic[uid];
    //    };
    public EnemyObj GetEnemyObj(int _uid)
    {
        enemyObjDic.TryGetValue(_uid, out EnemyObj value);
        return value;
    }

    public Transform GetWorldRoot()
    {
        return worldRoot;
    }

    private void Start()
    {
        InitObjects();
        UpdateEnemySpawn().Forget();
        SpawnTanks();
    }
    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        mainCam = Camera.main;
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

    public void ShowBoomEffect(Vector2 _pos, string name = default)
    {
        var boomEffect = Instantiate(boomPref);
        boomEffect.name = name;
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
        var randomePos = Random.insideUnitCircle * 2f;
        var pos = enemySpawnPos.position + new Vector3(randomePos.x, randomePos.y, 0);
        var enemyData = UserData.Instance.AddEnemy(UserData.Instance.GenerateUID());
        var obj = Lean.Pool.LeanPool.Spawn(skeletonPref, pos, Quaternion.identity, worldRoot);
        obj.SetData(enemyData.uid, ()=> {
            RemoveEnemy(enemyData.uid);
        });
        Debug.Log($"Enemy UID {enemyData.uid} pos {obj.transform.position}");
        enemyObjDic.Add(enemyData.uid, obj);
    }

    public void RemoveEnemy(int uid)
    {
        if (UserData.Instance.EnemyDataDic.ContainsKey(uid))
        {
            Debug.Log($"Destroy Enemy UID {uid} pos {enemyObjDic[uid].transform.position}");
            GameManager.Instance.ShowBoomEffect(enemyObjDic[uid].transform.position, uid.ToString());
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

    public void AddItem()
    {
        var tile = UserData.Instance.GetEmptyTile();
        var itemData = UserData.Instance.AddItemData(UserData.Instance.GenerateUID(), tile.Item1, tile.Item2);
        MergeManager.Instance.AddItem(itemData.x, itemData.y);
        
    }

}
