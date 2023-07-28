using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D;
using UnityEngine.AI;

public class WaterMove : StateMachine<WaterMonsterBehaviour>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

        m_MonoBehaviour.FindTarget();

        //if (m_MonoBehaviour.monsterController.Agent.pathStatus == NavMeshPathStatus.PathPartial
        //    || m_MonoBehaviour.monsterController.Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        //{
        //    m_MonoBehaviour.StopPursuit();
        //    return;
        //}

        if (m_MonoBehaviour.player == null || m_MonoBehaviour.player.isRespawning)
        {//if the target was lost or is respawning, we stop the pursit
            m_MonoBehaviour.StopPursuit();
        }
        else
        {
            m_MonoBehaviour.RequestTargetPosition();

            Vector3 toTarget = m_MonoBehaviour.player.transform.position - m_MonoBehaviour.transform.position;

            if (toTarget.sqrMagnitude < m_MonoBehaviour.attackDistance * m_MonoBehaviour.attackDistance)
            {
                m_MonoBehaviour.TriggerAttack();
            }
            else if (m_MonoBehaviour.FollowerData.assignedSlot != -1)
            {
                Vector3 targetPoint = m_MonoBehaviour.player.transform.position +
                    m_MonoBehaviour.FollowerData.distributor.GetDirection(m_MonoBehaviour.FollowerData
                        .assignedSlot) * m_MonoBehaviour.attackDistance * 0.9f;

                m_MonoBehaviour.monsterController.SetTarget(targetPoint);
            }
            else
            {
                m_MonoBehaviour.StopPursuit();
            }
        }
    }
}
