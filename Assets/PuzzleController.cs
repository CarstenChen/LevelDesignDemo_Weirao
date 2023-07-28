using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    public Transform respawnPos;
    public bool playerStepOnWrongGrid;

    public PuzzleGrid[] grids;
    public int rightGridNum;
    public PuzzleGrid[] rightGrids;
    public Treasure gift;

    private bool isReset;
    public bool hasPassed;
    public bool isReseting;

    private Coroutine resetPlayerCoroutine;
    // Start is called before the first frame update
    void Start()
    {


        grids = GetComponentsInChildren<PuzzleGrid>();
        rightGrids = new PuzzleGrid[rightGridNum];
        int index = 0;
        for (int i = 0; i < grids.Length; i++)
        {
            if (grids[i].isSolution)
            {
                Debug.Log(string.Format("Right{0},grid{1}", index, i));
                rightGrids[index++] = grids[i];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPassed) return;

        if (PassPuzzle())
        {
            hasPassed = true;
            gift.enabled = true;
        }

        //if(!PlayerOnPuzzle() && !hasPassed)
        //{
        //    ResetPuzzle(); 
        //}

        if (playerStepOnWrongGrid && !hasPassed &&!PlayerController.Instance.isRespawning)
        {
            playerStepOnWrongGrid = false;

            if (resetPlayerCoroutine == null)
                resetPlayerCoroutine = StartCoroutine(SetPlayer());
            else
            {
                StopCoroutine(resetPlayerCoroutine);
                resetPlayerCoroutine = StartCoroutine(SetPlayer());
            }

            StartCoroutine(WaitToResetPuzzle());
        }
    }
    IEnumerator SetPlayer()
    {
        isReseting = true;
        yield return new WaitForSeconds(0.5f);
        PlayerController.Instance.waitToRespawn = true;
        PlayerController.Instance.SetPosition(PlayerController.Instance.currentRespawnPos);
        yield return new WaitForSeconds(0.1f);
        if (!PlayerController.Instance.isRespawning)
        {
            PlayerController.Instance.gameObject.SetActive(false);
            PlayerController.Instance.gameObject.SetActive(true);
            PlayerController.Instance.Respawn();
        }
        isReseting = false;
        PlayerController.Instance.waitToRespawn = false;

    }

    IEnumerator WaitToResetPuzzle()
    {
        yield return new WaitForSeconds(0.5f);
        ResetPuzzle();
    }

    private void ResetPuzzle()
    {
      
        foreach (var grid in grids)
        {
            grid.ResetPosition();
        }
    }

    private bool PassPuzzle()
    {
        for (int i = 0; i < rightGrids.Length; i++)
        {
            if (!rightGrids[i].isTriggered)
            {
                return false;
            }
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerController.Instance.RegisterRespawnPos(respawnPos.position);
        }
    }

    bool PlayerOnPuzzle()
    {
        for (int i = 0; i < rightGrids.Length; i++)
        {
            if (rightGrids[i].playerOn)
            {
                return true;
            }
        }

        return false;
    }
}
