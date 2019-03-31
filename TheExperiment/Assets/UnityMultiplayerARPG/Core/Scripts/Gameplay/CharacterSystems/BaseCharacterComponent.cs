﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterComponent : MonoBehaviour
    {
        private BaseCharacterEntity cacheCharacterEntity;
        public BaseCharacterEntity CacheCharacterEntity
        {
            get
            {
                if (cacheCharacterEntity == null)
                    cacheCharacterEntity = GetComponent<BaseCharacterEntity>();
                return cacheCharacterEntity;
            }
        }
    }
}
