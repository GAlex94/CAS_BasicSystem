using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [CreateAssetMenu(fileName = "DefaultProfile", menuName = "Data/DefaultProfile")]
    public class DefaultProfile : ScriptableObject
    {
        public GameData profileData;
    }
}
