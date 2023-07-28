using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGrid : MonoBehaviour
{
    public bool isSolution;
    public float lowerHeight = 0.2f;
    public Material defaultMaterial;
    public Material rightMaterial;
    public Material falseMaterial;
    public GameObject body;
    public GameObject water;
    public GameObject toMove;
    [SerializeField] protected  Vector3 originalPos;
    [SerializeField] protected  Vector3 lowPos;
    protected bool isLow;
    [SerializeField]
    protected bool isMoving;
    public bool isTriggered;
    public bool playerOn;
    public bool ignoreReset;
    Coroutine currentMove;
    PuzzleController puzzleController;

    private void Awake()
    {
        originalPos = toMove.transform.position;
        lowPos = toMove.transform.position - new Vector3(0, lowerHeight, 0);
    }
    private void Start()
    {
        puzzleController = GetComponentInParent<PuzzleController>();

        if(isTriggered)
        {
            if (currentMove != null) StopCoroutine(currentMove);
            currentMove = StartCoroutine(GetLow());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (puzzleController.hasPassed) return;

        if (other.tag == "Player")
        {
            playerOn = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (puzzleController.hasPassed || PlayerController.Instance.isRespawning||puzzleController.isReseting) return;

        if(other.tag == "Player")
        {
            if (puzzleController.isReseting) return;

            body. GetComponent<MeshRenderer>().material = isSolution ? rightMaterial : falseMaterial;
            playerOn = true;

            if (isSolution)
                water.SetActive(true);

            if (!isSolution)
            {
                puzzleController.playerStepOnWrongGrid = true;
                //this.gameObject.SetActive(false);
                return;

            }

            
            Debug.Log("PlayerEnter");
            //if (isLow)
            //{
            //    isTriggered = false;
            //    if (currentMove != null) StopCoroutine(currentMove);
            //    currentMove = StartCoroutine(GetHigh());
            //    GetComponent<MeshRenderer>().material = defaultMaterial;
            //}

            if(!isLow)
            {
                isTriggered = true;
                if (currentMove != null) StopCoroutine(currentMove);
                currentMove = StartCoroutine(GetLow());
            }

        }
    }

    
    IEnumerator GetLow()
    {
        isMoving = true;
        isLow = true;
        while ((toMove.transform.position - lowPos).magnitude > 0.01f)
        {
            Low();
            yield return null;
        }

        toMove.transform.position = lowPos;
        isMoving = false;

    }

    IEnumerator GetHigh()
    {
        isMoving = true;
        isLow = false;
        while ((originalPos- toMove.transform.position).magnitude > 0.01f)
        {
            High();
            yield return null;
        }

        toMove.transform.position = originalPos;
        isMoving = false;
    }
    void Low()
    {

        toMove.transform.Translate((lowPos - toMove.transform.position)*Time.deltaTime,Space.Self);
    }
    void High()
    {
        transform.Translate((originalPos - toMove.transform.position) * Time.deltaTime, Space.Self);
    }

    public void ResetPosition()
    {
        if (!ignoreReset)
        {
            isTriggered = false;
            isLow = false;
            water.SetActive(false);
            toMove.transform.position = originalPos;
            body. GetComponent<MeshRenderer>().material = defaultMaterial;
            StopAllCoroutines();
        }

    }
}
