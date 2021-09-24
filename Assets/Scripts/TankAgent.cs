using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
//using Unity.MLAgents.Policies;

//mlagents-learn E:\Config\Tank.yaml --env=E:\UnityExecutable\TwoTanks\TwoTanks --run-id=Tank1 --force
//结果在C:\Users\WG\results 
//tensorboard --logdir results --port 6006
public enum Team
{
    Red = 2, Green = 1
}
public class TankAgent : Agent
{
    Rigidbody rBody1;
    Rigidbody rBody2;
    public GameObject Turret;
    //public GameObject target;
    public KeyCode fireKey = KeyCode.Space;
    public float speed = 500;
    public float angularSpeed = 100;
    public float turretSpeed = 30;
    public float shellSpeed = 70;

    public GameObject shellPrefab;
    private Vector3 turretPosition;
    private Vector3 StartingPos;
    private Quaternion StartingRot;
    private Transform firePosition;
    private TankHealth tankHealth;
    private TankReward tankReward;
    private bool coolingDown = true;
    private float cooldownTimer = 0f;
    //private int resetTimer;
    private const float cooldownTime = 0.5f;
    private float count;

    //[HideInInspector]
    public Team team;

    void Start()
    {
        rBody1 = this.GetComponent<Rigidbody>();
        rBody2 = Turret.GetComponent<Rigidbody>();
        firePosition = transform.Find("TankRenderers/TankTurret/FirePosition");
        tankHealth = this.GetComponent<TankHealth>();
        tankReward = this.GetComponentInParent<TankReward>();
        StartingPos = gameObject.transform.position;
        StartingRot = gameObject.transform.rotation;
        turretPosition = Turret.transform.localPosition;
    }
    void FixedUpdate()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= cooldownTime)
        {
            cooldownTimer = 0f;
            coolingDown = false;
        }
    }
    public override void OnEpisodeBegin()
    {
        count = 180;
        // 速度和位置重置 
        rBody1.velocity = new Vector3(0, 0, 0);
        //rBody1.angularVelocity = new Vector3(0, 0, 0);
        //rBody2.angularVelocity = new Vector3(0, 0, 0);
        Turret.transform.localEulerAngles = new Vector3(0, 0, 0);
        Turret.transform.localPosition = turretPosition;

        gameObject.transform.SetPositionAndRotation(StartingPos, StartingRot);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float hp = tankHealth.Hp();
        sensor.AddObservation(hp);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (coolingDown || count == 0)
        {
            //Format of action mask is: actionMask.SetActionEnabled(branch, actionIndex, isEnabled);
            actionMask.SetActionEnabled(3, 1, false);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, branch size = 4
        var forwardAxis = actionBuffers.DiscreteActions[0] ; //0,1,2
        var rotateAxis  = actionBuffers.DiscreteActions[1] ;
        var turretAxis  = actionBuffers.DiscreteActions[2];
        var attackAxis  = actionBuffers.DiscreteActions[3] ; //0,1
        rBody1.velocity = transform.forward * (forwardAxis-1) * speed * Time.deltaTime;
        
        rBody1.MoveRotation(rBody1.rotation * Quaternion.Euler(transform.up * Time.deltaTime * angularSpeed * (rotateAxis - 1)));
        rBody2.MoveRotation(rBody2.rotation * Quaternion.Euler(transform.up * Time.deltaTime * turretSpeed  * (turretAxis - 1)));
        //Debug.Log(team + "reward:" + GetCumulativeReward());
        if (attackAxis == 1) { Fire(); }
    }

    //如果Is Trigger勾选一定要用OnTriggerEnter方法，没有勾选一定用OnCollisionEnter，否则不会生效
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Shell")
        {
            tankHealth.TakeDamage();
            tankReward.Reward(team);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Env"))
        {
            AddReward(-0.5f);
            rBody1.velocity = new Vector3(0, 0, 0);
            rBody1.angularVelocity = new Vector3(0, 0, 0);
            rBody2.angularVelocity = new Vector3(0, 0, 0);
        }

        if (collision.gameObject.CompareTag("Tank1") || collision.gameObject.CompareTag("Tank2"))
        {
            AddReward(-0.5f);
            rBody1.freezeRotation = true;
            rBody1.velocity = new Vector3(0, 0, 0);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = (int)Input.GetAxisRaw("VerticalPlayer" + (int)team)+1; 
        discreteActionsOut[1] = (int)Input.GetAxisRaw("HorizontalPlayer" + (int)team) +1;
        discreteActionsOut[2] = (int)Input.GetAxisRaw("TurretPlayer" + (int)team) +1;
        discreteActionsOut[3] = Input.GetKey(fireKey) ? 1 : 0;
        
    }
    private void Fire()
    {
        if (coolingDown) return;

        coolingDown = true;
        cooldownTimer = 0f;
        count -= 1;
        if (count == 0) { AddReward(0.2f); }
        GameObject go = GameObject.Instantiate(shellPrefab, firePosition.position, firePosition.rotation) as GameObject;
        go.GetComponent<Rigidbody>().velocity = go.transform.forward * shellSpeed;  
    }
    
    public void GetCloseReward(float dis)
    {
        AddReward((float)System.Math.Exp(-dis * 1 / 2) / 10);
        Debug.Log(team + "reward :" + GetCumulativeReward());
    }
}

