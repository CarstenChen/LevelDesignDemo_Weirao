using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwitcher : MonoBehaviour
{
    public Material switchTo;
    public PuzzleGrid relatedTo;
    // Start is called before the first frame update
    public void SwitchMaterial()
    {
        GetComponent<MeshRenderer>().material = switchTo;
    }

     void Update()
    {
        if (relatedTo.isTriggered) SwitchMaterial();
    }
}
