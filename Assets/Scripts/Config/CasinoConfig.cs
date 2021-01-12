using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public enum CasinoGameType
    {
        None,
        Slots,
        Pokers
    }

    [CreateAssetMenu(fileName = "CasinoConfig", menuName = "Data/CasinoConfig")]
    public class CasinoConfig : ScriptableObject
    {
        public LobbyGamesConfig[] allGames;

        public LobbyGamesConfig FindGame(CasinoGameType gameType)
        {
            int foundIndex = Array.FindIndex(allGames, curGame => curGame.gameType == gameType);
            if (foundIndex != -1)
            {
                return allGames[foundIndex];
            }
            else
            {
                Debug.LogError("Game " + gameType + " not found in configs!");
                return null;
            }
        }
    }
}
