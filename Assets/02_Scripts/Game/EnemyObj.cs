using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObj : MonoBehaviour, Damageable
{

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(-1 * Time.deltaTime, 0));
    }

    public void GetDamaged()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
    }
}
