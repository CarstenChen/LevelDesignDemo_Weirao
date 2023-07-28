using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D;

public class WoodFrame : StateMachine<WoodMonsterBehaviour>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_MonoBehaviour.player == null)
        {
            //if we reached the shooting state without a target, mean the target move outside of our detection range
            //so just go back to idle.
            animator.Play("Idle");
            return;
        }

        m_MonoBehaviour.monsterController.SetFollowAgent(false);

        //m_MonoBehaviour.RememberTargetPosition();
        Vector3 toTarget = m_MonoBehaviour.player.transform.position - m_MonoBehaviour.transform.position;
        toTarget.y = 0;

        m_MonoBehaviour.transform.forward = toTarget.normalized;
        m_MonoBehaviour.monsterController.SetForward(m_MonoBehaviour.transform.forward);
    }
}