using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Casino
{
    public class LobbyScreen : GUIScreen
    {
        [SerializeField]
        private Button loadSlotsButton;

        [SerializeField]
        private Button loadPokerButton;

        void Start()
        {
            loadSlotsButton.onClick.RemoveAllListeners();
            loadSlotsButton.onClick.AddListener(() => OnGameClick(CasinoGameType.Slots));

            loadPokerButton.onClick.RemoveAllListeners();
            loadPokerButton.onClick.AddListener(() => OnGameClick(CasinoGameType.Pokers));
        }

        void OnGameClick(CasinoGameType gameType)
        {
            GUIController.Instance.HideScreen<LobbyScreen>();
            GameManager.Instance.ActivateMainGame(gameType);
        }
    }
}
