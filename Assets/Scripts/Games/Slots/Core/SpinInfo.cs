using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public enum SpinStatus
    {
        None = 0,
        Lose = 1,
        False = 2,
        Lucky = 3,
        Big = 4
    }

    public class SpinInfo
    {
        public int id;

        public SpinModificatorType currentActivedMod;

        public SpinStatus baseStatus;
        public SpinStatus finalStatus;

        public float baseRewardPercent;
        public float finalRewardPercent;

        public float luckyRTPFactor;
        public float bigWinRTPFactor;

        public int bet;
        public int moneyReward;

        public override string ToString()
        {
            return "Id=" + id + 
                   "; baseStatus=" + baseStatus + "; finalStatus=" + finalStatus +
                   "; baseReward=" + baseRewardPercent + "; finalReward=" + finalRewardPercent +
                   "; luckyRTPFactor=" + luckyRTPFactor + "; bigWinRTPFactor=" + bigWinRTPFactor +
                   "; activeMod=" + currentActivedMod + 
                   "; startBet=" + bet + "; moneyReward=" + moneyReward;
        }

        public static string PrintCSVHeaders()
        {
            return "id " + "baseStatus " + "finalStatus " + "baseReward " + "finalReward " + "luckyRTPFactor " +
                   "bigWinRTPFactor " + "activeMod " + "startBet " + "moneyReward\n";
        }

        public string ToCSV()
        {
            return id + " " + baseStatus + " " + finalStatus + " " + baseRewardPercent + " " + finalRewardPercent +
                   " " + luckyRTPFactor + " " + bigWinRTPFactor + " " + currentActivedMod + " " + bet + " " +
                   moneyReward + "\n";
        }
    }
}
