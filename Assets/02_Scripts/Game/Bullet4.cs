using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet4 : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
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
        dstPos = dstObj.position;
        elapse = 0f;
        speed = _speed;
    }
    private void FixedUpdate()
    {
    }

    private void Update()
    {
        UpdateMissile();
    }
    private void UpdateMissile()
    {
        if (dstObj == null)
            return;
        float dist = Vector2.Distance(srcPos, dstObj.position);
        elapse += Time.deltaTime / dist * 10;

        var height = curve.Evaluate(elapse);

        var pos = Vector2.Lerp(srcPos, dstObj.position, elapse) + new Vector2(0, height);
        transform.position = pos;
        if (elapse >= 1)
        {
            Dispose();
        }
    }

    private void Dispose()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
        elapse = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            damagable.GetDamaged();
            //GameManager.Instance.ShowBoomEffect(collision.ClosestPoint(transform.position));

        }
        Dispose();
    }
}
