using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet4 : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector3 srcPos;
    private Vector3 dstPos;

    private float elapse;
    private float speed;
    private int targetUID;
    private Vector2 prevPos;

    private Quaternion quaternionRot;

    public void UpdateData(int _itemTID)
    {
        DataManager.ItemLevel itemData = DataManager.Instance.GetItemLevelData(_itemTID);
        spriteRenderer.SetSprite(itemData.iconpath);
    }

    public void Shoot(int _targetUID, float _speed)
    {
        targetUID = _targetUID;
        var enemyObj = GameManager.Instance.GetEnemyObj(targetUID);
        dstPos = enemyObj.transform.position;
        srcPos = transform.position;
        elapse = 0f;
        speed = _speed;
        prevPos = srcPos;
    }

    private void Update()
    {
        UpdateMissile();
    }

    private void UpdateMissile()
    {
        var enemyObj = GameManager.Instance.GetEnemyObj(targetUID);
        if (enemyObj != null)
        {
            dstPos = enemyObj.transform.position;
        }
        
        float dist = Vector2.Distance(srcPos, dstPos);
        elapse += Time.deltaTime / dist * 5f;

        var height = curve.Evaluate(elapse);

        var pos = Vector2.Lerp(srcPos, dstPos, elapse) + new Vector2(0, height);
        transform.position = pos;
        transform.rotation = GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT);

        prevPos = pos;
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
