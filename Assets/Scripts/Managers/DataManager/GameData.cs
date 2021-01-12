using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Casino
{
    [Serializable]
    public class PlayerData
    {
        public int Money;
        public int Experience;

        PlayerData()
        {
            Money = 0;
            Experience = 0;
        }
    }

    [Serializable]
    public class GameData
    {
        public string Version;
        public float MusicValue;
        public float SoundValue;
        public PlayerData PlayerData;
        public SlotsGameData SlotsData;

        public GameData()
        {
            Version = "2.0";
        }
    }
}