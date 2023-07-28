using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public float raiseHeight;
    public float speed;
    private Vector3 originalPos;
    private Vector3 targetPos;
    private bool playerOn;
    private bool isMoving;

    private void Start()
    {
        originalPos = transform.position;
        targetPos = transform.position + new Vector3(0, raiseHeight, 0);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!isMoving)
            {
                PlayerController.Instance.transform.SetParent(this.transform);
                StopAllCoroutines();
                StartCoroutine(Raise(targetPos));
            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerController.Instance.transform.SetParent(null);
            StopAllCoroutines();
            StartCoroutine(Raise(originalPos));
        }
    }

    IEnumerator Raise(Vector3 toPosition)
    {
        isMoving = true;
        while ((toPosition - transform.position).magnitude > 0.2f)
        {
            High(toPosition);
            yield return null;
        }

        transform.position = toPosition;
        isMoving = false;
    }

    void High(Vector3 toPosition)
    {
        transform.Translate((toPosition - transform.position) * Time.deltaTime *speed, Space.World);

        transform.position = Vector3.Lerp(transform.position, toPosition, Time.deltaTime * speed);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
    }
}
