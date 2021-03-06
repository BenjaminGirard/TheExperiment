﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Profiling;

namespace LiteNetLibManager
{
    [RequireComponent(typeof(LiteNetLibAssets))]
    public class LiteNetLibGameManager : LiteNetLibManager
    {
        public class GameMsgTypes
        {
            public const ushort ClientEnterGame = 0;
            public const ushort ClientReady = 1;
            public const ushort ClientNotReady = 2;
            public const ushort ClientCallFunction = 3;
            public const ushort ServerSpawnSceneObject = 4;
            public const ushort ServerSpawnObject = 5;
            public const ushort ServerDestroyObject = 6;
            public const ushort ServerUpdateSyncField = 7;
            public const ushort ServerCallFunction = 8;
            public const ushort ServerUpdateSyncList = 9;
            public const ushort ServerTime = 10;
            public const ushort ServerSyncBehaviour = 11;
            public const ushort ServerError = 12;
            public const ushort ServerSceneChange = 13;
            public const ushort ClientSendTransform = 14;
            public const ushort Highest = 14;
        }

        public class DestroyObjectReasons
        {
            public const byte RequestedToDestroy = 0;
            public const byte RemovedFromSubscribing = 1;
            public const byte Highest = 1;
        }

        public float updateServerTimeDuration = 5f;
        public bool doNotEnterGameOnConnect;
        public bool doNotDestroyOnSceneChanges;

        internal readonly Dictionary<long, LiteNetLibPlayer> Players = new Dictionary<long, LiteNetLibPlayer>();

        private float lastSendServerTime;
        private string serverSceneName;
        private AsyncOperation loadSceneAsyncOperation;

        public float ServerTimeOffset { get; protected set; }
        public float ServerTime
        {
            get
            {
                if (IsServer)
                    return Time.unscaledTime;
                return Time.unscaledTime + ServerTimeOffset;
            }
        }

        public string ServerSceneName
        {
            get
            {
                if (IsServer)
                    return serverSceneName;
                return string.Empty;
            }
        }

