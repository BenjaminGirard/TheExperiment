﻿using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        [Header("Format")]
        [Tooltip("Hp Stats Format => {0} = {Amount}")]
        public string hpStatsFormat = "Hp: {0}";
        [Tooltip("Mp Stats Format => {0} = {Amount}")]
        public string mpStatsFormat = "Mp: {0}";
        [Tooltip("Armor Stats Format => {0} = {Amount}")]
        public string armorStatsFormat = "Armor: {0}";
        [Tooltip("Accuracy Stats Format => {0} = {Amount}")]
        public string accuracyStatsFormat = "Acc: {0}";
        [Tooltip("Evasion Format => {0} = {Amount}")]
        public string evasionStatsFormat = "Eva: {0}";
        [Tooltip("Cri Rate Stats Format => {0} = {Amount}")]
        public string criRateStatsFormat = "Critical: {0}%";
        [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
        public string criDmgRateStatsFormat = "Cri Dmg: {0}%";
        [Tooltip("Block Rate Stats Format => {0} = {Amount}")]
        public string blockRateStatsFormat = "Block: {0}%";
        [Tooltip("Block Dmg Rate Stats Format => {0} = {Amount}")]
        public string blockDmgRateStatsFormat = "Block Dmg: {0}%";
        [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
        public string weightLimitStatsFormat = "Weight Limit: {0}";
        [Tooltip("Stamina Stats Format => {0} = {Amount}")]
        public string staminaStatsFormat = "Stamina: {0}";
        [Tooltip("Food Stats Format => {0} = {Amount}")]
        public string foodStatsFormat = "Food: {0}";
        [Tooltip("Water Stats Format => {0} = {Amount}")]
        public string waterStatsFormat = "Water: {0}";

        [Header("UI Elements")]
        public TextWrapper uiTextStats;
        public TextWrapper uiTextHp;
        public TextWrapper uiTextMp;
        public TextWrapper uiTextArmor;
        public TextWrapper uiTextAccuracy;
        public TextWrapper uiTextEvasion;
        public TextWrapper uiTextCriRate;
        public TextWrapper uiTextCriDmgRate;
        public TextWrapper uiTextBlockRate;
        public TextWrapper uiTextBlockDmgRate;
        public TextWrapper uiTextWeightLimit;
        public TextWrapper uiTextStamina;
        public TextWrapper uiTextFood;
        public TextWrapper uiTextWater;

        protected override void UpdateData()
        {
            var statsString = "";
            var statsStringPart = "";

            // Hp
            statsStringPart = string.Format(hpStatsFormat, Data.hp.ToString("N0"));
            if (Data.hp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextHp != null)
                uiTextHp.text = statsStringPart;

            // Mp
            statsStringPart = string.Format(mpStatsFormat, Data.mp.ToString("N0"));
            if (Data.mp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMp != null)
                uiTextMp.text = statsStringPart;

            // Armor
            statsStringPart = string.Format(armorStatsFormat, Data.armor.ToString("N0"));
            if (Data.armor != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextArmor != null)
                uiTextArmor.text = statsStringPart;

            // Accuracy
            statsStringPart = string.Format(accuracyStatsFormat, Data.accuracy.ToString("N0"));
            if (Data.accuracy != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAccuracy != null)
                uiTextAccuracy.text = statsStringPart;

            // Evasion
            statsStringPart = string.Format(evasionStatsFormat, Data.evasion.ToString("N0"));
            if (Data.evasion != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextEvasion != null)
                uiTextEvasion.text = statsStringPart;

            // Cri Rate
            statsStringPart = string.Format(criRateStatsFormat, (Data.criRate * 100).ToString("N2"));
            if (Data.criRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriRate != null)
                uiTextCriRate.text = statsStringPart;

            // Cri Dmg Rate
            statsStringPart = string.Format(criDmgRateStatsFormat, (Data.criDmgRate * 100).ToString("N2"));
            if (Data.criDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriDmgRate != null)
                uiTextCriDmgRate.text = statsStringPart;

            // Block Rate
            statsStringPart = string.Format(blockRateStatsFormat, (Data.blockRate * 100).ToString("N2"));
            if (Data.blockRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockRate != null)
                uiTextBlockRate.text = statsStringPart;

            // Block Dmg Rate
            statsStringPart = string.Format(blockDmgRateStatsFormat, (Data.blockDmgRate * 100).ToString("N2"));
            if (Data.blockDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockDmgRate != null)
                uiTextBlockDmgRate.text = statsStringPart;

            // Weight
            statsStringPart = string.Format(weightLimitStatsFormat, Data.weightLimit.ToString("N2"));
            if (Data.weightLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextWeightLimit != null)
                uiTextWeightLimit.text = statsStringPart;

            // Stamina
            statsStringPart = string.Format(staminaStatsFormat, Data.stamina.ToString("N0"));
            if (Data.stamina != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextStamina != null)
                uiTextStamina.text = statsStringPart;

            // Food
            statsStringPart = string.Format(foodStatsFormat, Data.food.ToString("N0"));
            if (Data.food != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextFood != null)
                uiTextFood.text = statsStringPart;

            // Water
            statsStringPart = string.Format(waterStatsFormat, Data.water.ToString("N0"));
            if (Data.water != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextWater != null)
                uiTextWater.text = statsStringPart;

            // All stats text
            if (uiTextStats != null)
            {
                uiTextStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
                uiTextStats.text = statsString;
            }
        }
    }
}
