using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public enum SpinModificatorType
    {
        None,
        FinishPeriod,
        UserPay,
        UserAbsent,
        BankNearZero,
        MakeMaxBet,
    }

    public class SpinModificator
    {
        protected SpinModificatorConfig baseModConfig;

        public SpinModificatorType ModType => baseModConfig.ModType;
        public bool Enabled => baseModConfig.Enabled;
        public bool MayRestart => baseModConfig.MayRestart;

        public void Init(SpinModificatorConfig config)
        {
            baseModConfig = config;
            BaseInit(config);
        }

        protected virtual void BaseInit(SpinModificatorConfig config)
        {

        }

        public virtual bool NeedToActivate(SpinInfo spinInfo)
        {
            return false;
        }

        public virtual bool NeedToDeactivate(SpinInfo spinInfo)
        {
            return false;
        }

        public virtual void Activate()
        {

        }

        public virtual SpinStatus ProcessSpinStatus(SpinInfo spinInfo)
        {
            return SpinStatus.None;
        }

        public virtual float ProcessReward(SpinInfo spinInfo)
        {
            return 0.0f;
        }
    }
}
