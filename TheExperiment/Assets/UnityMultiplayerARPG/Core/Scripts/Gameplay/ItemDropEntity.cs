﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public sealed class ItemDropEntity : BaseGameEntity
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public DimensionType dimensionType;
        public CharacterItem dropData;
        public HashSet<uint> looters;
        public Transform modelContainer;
        private float dropTime;

        [SerializeField]
        private SyncFieldInt itemDataId = new SyncFieldInt();

        public Item Item
        {
            get
            {
                Item item;
                if (GameInstance.Items.TryGetValue(itemDataId, out item))
                    return item;
                return null;
            }
        }

        public override string Title
        {
            get
            {
                var item = Item;
                return item == null ? "Unknow" : item.title;
            }
            set { }
        }

        public Transform CacheModelContainer
        {
            get
            {
                if (modelContainer == null)
                    modelContainer = GetComponent<Transform>();
                return modelContainer;
            }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = GameInstance.itemDropTag;
            gameObject.layer = GameInstance.itemDropLayer;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            if (IsServer)
            {
                var id = dropData.dataId;
                dropTime = Time.unscaledTime;
                if (!GameInstance.Items.ContainsKey(id))
                    NetworkDestroy();
                itemDataId.Value = id;
                NetworkDestroy(GameInstance.itemAppearDuration);
            }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            itemDataId.sendOptions = SendOptions.ReliableOrdered;
            itemDataId.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            itemDataId.onChange += OnItemDataIdChange;
        }

        private void OnItemDataIdChange(int itemDataId)
        {
            Item item;
            if (GameInstance.Items.TryGetValue(itemDataId, out item) && item.dropModel != null)
            {
                var model = Instantiate(item.dropModel, CacheModelContainer);
                model.gameObject.SetLayerRecursively(GameInstance.itemDropLayer, true);
                model.gameObject.SetActive(true);
                model.RemoveComponentsInChildren<Collider>(false);
                model.transform.localPosition = Vector3.zero;
            }
        }

        public bool IsAbleToLoot(BaseCharacterEntity baseCharacterEntity)
        {
            if (looters == null || 
                looters.Contains(baseCharacterEntity.ObjectId) || 
                Time.unscaledTime - dropTime > GameInstance.itemLootLockDuration)
                return true;
            return false;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            itemDataId.onChange -= OnItemDataIdChange;
        }

        public static ItemDropEntity DropItem(BaseGameEntity dropper, int itemDataId, short level, short amount, IEnumerable<uint> looters)
        {
            var gameInstance = GameInstance.Singleton;
            if (gameInstance.itemDropEntityPrefab == null)
                return null;

            var dropPosition = dropper.CacheTransform.position;
            var dropRotation = Quaternion.identity;
            switch (gameInstance.itemDropEntityPrefab.dimensionType)
            {
                case DimensionType.Dimension3D:
                    // Random drop position around character
                    dropPosition = dropper.CacheTransform.position + new Vector3(Random.Range(-1f, 1f) * gameInstance.dropDistance, 0, Random.Range(-1f, 1f) * gameInstance.dropDistance);
                    // Raycast to find hit floor
                    Vector3? aboveHitPoint = null;
                    Vector3? underHitPoint = null;
                    var raycastLayerMask = gameInstance.GetItemDropGroundDetectionLayerMask();
                    RaycastHit tempHit;
                    if (Physics.Raycast(dropPosition, Vector3.up, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                        aboveHitPoint = tempHit.point;
                    if (Physics.Raycast(dropPosition, Vector3.down, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                        underHitPoint = tempHit.point;
                    // Set drop position to nearest hit point
                    if (aboveHitPoint.HasValue && underHitPoint.HasValue)
                    {
                        if (Vector3.Distance(dropPosition, aboveHitPoint.Value) < Vector3.Distance(dropPosition, underHitPoint.Value))
                            dropPosition = aboveHitPoint.Value;
                        else
                            dropPosition = underHitPoint.Value;
                    }
                    else if (aboveHitPoint.HasValue)
                        dropPosition = aboveHitPoint.Value;
                    else if (underHitPoint.HasValue)
                        dropPosition = underHitPoint.Value;
                    // Random rotation
                    dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                    break;
                case DimensionType.Dimension2D:
                    dropPosition = dropper.CacheTransform.position + new Vector3(Random.Range(-1f, 1f) * gameInstance.dropDistance, Random.Range(-1f, 1f) * gameInstance.dropDistance);
                    break;
            }
            var identity = dropper.Manager.Assets.NetworkSpawn(gameInstance.itemDropEntityPrefab.Identity, dropPosition, dropRotation);
            var itemDropEntity = identity.GetComponent<ItemDropEntity>();
            var dropData = CharacterItem.Create(itemDataId, level, amount);
            itemDropEntity.dropData = dropData;
            itemDropEntity.looters = new HashSet<uint>(looters);
            return itemDropEntity;
        }
    }
}
