using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowRewardSource : RewardSource
{

    [SerializeField]
    Transform projectile;

    [SerializeField]
    Target target;

    public override float Reward => CalculateReward();

    public override void OnAgentInitialize()
    {

    }

    // Start is called before the first frame update
    float CalculateReward()
    {
        if (target.h) return 1f;
        else 
        {
            return Mathf.Exp(-4 * (projectile.position - target.transform.position).magnitude * (projectile.position - target.transform.position).magnitude);
        }
    }
}
