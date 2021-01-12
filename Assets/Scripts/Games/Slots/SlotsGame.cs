using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [RequireComponent(typeof(SpinGenerator))]
    public class SlotsGame : Singleton<SlotsGame>, IGame
    {
        private int minBet;
        private int maxBet;
        private int currentBet;

        private SlotsGameConfig currentSlotsConfig;
        private SlotMachineConfig currentSlotMachineConfig;

        private SpinGenerator spinGenerator;
        private MatrixGenerator matrixGenerator;

        public SlotsGameConfig SlotsConfig => currentSlotsConfig;
        public SlotMachineConfig SlotMachineConfig => currentSlotMachineConfig;
        public int MaxBet => maxBet;

        private RewardModificator rewardMods;

        private List<SpinModificator> spinModificators = new List<SpinModificator>();

        public int CurrentBet
        {
            get { return currentBet; }
            set { currentBet = value; }
        }

        public SpinGenerator Generator => spinGenerator;

        public float MinimumReward
        {
            get { return 20.0f; }
        }

        void Awake()
        {
            spinGenerator = GetComponent<SpinGenerator>();
            matrixGenerator = GetComponent<MatrixGenerator>();
        }

        void Start() //do not use this method for complex logic, use only for simple initializations
        {
        }

        public void StartGame()
        {
            Debug.Log("Start SlotsGame...");

            currentSlotsConfig = (SlotsGameConfig)DataManager.Instance.CurrentGameTypeConfig;
            currentSlotMachineConfig = (SlotMachineConfig)DataManager.Instance.CurrentSimpleGameConfig;

            GUIController.Instance.ShowScreen<SlotMachineScreen>();
            GUIController.Instance.ShowScreen<TopBarScreen>();

            rewardMods = new RewardModificator(currentSlotMachineConfig);

            spinGenerator.SetRewardModificator(rewardMods);

            foreach (var curModConfig in currentSlotsConfig.modConfigs)
            {
                spinModificators.Add(curModConfig.CreateMod());
            }

            spinGenerator.SetSpinModificators(spinModificators);

            CalculateBets();
        }

        void CalculateBets()
        {
            minBet = 100;
            maxBet = 1000;
            currentBet = minBet;
        }

        public void StartSpin()
        {
            Debug.Log("Start spin");

            SpinInfo newSpin = spinGenerator.GenerateSpin();

            DataManager.Instance.SpendMoney(currentBet);

            if (newSpin.finalStatus != SpinStatus.Lose)
            {
                DataManager.Instance.AddMoney(newSpin.moneyReward);
            }

            Debug.Log("New spin: " + newSpin.ToString());
        }

        public void TestMatrix(float reward)
        {
            SpinInfo newSpinInfo = new SpinInfo();
            newSpinInfo.finalRewardPercent = reward;

            matrixGenerator.GenerateMatrix(newSpinInfo);
        }

    }
}
