using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlink : MonoBehaviour
{
    public float frequency1;
    public float frequency2;
    public float low;
    public float high;
    public float currentCount;

    private bool isLowIntensity;
    private Light spotLight;
    private float frequency;
    private void Start()
    {
        spotLight = GetComponent<Light>();
        RollFrequency();
    }
    // Update is called once per frame
    void Update()
    {
        currentCount += Time.deltaTime;

        if (currentCount > frequency)
        {
            isLowIntensity = !isLowIntensity;
            spotLight.intensity = isLowIntensity ? low : high;

            currentCount = 0;
            RollFrequency();
        }
    }

    void RollFrequency()
    {
        frequency = Random.Range(frequency1, frequency2);
    }
}
