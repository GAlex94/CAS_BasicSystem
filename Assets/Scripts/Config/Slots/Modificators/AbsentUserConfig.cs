using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [CreateAssetMenu(fileName = "AbsentUserConfig", menuName = "Data/Slots/Modificators/AbsentUserConfig")]
    public class AbsentUserConfig : SpinModificatorConfig
    {
        [Header("Absent user params")]
        public int MaxWinSpins = 5;

        public override SpinModificator CreateMod()
        {
            SpinModificator newMod = new AbsentUserMod();
            newMod.Init(this);
            return newMod;
        }
    }
}
