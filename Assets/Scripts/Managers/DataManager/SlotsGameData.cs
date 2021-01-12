using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Casino
{
    [Serializable]
    public class PeriodData
    {
        public int startSpinId;
        public float curRTP;
        public bool isFirstPeriod;
        public bool isPositiveTrend;
        public int playerBankOnStart;
        public bool finished;
        public bool needOverrideNextTrend;
        public bool nextPositiveTrend;

        public PeriodData()
        {
            finished = true;
            isFirstPeriod = true;
            needOverrideNextTrend = false;
            nextPositiveTrend = false;
        }
    }

    [Serializable]
    public class FinishPeriodModData
    {
        public int startSpinIdByDescreaseBank;
    }

    [Serializable]
    public class PayUserModData
    {
        public int startSpinId;
        public int bigWinSpinIndex;
    }

    [Serializable]
    public class AbsentUserModData
    {
        public int startSpinId;
    }

    [Serializable]
    public class BankNearZeroModData
    {
        public int countNeedUse;
    }

    [Serializable]
    public class MakeMaxBetModData
    {
        public bool lastBetMax;
        public int countNoMaxBet;
        public int countSpinToNextPeriod;
    }

    [Serializable]
    public class SlotsGameData
    {
        public int lastSpinId;
        public bool userPaid;
        public bool userAbsent;
        public PeriodData periodData;
        public SpinModificatorType modActivated;
        public FinishPeriodModData finishPeriodModData;
        public PayUserModData payUserModData;
        public AbsentUserModData absentUserModData;
        public BankNearZeroModData bankNearZeroModData;
        public MakeMaxBetModData makeMaxBetModData;
    }
}