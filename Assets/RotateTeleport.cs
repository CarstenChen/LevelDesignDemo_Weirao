using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTeleport : MonoBehaviour
{
    public float toAngle;
    public float speed;
    private bool playerOn;
    private bool isMoving;
    bool waitNextTime;
    private void Start()
    {
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!isMoving && !waitNextTime)
            {
                waitNextTime = true;
                PlayerController.Instance.transform.SetParent(this.transform);
                StopAllCoroutines();
                StartCoroutine(Rotate(90,speed));
            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            waitNextTime = false;
            PlayerController.Instance.transform.SetParent(null);
            StopAllCoroutines();
            isMoving = false;
            //StartCoroutine(Rotate(0, -speed));
        }
    }

    IEnumerator Rotate(float toAngle, float speed) 
    { 
        isMoving = true;

        while (Compare(Quaternion.Euler(transform.eulerAngles), Quaternion.Euler(0, toAngle-180, 0))>0.1f)
        {
            Debug.Log(Compare(Quaternion.Euler(transform.eulerAngles), Quaternion.Euler(0, toAngle - 180, 0)));
            Go(toAngle,speed);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, toAngle - 180, 0);
        isMoving = false;
    }

    void Go(float toAngle, float speed)
    {
        transform.rotation= Quaternion.RotateTowards(Quaternion.Euler(transform.eulerAngles), Quaternion.Euler(0, toAngle, 0), speed*Time.deltaTime);
    }

    private float Compare(Quaternion quatA, Quaternion quatB)
    {
        Debug.Log(Quaternion.Angle(quatA, quatB));

        return Quaternion.Angle(quatA, quatB);
}
}
