using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
//using Unity.MLAgents.Policies;

//随机初始位置 + 三种射程
//mlagents-learn E:\Config\Tank-poca.yaml --env=E:\UnityExecutable\MultiTanks\MultiTanks --run-id=MultiTanks1 --num-envs=4 --results-dir RESULTS_DIR
//结果在C:\Users\WG\results 
//tensorboard --logdir results --port 6006
//public enum Team
//{
//    Red = 2, Green = 1
//}
public class MultiTankAgent2 : Agent
{
    Rigidbody rBody1;
    Rigidbody rBody2;
    public GameObject Turret;
    //public GameObject target;
    public KeyCode fireKey = KeyCode.Space;
    public float speed = 500;
    public float angularSpeed = 100;
    public float turretSpeed = 30;
    //BehaviorParameters m_BehaviorParameters;
    //public float KillReward = 1f;
    public float ShootReward = 0.5f;
    //public float DeathPenalty = -1f;
    public bool useVectorObs;

    //public GameObject Target;
    public GameObject shellPrefab;
    //public MultiTankReward tankReward;
    private Transform firePosition;
    private Vector3 turretPosition;
    private TankHealth tankHealth;
    private MultiTanksEnvController tankEnvController;
    private bool coolingDown = true;
    //private bool takeAim;
    private bool reach;
    private float cooldownTimer = 0f;
    private const float cooldownTime = 0.5f;
    private float count;
    //private float tmp;

    //[HideInInspector]
    public Team team;

    public override void Initialize()
    {
        rBody1 = this.GetComponent<Rigidbody>();
        rBody2 = Turret.GetComponent<Rigidbody>();
        firePosition = transform.Find("TankRenderers/TankTurret/FirePosition");
        turretPosition = Turret.transform.localPosition;
        tankHealth = this.GetComponent<TankHealth>();
        tankEnvController = GetComponentInParent<MultiTanksEnvController>();
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
        count = 300;
        reach = false;
        //tmp = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            // Agent positions
            sensor.AddObservation(this.transform.localPosition.x);
            sensor.AddObservation(this.transform.localPosition.z);
            sensor.AddObservation(this.transform.localEulerAngles.y);
            sensor.AddObservation(Turret.transform.localEulerAngles.y);
        }
        float hp = tankHealth.Hp();
        sensor.AddObservation(hp);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        //if (coolingDown || count == 0 || takeAim)
        if (coolingDown || count == 0)
        {
            //Format of action mask is: actionMask.SetActionEnabled(branch, actionIndex, isEnabled);
            actionMask.SetActionEnabled(3, 1, false);
            //actionMask.SetActionEnabled(3, 2, false);
            //actionMask.SetActionEnabled(3, 3, false);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, new Vector3(0f, 0f, 0f));
        //if (distanceToTarget < 30)
        //{
        //    //AddReward((float)System.Math.Exp(-distanceToTarget * 1 / 2) / 10);
        //    AddReward(1f);
        //}
        //if (distanceToTarget >= tmp)
        //{
        //    AddReward(-0.001f);
        //}
        //tmp = distanceToTarget;
        //Debug.Log(team+"reward:" + GetCumulativeReward());
        //string hebing = new string(gameObject.transform.name)[0];
        //Debug.Log("Name+tag:" + gameObject.ToString()[0]+); //char
        if (distanceToTarget < 30 && !reach)
        {
            AddReward(0.5f);
            reach = true;
        }

        // Actions, branch size = 4
        var forwardAxis = actionBuffers.DiscreteActions[0]; //0,1,2
        var rotateAxis = actionBuffers.DiscreteActions[1];
        var turretAxis = actionBuffers.DiscreteActions[2];
        var attackAxis = actionBuffers.DiscreteActions[3]; //0,1,2,3

        //if (forwardAxis == 1 && turretAxis == 1) { takeAim = false; }
        //else { takeAim = true; }

        rBody1.velocity = transform.forward * (forwardAxis - 1) * speed * Time.deltaTime;
        rBody1.MoveRotation(rBody1.rotation * Quaternion.Euler(transform.up * Time.deltaTime * angularSpeed * (rotateAxis - 1)));
        rBody2.MoveRotation(rBody2.rotation * Quaternion.Euler(transform.up * Time.deltaTime * turretSpeed * (turretAxis - 1)));


        switch (attackAxis)
        {
            case 1:
                Fire(70f);
                break;
            case 0:
                break;
        }
    }

    //如果Is Trigger勾选一定要用OnTriggerEnter方法，没有勾选一定用OnCollisionEnter，否则不会生效
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Shell")
        {
            tankHealth.TakeDamage();
            tankEnvController.Reward(team);
            //AddReward(-0.5f);
            if (tankHealth.ZeroHp()) 
            {
                gameObject.SetActive(false);
                tankEnvController.Killed(team); 
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Env"))
        {
            AddReward(-0.5f);
            rBody1.freezeRotation = true;
            rBody1.velocity = new Vector3(0, 0, 0);
            rBody1.angularVelocity = new Vector3(0, 0, 0);
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
        discreteActionsOut[0] = (int)Input.GetAxisRaw("VerticalPlayer" + (int)team) + 1;
        discreteActionsOut[1] = (int)Input.GetAxisRaw("HorizontalPlayer" + (int)team) + 1;
        discreteActionsOut[2] = (int)Input.GetAxisRaw("TurretPlayer" + (int)team) + 1;
        discreteActionsOut[3] = Input.GetKey(fireKey) ? 1 : 0;
        //if (team == Team.Green)
        //{
        //    if (Input.GetKey(KeyCode.T))
        //    {
        //        discreteActionsOut[3] = 1;
        //    }
        //    else if (Input.GetKey(KeyCode.Y))
        //    {
        //        discreteActionsOut[3] = 3;
        //    }
        //    else if (Input.GetKey(KeyCode.Space))
        //    {
        //        discreteActionsOut[3] = 2;
        //    }

        //}
        //else
        //{
        //    if (Input.GetKey(KeyCode.K))
        //    {
        //        discreteActionsOut[3] = 1;
        //    }
        //    else if (Input.GetKey(KeyCode.L))
        //    {
        //        discreteActionsOut[3] = 3;
        //    }
        //    else if (Input.GetKey(KeyCode.KeypadEnter))
        //    {
        //        discreteActionsOut[3] = 2;
        //    }

        //}
    }
    private void Fire(float shellSpeed)
    {
        if (coolingDown) return;

        coolingDown = true;
        cooldownTimer = 0f;
        count -= 1;
        if (count == 0) { AddReward(0.2f); }
        GameObject go = GameObject.Instantiate(shellPrefab, firePosition.position, firePosition.rotation) as GameObject;
        go.GetComponent<Rigidbody>().velocity = go.transform.forward * shellSpeed;

    }
    public void ResetTurret()
    {
        Turret.transform.localEulerAngles = new Vector3(0, 0, 0);
        Turret.transform.localPosition = turretPosition;
    }
}


