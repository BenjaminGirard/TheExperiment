﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public System.Action onDead;
        public System.Action onRespawn;
        public System.Action onLevelUp;

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack()
        {
            if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
                return;
            lastActionCommandReceivedTime = Time.unscaledTime;

            if (!CanMoveOrDoActions())
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int dataId;
            int animationIndex;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetAttackingData(
                out animActionType,
                out dataId,
                out animationIndex,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);
            
            // Reduce ammo amount
            if (weapon != null)
            {
                var weaponType = weapon.GetWeaponItem().WeaponType;
                if (weaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    var firstEntry = decreaseItems.FirstOrDefault();
                    var characterItem = firstEntry.Key;
                    var item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, dataId, (byte)animationIndex);

            // Start attack routine
            StartCoroutine(AttackRoutine(CacheTransform.position, triggerDuration, totalDuration, weapon, damageInfo, allDamageAmounts));
        }

        private IEnumerator AttackRoutine(
            Vector3 position,
            float triggerDuration,
            float totalDuration,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            yield return new WaitForSecondsRealtime(triggerDuration);
            LaunchDamageEntity(position, weapon, damageInfo, allDamageAmounts, CharacterBuff.Empty, 0);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="position">Target position to apply skill at</param>
        /// <param name="dataId">Skill's data id which will be used</param>
        protected virtual void NetFuncUseSkill(Vector3 position, int dataId)
        {
            if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
                return;
            lastActionCommandReceivedTime = Time.unscaledTime;

            if (!CanMoveOrDoActions())
                return;

            var index = this.IndexOfSkill(dataId);
            if (index < 0)
                return;

            var characterSkill = skills[index];
            if (!characterSkill.CanUse(this))
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int animationIndex;
            SkillAttackType skillAttackType;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetUsingSkillData(
                characterSkill,
                out animActionType,
                out dataId,
                out animationIndex,
                out skillAttackType,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);
            
            if (weapon != null)
            {
                var weaponType = weapon.GetWeaponItem().WeaponType;
                // Reduce ammo amount
                if (skillAttackType != SkillAttackType.None && weaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    var firstEntry = decreaseItems.FirstOrDefault();
                    var characterItem = firstEntry.Key;
                    var item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, dataId, (byte)animationIndex);

            // Start use skill routine
            StartCoroutine(UseSkillRoutine(characterSkill, position, triggerDuration, totalDuration, skillAttackType, weapon, damageInfo, allDamageAmounts));
        }

        private IEnumerator UseSkillRoutine(
            CharacterSkill characterSkill,
            Vector3 position,
            float triggerDuration,
            float totalDuration,
            SkillAttackType skillAttackType,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Update skill usage states
            var newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, characterSkill.dataId);
            newSkillUsage.Use(this, characterSkill.level);
            skillUsages.Add(newSkillUsage);

            yield return new WaitForSecondsRealtime(triggerDuration);
            ApplySkill(characterSkill, position, skillAttackType, weapon, damageInfo, allDamageAmounts);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
        }

        /// <summary>
        /// This will be called on server to use item
        /// </summary>
        /// <param name="dataId"></param>
        protected virtual void NetFuncUseItem(ushort itemIndex)
        {
            if (IsDead())
                return;
            
            if (itemIndex >= nonEquipItems.Count)
                return;

            var characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            var potionItem = characterItem.GetPotionItem();
            if (potionItem != null && this.DecreaseItemsByIndex(itemIndex, 1))
                ApplyPotionBuff(potionItem, characterItem.level);
            var petItem = characterItem.GetPetItem();
            if (petItem != null && this.DecreaseItemsByIndex(itemIndex, 1))
                ApplyItemPetSummon(petItem, characterItem.level, characterItem.exp);
        }

        /// <summary>
        /// This will be called at every clients to play any action animation
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="animActionType"></param>
        protected virtual void NetFuncPlayActionAnimation(byte byteAnimActionType, int dataId, byte index)
        {
            if (IsDead())
                return;
            animActionType = (AnimActionType)byteAnimActionType;
            StartCoroutine(PlayActionAnimationRoutine(animActionType, dataId, index));
        }

        private IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int dataId, int index)
        {
            var playSpeedMultiplier = 1f;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                case AnimActionType.AttackLeftHand:
                    playSpeedMultiplier = CacheAtkSpeed;
                    break;
            }
            if (CharacterModel != null)
                yield return CharacterModel.PlayActionAnimation(animActionType, dataId, index, playSpeedMultiplier);
            this.animActionType = AnimActionType.None;
        }

        /// <summary>
        /// This will be called at server to order character to pickup items
        /// </summary>
        /// <param name="objectId"></param>
        protected virtual void NetFuncPickupItem(PackedUInt objectId)
        {
            if (!CanMoveOrDoActions())
                return;

            ItemDropEntity itemDropEntity = null;
            if (!TryGetEntityByObjectId(objectId, out itemDropEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) > GameInstance.pickUpItemDistance + 5f)
                return;

            if (!itemDropEntity.IsAbleToLoot(this))
            {
                GameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotAbleToLoot);
                return;
            }

            var itemDropData = itemDropEntity.dropData;
            if (!itemDropData.IsValid())
            {
                // Destroy item drop entity without item add because this is not valid
                itemDropEntity.NetworkDestroy();
                return;
            }
            var itemDataId = itemDropData.dataId;
            var level = itemDropData.level;
            var amount = itemDropData.amount;
            if (!IncreasingItemsWillOverwhelming(itemDataId, amount) && this.IncreaseItems(itemDataId, level, amount))
                itemDropEntity.NetworkDestroy();
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        protected virtual void NetFuncDropItem(ushort index, short amount)
        {
            if (!CanMoveOrDoActions() ||
                index >= nonEquipItems.Count)
                return;

            var nonEquipItem = nonEquipItems[index];
            if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
                return;

            var itemDataId = nonEquipItem.dataId;
            var level = nonEquipItem.level;
            if (this.DecreaseItemsByIndex(index, amount))
                ItemDropEntity.DropItem(this, itemDataId, level, amount, new uint[] { ObjectId });
        }

        /// <summary>
        /// This will be called at server to order character to equip equipments
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipPosition"></param>
        protected virtual void NetFuncEquipItem(ushort nonEquipIndex, string equipPosition)
        {
            if (!CanMoveOrDoActions() ||
                nonEquipIndex < 0 ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            var equippingItem = nonEquipItems[nonEquipIndex];

            string reasonWhyCannot;
            HashSet<string> shouldUnequipPositions;
            if (!CanEquipItem(equippingItem, equipPosition, out reasonWhyCannot, out shouldUnequipPositions))
            {
                Debug.LogError("Cannot equip item " + nonEquipIndex + " " + equipPosition + " " + reasonWhyCannot);
                return;
            }

            // Unequip equipped item if exists
            foreach (var shouldUnequipPosition in shouldUnequipPositions)
            {
                NetFuncUnEquipItem(shouldUnequipPosition);
            }
            // Equipping items
            var tempEquipWeapons = EquipWeapons;
            if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
            {
                tempEquipWeapons.rightHand = equippingItem;
                EquipWeapons = tempEquipWeapons;
            }
            else if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
            {
                tempEquipWeapons.leftHand = equippingItem;
                EquipWeapons = tempEquipWeapons;
            }
            else
            {
                equipItems.Add(equippingItem);
                equipItemIndexes.Add(equipPosition, equipItems.Count - 1);
            }
            nonEquipItems.RemoveAt(nonEquipIndex);
        }

        /// <summary>
        /// This will be called at server to order character to unequip equipments
        /// </summary>
        /// <param name="fromEquipPosition"></param>
        protected virtual void NetFuncUnEquipItem(string fromEquipPosition)
        {
            if (!CanMoveOrDoActions())
                return;

            var equippedArmorIndex = -1;
            var tempEquipWeapons = EquipWeapons;
            var unEquipItem = CharacterItem.Empty;
            if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
            {
                unEquipItem = tempEquipWeapons.rightHand;
                tempEquipWeapons.rightHand = CharacterItem.Empty;
                EquipWeapons = tempEquipWeapons;
            }
            else if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
            {
                unEquipItem = tempEquipWeapons.leftHand;
                tempEquipWeapons.leftHand = CharacterItem.Empty;
                EquipWeapons = tempEquipWeapons;
            }
            else if (equipItemIndexes.TryGetValue(fromEquipPosition, out equippedArmorIndex))
            {
                unEquipItem = equipItems[equippedArmorIndex];
                equipItems.RemoveAt(equippedArmorIndex);
                UpdateEquipItemIndexes();
            }
            if (unEquipItem.IsValid())
                nonEquipItems.Add(unEquipItem);
        }

        protected virtual void NetFuncOnDead()
        {
            animActionType = AnimActionType.None;
            if (onDead != null)
                onDead.Invoke();
        }

        protected virtual void NetFuncOnRespawn()
        {
            animActionType = AnimActionType.None;
            if (onRespawn != null)
                onRespawn.Invoke();
        }

        protected virtual void NetFuncOnLevelUp()
        {
            if (GameInstance.levelUpEffect != null && CharacterModel != null)
                CharacterModel.InstantiateEffect(new GameEffect[] { GameInstance.levelUpEffect });
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }

        protected virtual void NetFuncUnSummon(PackedUInt objectId)
        {
            var index = this.IndexOfSummon(objectId);
            if (index < 0)
                return;

            var summon = Summons[index];
            if (summon.type != SummonType.Pet)
                return;

            Summons.RemoveAt(index);
            summon.UnSummon(this);
        }
    }
}