        private LiteNetLibAssets assets;
        public LiteNetLibAssets Assets
        {
            get
            {
                if (assets == null)
                    assets = GetComponent<LiteNetLibAssets>();
                return assets;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            serverSceneName = string.Empty;
            if (doNotDestroyOnSceneChanges)
                DontDestroyOnLoad(gameObject);
        }

        protected override void Update()
        {
            if (IsServer && loadSceneAsyncOperation == null)
            {
                Profiler.BeginSample("LiteNetLibGameManager - Update Spawned Objects");
                foreach (var spawnedObject in Assets.SpawnedObjects.Values)
                {
                    spawnedObject.NetworkUpdate();
                }
                Profiler.EndSample();
                if (Time.unscaledTime - lastSendServerTime > updateServerTimeDuration)
                {
                    SendServerTime();
                    lastSendServerTime = Time.unscaledTime;
                }
            }
            base.Update();
        }

        /// <summary>
        /// Call this function to change gameplay scene at server, then the server will tell clients to change scene
        /// </summary>
        /// <param name="sceneName"></param>
        public void ServerSceneChange(string sceneName)
        {
            if (!IsServer)
                return;
            StartCoroutine(LoadSceneRoutine(sceneName, true));
        }

        /// <summary>
        /// This function will be called to load scene async
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="online"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneRoutine(string sceneName, bool online)
        {
            if (loadSceneAsyncOperation == null)
            {
                // If doNotDestroyOnSceneChanges not TRUE still not destroy this game object
                // But it will be destroyed after scene loaded, if scene is offline scene
                if (!doNotDestroyOnSceneChanges)
                    DontDestroyOnLoad(gameObject);

                if (online)
                {
                    foreach (var player in Players.Values)
                    {
                        player.IsReady = false;
                        player.SubscribingObjects.Clear();
                        player.SpawnedObjects.Clear();
                    }
                    Assets.Clear();
                }

                if (LogDev) Debug.Log("[LiteNetLibGameManager] Loading Scene: " + sceneName + " is online: " + online);
                if (Assets.onLoadSceneStart != null)
                    Assets.onLoadSceneStart.Invoke(sceneName, online, 0f);

                yield return null;
                loadSceneAsyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (loadSceneAsyncOperation != null && !loadSceneAsyncOperation.isDone)
                {
                    if (Assets.onLoadSceneProgress != null)
                        Assets.onLoadSceneProgress.Invoke(sceneName, online, loadSceneAsyncOperation.progress);
                    yield return null;
                }
                loadSceneAsyncOperation = null;
                yield return null;

                if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " is online: " + online);
                if (Assets.onLoadSceneFinish != null)
                    Assets.onLoadSceneFinish.Invoke(sceneName, online, 1f);
                yield return null;

                if (online)
                {
                    Assets.Initialize();
                    if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " -> Assets.Initialize()");
                    yield return null;
                    if (IsClient)
                    {
                        OnClientOnlineSceneLoaded();
                        if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " -> OnClientOnlineSceneLoaded()");
                    }
                    yield return null;
                    if (IsServer)
                    {
                        serverSceneName = sceneName;
                        Assets.SpawnSceneObjects();
                        if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " -> Assets.SpawnSceneObjects()");
                        OnServerOnlineSceneLoaded();
                        if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " -> OnServerOnlineSceneLoaded()");
                    }
                    yield return null;
                    if (IsServer)
                    {
                        SendServerSceneChange(sceneName);
                        if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " -> SendServerSceneChange()");
                    }
                    yield return null;
                    if (IsClient)
                    {
                        SendClientReady();
                        if (LogDev) Debug.Log("[LiteNetLibGameManager] Loaded Scene: " + sceneName + " -> SendClientReady()");
                    }
                }
                else if (!doNotDestroyOnSceneChanges)
                {
                    // Destroy manager's game object if loaded scene is not online scene
                    Destroy(gameObject);
                }
            }
        }

        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            RegisterServerMessage(GameMsgTypes.ClientEnterGame, HandleClientEnterGame);
            RegisterServerMessage(GameMsgTypes.ClientReady, HandleClientReady);
            RegisterServerMessage(GameMsgTypes.ClientNotReady, HandleClientNotReady);
            RegisterServerMessage(GameMsgTypes.ClientCallFunction, HandleClientCallFunction);
            RegisterServerMessage(GameMsgTypes.ClientSendTransform, HandleClientSendTransform);
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterClientMessage(GameMsgTypes.ServerSpawnSceneObject, HandleServerSpawnSceneObject);
            RegisterClientMessage(GameMsgTypes.ServerSpawnObject, HandleServerSpawnObject);
            RegisterClientMessage(GameMsgTypes.ServerDestroyObject, HandleServerDestroyObject);
            RegisterClientMessage(GameMsgTypes.ServerUpdateSyncField, HandleServerUpdateSyncField);
            RegisterClientMessage(GameMsgTypes.ServerCallFunction, HandleServerCallFunction);
            RegisterClientMessage(GameMsgTypes.ServerUpdateSyncList, HandleServerUpdateSyncList);
            RegisterClientMessage(GameMsgTypes.ServerTime, HandleServerTime);
            RegisterClientMessage(GameMsgTypes.ServerSyncBehaviour, HandleServerSyncBehaviour);
            RegisterClientMessage(GameMsgTypes.ServerError, HandleServerError);
            RegisterClientMessage(GameMsgTypes.ServerSceneChange, HandleServerSceneChange);
        }

        public override void OnPeerConnected(long connectionId)
        {
            base.OnPeerConnected(connectionId);
            if (!Players.ContainsKey(connectionId))
            {
                SendServerTime(connectionId);
                Players[connectionId] = new LiteNetLibPlayer(this, connectionId);
            }
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(connectionId, disconnectInfo);
            if (Players.ContainsKey(connectionId))
            {
                var player = Players[connectionId];
                player.ClearSubscribing(false);
                player.DestroyAllObjects();
                Players.Remove(connectionId);
            }
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            if (!doNotEnterGameOnConnect)
                SendClientEnterGame();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (!Assets.onlineScene.IsSet() || Assets.onlineScene.SceneName.Equals(SceneManager.GetActiveScene().name))
            {
                serverSceneName = SceneManager.GetActiveScene().name;
                Assets.Initialize();
                Assets.SpawnSceneObjects();
                OnServerOnlineSceneLoaded();
            }
            else
            {
                serverSceneName = Assets.onlineScene.SceneName;
                StartCoroutine(LoadSceneRoutine(Assets.onlineScene.SceneName, true));
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Players.Clear();
            Assets.Clear();
            if (Assets.offlineScene.IsSet() && !Assets.offlineScene.SceneName.Equals(SceneManager.GetActiveScene().name))
                StartCoroutine(LoadSceneRoutine(Assets.offlineScene.SceneName, false));
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (!IsServer)
            {
                Players.Clear();
                Assets.Clear();
                if (Assets.offlineScene.IsSet() && !Assets.offlineScene.SceneName.Equals(SceneManager.GetActiveScene().name))
                    StartCoroutine(LoadSceneRoutine(Assets.offlineScene.SceneName, false));
            }
        }

        #region Send messages functions
        public void SendClientEnterGame()
        {
            if (!IsClientConnected)
                return;
            ClientSendPacket(SendOptions.ReliableOrdered, GameMsgTypes.ClientEnterGame);
        }

        public void SendClientReady()
        {
            if (!IsClientConnected)
                return;
            ClientSendPacket(SendOptions.ReliableOrdered, GameMsgTypes.ClientReady, SerializeClientReadyExtra);
        }

        public void SendClientNotReady()
        {
            if (!IsClientConnected)
                return;
            ClientSendPacket(SendOptions.ReliableOrdered, GameMsgTypes.ClientNotReady);
        }

        public void SendServerTime()
        {
            if (!IsServer)
                return;
            foreach (var connectionId in ConnectionIds)
            {
                SendServerTime(connectionId);
            }
        }

        public void SendServerTime(long connectionId)
        {
            if (!IsServer)
                return;
            var message = new ServerTimeMessage();
            message.serverTime = ServerTime;
            ServerSendPacket(connectionId, SendOptions.Sequenced, GameMsgTypes.ServerTime, message);
        }

        public void SendServerSpawnSceneObject(LiteNetLibIdentity identity)
        {
            if (!IsServer)
                return;
            foreach (var connectionId in ConnectionIds)
            {
                SendServerSpawnSceneObject(connectionId, identity);
            }
        }

        public void SendServerSpawnSceneObject(long connectionId, LiteNetLibIdentity identity)
        {
            if (!IsServer)
                return;
            LiteNetLibPlayer player = null;
            if (!Players.TryGetValue(connectionId, out player) || !player.IsReady)
                return;
            var message = new ServerSpawnSceneObjectMessage();
            message.objectId = identity.ObjectId;
            message.position = identity.transform.position;
            message.rotation = identity.transform.rotation;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, GameMsgTypes.ServerSpawnSceneObject, message);
        }

        public void SendServerSpawnObject(LiteNetLibIdentity identity)
        {
            if (!IsServer)
                return;
            foreach (var connectionId in ConnectionIds)
            {
                SendServerSpawnObject(connectionId, identity);
            }
        }

        public void SendServerSpawnObject(long connectionId, LiteNetLibIdentity identity)
        {
            if (!IsServer)
                return;
            LiteNetLibPlayer player = null;
            if (!Players.TryGetValue(connectionId, out player) || !player.IsReady)
                return;
            var message = new ServerSpawnObjectMessage();
            message.hashAssetId = identity.HashAssetId;
            message.objectId = identity.ObjectId;
            message.isOwner = identity.ConnectionId == connectionId;
            message.position = identity.transform.position;
            message.rotation = identity.transform.rotation;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, GameMsgTypes.ServerSpawnObject, message);
        }

        public void SendServerSpawnObjectWithData(long connectionId, LiteNetLibIdentity identity)
        {
            if (identity == null)
                return;

            if (Assets.ContainsSceneObject(identity.ObjectId))
                SendServerSpawnSceneObject(connectionId, identity);
            else
                SendServerSpawnObject(connectionId, identity);
            identity.SendInitSyncFields(connectionId);
            identity.SendInitSyncLists(connectionId);
        }

        public void SendServerDestroyObject(uint objectId, byte reasons)
        {
            if (!IsServer)
                return;
            foreach (var connectionId in ConnectionIds)
            {
                SendServerDestroyObject(connectionId, objectId, reasons);
            }
        }

        public void SendServerDestroyObject(long connectionId, uint objectId, byte reasons)
        {
            if (!IsServer)
                return;
            LiteNetLibPlayer player = null;
            if (!Players.TryGetValue(connectionId, out player) || !player.IsReady)
                return;
            var message = new ServerDestroyObjectMessage();
            message.objectId = objectId;
            message.reasons = reasons;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, GameMsgTypes.ServerDestroyObject, message);
        }

        public void SendServerError(bool shouldDisconnect, string errorMessage)
        {
            if (!IsServer)
                return;
            foreach (var connectionId in ConnectionIds)
            {
                SendServerError(connectionId, shouldDisconnect, errorMessage);
            }
        }

        public void SendServerError(long connectionId, bool shouldDisconnect, string errorMessage)
        {
            if (!IsServer)
                return;
            LiteNetLibPlayer player = null;
            if (!Players.TryGetValue(connectionId, out player) || !player.IsReady)
                return;
            var message = new ServerErrorMessage();
            message.shouldDisconnect = shouldDisconnect;
            message.errorMessage = errorMessage;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, GameMsgTypes.ServerDestroyObject, message);
        }

        public void SendServerSceneChange(string sceneName)
        {
            if (!IsServer)
                return;
            foreach (var connectionId in ConnectionIds)
            {
                SendServerSceneChange(connectionId, sceneName);
            }
        }

        public void SendServerSceneChange(long connectionId, string sceneName)
        {
            if (!IsServer)
                return;
            var message = new ServerSceneChangeMessage();
            message.serverSceneName = sceneName;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, GameMsgTypes.ServerSceneChange, message);
        }
        #endregion

        #region Message Handlers
        protected virtual void HandleClientEnterGame(LiteNetLibMessageHandler messageHandler)
        {
            SendServerSceneChange(messageHandler.connectionId, ServerSceneName);
        }

        protected virtual void HandleClientReady(LiteNetLibMessageHandler messageHandler)
        {
            SetPlayerReady(messageHandler.connectionId, messageHandler.reader);
        }

        protected virtual void HandleClientNotReady(LiteNetLibMessageHandler messageHandler)
        {
            SetPlayerNotReady(messageHandler.connectionId, messageHandler.reader);
        }

        protected virtual void HandleClientCallFunction(LiteNetLibMessageHandler messageHandler)
        {
            var reader = messageHandler.reader;
            FunctionReceivers receivers = (FunctionReceivers)reader.GetByte();
            long connectId = 0;
            if (receivers == FunctionReceivers.Target)
                connectId = (long)reader.GetPackedULong();
            var info = LiteNetLibElementInfo.DeserializeInfo(reader);
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(info.objectId, out identity))
            {
                if (receivers == FunctionReceivers.Server)
                    identity.ProcessNetFunction(info, reader, true);
                else
                {
                    var netFunction = identity.ProcessNetFunction(info, reader, false);
                    if (receivers == FunctionReceivers.Target)
                        netFunction.Call(connectId);
                    else
                        netFunction.Call(receivers);
                }
            }
        }

        protected virtual void HandleClientSendTransform(LiteNetLibMessageHandler messageHandler)
        {
            var reader = messageHandler.reader;
            var objectId = reader.GetPackedUInt();
            var behaviourIndex = reader.GetByte();
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(objectId, out identity))
            {
                LiteNetLibTransform netTransform;
                if (identity.TryGetBehaviour(behaviourIndex, out netTransform))
                    netTransform.HandleClientSendTransform(reader);
            }
        }

        protected virtual void HandleServerSpawnSceneObject(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<ServerSpawnSceneObjectMessage>();
            if (!IsServer)
                Assets.NetworkSpawnScene(message.objectId, message.position, message.rotation); LiteNetLibIdentity identity;
            // If it is host, it may hidden so show it
            if (IsServer && Assets.TryGetSpawnedObject(message.objectId, out identity))
                identity.OnServerSubscribingAdded();
        }

        protected virtual void HandleServerSpawnObject(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<ServerSpawnObjectMessage>();
            if (!IsServer)
                Assets.NetworkSpawn(message.hashAssetId, message.position, message.rotation, message.objectId, 0);
            // Setup owner client
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(message.objectId, out identity))
            {
                identity.SetOwnerClient(message.isOwner);
                // If it is host, it may hidden so show it
                if (IsServer)
                    identity.OnServerSubscribingAdded();
            }
        }

        protected virtual void HandleServerDestroyObject(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<ServerDestroyObjectMessage>();
            if (!IsServer)
                Assets.NetworkDestroy(message.objectId, message.reasons);
            // If this is host and reasons is removed from subscribing so hide it, don't destroy it
            LiteNetLibIdentity identity;
            if (IsServer && message.reasons == DestroyObjectReasons.RemovedFromSubscribing && Assets.TryGetSpawnedObject(message.objectId, out identity))
                identity.OnServerSubscribingRemoved();
        }

        protected virtual void HandleServerUpdateSyncField(LiteNetLibMessageHandler messageHandler)
        {
            // Field updated at server, if this is host (client and server) then skip it.
            if (IsServer)
                return;
            var reader = messageHandler.reader;
            var info = LiteNetLibElementInfo.DeserializeInfo(reader);
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(info.objectId, out identity))
                identity.ProcessSyncField(info, reader);
        }

        protected virtual void HandleServerCallFunction(LiteNetLibMessageHandler messageHandler)
        {
            var reader = messageHandler.reader;
            var info = LiteNetLibElementInfo.DeserializeInfo(reader);
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(info.objectId, out identity))
                identity.ProcessNetFunction(info, reader, true);
        }

        protected virtual void HandleServerUpdateSyncList(LiteNetLibMessageHandler messageHandler)
        {
            // List updated at server, if this is host (client and server) then skip it.
            if (IsServer)
                return;
            var reader = messageHandler.reader;
            var info = LiteNetLibElementInfo.DeserializeInfo(reader);
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(info.objectId, out identity))
                identity.ProcessSyncList(info, reader);
        }

        protected virtual void HandleServerTime(LiteNetLibMessageHandler messageHandler)
        {
            // Server time updated at server, if this is host (client and server) then skip it.
            if (IsServer)
                return;
            var message = messageHandler.ReadMessage<ServerTimeMessage>();
            ServerTimeOffset = message.serverTime - Time.unscaledTime;
        }

        protected virtual void HandleServerSyncBehaviour(LiteNetLibMessageHandler messageHandler)
        {
            // Behaviour sync from server, if this is host (client and server) then skip it.
            if (IsServer)
                return;
            var reader = messageHandler.reader;
            var objectId = reader.GetPackedUInt();
            var behaviourIndex = reader.GetByte();
            LiteNetLibIdentity identity;
            if (Assets.TryGetSpawnedObject(objectId, out identity))
                identity.ProcessSyncBehaviour(behaviourIndex, reader);
        }

        protected virtual void HandleServerError(LiteNetLibMessageHandler messageHandler)
        {
            // Error sent from server
            var message = messageHandler.ReadMessage<ServerErrorMessage>();
            OnServerError(message);
        }

        protected virtual void HandleServerSceneChange(LiteNetLibMessageHandler messageHandler)
        {
            // Server scene changes made from server, if this is host (client and server) then skip it.
            if (IsServer)
                return;
            // Scene name sent from server
            var message = messageHandler.ReadMessage<ServerSceneChangeMessage>();
            var serverSceneName = message.serverSceneName;
            if (string.IsNullOrEmpty(serverSceneName) || serverSceneName.Equals(SceneManager.GetActiveScene().name))
            {
                if (!IsServer)
                    Assets.Initialize();
                SendClientReady();
                OnClientOnlineSceneLoaded();
            }
            else
            {
                if (!IsServer)
                    StartCoroutine(LoadSceneRoutine(serverSceneName, true));
            }
        }
        #endregion

        /// <summary>
        /// Overrride this function to send custom data when send client ready message
        /// </summary>
        /// <param name="writer"></param>
        public virtual void SerializeClientReadyExtra(NetDataWriter writer) { }

        /// <summary>
        /// Override this function to read custom data that come with send client ready message
        /// </summary>
        /// <param name="playerIdentity"></param>
        /// <param name="reader"></param>
        public virtual void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, long connectionId, NetDataReader reader) { }

        /// <summary>
        /// Override this function to do anything after online scene loaded at server side
        /// </summary>
        public virtual void OnServerOnlineSceneLoaded() { }

        /// <summary>
        /// Override this function to do anything after online scene loaded at client side
        /// </summary>
        public virtual void OnClientOnlineSceneLoaded() { }

        /// <summary>
        /// Override this function to show error message / disconnect
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnServerError(ServerErrorMessage message)
        {
            if (message.shouldDisconnect && !IsServer)
                StopClient();
        }

        public virtual void SetPlayerReady(long connectionId, NetDataReader reader)
        {
            if (!IsServer)
                return;

            var player = Players[connectionId];
            if (player.IsReady)
                return;

            player.IsReady = true;
            var playerIdentity = SpawnPlayer(connectionId);
            DeserializeClientReadyExtra(playerIdentity, connectionId, reader);
            foreach (var spawnedObject in Assets.SpawnedObjects.Values)
            {
                if (spawnedObject.ConnectionId == player.ConnectionId)
                    continue;

                if (spawnedObject.ShouldAddSubscriber(player))
                    spawnedObject.AddSubscriber(player);
            }
        }

        public virtual void SetPlayerNotReady(long connectionId, NetDataReader reader)
        {
            if (!IsServer)
                return;

            var player = Players[connectionId];
            if (!player.IsReady)
                return;

            player.IsReady = false;
            player.ClearSubscribing(true);
            player.DestroyAllObjects();
        }

        protected LiteNetLibIdentity SpawnPlayer(long connectionId)
        {
            if (Assets.PlayerPrefab == null)
                return null;
            return SpawnPlayer(connectionId, assets.PlayerPrefab);
        }

        protected LiteNetLibIdentity SpawnPlayer(long connectionId, LiteNetLibIdentity prefab)
        {
            if (prefab == null)
                return null;
            return SpawnPlayer(connectionId, prefab.HashAssetId);
        }

        protected LiteNetLibIdentity SpawnPlayer(long connectionId, int hashAssetId)
        {
            var spawnedObject = Assets.NetworkSpawn(hashAssetId, Assets.GetPlayerSpawnPosition(), Quaternion.identity, 0, connectionId);
            if (spawnedObject != null)
                return spawnedObject;
            return null;
        }
    }
}
