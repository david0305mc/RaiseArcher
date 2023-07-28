using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPref;
    [SerializeField] private Transform arrow;

    public int arrowSpeed = 20;
    private float deg;

    private void Start()
    {
        deg = 0f;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            deg += Time.deltaTime * arrowSpeed;
            float rad = deg * Mathf.Deg2Rad;
            float cosX = Mathf.Cos(rad);
            float signX = Mathf.Sin(rad);
            Debug.Log($"cosX {cosX} signX {signX} ");
            arrow.localPosition = new Vector3(cosX, signX, 0);
            arrow.eulerAngles = new Vector3(0, 0, deg);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            deg -= Time.deltaTime * arrowSpeed;
            float rad = deg * Mathf.Deg2Rad;
            float cosX = Mathf.Cos(rad);
            float signX = Mathf.Sin(rad);
            Debug.Log($"cosX {cosX} signX {signX} ");
            arrow.localPosition = new Vector3(cosX, signX, 0);
            arrow.eulerAngles = new Vector3(0, 0, deg);
        }

        if (Input.GetMouseButtonUp(0))
        {
         
            var obj = GameObject.Instantiate(bulletPref);
            obj.transform.position = arrow.transform.position;
        }
    }
}
