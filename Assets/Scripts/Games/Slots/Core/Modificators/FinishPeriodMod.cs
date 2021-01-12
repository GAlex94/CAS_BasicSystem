using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class FinishPeriod : SpinModificator
    {
        private FinishPeriodConfig selfConfig;
        private bool needChangeReward = false;
        private bool isDecreaseBank = false;

        protected override void BaseInit(SpinModificatorConfig config)
        {
            selfConfig = (FinishPeriodConfig)config;
        }

        public override bool NeedToActivate(SpinInfo spinInfo)
        {
            PeriodData periodData = DataManager.Instance.SlotsData.periodData;
            int leftSpins = DataManager.Instance.SlotsData.lastSpinId - periodData.startSpinId;
            if (leftSpins > SlotsGame.Instance.SlotsConfig.SpinsCountOnPeriod)
            {
                return true;
            }
            return false;
        }

        public override bool NeedToDeactivate(SpinInfo spinInfo)
        {
            PeriodData periodData = DataManager.Instance.SlotsData.periodData;
            int leftSpins = DataManager.Instance.SlotsData.lastSpinId - periodData.startSpinId;
            if (leftSpins < SlotsGame.Instance.SlotsConfig.SpinsCountOnPeriod)
            {
                return true;
            }
            return false;
        }

        public override void Activate()
        {
            DataManager.Instance.StartDecreaseBankForFinishPeriod();
        }

        public override SpinStatus ProcessSpinStatus(SpinInfo spinInfo)
        {
            PeriodData periodData = DataManager.Instance.SlotsData.periodData;
            int needBank = (int)(periodData.playerBankOnStart * (periodData.curRTP / 100.0f));

            int delta = DataManager.Instance.Money - needBank;

            isDecreaseBank = false;
            needChangeReward = false;

            if (delta < 0)
            {
                needChangeReward = true;

                float rewardPercent = Mathf.Abs(delta) / (float)spinInfo.bet;
                if ((rewardPercent * 100) > SlotsGame.Instance.SlotMachineConfig.rewardLimits.MaxLuckyReward)
                    return SpinStatus.Big;
                else
                    return SpinStatus.Lucky;
            }
            else
            {
                int spinsLeft = spinInfo.id - DataManager.Instance.SlotsData.finishPeriodModData.startSpinIdByDescreaseBank;

                if (spinsLeft >= selfConfig.MaxSpinsToDecreaseBank)
                {
                    DataManager.Instance.FinishPeriod(true, false);
                    return spinInfo.baseStatus;
                }

                isDecreaseBank = true;

                if (spinInfo.baseStatus != SpinStatus.Lose)
                {
                    float minFalseReward = SlotsGame.Instance.SlotMachineConfig.rewardLimits.MinFalseReward;
                    if (minFalseReward < 99.99f)
                    {
                        return SpinStatus.False;
                    }
                    else
                    {
                        needChangeReward = true;
                        return SpinStatus.Lucky;
                    }
                }
                else
                    return spinInfo.baseStatus;
            }
        }

        public override float ProcessReward(SpinInfo spinInfo)
        {
            if (!needChangeReward)
                return spinInfo.finalRewardPercent;

            if (isDecreaseBank)
            {
                if (spinInfo.finalStatus == SpinStatus.Lucky)
                    return Mathf.Max(1.0f, SlotsGame.Instance.SlotMachineConfig.rewardLimits.MinFalseReward / 100.0f);
            }
            else
            {
                PeriodData periodData = DataManager.Instance.SlotsData.periodData;
                int needBank = (int)(periodData.playerBankOnStart * (periodData.curRTP / 100.0f));

                int delta = DataManager.Instance.Money - needBank;
                float rewardPercent = Mathf.Abs(delta) / (float)spinInfo.bet;
                return rewardPercent;
            }
            return spinInfo.finalRewardPercent;
        }
    }
}
