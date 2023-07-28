using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class WoodAttack : StateMachine<WoodMonsterBehaviour>
    {
        protected Vector3 m_AttackPosition;

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateEnter(animator, stateInfo, layerIndex);

            m_MonoBehaviour.monsterController.SetFollowAgent(false);

            if (m_MonoBehaviour.player != null)
            {
                m_AttackPosition = m_MonoBehaviour.player.transform.position;
                Vector3 toTarget = m_AttackPosition - m_MonoBehaviour.transform.position;
                toTarget.y = 0;

                m_MonoBehaviour.transform.forward = toTarget.normalized;
                m_MonoBehaviour.monsterController.SetForward(m_MonoBehaviour.transform.forward);
            }


        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateExit(animator, stateInfo, layerIndex);

            m_MonoBehaviour.monsterController.SetFollowAgent(true);
        }
    }
}