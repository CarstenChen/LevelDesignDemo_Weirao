using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Treasure : MonoBehaviour
{
    public GameObject wheel;
    //public GameObject[] links;
    public GameObject treasure;
    public GameObject gate;
    public CinemachineVirtualCamera showCamera; 
    public GameObject reverseAble;
    // Start is called before the first frame update
    void OnEnable()
    {if (!wheel.activeSelf)
            wheel.SetActive(true);

        if (reverseAble != null) reverseAble.SetActive(!reverseAble.activeSelf);
        wheel.GetComponent<RotateMethod>().canRotate = true;

        //if (links.Length != 0)
        //{
        //    foreach (var link in links)
        //    {
        //        link.GetComponent<SpriteSwitcher>().SwitchMaterial();
        //    }
        //}


        treasure.GetComponent<SelfRaise>().StartRaise();

        if(gate!=null)
        StartCoroutine(OpenTheGate());
    }

    IEnumerator OpenTheGate()
    {
        yield return new WaitUntil(()=>treasure.GetComponent<SelfRaise>().isMoving==false);

        showCamera.Priority = 50;
        PlayerController.Instance.inNarrative = true;
        yield return new WaitForSeconds(1f);
        gate.GetComponent<SelfRaise>().StartRaise();

        yield return new WaitForSeconds(3f);
        PlayerController.Instance.inNarrative = false;
        showCamera.Priority = 1;
    }

}
