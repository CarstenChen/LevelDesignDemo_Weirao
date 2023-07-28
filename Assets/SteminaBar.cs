using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteminaBar : MonoBehaviour
{
    public PlayerController player;
    public GameObject steminaBar;
    public Image fillImg;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (player.isCostingStemina || player.isRecoveringStemina)
        {
            steminaBar.SetActive(true);
        }
        else
        {
            steminaBar.SetActive(false);
        }

        if (player.GetCurrentSteminaRate() < 0.3f) { fillImg.color = Color.red; } else
        {
            fillImg.color = Color.green;
        }

        fillImg.fillAmount = player.GetCurrentSteminaRate();
    }
}
