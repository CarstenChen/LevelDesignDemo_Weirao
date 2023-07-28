using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMethod : MonoBehaviour
{
    public bool canRotate;
    private void Update()
    {
        if (canRotate)
        {
            RotateTransform();
        }
    }

    public void RotateTransform()
    {
        transform.Rotate(new Vector3(0, 0, 1));
    }
}
