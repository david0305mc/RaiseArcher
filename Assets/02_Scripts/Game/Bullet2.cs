using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet2 : MonoBehaviour
{
    private Vector3 src;
    private Transform dst;
    private float elapse;
    [SerializeField] private Rigidbody2D rigid2d;

    private Quaternion quaternionRot;
    public void Shoot(Transform _dst)
    {
        src = transform.position;
        dst = _dst;
        elapse = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (elapse < 1f)
        {
            elapse += Time.deltaTime;
            Vector3 center = (src + dst.position) * 0.5F;
            center -= new Vector3(0, 1, 0);
            var prevPos = transform.position;
            transform.position = Vector3.Slerp(src - center, dst.position - center, elapse);
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
