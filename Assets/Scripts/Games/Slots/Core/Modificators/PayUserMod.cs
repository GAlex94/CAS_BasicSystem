using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class PayUserMod : SpinModificator
    {
        private PayUserConfig selfConfig;

        protected override void BaseInit(SpinModificatorConfig config)
        {
            selfConfig = (PayUserConfig)config;
        }

        public override bool NeedToActivate(SpinInfo spinInfo)
        {
            return DataManager.Instance.SlotsData.userPaid;
        }

        public override bool NeedToDeactivate(SpinInfo spinInfo)
        {
            int startSpinId = DataManager.Instance.SlotsData.payUserModData.startSpinId;
            int currentSpinId = DataManager.Instance.SlotsData.lastSpinId;
            return (currentSpinId - startSpinId) >= selfConfig.MaxWinSpins;
        }

        public override void Activate()
        {
            int bigWinIndex = UnityEngine.Random.Range(0, selfConfig.MaxWinSpins);
            DataManager.Instance.StartPayUserPeriod(bigWinIndex);
        }

        public override SpinStatus ProcessSpinStatus(SpinInfo spinInfo)
        {
            int spinIndex = DataManager.Instance.SlotsData.lastSpinId -
                            DataManager.Instance.SlotsData.payUserModData.startSpinId;

            if (spinIndex >= (selfConfig.MaxWinSpins - 1))
            {
                DataManager.Instance.FinishPeriod(false);
            }

            if (spinIndex == DataManager.Instance.SlotsData.payUserModData.bigWinSpinIndex)
                return SpinStatus.Big;
            else if (spinInfo.baseStatus == SpinStatus.Lose)
                return SpinStatus.Lucky;

            return spinInfo.baseStatus;
        }

        public override float ProcessReward(SpinInfo spinInfo)
        {
            return spinInfo.finalRewardPercent;
        }
    }
}
