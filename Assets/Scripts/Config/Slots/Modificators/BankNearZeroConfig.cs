using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Casino
{
    [CreateAssetMenu(fileName = "BankNearZeroConfig", menuName = "Data/Slots/Modificators/BankNearZeroConfig")]
    public class BankNearZeroConfig : SpinModificatorConfig
    {
        [Header("Bank near zero params")]
        [Tooltip("Спины активаторы")]
        public List<int> NubmerSpinActive;
        [Tooltip("Спин на котором модификатор будет срабатывать после перебора всех возможных спинов активатор")]
        public int spinMoreActive; 
        public float Reward = 200.0f;

        public override SpinModificator CreateMod()
        {
            SpinModificator newMod = new BankNearZeroMod();
            newMod.Init(this);
            return newMod;
        }

        
    }
}