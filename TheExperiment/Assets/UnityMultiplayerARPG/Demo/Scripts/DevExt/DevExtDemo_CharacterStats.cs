using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class CharacterStats
    {
        [Header("Demo Developer Extension")]
        public float testStats;

        [DevExtMethods("Add")]
        public void DevExtDemo_Add(CharacterStats result, CharacterStats b)
        {
            result.testStats = testStats + b.testStats;
        }

        [DevExtMethods("Multiply")]
        public void DevExtDemo_Multiply(CharacterStats result, float multiplier)
        {
            result.testStats = testStats * multiplier;
        }
    }
}
