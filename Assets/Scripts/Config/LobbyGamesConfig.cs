using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [Serializable]
    public class GameMeta
    {
        public string gameNameTag;
        public Sprite gameSprite;
        public string gameConfigInResources;
    }

    [CreateAssetMenu(fileName = "LobbyGamesConfig", menuName = "Data/LobbyGamesConfig")]
    public class LobbyGamesConfig : ScriptableObject
    {
        public CasinoGameType gameType;
        public GUIScreen gameLobby;
        public string mainGameConfigInResources;
        public GameMeta[] games;
        public bool enabled;
    }
}
