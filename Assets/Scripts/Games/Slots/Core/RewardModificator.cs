using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class RewardModificator
    {
        private SlotMachineConfig originalConfig;

        private float luckyFactor = 1.0f;
        private float bigWinFactor = 1.0f;

        public float LuckyFactor => luckyFactor;
        public float BigWinFactor => bigWinFactor;

        public RewardModificator(SlotMachineConfig originalConfig)
        {
            this.originalConfig = originalConfig;
        }

        public void UpdateRewardFactors(int bankRemain, int spinsRemain, int currentBet)
        {
            if (spinsRemain == 0)
            {
                luckyFactor = 1.0f;
                bigWinFactor = 1.0f;
                return;
            }
            //float rtpBets = startMoney * (rtp - 1.0f) / defaultBet;
            float rtpBets = (float)bankRemain / currentBet;
            float defaultReward = CalculateReward(originalConfig.spinChances, originalConfig.rewardLimits) * (spinsRemain / 100.0f);
            float rewardBoost = (rtpBets + defaultReward) / defaultReward;

            luckyFactor = (rewardBoost * (originalConfig.rewardLimits.MaxLuckyReward - 100.0f) + 100.0f) /
                          originalConfig.rewardLimits.MaxLuckyReward;

            float winRewardSum = originalConfig.rewardLimits.MaxBigReward +
                                 originalConfig.rewardLimits.MinBigReward;
            bigWinFactor = (rewardBoost * (winRewardSum * 0.5f - 100.0f) + 100.0f) / (winRewardSum * 0.5f);
        }

        public static float CalculateReward(SpinChances spinChances, RewardLimits rewardLimits)
        {
            return spinChances.LuckyChancePercent * (rewardLimits.MaxLuckyReward - 100.0f) * 0.5f / 100.0f +
                         spinChances.BigWinChancePercent * ((rewardLimits.MaxBigReward + rewardLimits.MinBigReward) * 0.5f - 100.0f) / 100.0f;
        }

        public static float CalculateLose(SpinChances spinChances, RewardLimits rewardLimits)
        {
            return spinChances.LoseChancePercent + spinChances.FalseChancePercent * (100.0f - rewardLimits.MinFalseReward) * 0.5f / 100.0f;
        }

    }
}
