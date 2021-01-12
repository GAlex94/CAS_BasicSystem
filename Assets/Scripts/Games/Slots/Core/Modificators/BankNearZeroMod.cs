using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class BankNearZeroMod : SpinModificator
    {
        private BankNearZeroConfig selfConfig;

        protected override void BaseInit(SpinModificatorConfig config)
        {
            selfConfig = (BankNearZeroConfig) config;
        }

        public override bool NeedToActivate(SpinInfo spinInfo)
        {
            var needActivate = !DataManager.Instance.CheckForSpend(spinInfo.bet * 2);
            if (needActivate)
            {
                DataManager.Instance.SlotsData.bankNearZeroModData.countNeedUse++;
                needActivate = CheckSpin();
            }

            return needActivate;
        }

        public bool CheckSpin()
        {         
            var numberUse = DataManager.Instance.SlotsData.bankNearZeroModData.countNeedUse;

            foreach (var nubmerSpin in selfConfig.NubmerSpinActive)
            {
                if (nubmerSpin == numberUse)
                {
                    return true;
                }
            }

            return numberUse >= selfConfig.spinMoreActive && numberUse % selfConfig.spinMoreActive == 0;
        }

        public override bool NeedToDeactivate(SpinInfo spinInfo)
        {
            return CheckSpin();
        }

        public override void Activate()
        {
           // DataManager.Instance.SlotsData.bankNearZeroModData.countNeedUse++;
        }

        public override SpinStatus ProcessSpinStatus(SpinInfo spinInfo)
        {
            return SpinStatus.Lucky;
        }

        public override float ProcessReward(SpinInfo spinInfo)
        {
            return selfConfig.Reward;
        }
    }
}
