using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class SpinModificatorConfig : ScriptableObject
    {
        [Header("Base spin's modificator params")]
        public SpinModificatorType ModType;

        public bool Enabled = true;

        public bool MayRestart = false;

        public virtual SpinModificator CreateMod()
        {
            return null;
        }
    }
}
