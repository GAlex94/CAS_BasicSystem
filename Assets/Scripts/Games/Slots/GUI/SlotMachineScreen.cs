using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Casino
{
    public class SlotMachineScreen : GUIScreen
    {
        [SerializeField]
        private TextMeshProUGUI title;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Button spinButton;

        [SerializeField]
        private Button forcePayMod;

        [SerializeField]
        private Button forceAbsentMod;

        [SerializeField]
        private Button forceMinBetMod;

        [SerializeField]
        private Button forceMaxBetMod;

        [SerializeField]
        private Button testMatrixButton;

        [SerializeField]
        private InputField testReward;

        void Start()
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                GameManager.Instance.LoadMenu();
            });

            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(() =>
            {
                SlotsGame.Instance.StartSpin();
            });

            forcePayMod.onClick.RemoveAllListeners();
            forcePayMod.onClick.AddListener(() =>
            {
                DataManager.Instance.UserPaid();
            });

            forceAbsentMod.onClick.RemoveAllListeners();
            forceAbsentMod.onClick.AddListener(() =>
            {
                DataManager.Instance.UserAbsent();
            });


            forceMinBetMod.onClick.RemoveAllListeners();
            forceMinBetMod.onClick.AddListener(() =>
            {
                DataManager.Instance.SetmaxBetMode(false);
            });


            forceMaxBetMod.onClick.RemoveAllListeners();
            forceMaxBetMod.onClick.AddListener(() =>
            {
                DataManager.Instance.SetmaxBetMode(true);
            });

            testMatrixButton.onClick.RemoveAllListeners();
            testMatrixButton.onClick.AddListener(() =>
            {
                SlotsGame.Instance.TestMatrix(Convert.ToSingle(testReward.text));
            });
        }

        protected override void OnShow()
        {
            title.text = DataManager.Instance.CurrentSimpleGameConfig.loadingTextTag;
            Debug.Log("SlotMachineScreen OnShow");
        }

        protected override void OnHide()
        {
            Debug.Log("SlotMachineScreen OnHide");
        }
    }
}
