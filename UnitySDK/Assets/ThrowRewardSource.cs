using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowRewardSource : RewardSource
{

    [SerializeField]
    Transform projectile;

    [SerializeField]
    Target target;

    [SerializeField]
    float rewardFallOff=-4f;

    [SerializeField]
    float rewardOffset = 0f;

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
            var d = (projectile.position - target.transform.position).magnitude - rewardOffset;
            if (d < 0) d = 0f;
            return Mathf.Exp(rewardFallOff * d * d);
        }
    }
}
