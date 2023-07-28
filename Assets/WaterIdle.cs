using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class WaterIdle : StateMachine<WaterMonsterBehaviour>
    {
        public float minimumIdleTime = 2.0f;
        public float maximumIdleTime = 5.0f;

        protected float remainingToNextIdle = 0.0f;

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if (minimumIdleTime > maximumIdleTime)
                minimumIdleTime = maximumIdleTime;

            remainingToNextIdle = Random.Range(minimumIdleTime, maximumIdleTime);
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

            remainingToNextIdle -= Time.deltaTime;

            if (remainingToNextIdle < 0)
            {
                remainingToNextIdle = Random.Range(minimumIdleTime, maximumIdleTime);
            }

            m_MonoBehaviour.FindTarget();

            if (m_MonoBehaviour.player != null)
            {
                m_MonoBehaviour.StartPursuit();
            }
        }
    }
}