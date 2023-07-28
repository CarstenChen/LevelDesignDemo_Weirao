using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffects : MonoBehaviour
{
    public Light staffLight;

    Animation animation;

    void Awake()
    {
        animation = GetComponentInChildren<Animation>();

        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        staffLight.enabled = true;

        if (animation)
            animation.Play();

        StartCoroutine(DisableAtEndOfAnimation());
    }

    IEnumerator DisableAtEndOfAnimation()
    {
        yield return new WaitForSeconds(animation.clip.length);

        gameObject.SetActive(false);
        staffLight.enabled = false;
    }
}
