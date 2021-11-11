using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class RewardSignal : MonoBehaviour
{
    [SerializeField]
    private List<WeightedRewardSource> weightedRewards;

    [SerializeField]
    [DefaultValue(RewardMixing.MixType.Linear)]
    private RewardMixing.MixType MixingMethod;

    public float Reward { get => weightedRewards.MixRewards(MixingMethod); }

    public void OnAgentInitialize()
    {
        foreach (var wr in weightedRewards)
        {
            wr.OnAgentInitialize();
        }
    }
}

public static class RewardMixing
{
    public enum MixType
    {
        Linear,
        Unweighted
    }

    /// <summary>Weighted sum of a collection reward sources</summary>
    public static float LinearMix(this IEnumerable<WeightedRewardSource> rewardList)
    {
        return rewardList.Select(x => x.Weight * x.Reward).Sum();
    }

    /// <summary>Sum of a collection reward sources, ignoring the weight field in the WeightedRewardSource</summary>
    public static float UnweightedMix(this IEnumerable<WeightedRewardSource> rewardList)
    {
        return rewardList.Select(x => x.Reward).Sum();
    }

    ///<summary>Mix rewards with method selected by enum</summary>
    public static float MixRewards(this IEnumerable<WeightedRewardSource> rewardsToMix, RewardMixing.MixType mixType)
    {
        switch (mixType)
        {
            case MixType.Linear:
                return rewardsToMix.LinearMix();

            case MixType.Unweighted:
                return rewardsToMix.UnweightedMix();

            default:
                return rewardsToMix.LinearMix();
        }
    }
}
