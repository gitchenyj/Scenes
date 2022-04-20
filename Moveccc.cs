using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Moveccc : MonoBehaviour
{
    Transform m_transform;
    //终点位置
    Transform m_target;
    //寻路组件
    NavMeshAgent m_agent;
    //移动速度
    float m_speed = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = this.transform;
        m_target = GameObject.Find("Cylinderdes").GetComponent<Transform>();

        //获得寻路组件
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.speed = m_speed;
        m_agent.SetDestination(m_target.position);
    }
}


