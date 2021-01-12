using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Casino
{
    public enum SlotSymbolType
    {
        Default,
        Wild,
        Scatter
    }

    [Serializable]
    public class SpinChances
    {
        public float LoseChancePercent = 40.0f;
        public float FalseChancePercent = 30.0f;
        public float LuckyChancePercent = 28.0f;
        public float BigWinChancePercent = 2.0f;
    }

    [Serializable]
    public class RewardLimits
    {
        public float MinFalseReward = 20.0f;
        public float MaxLuckyReward = 130.0f;
        public float MinBigReward = 200.0f;
        public float MaxBigReward = 500.0f;
    }

    [Serializable]
    public class SymbolCombination
    {
        public int count;
        public int reward;
        public float spawnProbalityPercent;
    }

    [Serializable]
    public class SlotSymbol
    {
        public string tag;
        public SlotSymbolType type;
        public Sprite defaultSprite;
        public Sprite blurSprite;
        public Sprite activeSprite;
        public SymbolCombination[] combinations;

        public float GetMinimumReward()
        {
            return combinations[0].reward;
        }
        public float GetMaximumReward()
        {
            return combinations.Last().reward;
        }
    }

    [Serializable]
    public class SlotLine
    {
        public string tag;
        public int[] indexOnDrum;

        public override string ToString()
        {
            string result = "";
            foreach (var curIndex in indexOnDrum)
            {
                result = result + curIndex;
            }

            return result;
        }
    }

    [CreateAssetMenu(fileName = "SlotMachineConfig", menuName = "Data/Slots/SlotMachineConfig")]
    public class SlotMachineConfig : SimpleGameConfig
    {
        [Header("Volatility")]
        public SpinChances spinChances;
        public RewardLimits rewardLimits;

        [Header("Configuration")]
        public SlotSymbol[] symbols;
        public SlotSymbol[] allSymbols;

        public int[] drums;

        public SlotLine[] lines;

        public SlotLine[] lines20x;
        public SlotLine[] lines5x;

        public float rewardThershold = 10.0f;

        public int MaxDrumHeight()
        {
            int maxHeight = 0;
            foreach (var curDrum in drums)
            {
                if (curDrum > maxHeight)
                    maxHeight = curDrum;
            }

            return maxHeight;
        }

#if UNITY_EDITOR
        [ContextMenu("Slot machine balance utility")]
        void ShowBalanceUtility()
        {
            SlotBalanceUtility window = (SlotBalanceUtility)EditorWindow.GetWindow(typeof(SlotBalanceUtility));
            window.losePercent = spinChances.LoseChancePercent;
            window.falsePercent = spinChances.FalseChancePercent;
            window.luckyPercent = spinChances.LuckyChancePercent;
            window.bigWinPercent = spinChances.BigWinChancePercent;
            window.minFalseReward = rewardLimits.MinFalseReward;
            window.maxLuckyReward = rewardLimits.MaxLuckyReward;
            window.minBigReward = rewardLimits.MinBigReward;
            window.maxBigReward = rewardLimits.MaxBigReward;
            window.Init(() =>
            {
                spinChances.LoseChancePercent = window.losePercent;
                spinChances.FalseChancePercent = window.falsePercent;
                spinChances.LuckyChancePercent = window.luckyPercent;
                spinChances.BigWinChancePercent = window.bigWinPercent;
                rewardLimits.MinFalseReward = window.minFalseReward;
                rewardLimits.MaxLuckyReward = window.maxLuckyReward;
                rewardLimits.MinBigReward = window.minBigReward;
                rewardLimits.MaxBigReward = window.maxBigReward;
            });

            window.Show();
        }

        [ContextMenu("CopySymbols")]
        void CopySymbols()
        {
            allSymbols = new SlotSymbol[symbols.Length];
            for (int i = 0; i < symbols.Length; i++)
            {
                allSymbols[i] = new SlotSymbol();
                allSymbols[i].type = symbols[i].type;
                allSymbols[i].defaultSprite = symbols[i].defaultSprite;
                allSymbols[i].activeSprite = symbols[i].activeSprite;
                allSymbols[i].blurSprite = symbols[i].blurSprite;
                allSymbols[i].tag = symbols[i].tag;

                allSymbols[i].combinations = new SymbolCombination[symbols[i].combinations.Length];
                for (int j = 0; j < symbols[i].combinations.Length; j++)
                {
                    allSymbols[i].combinations[j] = new SymbolCombination();
                    allSymbols[i].combinations[j].count = symbols[i].combinations[j].count;
                    allSymbols[i].combinations[j].reward = symbols[i].combinations[j].reward;
                    allSymbols[i].combinations[j].spawnProbalityPercent = symbols[i].combinations[j].spawnProbalityPercent;
                }
            }
        }

        [ContextMenu("FormLineTagsFor20X")]
        void FormLineTagsFor20X()
        {
            for (int i = 0; i < lines20x.Length; i++)
            {
                lines20x[i].tag = "line." + i;
            }
        }

        [ContextMenu("CopyLinesFrom20X")]
        void CopyLinesFrom20X()
        {
            lines = new SlotLine[lines20x.Length];
            for (int i = 0; i < lines20x.Length; i++)
            {
                lines[i] = new SlotLine();
                lines[i].indexOnDrum = lines20x[i].indexOnDrum;
                lines[i].tag = lines20x[i].tag;
            }
        }

        [ContextMenu("CopyLinesFrom5X")]
        void CopyLinesFrom5X()
        {
            lines = new SlotLine[lines5x.Length];
            for (int i = 0; i < lines5x.Length; i++)
            {
                lines[i] = new SlotLine();
                lines[i].indexOnDrum = lines5x[i].indexOnDrum;
                lines[i].tag = lines5x[i].tag;
            }
        }
#endif
    }
}
