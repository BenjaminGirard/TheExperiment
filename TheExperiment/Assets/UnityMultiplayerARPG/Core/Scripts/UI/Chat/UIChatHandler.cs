﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIChatHandler : UIBase
    {
        public static readonly List<ChatMessage> ChatMessages = new List<ChatMessage>();
        
        public string globalCommand = "/a";
        public string whisperCommand = "/w";
        public string partyCommand = "/p";
        public string guildCommand = "/g";
        public KeyCode enterChatKey = KeyCode.Return;
        public int chatEntrySize = 30;
        public GameObject[] enterChatActiveObjects;
        public InputFieldWrapper uiEnterChatField;
        public UIChatMessage uiChatMessagePrefab;
        public Transform uiChatMessageContainer;
        public ScrollRect scrollRect;
        
        private bool enterChatFieldVisible;

        public string EnterChatMessage
        {
            get { return uiEnterChatField == null ? string.Empty : uiEnterChatField.text; }
            set { if (uiEnterChatField != null) uiEnterChatField.text = value; }
        }

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiChatMessagePrefab.gameObject;
                    cacheList.uiContainer = uiChatMessageContainer;
                }
                return cacheList;
            }
        }

        private void Start()
        {
            CacheList.Generate(ChatMessages, (index, message, ui) =>
            {
                var uiChatMessage = ui.GetComponent<UIChatMessage>();
                uiChatMessage.uiChatHandler = this;
                uiChatMessage.Data = message;
                uiChatMessage.Show();
            });
            StartCoroutine(VerticalScroll(0f));
            
            HideEnterChatField();
            if (uiEnterChatField != null)
            {
                uiEnterChatField.onValueChanged.RemoveListener(OnInputFieldValueChange);
                uiEnterChatField.onValueChanged.AddListener(OnInputFieldValueChange);
            }
        }

        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onClientReceiveChat += OnReceiveChat;
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onClientReceiveChat -= OnReceiveChat;
        }

        private void Update()
        {
            if (Input.GetKeyDown(enterChatKey))
            {
                if (!enterChatFieldVisible)
                    ShowEnterChatField();
                else
                    SendChatMessage();
            }
        }

        public void ToggleEnterChatField()
        {
            if (enterChatFieldVisible)
                HideEnterChatField();
            else
                ShowEnterChatField();
        }

        public void ShowEnterChatField()
        {
            foreach (var enterChatActiveObject in enterChatActiveObjects)
            {
                if (enterChatActiveObject != null)
                    enterChatActiveObject.SetActive(true);
            }
            if (uiEnterChatField != null)
            {
                uiEnterChatField.Select();
                uiEnterChatField.ActivateInputField();
            }
            enterChatFieldVisible = true;
        }

        public void HideEnterChatField()
        {
            foreach (var enterChatActiveObject in enterChatActiveObjects)
            {
                if (enterChatActiveObject != null)
                    enterChatActiveObject.SetActive(false);
            }
            if (uiEnterChatField != null)
                uiEnterChatField.DeactivateInputField();
            enterChatFieldVisible = false;
        }

        public void SendChatMessage()
        {
            if (BasePlayerCharacterController.OwningCharacter == null)
                return;

            var trimText = EnterChatMessage.Trim();
            if (trimText.Length == 0)
                return;

            EnterChatMessage = string.Empty;
            var channel = ChatChannel.Local;
            var message = trimText;
            var sender = BasePlayerCharacterController.OwningCharacter.CharacterName;
            var receiver = string.Empty;
            var splitedText = trimText.Split(' ');
            if (splitedText.Length > 0)
            {
                var cmd = splitedText[0];
                if (cmd == whisperCommand && splitedText.Length > 2)
                {
                    channel = ChatChannel.Whisper;
                    receiver = splitedText[1];
                    message = trimText.Substring(cmd.Length + receiver.Length + 1); // +1 for space
                    EnterChatMessage = trimText.Substring(0, cmd.Length + receiver.Length + 1); // +1 for space
                }
                if ((cmd == globalCommand || cmd == partyCommand || cmd == guildCommand) && splitedText.Length > 1)
                {
                    if (cmd == globalCommand)
                        channel = ChatChannel.Global;
                    if (cmd == partyCommand)
                        channel = ChatChannel.Party;
                    if (cmd == guildCommand)
                        channel = ChatChannel.Guild;
                    message = trimText.Substring(cmd.Length + 1); // +1 for space
                    EnterChatMessage = trimText.Substring(0, cmd.Length + 1); // +1 for space
                }
            }
            BaseGameNetworkManager.Singleton.EnterChat(channel, message, sender, receiver);
            HideEnterChatField();
        }

        private void OnReceiveChat(ChatMessage chatMessage)
        {
            ChatMessages.Add(chatMessage);
            if (ChatMessages.Count > chatEntrySize)
                ChatMessages.RemoveAt(0);
            CacheList.Generate(ChatMessages, (index, message, ui) =>
            {
                var uiChatMessage = ui.GetComponent<UIChatMessage>();
                uiChatMessage.uiChatHandler = this;
                uiChatMessage.Data = message;
                uiChatMessage.Show();
            });
            StartCoroutine(VerticalScroll(0f));
        }

        private void OnInputFieldValueChange(string text)
        {
            if (text.Length > 0 && !enterChatFieldVisible)
                ShowEnterChatField();
        }

        IEnumerator VerticalScroll(float normalize)
        {
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                yield return null;
                scrollRect.verticalScrollbar.value = normalize;
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}
