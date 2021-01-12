using System;
using System.Collections;
using System.Collections.Generic;
using Casino;
using UnityEngine;
using UnityEditor;

namespace Casino
{
    public class SlotBalanceUtility : EditorWindow
    {
        public float losePercent = 100.0f;
        public float falsePercent = 100.0f;
        public float luckyPercent = 100.0f;
        public float bigWinPercent = 100.0f;

        public float minFalseReward = 30.0f;
        public float maxLuckyReward = 130.0f;
        public float minBigReward = 200.0f;
        public float maxBigReward = 500.0f;

        private float periodBalance = 0.0f;
        private Action onFinishClick;

        public void Init(Action onFinishClick)
        {
            this.onFinishClick = onFinishClick;
        }

        void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label("Lose percent: " + losePercent.ToString("0.00"));
            losePercent = GUILayout.HorizontalScrollbar(losePercent, 1.0f, 0.0f, 100.0f);

            GUILayout.Label("False percent: " + falsePercent.ToString("0.00"));
            falsePercent = GUILayout.HorizontalScrollbar(falsePercent, 1.0f, 0.0f, 100.0f);

            GUILayout.Label("Lucky percent: " + luckyPercent.ToString("0.00"));
            luckyPercent = GUILayout.HorizontalScrollbar(luckyPercent, 1.0f, 0.0f, 100.0f);

            GUILayout.Label("bigWin percent: " + bigWinPercent.ToString("0.00"));
            bigWinPercent = GUILayout.HorizontalScrollbar(bigWinPercent, 1.0f, 0.0f, 100.0f);

            GUILayout.Space(20);

            GUILayout.Label("MinFalseReward percent: " + minFalseReward.ToString("0.00"));
            minFalseReward = GUILayout.HorizontalScrollbar(minFalseReward, 1.0f, 0.0f, 100.0f);

            GUILayout.Label("MaxLuckyReward percent: " + maxLuckyReward.ToString("0.00"));
            maxLuckyReward = GUILayout.HorizontalScrollbar(maxLuckyReward, 1.0f, 100.0f, 200.0f);

            GUILayout.Label("MinBigReward percent: " + minBigReward.ToString("0.00"));
            minBigReward = GUILayout.HorizontalScrollbar(minBigReward, 1.0f, 100.0f, 1000.0f);

            GUILayout.Label("MinBigReward percent: " + maxBigReward.ToString("0.00"));
            maxBigReward = GUILayout.HorizontalScrollbar(maxBigReward, 1.0f, 100.0f, 1000.0f);

            CalcPeriod();

            GUILayout.Space(10);

            GUILayout.TextField("Balance=" + periodBalance.ToString("0.00"), 25);

            if (GUILayout.Button("Move to config"))
            {
                if (onFinishClick != null)
                    onFinishClick();

                this.Close();
            }
        }

        void CalcPeriod()
        {
            float allPercents = 100.0f / (losePercent + falsePercent + luckyPercent + bigWinPercent);
            losePercent = losePercent * allPercents;
            falsePercent = falsePercent * allPercents;
            luckyPercent = luckyPercent * allPercents;
            bigWinPercent = bigWinPercent * allPercents;

            float loseBets = losePercent * 100.0f + falsePercent * (100.0f - minFalseReward) * 0.5f;
            float rewardBets = luckyPercent * (maxLuckyReward - 100.0f) * 0.5f + bigWinPercent * ((maxBigReward + minBigReward) * 0.5f - 100.0f);
            periodBalance = rewardBets - loseBets;
        }


    }

}
