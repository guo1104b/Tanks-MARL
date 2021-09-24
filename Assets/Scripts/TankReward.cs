using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankReward : MonoBehaviour
{
    public TankAgent greenTank; //Tank1
    public TankAgent redTank;   //Tank2
    public Shell shell;
    private TankHealth tankHealth1;
    private TankHealth tankHealth2;
    private int resetTimer;

    public float KillReward = 1;
    public float ShootReward = 1f;
    public float DeathPenalty = -1;
    // Start is called before the first frame update
    void Start()
    {
        tankHealth1 = greenTank.GetComponent<TankHealth>();
        tankHealth2 = redTank.GetComponent<TankHealth>();
        resetTimer = 0;
    }
    private void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= greenTank.MaxStep)
        {
            //Debug.Log("Green tank reward reset" + greenTank.GetCumulativeReward());
            //Debug.Log("Red tank reward reset" + redTank.GetCumulativeReward()); 
            Reset();

        }
    }
    public void Reward(Team team)
    {

        if (team == Team.Green)
        {
            greenTank.AddReward(-ShootReward);
            redTank.AddReward(ShootReward);
            if (tankHealth1.ZeroHp())
            {
                //Debug.Log("Green Tank is dead!");
                greenTank.AddReward(DeathPenalty + (float)resetTimer / greenTank.MaxStep);
                redTank.AddReward(KillReward - (float)resetTimer / greenTank.MaxStep);
                //Debug.Log("Green tank reward done:" + greenTank.GetCumulativeReward());
                //Debug.Log("Red tank reward done:" + redTank.GetCumulativeReward());
                Reset();
            }
        }
        else
        {
            greenTank.AddReward(ShootReward);
            redTank.AddReward(-ShootReward);
            if (tankHealth2.ZeroHp())
            {
                //Debug.Log("Green Tank is dead!");
                greenTank.AddReward(KillReward - (float)resetTimer / greenTank.MaxStep);
                redTank.AddReward(DeathPenalty + (float)resetTimer / greenTank.MaxStep);
                //Debug.Log("Green tank reward" + greenTank.GetCumulativeReward());
                //Debug.Log("Red tank reward" + redTank.GetCumulativeReward());
                Reset();
            }
        }
    }

    void Reset()
    {
        greenTank.EndEpisode();
        redTank.EndEpisode();
        tankHealth1.ResetHp();
        tankHealth2.ResetHp();
        resetTimer = 0;
    }
}
