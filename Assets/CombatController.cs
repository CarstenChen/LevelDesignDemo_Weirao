using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CombatController : MonoBehaviour
{
    public int shouldDead;
    int deathNum;
    public CinemachineVirtualCamera showCamera;
    public GameObject gate;
    public GameObject[] enableInTrigger;
    public Transform respawnPos;
    public bool hasSpawn;
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        if(deathNum>= shouldDead)
        {
            StartCoroutine(OpenTheGate());
        }
    }

    IEnumerator OpenTheGate()
    {

        showCamera.Priority = 50;
        PlayerController.Instance.inNarrative = true;
        yield return new WaitForSeconds(1f);
        gate.GetComponent<SelfRaise>().StartRaise();

        yield return new WaitForSeconds(3f);
        PlayerController.Instance.inNarrative = false;
        showCamera.Priority = 1;
        this.gameObject.SetActive(false);
    }
    
    public void AddDeath()
    {
        deathNum++;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "Player")
        {
            PlayerController.Instance.RegisterRespawnPos(respawnPos.position);


            if (!hasSpawn)
            {
                hasSpawn = true;
                foreach (var g in enableInTrigger)
                {
                    g.SetActive(true);
                }
            }

        }

    }
}
