using System.Collections;
using System.Collections.Generic;
//using GM.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Casino
{
    public class ManagersCreator : MonoBehaviour
    {
        [Header("Game managers settings")]
        [SerializeField]
        private bool isDebug = true;
        [SerializeField]
        private bool isFakeShop = true;
        [SerializeField]
        private CasinoGameType curGame = CasinoGameType.None;
        [SerializeField]
        private string curSimpleGameTag = "";

        [Header("Data managers settings")]
        [SerializeField]
        private string profileName = "MainProfile";

        [SerializeField]
        private bool clearProfile = false;

        [SerializeField]
        private DefaultProfile defaultProfile = null;

        [SerializeField]
        private CasinoConfig casinoConfig = null;

        void Awake()
        {
            Debug.Log("ManagersCreator awake");
            if (!GameManager.IsAwake)
            {
                DataManager.Instance.Init(profileName, clearProfile, defaultProfile, casinoConfig);
                GameManager.Instance.Init(isDebug, isFakeShop, curGame, curSimpleGameTag);
                //LocalizationService.Activate();
            }
        }
    }
}
