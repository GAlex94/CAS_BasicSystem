using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Casino
{
    public class TopBarScreen : GUIScreen, IMoneyListener
    {
        [SerializeField]
        private TextMeshProUGUI moneyText;

        private int curMoney = 0;
        private int newMoney = 0;

        void Start()
        {
            newMoney = DataManager.Instance.Money;

            DataManager.Instance.AddMoneyListener(this);
        }

        void OnDestroy()
        {
            DataManager.Instance.RemoveMoneyListener(this);
        }

        public void OnMoneyChange(int newMoney, int oldMoney)
        {
            this.newMoney = newMoney;
        }

        void Update()
        {
            if (newMoney != curMoney)
            {
                float sign = Mathf.Sign(newMoney - curMoney);
                float increment = Mathf.Max(Mathf.Abs(newMoney - curMoney) * Time.deltaTime * 3.0f, 1.0f);
                curMoney = (int)(curMoney + increment * sign);
                if (Mathf.Abs(curMoney - newMoney) < 2)
                    curMoney = newMoney;

                moneyText.text = curMoney.ToString();
            }
        }
    }
}
