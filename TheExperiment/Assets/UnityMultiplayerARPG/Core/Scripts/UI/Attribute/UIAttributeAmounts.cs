﻿using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIAttributeAmounts : UISelectionEntry<Dictionary<Attribute, short>>
    {
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountFormat = "{0}: {1}/{2}";
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountNotReachTargetFormat = "{0}: <color=red>{1}/{2}</color>";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIAttributeTextPair[] textAmounts;

        private Dictionary<Attribute, TextWrapper> cacheTextAmounts;
        public Dictionary<Attribute, TextWrapper> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<Attribute, TextWrapper>();
                    foreach (var textAmount in textAmounts)
                    {
                        if (textAmount.attribute == null || textAmount.uiText == null)
                            continue;
                        var key = textAmount.attribute;
                        var textComp = textAmount.uiText;
                        textComp.text = string.Format(amountFormat, key.title, "0", "0");
                        cacheTextAmounts[key] = textComp;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);

                foreach (var textAmount in CacheTextAmounts)
                {
                    var element = textAmount.Key;
                    textAmount.Value.text = string.Format(amountFormat, element.title, "0", "0");
                }
            }
            else
            {
                var text = "";
                foreach (var dataEntry in Data)
                {
                    var attribute = dataEntry.Key;
                    var targetAmount = dataEntry.Value;
                    if (attribute == null || targetAmount == 0)
                        continue;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    short currentAmount = 0;
                    if (owningCharacter != null)
                        owningCharacter.CacheAttributes.TryGetValue(attribute, out currentAmount);
                    var format = currentAmount >= targetAmount ? amountFormat : amountNotReachTargetFormat;
                    var amountText = string.Format(format, attribute.title, currentAmount.ToString("N0"), targetAmount.ToString("N0"));
                    text += amountText;
                    TextWrapper cacheTextAmount;
                    if (CacheTextAmounts.TryGetValue(attribute, out cacheTextAmount))
                        cacheTextAmount.text = amountText;
                }
                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(text));
                    uiTextAllAmounts.text = text;
                }
            }
        }
    }
}
