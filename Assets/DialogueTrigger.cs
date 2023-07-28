using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public int plotID;
    private bool finished;
    private void OnTriggerStay(Collider other)
    {
        if (LinesManager.isPlayingLines) return;
        if (other.tag == "Player")
        {
            if (!finished)
            {
                LinesManager.Instance.DisplayLine(plotID, 0);
                finished = true;
            }

        }
    }
}
