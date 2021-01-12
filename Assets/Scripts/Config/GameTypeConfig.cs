using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class GameTypeConfig : ScriptableObject
    {
        [Header("Base game parameters")]
        public CasinoGameType gameType;

        public virtual SimpleGameConfig LoadSimpleGameConfig(string simpleGameConfig)
        {
            return null;
        }
    }

    public class SimpleGameConfig : ScriptableObject
    {
        [Header("Base simple game parameters")]
        public string sceneName;
        public Sprite loadingBack;
        public string loadingTextTag;
    }
}
