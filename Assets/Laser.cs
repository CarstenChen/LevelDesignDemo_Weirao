using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Transform target;
    public Transform thisHead;
    public GameObject disableShield;
    private LineRenderer lr;
    private Transform[] points;
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        SetUpLine();

        if (disableShield!=null)
        {
            disableShield.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUpLine()
    {
        lr.positionCount = 2;
        lr.SetPosition(0, thisHead.position);
        lr.SetPosition(1, target.position);
    }
}
