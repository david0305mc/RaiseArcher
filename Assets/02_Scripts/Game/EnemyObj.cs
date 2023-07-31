using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObj : MonoBehaviour, Damageable
{
    public int speed = 1;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
    }

    public void GetDamaged()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
    }
}
