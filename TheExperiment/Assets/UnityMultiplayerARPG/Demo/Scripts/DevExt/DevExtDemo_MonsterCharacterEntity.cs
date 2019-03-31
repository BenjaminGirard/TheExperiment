using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class MonsterCharacterEntity
    {
        [Header("Demo Developer Extension")]
        public bool writeAddonLog;
        [DevExtMethods("Awake")]
        protected void DevExtAwakeDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.Awake()");
        }

        [DevExtMethods("Start")]
        protected void DevExtStartDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.Start()");
        }

        [DevExtMethods("OnEnable")]
        protected void DevExtOnEnableDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.OnEnable()");
        }

        [DevExtMethods("OnDisable")]
        protected void DevExtOnDisableDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.OnDisable()");
        }

        [DevExtMethods("Update")]
        protected void DevExtUpdateDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.Update()");
        }

        [DevExtMethods("LateUpdate")]
        protected void DevExtLateUpdateDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.LateUpdate()");
        }

        [DevExtMethods("FixedUpdate")]
        protected void DevExtFixedUpdateDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.FixedUpdate()");
        }

        [DevExtMethods("OnDestroy")]
        protected void DevExtOnDestroyDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.OnDestroy()");
        }

        [DevExtMethods("OnSetup")]
        protected void DevExtOnSetupDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.OnSetup()");
        }

        [DevExtMethods("SetupNetElements")]
        protected void DevExtSetupNetElementsDemo()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.SetupNetElements()");
        }

        [DevExtMethods("OnNetworkDestroy")]
        protected void DevExtOnNetworkDestroyDemo(byte reasons)
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.OnNetworkDestroy(" + reasons + ")");
        }

        [DevExtMethods("ReceiveDamage")]
        protected void DevExtReceiveDamageDemo(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.ReceiveDamage("
                + attacker.Title + ", " + weapon + ", " + allDamageAmounts.Count + ", " + debuff + ", " + hitEffectsId + ")");
        }

        [DevExtMethods("ReceivedDamage")]
        protected void DevExtReceivedDamageDemo(BaseCharacterEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            if (writeAddonLog) Debug.Log("[" + name + "] MonsterCharacterEntity.ReceivedDamage("
                + attacker.Title + ", " + combatAmountType + ", " + damage + ")");
        }
    }
}
