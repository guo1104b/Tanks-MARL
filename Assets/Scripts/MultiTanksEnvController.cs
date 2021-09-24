using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class MultiTanksEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public MultiTankAgent2 Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public TankHealth tankHealth;
    }

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 20000;

    public Shell shell;
    private int resetTimer;

    private SimpleMultiAgentGroup m_GreenAgentGroup;
    private SimpleMultiAgentGroup m_RedAgentGroup;

    public float KillReward = 1f;
    public float ShootReward = 0.5f;
    public float DeathPenalty = -1f;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    // Start is called before the first frame update
    void Start()
    {
        resetTimer = 0;
        // Initialize TeamManager
        m_GreenAgentGroup = new SimpleMultiAgentGroup();
        m_RedAgentGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            item.tankHealth = item.Agent.GetComponent<TankHealth>(); 
            if (item.Agent.team == Team.Green)
            {
                m_GreenAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_RedAgentGroup.RegisterAgent(item.Agent);
            }
        }
    }
    private void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps)
        {
            m_GreenAgentGroup.GroupEpisodeInterrupted();
            m_RedAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
        //Debug.Log("m_RedAgentGroup:" + m_RedAgentGroup.GetRegisteredAgents().Count);
        //Debug.Log("m_GreenAgentGroup:" + m_GreenAgentGroup.GetRegisteredAgents().Count);
        //Debug.Log("m_GreenAgentGroup.GetId():" + m_GreenAgentGroup.GetId());
        //Debug.Log("m_RedAgentGroup.GetId():" + m_RedAgentGroup.GetId());
    }
    public void Reward(Team team)
    {

        if (team == Team.Green)
        {
            m_RedAgentGroup.AddGroupReward(ShootReward);
            m_GreenAgentGroup.AddGroupReward(-ShootReward);
        }
        else
        {
            m_RedAgentGroup.AddGroupReward(-ShootReward);
            m_GreenAgentGroup.AddGroupReward(ShootReward);
        }
    }
    public void Killed(Team team)
    {
        if (team == Team.Green)
        {
            m_RedAgentGroup.AddGroupReward(KillReward);
            m_GreenAgentGroup.AddGroupReward(DeathPenalty);
            if (m_GreenAgentGroup.GetRegisteredAgents().Count == 0)
            {
                m_RedAgentGroup.AddGroupReward(KillReward - (float)resetTimer / MaxEnvironmentSteps);
                m_GreenAgentGroup.AddGroupReward(DeathPenalty + (float)resetTimer / MaxEnvironmentSteps);
                m_GreenAgentGroup.EndGroupEpisode();
                m_RedAgentGroup.EndGroupEpisode();
                ResetScene();
            }  
        }
        else
        {
            m_RedAgentGroup.AddGroupReward(DeathPenalty);
            m_GreenAgentGroup.AddGroupReward(KillReward);
            if (m_RedAgentGroup.GetRegisteredAgents().Count == 0)
            {
                m_RedAgentGroup.AddGroupReward(DeathPenalty + (float)resetTimer / MaxEnvironmentSteps);
                m_GreenAgentGroup.AddGroupReward(KillReward - (float)resetTimer / MaxEnvironmentSteps);
                m_GreenAgentGroup.EndGroupEpisode();
                m_RedAgentGroup.EndGroupEpisode();
                ResetScene();
            }
           
        }
    }

    void ResetScene()
    {
        resetTimer = 0;
        foreach (var item in AgentsList)
        {
            //var randomPosZ = Random.Range(-20f, 20f);
            //var newStartPos = item.StartingPos + new Vector3(0f, 0f, randomPosZ);
            //item.Agent.transform.SetPositionAndRotation(item.StartingPos, item.StartingRot);
            //item.Agent.transform.SetPositionAndRotation(newStartPos, item.StartingRot);
            item.Agent.transform.SetPositionAndRotation(item.StartingPos, item.StartingRot); 
            item.Rb.velocity = new Vector3(0, 0, 0);
            item.Rb.angularVelocity = new Vector3(0, 0, 0);
            item.tankHealth.ResetHp();
            item.Agent.ResetTurret();
            item.Agent.gameObject.SetActive(true);
            if (item.Agent.team == Team.Green)
            {
                m_GreenAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_RedAgentGroup.RegisterAgent(item.Agent);
            }
        }
    }
}


