using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet2 : MonoBehaviour
{
    private Vector3 srcPos;
    private Vector3 dstPos;
    private Transform dstObj;
    private float elapse;
    private float speed;


    [SerializeField] private Rigidbody2D rigid2d;

    private Quaternion quaternionRot;
    public void Shoot(Transform _dst, float _speed)
    {
        srcPos = transform.position;
        dstObj = _dst;
        elapse = 0f;
        speed = _speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (elapse < 1f)
        {
            elapse += Time.deltaTime * speed;
            if (dstObj != null)
            {
                dstPos = dstObj.position;
            }
            Vector3 center = (srcPos + dstPos) * 0.5F;
            center -= new Vector3(0, 1, 0);
            var prevPos = transform.position;
            transform.position = Vector3.Slerp(srcPos - center, dstPos - center, elapse);
            transform.position += center;
            //transform.LookAt(dst.position, Vector3.right);
            quaternionRot = GameUtil.LookAt2D(prevPos, transform.position, GameUtil.FacingDirection.RIGHT);
            if (elapse >= 1f)
            {
                Dispose();
            }
        }
    }

    private void FixedUpdate()
    {
        transform.rotation = quaternionRot;
    }

    private void Dispose()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.Instance.ShowBoomEffect(collision.ClosestPoint(transform.position));
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            damagable.GetDamaged();
        }
        Dispose();
    }
}
