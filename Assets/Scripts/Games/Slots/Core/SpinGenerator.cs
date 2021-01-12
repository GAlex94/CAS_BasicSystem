using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Casino
{
    class SpinChance
    {
        public float originalChance;
        public float chanceOffset;
        public SpinStatus status;
    }

    public class SpinGenerator : MonoBehaviour
    {
        private RewardModificator rewardMods;
        private List<SpinModificator> spinModificators;
        private SpinModificator currentActivedMod = null;
        private SpinInfo currentSpin;

        private SlotsGameData SlotsData => DataManager.Instance.SlotsData;
        private List<SpinChance> currentChances = new List<SpinChance>();

        public void SetRewardModificator(RewardModificator rewardMod)
        {
            rewardMods = rewardMod;
        }

        public void SetSpinModificators(List<SpinModificator> spinModificators)
        {
            this.spinModificators = spinModificators;
        }

        public SpinInfo GenerateSpin()
        {
            //UnityEngine.Random.InitState((int)Time.time);

            currentSpin = new SpinInfo();

            currentSpin.id = DataManager.Instance.GenerateSpinId();
            currentSpin.bet = SlotsGame.Instance.CurrentBet;

            //if (SlotsData.periodData.finished)
            CheckForNewPeriod();

            float bankRemain = SlotsData.periodData.playerBankOnStart * (SlotsData.periodData.curRTP / 100.0f) - DataManager.Instance.Money;            
            int spinsRemain = Mathf.Max(SlotsGame.Instance.SlotsConfig.SpinsCountOnPeriod - (SlotsData.lastSpinId - SlotsData.periodData.startSpinId), 0);
            rewardMods.UpdateRewardFactors(Mathf.RoundToInt(bankRemain), spinsRemain, currentSpin.bet);

            currentSpin.luckyRTPFactor = rewardMods.LuckyFactor;
            currentSpin.bigWinRTPFactor = rewardMods.BigWinFactor;

            CalculateChances();

            currentSpin.baseStatus = RandomSpinStatus();
            currentSpin.baseRewardPercent = CalculateReward(currentSpin.baseStatus);

            FindAndActivateModificator(currentSpin);

            currentSpin.finalStatus = currentSpin.baseStatus;
            if (currentSpin.currentActivedMod != SpinModificatorType.None)
            {
                currentSpin.finalStatus = currentActivedMod.ProcessSpinStatus(currentSpin);
            }

            currentSpin.finalRewardPercent = CalculateReward(currentSpin.finalStatus);
            if (currentSpin.currentActivedMod != SpinModificatorType.None)
            {
                currentSpin.finalRewardPercent = currentActivedMod.ProcessReward(currentSpin);
            }

            currentSpin.moneyReward = Mathf.RoundToInt((float)currentSpin.bet * currentSpin.finalRewardPercent);

            DataManager.Instance.PushNewSpin(currentSpin);

            return currentSpin;
        }

        void FindAndActivateModificator(SpinInfo spinInfo)
        {
            int activatedModIndex = -1;
            int highNeedActivateIndex = -1;
            int toActivateIndex = -1;

            for (int i = 0; i < spinModificators.Count; i++)
            {
                if (!spinModificators[i].Enabled)
                    continue;

                if (SlotsData.modActivated == spinModificators[i].ModType)
                {
                    if (spinModificators[i].NeedToDeactivate(spinInfo))
                    {
                        DataManager.Instance.SetActivatedSpinModificator(SpinModificatorType.None);
                        currentActivedMod = null;
                        spinInfo.currentActivedMod = SpinModificatorType.None;
                    }
                    else
                    {
                        activatedModIndex = i;
                    }
                }
                if (spinModificators[i].NeedToActivate(spinInfo) && highNeedActivateIndex == -1)
                {
                    highNeedActivateIndex = i;
                }
            }

            if (highNeedActivateIndex != -1)
            {
                toActivateIndex = -1;

                if (activatedModIndex == -1)
                {
                    toActivateIndex = highNeedActivateIndex;
                }
                else if (highNeedActivateIndex == activatedModIndex &&
                         spinModificators[highNeedActivateIndex].MayRestart)
                {
                    toActivateIndex = highNeedActivateIndex;
                }
                else if (highNeedActivateIndex < activatedModIndex)
                {
                    toActivateIndex = highNeedActivateIndex;
                }
            }

            if (toActivateIndex != -1)
            {
                spinModificators[toActivateIndex].Activate();
                currentActivedMod = spinModificators[toActivateIndex];
                spinInfo.currentActivedMod = currentActivedMod.ModType;

                DataManager.Instance.SetActivatedSpinModificator(currentActivedMod.ModType);
            }
            else if (activatedModIndex != -1)
            {
                currentActivedMod = spinModificators[activatedModIndex];
                spinInfo.currentActivedMod = currentActivedMod.ModType;
            }
            else
            {
                currentActivedMod = null;
                spinInfo.currentActivedMod = SpinModificatorType.None;
            }
        }

        void CalculateChances()
        {
            currentChances.Clear();
            var slotConfig = SlotsGame.Instance.SlotMachineConfig;

            currentChances.Add(new SpinChance() { originalChance = slotConfig.spinChances.LoseChancePercent, status = SpinStatus.Lose });
            currentChances.Add(new SpinChance() { originalChance = slotConfig.spinChances.FalseChancePercent, status = SpinStatus.False });
            currentChances.Add(new SpinChance() { originalChance = slotConfig.spinChances.LuckyChancePercent, status = SpinStatus.Lucky });
            currentChances.Add(new SpinChance() { originalChance = slotConfig.spinChances.BigWinChancePercent, status = SpinStatus.Big });

            //todo: need to modificate original chances by RTP

            float allChances = 0.0f;
            foreach (var curChance in currentChances)
            {
                allChances += curChance.originalChance;
            }

            allChances = 100.0f / allChances;

            float lastOffset = 0.0f;
            for (int i = 0; i < currentChances.Count; i++)
            {
                currentChances[i].originalChance *= allChances; //normalize chances
                currentChances[i].chanceOffset = currentChances[i].originalChance + lastOffset;

                lastOffset = currentChances[i].chanceOffset;
            }

            //Debug.Log("Print chances: Lose=" + currentChances[0].originalChance + "; False=" + currentChances[1].originalChance + "; Lucky=" + currentChances[2].originalChance + "; Big=" + currentChances[3].originalChance);
            //Debug.Log("Print chances offset: Lose=" + currentChances[0].chanceOffset + "; False=" + currentChances[1].chanceOffset + "; Lucky=" + currentChances[2].chanceOffset + "; Big=" + currentChances[3].chanceOffset);
        }

        SpinStatus RandomSpinStatus()
        {
            float randomValue = UnityEngine.Random.value * 100.0f;
            for (int i = 0; i < currentChances.Count; i++)
            {
                if (randomValue <= (currentChances[i].chanceOffset))
                    return currentChances[i].status;
            }

            return currentChances.Last().status;
        }

        void CheckForNewPeriod()
        {
            bool createNewPeriod = false;

            PeriodData periodData = DataManager.Instance.SlotsData.periodData;
            int leftSpins = DataManager.Instance.SlotsData.lastSpinId - periodData.startSpinId;
            if (leftSpins > SlotsGame.Instance.SlotsConfig.SpinsCountOnPeriod)
            {
                int needBank = (int)(periodData.playerBankOnStart * (periodData.curRTP / 100.0f));

                int delta = DataManager.Instance.Money - needBank;
                int maxDelta = SlotsGame.Instance.CurrentBet / 2;

                if (Mathf.Abs(delta) < maxDelta)
                {
                    createNewPeriod = true;
                }
                Debug.Log("Check for finish period. Current bank=" + DataManager.Instance.Money + "; Need bank=" + needBank + "; MaxDelta=" + maxDelta);
            }

            if (createNewPeriod || periodData.finished)
            {
                PeriodData newPeriodData = new PeriodData();
                newPeriodData.finished = false;
                newPeriodData.isFirstPeriod = false;
                newPeriodData.startSpinId = currentSpin.id;
                newPeriodData.playerBankOnStart = DataManager.Instance.Money;

                float positiveTrendValue = SlotsGame.Instance.SlotsConfig.StartPositiveTrendPercent / 100.0f;
                if (!SlotsData.periodData.isFirstPeriod)
                {
                    float changeValue = SlotsGame.Instance.SlotsConfig.ChangeTrendPercent / 100.0f;
                    if (SlotsData.periodData.isPositiveTrend)
                    {
                        positiveTrendValue -= changeValue;
                    }
                    else
                    {
                        positiveTrendValue += changeValue;
                    }
                }

                bool currentPositiveTrend = UnityEngine.Random.value < positiveTrendValue;
                newPeriodData.isPositiveTrend = currentPositiveTrend;

                if (SlotsData.periodData.needOverrideNextTrend)
                    newPeriodData.isPositiveTrend = SlotsData.periodData.nextPositiveTrend;

                newPeriodData.needOverrideNextTrend = false;

                if (newPeriodData.isPositiveTrend)
                    newPeriodData.curRTP = UnityEngine.Random.Range(100.0f, SlotsGame.Instance.SlotsConfig.RTPMaxPercent);
                else
                    newPeriodData.curRTP = UnityEngine.Random.Range(SlotsGame.Instance.SlotsConfig.RTPMinPercent, 100.0f);

                string createReason = "default";
                if (periodData.finished)
                    createReason = "forced";

                Debug.Log("New period created " + createReason + "! Start spin= " + newPeriodData.startSpinId + "; New RTP: " + newPeriodData.curRTP + ". Player bank: " + newPeriodData.playerBankOnStart);

                DataManager.Instance.SetNewPeriod(newPeriodData);
            }
        }

        float CalculateReward(SpinStatus inputStatus)
        {
            float minFalseReward = SlotsGame.Instance.SlotMachineConfig.rewardLimits.MinFalseReward;
            float maxLuckyReward = SlotsGame.Instance.SlotMachineConfig.rewardLimits.MaxLuckyReward;
            float minBigReward = SlotsGame.Instance.SlotMachineConfig.rewardLimits.MinBigReward;
            float maxBigReward = SlotsGame.Instance.SlotMachineConfig.rewardLimits.MaxBigReward;

            float outputReward = 0.0f;
            if (inputStatus == SpinStatus.Lose)
            {
                outputReward = 0.0f;
            }
            else if (inputStatus == SpinStatus.False)
            {
                outputReward =
                    Random.Range(minFalseReward / 100.0f, 1.0f);
            }
            else if (inputStatus == SpinStatus.Lucky)
            {
                outputReward =
                    Random.Range(1.0f, Mathf.Max((maxLuckyReward * rewardMods.LuckyFactor) / 100.0f, 1.0f));
            }
            else
            {
                outputReward =
                    Random.Range(Mathf.Max((minBigReward * rewardMods.BigWinFactor) / 100.0f, 1.0f),
                        Mathf.Max((maxBigReward * rewardMods.BigWinFactor) / 100.0f, 1.0f));
            }

            return outputReward;


            /*
            //todo: override base reward by modificators
            if (currentSpin.currentActivedMod != SpinModificatorType.None)
            {
                currentSpin.finalRewardPercent = currentActivedMod.ProcessReward(currentSpin);
            }
            else
            {
                currentSpin.finalRewardPercent = currentSpin.baseRewardPercent;
            }
            currentSpin.moneyReward = Mathf.RoundToInt((float)currentSpin.bet * currentSpin.finalRewardPercent);
            */
        }
    }
}
