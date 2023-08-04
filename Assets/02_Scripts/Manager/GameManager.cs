using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

public partial class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private List<Transform> slotLists;
    [SerializeField] private GameObject boomPref;
    [SerializeField] private EnemyObj skeletonPref;
    [SerializeField] private TankObj tankPref;
    [SerializeField] private Transform enemySpawnPos;
    [SerializeField] private Transform worldRoot;


    private Camera mainCam;
    private Dictionary<int, EnemyObj> enemyObjDic = new Dictionary<int, EnemyObj>();
    private Dictionary<int, TankObj> tankDic = new Dictionary<int, TankObj>();

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
        InitMergeTile();
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
        Enumerable.Range(0, 8).ToList().ForEach(i => 
        {
            UserData.Instance.AddTank(i, -1);
            TankObj tankObj = Lean.Pool.LeanPool.Spawn(tankPref, slotLists[i]);
            tankObj.SetData(i);
            tankDic.Add(i, tankObj);
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

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            MouseDownEvent4Merge();
        }

    }
    private void MouseDownEvent4Merge()
    {
        var mouseDownPos4Merge = mainCam.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(mouseDownPos4Merge, Vector3.zero, 10, Game.GameConfig.ItemLayerMask);
        if (hit.collider != null)
        {
            UIItemObj itemObj = hit.collider.GetComponentInParent<UIItemObj>();
            Debug.Log(itemObj.ItemData.uid);
            //touchDownObj = itemObj;
            //touchDownObj.IsDragging = false;
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
        enemyObjDic.Add(enemyData.uid, obj);
    }

    public void RemoveEnemy(int uid)
    {
        if (UserData.Instance.EnemyDataDic.ContainsKey(uid))
        {
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

    public void SetTankSlot(int _index, int _ItemUID)
    {
        UserData.Instance.SetTankSlot(_index, _ItemUID);
        tankDic[_index].SetData(_ItemUID);
    }
}
