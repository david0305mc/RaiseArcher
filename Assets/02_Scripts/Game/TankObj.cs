using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TankObj : MonoBehaviour
{
    [SerializeField] private Bullet bulletPref;
    [SerializeField] private Transform arrow;

    public int rorateSpeed = 10;
    public int arrowSpeed = 20;
    public float deg;

    private int rotateDirection;


    private void Start()
    {
        rotateDirection = 1;
        deg = 30f;
        AutoRotate().Forget();
        AutoShoot().Forget();
    }

    async UniTask AutoRotate()
    {
        while (true)
        {
            await UniTask.Delay(10);
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

    async UniTask AutoShoot()
    {
        while (true)
        {
            await UniTask.Delay(500);
            var bullet = Lean.Pool.LeanPool.Spawn(bulletPref);
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
