using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObj : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(-1 * Time.deltaTime, 0));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
        GameManager.Instance.ShowBoomEffect(collision.ClosestPoint(transform.position));
    }
}
