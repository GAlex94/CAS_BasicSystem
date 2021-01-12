using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [CreateAssetMenu(fileName = "FinishPeriodConfig", menuName = "Data/Slots/Modificators/FinishPeriodConfig")]
    public class FinishPeriodConfig : SpinModificatorConfig
    {
        [Header("Main finish period params")]
        public float MaxBankThreshold = 10.0f;

        [Header("Decrease period params")]
        public int MaxSpinsToDecreaseBank = 9;
        public float MaxLuckyRewardByDecrease = 105.0f;

        public override SpinModificator CreateMod()
        {
            SpinModificator newMod = new FinishPeriod();
            newMod.Init(this);
            return newMod;
        }
    }
}
