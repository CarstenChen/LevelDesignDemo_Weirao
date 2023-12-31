using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class WaterHit : StateMachine<WaterMonsterBehaviour>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.ResetTrigger("Attack");
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.monsterController.ClearForce();
        }
    }
}
