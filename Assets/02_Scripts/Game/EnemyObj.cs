using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObj : MonoBehaviour, Damageable
{
    public int speed = 1;
    private int uid;
    public int UID => uid;

    private System.Action removeAction;

    public void SetData(int _uid, System.Action _removeAction)
    {
        name = _uid.ToString();
        uid = _uid;
        removeAction = _removeAction;
    }

    void Update()
    {
        transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
    }

    public void GetDamaged()
    {
        removeAction?.Invoke();
    }
}
