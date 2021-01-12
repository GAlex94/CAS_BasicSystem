using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [CreateAssetMenu(fileName = "PayUserConfig", menuName = "Data/Slots/Modificators/PayUserConfig")]
    public class PayUserConfig : SpinModificatorConfig
    {
        [Header("Pay user params")]
        public int MaxWinSpins = 7;

        public override SpinModificator CreateMod()
        {
            SpinModificator newMod = new PayUserMod();
            newMod.Init(this);
            return newMod;
        }
    }
}
