using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Moveccc : MonoBehaviour
{
    Transform m_transform;
    //�յ�λ��
    Transform m_target;
    //Ѱ·���
    NavMeshAgent m_agent;
    //�ƶ��ٶ�
    float m_speed = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = this.transform;
        m_target = GameObject.Find("Cylinderdes").GetComponent<Transform>();

        //���Ѱ·���
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = m_speed;
        m_agent.SetDestination(m_target.position);
    }
}


