using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Casino
{
    public class SlotMachineButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI gameTitle;

        [SerializeField]
        private Image gameImage;

        [SerializeField]
        private Button selectButton;

        //init params
        private string gameNameTag;
        private Sprite gameSprite;
        private Action<string> onClick;

        public void Init(string gameTag, Sprite gameSprite, Action<string> onClick)
        {
            gameNameTag = gameTag;
            this.gameSprite = gameSprite;
            this.onClick = onClick;
        }

        void Start()
        {
            gameTitle.text = gameNameTag;
            gameImage.sprite = gameSprite;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick(gameNameTag));
        }
    }
}
