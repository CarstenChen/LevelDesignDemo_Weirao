using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//最后执行此类
[DefaultExecutionOrder(9999)]
public class GoFollower : MonoBehaviour
{
    [SerializeField] Transform toFollower;

    private void FixedUpdate()
    {
        transform.position = toFollower.position;
        transform.rotation = toFollower.rotation;
    }
}
