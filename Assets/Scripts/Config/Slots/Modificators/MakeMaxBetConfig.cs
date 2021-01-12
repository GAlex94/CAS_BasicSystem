using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [CreateAssetMenu(fileName = "MakeMaxBetConfig", menuName = "Data/Slots/Modificators/MakeMaxBetConfig")]
    public class MakeMaxBetConfig : SpinModificatorConfig
    {
        [Header("Make Max Bet params")]
        public int MinReward = 400;
        public int MaxReward = 500;
        public int MinCountSpin = 8;
        public int MaxCountSpin = 10;
        public int MaxWinSpins = 5;

        public override SpinModificator CreateMod()
        {
            SpinModificator newMod = new MakeMaxBetMod();
            newMod.Init(this);
            return newMod;
        }
    }
}
