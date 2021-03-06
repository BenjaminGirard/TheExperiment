﻿using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterHotkeySelectionManager))]
    public partial class UICharacterHotkeys : UIBase
    {
        public UICharacterHotkeyPair[] uiCharacterHotkeys;

        private Dictionary<string, List<UICharacterHotkey>> cacheUICharacterHotkeys;
        public Dictionary<string, List<UICharacterHotkey>> CacheUICharacterHotkeys
        {
            get
            {
                InitCaches();
                return cacheUICharacterHotkeys;
            }
        }

        private UICharacterHotkeySelectionManager selectionManager;
        public UICharacterHotkeySelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterHotkeySelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        private void InitCaches()
        {
            if (cacheUICharacterHotkeys == null)
            {
                SelectionManager.DeselectSelectedUI();
                SelectionManager.Clear();
                var j = 0;
                cacheUICharacterHotkeys = new Dictionary<string, List<UICharacterHotkey>>();
                for (var i = 0; i < uiCharacterHotkeys.Length; ++i)
                {
                    var uiCharacterHotkey = uiCharacterHotkeys[i];
                    var id = uiCharacterHotkey.hotkeyId;
                    var ui = uiCharacterHotkey.ui;
                    if (!string.IsNullOrEmpty(id) && ui != null)
                    {
                        var characterHotkey = new CharacterHotkey();
                        characterHotkey.hotkeyId = id;
                        characterHotkey.type = HotkeyType.None;
                        characterHotkey.dataId = 0;
                        ui.Setup(characterHotkey, -1);
                        if (!cacheUICharacterHotkeys.ContainsKey(id))
                            cacheUICharacterHotkeys.Add(id, new List<UICharacterHotkey>());
                        cacheUICharacterHotkeys[id].Add(ui);
                        SelectionManager.Add(ui);
                        // Select first UI
                        if (j == 0)
                            ui.OnClickSelect();
                        ++j;
                    }
                }
            }
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        public void UpdateData(IPlayerCharacterData characterData)
        {
            InitCaches();
            var characterHotkeys = characterData.Hotkeys;
            for (var i = 0; i < characterHotkeys.Count; ++i)
            {
                var characterHotkey = characterHotkeys[i];
                List<UICharacterHotkey> uis;
                if (!string.IsNullOrEmpty(characterHotkey.hotkeyId) && CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out uis))
                {
                    foreach (var ui in uis)
                    {
                        ui.Setup(characterHotkey, i);
                        ui.Show();
                    }
                }
            }
        }
    }
}
