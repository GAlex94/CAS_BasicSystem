using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Casino
{
    public class SlotsLobbyScreen : GUIScreen
    {
        [SerializeField]
        private GameObject slotButtonPrefab;

        [SerializeField]
        private Transform slotsButtonContainer;

        [SerializeField]
        private Button backButton;

        private List<SlotMachineButton> slotButtons = new List<SlotMachineButton>();

        void Start()
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                GameManager.Instance.ClearCurrentMainGame();
                GUIController.Instance.HideScreen<SlotsLobbyScreen>();
                GUIController.Instance.ShowScreen<LobbyScreen>();
            });
        }

        protected override void OnShow()
        {
            Debug.Log("SlotsLobbyScreen OnShow");

            if (slotButtons.Count == 0)
            {
                foreach (var curGameMeta in DataManager.Instance.CurrentGameLobbyConfig.games)
                {
                    GameObject newObj = Instantiate(slotButtonPrefab);
                    newObj.transform.SetParent(slotsButtonContainer);

                    SlotMachineButton slotButton = newObj.GetComponent<SlotMachineButton>();
                    slotButton.Init(curGameMeta.gameNameTag, curGameMeta.gameSprite, OnSlotClick);
                    slotButtons.Add(slotButton);
                }
            }
        }

        protected override void OnHide()
        {
            Debug.Log("SlotsLobbyScreen OnHide");
        }

        void OnSlotClick(string gameNameTag)
        {
            Debug.Log("Click to slot " + gameNameTag);
            GameManager.Instance.LoadSimpleGame(gameNameTag);
        }
    }
}
