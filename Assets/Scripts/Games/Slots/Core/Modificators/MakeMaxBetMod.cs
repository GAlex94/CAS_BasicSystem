using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    public class MakeMaxBetMod : SpinModificator
    {
        private MakeMaxBetConfig selfConfig;

        protected override void BaseInit(SpinModificatorConfig config)
        {
            selfConfig = (MakeMaxBetConfig)config;
        }

        public override bool NeedToActivate(SpinInfo spinInfo)
        {
            if (spinInfo.bet != SlotsGame.Instance.MaxBet)
                DataManager.Instance.SlotsData.makeMaxBetModData.countNoMaxBet++;


            if (!DataManager.Instance.SlotsData.makeMaxBetModData.lastBetMax)
            {
                if (spinInfo.bet == SlotsGame.Instance.MaxBet)
                {
                    DataManager.Instance.SlotsData.makeMaxBetModData.lastBetMax = true;
                    return DataManager.Instance.SlotsData.makeMaxBetModData.countNoMaxBet >=
                           DataManager.Instance.SlotsData.makeMaxBetModData.countSpinToNextPeriod;
                }

                return false;
            }

            DataManager.Instance.SlotsData.makeMaxBetModData.lastBetMax = spinInfo.bet == SlotsGame.Instance.MaxBet;
            return false;


        }

        public override bool NeedToDeactivate(SpinInfo spinInfo)
        {
            return DataManager.Instance.SlotsData.makeMaxBetModData.countNoMaxBet <
                   DataManager.Instance.SlotsData.makeMaxBetModData.countSpinToNextPeriod || spinInfo.bet == SlotsGame.Instance.MaxBet;
        }

        public override void Activate()
        {
            DataManager.Instance.MakeMaxBetPeriod(UnityEngine.Random.Range(selfConfig.MinCountSpin, selfConfig.MaxCountSpin));
        }

        public override SpinStatus ProcessSpinStatus(SpinInfo spinInfo)
        {
            int spinIndex = DataManager.Instance.SlotsData.lastSpinId -
                            DataManager.Instance.SlotsData.absentUserModData.startSpinId;

            if (spinIndex >= (selfConfig.MaxWinSpins - 1))
            {
                DataManager.Instance.FinishPeriod(false);
            }

            return spinInfo.baseStatus == SpinStatus.Lose ? SpinStatus.Lucky : spinInfo.baseStatus;
        }

        public override float ProcessReward(SpinInfo spinInfo)
        {
            return UnityEngine.Random.Range(selfConfig.MinReward, selfConfig.MaxReward);
        }
    }
}
