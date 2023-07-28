using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class WoodSpotted : StateMachine<WoodMonsterBehaviour>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.Spotted();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.FindTarget();

            if (m_MonoBehaviour.player == null || m_MonoBehaviour.player.isRespawning)
            {
                m_MonoBehaviour.StopPursuit();
                return;
            }

            Vector3 toTarget = m_MonoBehaviour.player.transform.position - m_MonoBehaviour.transform.position;

            float onUp = Vector3.Dot(toTarget, m_MonoBehaviour.transform.up);
            toTarget -= m_MonoBehaviour.transform.up * onUp;

            toTarget.Normalize();

            m_MonoBehaviour.transform.forward =
                Vector3.Lerp(m_MonoBehaviour.transform.forward, toTarget, stateInfo.normalizedTime);
        }
    }
}