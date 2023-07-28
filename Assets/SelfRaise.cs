using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRaise : MonoBehaviour
{
    public float raiseHeight;
    private Vector3 targetPos;
    public bool isMoving;
    public Laser laser;
    public GameObject enable;
    // Start is called before the first frame update
    void Awake()
    {
        targetPos = transform.position + new Vector3(0, raiseHeight,0);
    }

    public void StartRaise()
    {
        GetComponent<MeshRenderer>().enabled = true;
        StartCoroutine(Raise());
    }

    IEnumerator Raise()
    {
        isMoving = true;
        while ((targetPos - transform.position).magnitude > 0.01f)
        {
            High();
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
        if (laser != null)
            laser.enabled = true;

        if (enable != null)
            enable.SetActive(true);
    }

    void High()
    {
        transform.Translate((targetPos - transform.position) *Time.deltaTime, Space.Self);
    }
}
