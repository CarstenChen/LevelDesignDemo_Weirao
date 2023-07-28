using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTrail : StateMachineBehaviour
{
    public int effectIndex;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.GetComponent<PlayerController>();
        playerController.meleeWeapon.trailEffects[effectIndex].Activate();
    }
}
