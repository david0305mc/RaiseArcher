using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TankObj : MonoBehaviour
{
    [SerializeField] private Bullet bulletPref;
    [SerializeField] private Bullet2 bulletPref2;
    [SerializeField] private Bullet3 bulletPref3;
    [SerializeField] private Bullet4 bulletPref4;
    [SerializeField] private Transform arrow;

    public int rorateSpeed = 10;
    public float bulletSpeed = 2;
    public float deg;
    public int itemTID;
    private int slotIndex;

    private int rotateDirection;

    private CancellationTokenSource cts;

    private void Start()
    {
        rotateDirection = 1;
        deg = 30f;

        AutoRotate().Forget();
        AutoShoot2().Forget();
    }

    public void SetData(int _index)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource(); 
        slotIndex = _index;
        UpdateData();
    }

    public void UpdateData()
    {
        var slotData = UserData.Instance.LocalData.playSlotDataDic[slotIndex];
        var itemData = UserData.Instance.LocalData.GetItem(slotData.itemUID);
        itemTID = itemData.tid;

        Debug.Log($"slotIndex {slotIndex} slotData.itemUID {itemTID}");

        if (itemTID < 0)
        {
            Debug.LogError("itemTid = -1");
        }
    }

    async UniTask AutoRotate()
    {
        while (true)
        {
            await UniTask.Delay(1000, cancellationToken: cts.Token);
            deg += Time.deltaTime * rorateSpeed * rotateDirection;
            if (deg >= 50)
            {
                rotateDirection = -1;
            }
            else if (deg <= 20)
            {
                rotateDirection = 1;
            }

            float rad = deg * Mathf.Deg2Rad;
            float cosX = Mathf.Cos(rad);
            float signX = Mathf.Sin(rad);

            arrow.localPosition = new Vector3(cosX, signX, 0);
            arrow.eulerAngles = new Vector3(0, 0, deg);
        }
    }

    async UniTask AutoShoot2()
    {
        while (true)
        {
            await UniTask.Delay(300, cancellationToken: cts.Token);
            var target = GameManager.Instance.GetRandomeEnemy();
            if (target != null)
            {
                var bullet = Lean.Pool.LeanPool.Spawn(bulletPref4, arrow.transform.position, Quaternion.identity, GameManager.Instance.GetWorldRoot());
                if (itemTID > 0)
                {
                    bullet.UpdateData(itemTID);
                }
                else
                {
                    Debug.Log("itemTid = -1");
                }
                bullet.Shoot(target.UID, bulletSpeed);
            }
        }
    }

    async UniTask AutoShoot()
    {
        while (true)
        {
            await UniTask.Delay(500);
            Bullet bullet = Lean.Pool.LeanPool.Spawn(bulletPref);
            bullet.transform.position = arrow.transform.position;
            bullet.Shoot(arrow.localPosition.normalized);
        }
    }


    //void Update()
    //{
    //    if (Input.GetKey(KeyCode.UpArrow))
    //    {
    //        deg += Time.deltaTime * arrowSpeed;
    //        float rad = deg * Mathf.Deg2Rad;
    //        float cosX = Mathf.Cos(rad);
    //        float signX = Mathf.Sin(rad);
    //        Debug.Log($"cosX {cosX} signX {signX} ");
    //        arrow.localPosition = new Vector3(cosX, signX, 0);
    //        arrow.eulerAngles = new Vector3(0, 0, deg);
    //    }
    //    else if (Input.GetKey(KeyCode.DownArrow))
    //    {
    //        deg -= Time.deltaTime * arrowSpeed;
    //        float rad = deg * Mathf.Deg2Rad;
    //        float cosX = Mathf.Cos(rad);
    //        float signX = Mathf.Sin(rad);
    //        Debug.Log($"cosX {cosX} signX {signX} ");
    //        arrow.localPosition = new Vector3(cosX, signX, 0);
    //        arrow.eulerAngles = new Vector3(0, 0, deg);
    //    }

    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        var bullet = GameObject.Instantiate(bulletPref).GetComponent<Bullet>();
    //        bullet.transform.position = arrow.transform.position;
    //        bullet.Shoot(arrow.localPosition.normalized);
    //    }
    //}
}
