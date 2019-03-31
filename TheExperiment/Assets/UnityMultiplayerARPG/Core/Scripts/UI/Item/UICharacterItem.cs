﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterItem : UIDataForCharacter<CharacterItemTuple>
    {
        public CharacterItem CharacterItem { get { return Data.characterItem; } }
        public short Level { get { return Data.targetLevel; } }
        public string EquipPosition { get { return Data.equipPosition; } }
        public Item Item { get { return CharacterItem != null ? CharacterItem.GetItem() : null; } }
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public Item ArmorItem { get { return CharacterItem != null ? CharacterItem.GetArmorItem() : null; } }
        public Item WeaponItem { get { return CharacterItem != null ? CharacterItem.GetWeaponItem() : null; } }
        public Item PetItem { get { return CharacterItem != null ? CharacterItem.GetPetItem() : null; } }

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Refine Level Format => {0} = {Refine Level}")]
        public string refineLevelFormat = "+{0}";
        [Tooltip("Title Refine Level Format => {0} = {Refine Level}")]
        public string titleRefineLevelFormat = " (+{0})";
        [Tooltip("Sell Price Format => {0} = {Sell price}")]
        public string sellPriceFormat = "{0}";
        [Tooltip("Stack Format => {0} = {Amount}, {1} = {Max stack}")]
        public string stackFormat = "{0}/{1}";
        [Tooltip("Durability Format => {0} = {Durability}, {1} = {Max durability}")]
        public string durabilityFormat = "{0}/{1}";
        [Tooltip("Weight Format => {0} = {Weight}")]
        public string weightFormat = "{0}";
        [Tooltip("Exp Format => {0} = {Current exp}, {1} = {Max exp}")]
        public string expFormat = "Exp: {0}/{1}";
        [Tooltip("Lock Remains Duration Format => {0} = {Lock Remains duration}")]
        public string lockRemainsDurationFormat = "{0}";
        [Tooltip("Item Type Format => {0} = {Item Type title}")]
        public string itemTypeFormat = "Item Type: {0}";
        [Tooltip("Junk Item Type")]
        public string junkItemType = "Junk";
        [Tooltip("Shield Item Type")]
        public string shieldItemType = "Shield";
        [Tooltip("Potion Item Type")]
        public string potionItemType = "Potion";
        [Tooltip("Ammo Item Type")]
        public string ammoItemType = "Ammo";
        [Tooltip("Building Item Type")]
        public string buildingItemType = "Building";
        [Tooltip("Pet Item Type")]
        public string petItemType = "Pet";

        [Header("Input Dialog Settings")]
        public string dropInputTitle = "Drop Item";
        public string dropInputDescription = "";
        public string sellInputTitle = "Sell Item";
        public string sellInputDescription = "";
        public string setDealingInputTitle = "Offer Item";
        public string setDealingInputDescription = "";

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public TextWrapper uiTextItemType;
        public TextWrapper uiTextSellPrice;
        public TextWrapper uiTextStack;
        public TextWrapper uiTextDurability;
        public TextWrapper uiTextWeight;
        public TextWrapper uiTextExp;
        public TextWrapper uiTextLockRemainsDuration;

        [Header("Equipment - UI Elements")]
        public UIEquipmentItemRequirement uiRequirement;
        public UICharacterStats uiStats;
        public UIAttributeAmounts uiIncreaseAttributes;
        public UIResistanceAmounts uiIncreaseResistances;
        public UIDamageElementAmounts uiIncreaseDamageAmounts;

        [Header("Weapon - UI Elements")]
        public UIDamageElementAmount uiDamageAmounts;

        [Header("Events")]
        public UnityEvent onSetLevelZeroData;
        public UnityEvent onSetNonLevelZeroData;
        public UnityEvent onSetEquippedData;
        public UnityEvent onSetUnEquippedData;
        public UnityEvent onSetUnEquippableData;
        public UnityEvent onNpcSellItemDialogAppear;
        public UnityEvent onNpcSellItemDialogDisappear;
        public UnityEvent onRefineItemDialogAppear;
        public UnityEvent onRefineItemDialogDisappear;
        public UnityEvent onEnterDealingState;
        public UnityEvent onExitDealingState;

        [Header("Options")]
        public UICharacterItem uiNextLevelItem;
        public bool showAmountWhenMaxIsOne;
        public bool showLevelAsDefault;
        public bool dontAppendRefineLevelToTitle;

        private bool isSellItemDialogAppeared;
        private bool isRefineItemDialogAppeared;
        private bool isDealingStateEntered;

        protected float lockRemainsDuration;

        private void OnDisable()
        {
            lockRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (lockRemainsDuration <= 0f)
            {
                lockRemainsDuration = CharacterItem.lockRemainsDuration;
                if (lockRemainsDuration <= 1f)
                    lockRemainsDuration = 0f;
            }

            if (lockRemainsDuration > 0f)
            {
                lockRemainsDuration -= Time.deltaTime;
                if (lockRemainsDuration <= 0f)
                    lockRemainsDuration = 0f;
            }
            else
                lockRemainsDuration = 0f;

            if (uiTextLockRemainsDuration != null)
            {
                uiTextLockRemainsDuration.text = string.Format(lockRemainsDurationFormat, Mathf.CeilToInt(lockRemainsDuration).ToString("N0"));
                uiTextLockRemainsDuration.gameObject.SetActive(lockRemainsDuration > 0);
            }
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UICharacterItem - Update UI");
            if (!IsOwningCharacter() || !IsVisible())
                return;

            UpdateShopUIVisibility(false);
            UpdateRefineUIVisibility(false);
            UpdateDealingState(false);
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            if (Level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (EquipmentItem != null)
            {
                if (!string.IsNullOrEmpty(EquipPosition))
                    onSetEquippedData.Invoke();
                else
                    onSetUnEquippedData.Invoke();
            }
            else
                onSetUnEquippableData.Invoke();

            if (uiTextTitle != null)
            {
                var str = string.Format(titleFormat, Item == null ? "Unknow" : Item.title);
                if (!dontAppendRefineLevelToTitle && EquipmentItem != null)
                    str += string.Format(titleRefineLevelFormat, (Level - 1).ToString("N0"));
                uiTextTitle.text = str;
            }

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Item == null ? "N/A" : Item.description);

            if (uiTextLevel != null)
            {
                if (EquipmentItem != null)
                {
                    if (showLevelAsDefault)
                        uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"));
                    else
                        uiTextLevel.text = string.Format(refineLevelFormat, (Level - 1).ToString("N0"));
                }
                else if (PetItem != null)
                {
                    uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"));
                }
                uiTextLevel.gameObject.SetActive(EquipmentItem != null || PetItem != null);
            }

            if (imageIcon != null)
            {
                var iconSprite = Item == null ? null : Item.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextItemType != null)
            {
                switch (Item.itemType)
                {
                    case ItemType.Junk:
                        uiTextItemType.text = string.Format(itemTypeFormat, junkItemType);
                        break;
                    case ItemType.Armor:
                        uiTextItemType.text = string.Format(itemTypeFormat, ArmorItem.ArmorType.title);
                        break;
                    case ItemType.Weapon:
                        uiTextItemType.text = string.Format(itemTypeFormat, WeaponItem.WeaponType.title);
                        break;
                    case ItemType.Shield:
                        uiTextItemType.text = string.Format(itemTypeFormat, shieldItemType);
                        break;
                    case ItemType.Potion:
                        uiTextItemType.text = string.Format(itemTypeFormat, potionItemType);
                        break;
                    case ItemType.Ammo:
                        uiTextItemType.text = string.Format(itemTypeFormat, ammoItemType);
                        break;
                    case ItemType.Building:
                        uiTextItemType.text = string.Format(itemTypeFormat, buildingItemType);
                        break;
                    case ItemType.Pet:
                        uiTextItemType.text = string.Format(itemTypeFormat, petItemType);
                        break;
                }
            }

            if (uiTextSellPrice != null)
                uiTextSellPrice.text = string.Format(sellPriceFormat, Item == null ? "0" : Item.sellPrice.ToString("N0"));

            if (uiTextStack != null)
            {
                var stackString = "";
                if (Item == null)
                    stackString = string.Format(stackFormat, "0", "0");
                else
                    stackString = string.Format(stackFormat, CharacterItem.amount.ToString("N0"), Item.maxStack);
                uiTextStack.text = stackString;
                uiTextStack.gameObject.SetActive(showAmountWhenMaxIsOne || Item.maxStack > 1);
            }

            if (uiTextDurability != null)
            {
                var durabilityString = "";
                if (Item == null)
                    durabilityString = string.Format(durabilityFormat, "0", "0");
                else
                    durabilityString = string.Format(durabilityFormat, CharacterItem.durability.ToString("N0"), Item.maxDurability);
                uiTextDurability.text = durabilityString;
                uiTextDurability.gameObject.SetActive(EquipmentItem != null && Item.maxDurability > 0);
            }

            if (uiTextWeight != null)
                uiTextWeight.text = string.Format(weightFormat, Item == null ? "0" : Item.weight.ToString("N2"));

            if (uiRequirement != null)
            {
                if (EquipmentItem == null || (EquipmentItem.requirement.level == 0 && EquipmentItem.requirement.character == null && EquipmentItem.CacheRequireAttributeAmounts.Count == 0))
                    uiRequirement.Hide();
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = EquipmentItem;
                }
            }

            if (uiStats != null)
            {
                var stats = EquipmentItem.GetIncreaseStats(Level, CharacterItem.GetEquipmentBonusRate());
                if (EquipmentItem == null || stats.IsEmpty())
                    uiStats.Hide();
                else
                {
                    uiStats.Show();
                    uiStats.Data = stats;
                }
            }

            if (uiIncreaseAttributes != null)
            {
                var attributes = EquipmentItem.GetIncreaseAttributes(Level, CharacterItem.GetEquipmentBonusRate());
                if (EquipmentItem == null || attributes == null || attributes.Count == 0)
                    uiIncreaseAttributes.Hide();
                else
                {
                    uiIncreaseAttributes.Show();
                    uiIncreaseAttributes.Data = attributes;
                }
            }

            if (uiIncreaseResistances != null)
            {
                var resistances = EquipmentItem.GetIncreaseResistances(Level, CharacterItem.GetEquipmentBonusRate());
                if (EquipmentItem == null || resistances == null || resistances.Count == 0)
                    uiIncreaseResistances.Hide();
                else
                {
                    uiIncreaseResistances.Show();
                    uiIncreaseResistances.Data = resistances;
                }
            }

            if (uiIncreaseDamageAmounts != null)
            {
                var damageAmounts = EquipmentItem.GetIncreaseDamages(Level, CharacterItem.GetEquipmentBonusRate());
                if (EquipmentItem == null || damageAmounts == null || damageAmounts.Count == 0)
                    uiIncreaseDamageAmounts.Hide();
                else
                {
                    uiIncreaseDamageAmounts.Show();
                    uiIncreaseDamageAmounts.Data = damageAmounts;
                }
            }

            if (uiDamageAmounts != null)
            {
                if (WeaponItem == null)
                    uiDamageAmounts.Hide();
                else
                {
                    uiDamageAmounts.Show();
                    var keyValuePair = WeaponItem.GetDamageAmount(Level, CharacterItem.GetEquipmentBonusRate(), null);
                    uiDamageAmounts.Data = new DamageElementAmountTuple(keyValuePair.Key, keyValuePair.Value);
                }
            }
            
            if (PetItem != null && PetItem.petEntity != null)
            {
                var expTree = GameInstance.Singleton.ExpTree;
                var currentExp = 0;
                var nextLevelExp = 0;
                if (CharacterItem.GetNextLevelExp() > 0)
                {
                    currentExp = CharacterItem.exp;
                    nextLevelExp = CharacterItem.GetNextLevelExp();
                }
                else if (Level - 2 > 0 && Level - 2 < expTree.Length)
                {
                    var maxExp = expTree[Level - 2];
                    currentExp = maxExp;
                    nextLevelExp = maxExp;
                }

                if (uiTextExp != null)
                {
                    uiTextExp.text = string.Format(expFormat, currentExp.ToString("N0"), nextLevelExp.ToString("N0"));
                    uiTextExp.gameObject.SetActive(true);
                }
            }
            else
            {
                if (uiTextExp != null)
                    uiTextExp.gameObject.SetActive(false);
            }

            if (uiNextLevelItem != null)
            {
                if (Level + 1 > Item.MaxLevel)
                    uiNextLevelItem.Hide();
                else
                {
                    uiNextLevelItem.Setup(new CharacterItemTuple(CharacterItem, (short)(Level + 1), EquipPosition), character, indexOfData);
                    uiNextLevelItem.Show();
                }
            }
            UpdateShopUIVisibility(true);
            UpdateRefineUIVisibility(true);
            UpdateDealingState(true);
        }

        private void UpdateShopUIVisibility(bool initData)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
            {
                if (initData || isSellItemDialogAppeared)
                {
                    isSellItemDialogAppeared = false;
                    if (onNpcSellItemDialogDisappear != null)
                        onNpcSellItemDialogDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiNpcDialog != null)
            {
                if (uiGameplay.uiNpcDialog.IsVisible() &&
                    uiGameplay.uiNpcDialog.Data != null &&
                    uiGameplay.uiNpcDialog.Data.type == NpcDialogType.Shop &&
                    string.IsNullOrEmpty(EquipPosition))
                {
                    if (initData || !isSellItemDialogAppeared)
                    {
                        isSellItemDialogAppeared = true;
                        if (onNpcSellItemDialogAppear != null)
                            onNpcSellItemDialogAppear.Invoke();
                    }
                }
                else
                {
                    if (initData || isSellItemDialogAppeared)
                    {
                        isSellItemDialogAppeared = false;
                        if (onNpcSellItemDialogDisappear != null)
                            onNpcSellItemDialogDisappear.Invoke();
                    }
                }
            }
        }

        private void UpdateRefineUIVisibility(bool initData)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
            {
                if (initData || isRefineItemDialogAppeared)
                {
                    isRefineItemDialogAppeared = false;
                    if (onRefineItemDialogDisappear != null)
                        onRefineItemDialogDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiRefineItem != null)
            {
                if (uiGameplay.uiRefineItem.IsVisible() &&
                    Data.characterItem != null &&
                    Data.characterItem.GetEquipmentItem() != null &&
                    string.IsNullOrEmpty(EquipPosition))
                {
                    if (initData || !isRefineItemDialogAppeared)
                    {
                        isRefineItemDialogAppeared = true;
                        if (onRefineItemDialogAppear != null)
                            onRefineItemDialogAppear.Invoke();
                    }
                }
                else
                {
                    if (initData || isRefineItemDialogAppeared)
                    {
                        isRefineItemDialogAppeared = false;
                        if (onRefineItemDialogDisappear != null)
                            onRefineItemDialogDisappear.Invoke();
                    }
                }
            }
        }

        private void UpdateDealingState(bool initData)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
            {
                if (initData || isDealingStateEntered)
                {
                    isDealingStateEntered = false;
                    if (onExitDealingState != null)
                        onExitDealingState.Invoke();
                }
                return;
            }
            // Check visible dealing dialog
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiDealing != null)
            {
                if (uiGameplay.uiDealing.IsVisible() &&
                    uiGameplay.uiDealing.dealingState == DealingState.Dealing &&
                    string.IsNullOrEmpty(EquipPosition))
                {
                    if (initData || !isDealingStateEntered)
                    {
                        isDealingStateEntered = true;
                        if (onEnterDealingState != null)
                            onEnterDealingState.Invoke();
                    }
                }
                else
                {
                    if (initData || isDealingStateEntered)
                    {
                        isDealingStateEntered = false;
                        if (onExitDealingState != null)
                            onExitDealingState.Invoke();
                    }
                }
            }
        }

        public void OnClickEquip()
        {
            // Only unequpped equipment can be equipped
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(EquipPosition))
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestEquipItem((ushort)indexOfData);
        }

        public void OnClickUnEquip()
        {
            // Only equipped equipment can be unequipped
            if (!IsOwningCharacter() || string.IsNullOrEmpty(EquipPosition))
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestUnEquipItem(EquipPosition);
        }

        public void OnClickDrop()
        {
            // Only unequipped equipment can be dropped
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(EquipPosition))
                return;
            
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                if (owningCharacter != null)
                    owningCharacter.RequestDropItem((ushort)indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(dropInputTitle, dropInputDescription, OnDropAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
        }

        private void OnDropAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestDropItem((ushort)indexOfData, (short)amount);
        }

        public void OnClickSell()
        {
            // Only unequipped equipment can be sell
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(EquipPosition))
                return;
            
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                if (owningCharacter != null)
                    owningCharacter.RequestSellItem((ushort)indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(sellInputTitle, sellInputDescription, OnSellItemAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
        }

        private void OnSellItemAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestSellItem((ushort)indexOfData, (short)amount);
        }

        public void OnClickSetDealingItem()
        {
            // Only unequipped equipment can be sell
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(EquipPosition))
                return;
            
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                if (owningCharacter != null)
                    owningCharacter.RequestSetDealingItem((ushort)indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(setDealingInputTitle, setDealingInputDescription, OnSetDealingItemAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
        }

        private void OnSetDealingItemAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestSetDealingItem((ushort)indexOfData, (short)amount);
        }

        public void OnClickSetRefineItem()
        {
            // Only unequipped equipment can refining
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(EquipPosition))
                return;
            
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiRefineItem != null &&
                CharacterItem.GetEquipmentItem() != null &&
                string.IsNullOrEmpty(EquipPosition))
            {
                uiGameplay.uiRefineItem.Data = indexOfData;
                uiGameplay.uiRefineItem.Show();
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
            }
        }
    }
}
