using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractItem : MonoBehaviour
{
    [Header("Interactibe Settings")]
    public GameObject destroyAfterCollected;
    public GameObject particle;
    public GameObject specialEffect;
    public int plotID;
    
    public bool blockInteractionWhenReading;
    protected GameObject interactionUI;
    protected PlayerController player;
    [System.NonSerialized] public bool canInteract = true;

    protected virtual void Awake()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();

        interactionUI = player.transform.Find("PlayerCanvas").Find("InteractionUI").gameObject;
    }
    protected virtual void Start()
    {
        interactionUI.SetActive(false);
    }

    private void Update()
    {

    }
    public virtual void Interact()
    {
        if (specialEffect!=null)
        {
            specialEffect.SetActive(false);
            specialEffect.SetActive(true);
        }

        if (plotID != 0)
        {
            LinesManager.Instance.DisplayLine(plotID,0);
        }

        if (destroyAfterCollected != null)
        {
            HideObject();
        }
    }

    public virtual void HideObject()
    {
        MeshRenderer renderer = destroyAfterCollected.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
            if (particle != default)
                particle.SetActive(false);
        }

        else
        {
            destroyAfterCollected.SetActive(false);
            if (particle != default)
                particle.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && canInteract)
        {
            if ((LinesManager.isPlayingLines && blockInteractionWhenReading))
                interactionUI.SetActive(false);
            else
            {
                interactionUI.SetActive(true);

                if (PlayerInput.Instance.Interact)
                {
                    Interact();
                    interactionUI.SetActive(false);
                    canInteract = false;
                }
            }

        }

    }



    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            interactionUI.SetActive(false);
    }
}
