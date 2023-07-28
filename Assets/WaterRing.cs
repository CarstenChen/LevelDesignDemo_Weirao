using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRing : MonoBehaviour
{
    public GameObject startParticle;
    public GameObject ringParticle;
    public LayerMask ground;
    private void OnEnable()
    {
        RaycastHit hit;
        Physics.Raycast(PlayerController.Instance.transform.position, Vector3.down, out hit, 5f,ground);
        transform.position = hit.point;
        startParticle.SetActive(true);
        PlayParticle();

        StartCoroutine(WaitToShowRing());
    }

    IEnumerator WaitToShowRing()
    {
        yield return new WaitForSeconds(2.5f);
        startParticle.SetActive(false);
        ringParticle.SetActive(true);
        PlayParticle();
        StartCoroutine(FinishRing());
    }

    IEnumerator FinishRing()
    {
        yield return new WaitForSeconds(5f);
        ringParticle.SetActive(false);
        this.gameObject.SetActive(false);
    }

   void  PlayParticle()
    {
        ParticleSystem[] parts = GetComponentsInChildren<ParticleSystem>();
        foreach(var p in parts)
        {
            
            p.Play();
        }
    }

    public void Reset()
    {
        startParticle.SetActive(false);
        ringParticle.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
