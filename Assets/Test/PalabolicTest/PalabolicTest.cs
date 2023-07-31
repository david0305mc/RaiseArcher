using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalabolicTest : MonoBehaviour
{
    [SerializeField] private Transform src;
    [SerializeField] private Transform dst;
    [SerializeField] private GameObject cube;

    [SerializeField] private Transform A;
    [SerializeField] private Transform B;

    [SerializeField] private Transform C;

    [SerializeField] private LineRenderer lr;

    [Range(0, 1)] public float elapse;
    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    //cube.transform.position = Vector3.Slerp(src.position, dst.position, elapse);
    //    cube.transform.position = Vector3.Slerp(dst.position, src.position, elapse);
    //    //lr.SetPositions(new Vector3[] { src.position, dst.position });
    //}


    public Transform sunrise;
    public Transform sunset;

    // Time to move from sunrise to sunset position, in seconds.
    public float journeyTime = 1.0f;

    // The time at which the animation started.
    private float startTime;

    void Start()
    {
        // Note the time at the start of the animation.
        startTime = Time.time;
    }

    void Update()
    {
        // The center of the arc
        Vector3 center = (sunrise.position + sunset.position) * 0.5F;
        C.position = center;

        // move the center a bit downwards to make the arc vertical
        center -= new Vector3(0, 1, 0);

        // Interpolate over the arc relative to center
        Vector3 riseRelCenter = sunrise.position - center;
        Vector3 setRelCenter = sunset.position - center;
        A.position = riseRelCenter;
        B.position = setRelCenter;

        // The fraction of the animation that has happened so far is
        // equal to the elapsed time divided by the desired time for
        // the total journey.
        float fracComplete = (Time.time - startTime) / journeyTime;

        for (int i = 0; i < lr.positionCount; i++)
        {
            var point = Vector3.Slerp(riseRelCenter, setRelCenter, i / (float)(lr.positionCount - 1));
            lr.SetPosition(i, point + center);
        }

        transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, elapse);
        transform.position += center;


    }

}
