using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class AbsentUserMod : SpinModificator
    {
        private AbsentUserConfig selfConfig;

        protected override void BaseInit(SpinModificatorConfig config)
        {
            selfConfig = (AbsentUserConfig)config;
        }

        public override bool NeedToActivate(SpinInfo spinInfo)
        {
            return DataManager.Instance.SlotsData.userAbsent;
        }

        public override bool NeedToDeactivate(SpinInfo spinInfo)
        {
            int startSpinId = DataManager.Instance.SlotsData.absentUserModData.startSpinId;
            int currentSpinId = DataManager.Instance.SlotsData.lastSpinId;
            return (currentSpinId - startSpinId) >= selfConfig.MaxWinSpins;
        }

        public override void Activate()
        {
           DataManager.Instance.StartAbsentUserPeriod();
        }

        public override SpinStatus ProcessSpinStatus(SpinInfo spinInfo)
        {
            int spinIndex = DataManager.Instance.SlotsData.lastSpinId -
                            DataManager.Instance.SlotsData.absentUserModData.startSpinId;

            if (spinIndex >= (selfConfig.MaxWinSpins - 1))
            {
                DataManager.Instance.FinishPeriod(false);
            }

            return spinInfo.baseStatus == SpinStatus.Lose ? SpinStatus.Lucky : spinInfo.baseStatus;
        }

        public override float ProcessReward(SpinInfo spinInfo)
        {
            return spinInfo.finalRewardPercent;
        }
    }
}
