using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PlayerStatistics
{
    [CreateAssetMenu(fileName = "StatisticsData", menuName = "ScriptableObjects/StatisticsData")]
    public class StatisticsData : ScriptableObject
    {
        public int MaxHealth;
        public Color[] LifeColors;
    }
}
