using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    private static RespawnTrigger instance;
    public static RespawnTrigger Instance { get { return instance; } private set {; } }

    public Vector3 respawnPosition;

    public bool playerDetected;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            playerDetected = true;
            PlayerController.Instance.SetPosition(PlayerController.Instance.currentRespawnPos);
            StartCoroutine(SetPlayer());
        }
    }

    IEnumerator SetPlayer()
    {

        yield return new WaitForSeconds(0.1f);
        if (!PlayerController.Instance.isRespawning)
        {
            PlayerController.Instance.gameObject.SetActive(false);
            PlayerController.Instance.gameObject.SetActive(true);
            PlayerController.Instance.Respawn();
        }
           

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerDetected = false;
        }
    }
}
