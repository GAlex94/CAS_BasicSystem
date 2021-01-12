using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino
{
    [CreateAssetMenu(fileName = "SlotsGameConfig", menuName = "Data/Slots/SlotsGameConfig")]
    public class SlotsGameConfig : GameTypeConfig
    {
        [Header("Period parameters")]
        public int SpinsCountOnPeriod = 100;
        public float RTPMinPercent = 90.0f;
        public float RTPMaxPercent = 130.0f;
        public float StartPositiveTrendPercent = 50.0f;
        public float ChangeTrendPercent = 10.0f;

        [Header("Reward parameters")]
        public float MaxRewardForNegativeTrend = 110.0f;

        [Header("Spin modificators")]
        public List<SpinModificatorConfig> modConfigs;

        public override SimpleGameConfig LoadSimpleGameConfig(string simpleGameConfig)
        {
            return Resources.Load<SlotMachineConfig>(simpleGameConfig);
        }
    }
}
