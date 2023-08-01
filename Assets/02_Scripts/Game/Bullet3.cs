using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet3 : MonoBehaviour
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
    private void FixedUpdate()
    {
        UpdateHomingMissile();
    }
    private void UpdateHomingMissile()
    {
        if (dstObj == null)
            return;

        Vector2 dir = (Vector2)dstObj.position - rigid2d.position;
        dir.Normalize();

        float rotateAmount = Vector3.Cross(dir, transform.up).z;
        rigid2d.angularVelocity = -rotateAmount * 200;
        rigid2d.velocity = transform.up * 1;

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
