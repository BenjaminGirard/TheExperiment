﻿using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UISocialGroup : UIBase
    {
        public abstract int GetSocialId();
        public abstract int GetMaxMemberAmount();
        public abstract bool IsLeader(string characterId);
        public abstract bool IsOnline(string characterId);
        public abstract bool CanKick(string characterId);
        public abstract bool OwningCharacterIsLeader();
        public abstract bool OwningCharacterCanKick();
    }

    [RequireComponent(typeof(UISocialCharacterSelectionManager))]
    public abstract class UISocialGroup<T> : UISocialGroup
        where T : UISocialCharacter
    {
        [Header("Display Format")]
        [Tooltip("Member Amount Format => {0} = {current amount}, {1} = {max amount}")]
        public string memberAmountFormat = "Member Amount: {0}/{1}";
        [Tooltip("Member Amount Format => {0} = {current amount}")]
        public string memberAmountNoLimitFormat = "Member Amount: {0}";

        [Header("UI Elements")]
        public T uiMemberDialog;
        public T uiMemberPrefab;
        public Transform uiMemberContainer;
        public TextWrapper textMemberAmount;
        [Tooltip("These objects will be activated when owning character is in social group")]
        public GameObject[] owningCharacterIsInGroupObjects;
        [Tooltip("These objects will be activated when owning character is not in social group")]
        public GameObject[] owningCharacterIsNotInGroupObjects;
        [Tooltip("These objects will be activated when owning character is leader")]
        public GameObject[] owningCharacterIsLeaderObjects;
        [Tooltip("These objects will be activated when owning character is not leader")]
        public GameObject[] owningCharacterIsNotLeaderObjects;
        [Tooltip("These objects will be activated when owning character can kick")]
        public GameObject[] owningCharacterCanKickObjects;
        [Tooltip("These objects will be activated when owning character cannot kick")]
        public GameObject[] owningCharacterCannotKickObjects;

        protected int currentSocialId = 0;
        public int memberAmount { get; protected set; }

        private UIList memberList;
        public UIList MemberList
        {
            get
            {
                if (memberList == null)
                {
                    memberList = gameObject.AddComponent<UIList>();
                    memberList.uiPrefab = uiMemberPrefab.gameObject;
                    memberList.uiContainer = uiMemberContainer;
                }
                return memberList;
            }
        }

        private UISocialCharacterSelectionManager memberSelectionManager;
        public UISocialCharacterSelectionManager MemberSelectionManager
        {
            get
            {
                if (memberSelectionManager == null)
                    memberSelectionManager = GetComponent<UISocialCharacterSelectionManager>();
                memberSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return memberSelectionManager;
            }
        }

        protected virtual void Update()
        {
            if (currentSocialId != GetSocialId())
            {
                currentSocialId = GetSocialId();
                UpdateUIs();

                // Refresh guild info
                if (currentSocialId <= 0)
                    MemberList.HideAll();
            }
        }

        protected virtual void UpdateUIs()
        {
            if (textMemberAmount != null)
            {
                if (GetMaxMemberAmount() > 0)
                    textMemberAmount.text = string.Format(memberAmountFormat, memberAmount.ToString("N0"), GetMaxMemberAmount().ToString("N0"));
                else
                    textMemberAmount.text = string.Format(memberAmountNoLimitFormat, memberAmount.ToString("N0"));
            }
            
            foreach (var obj in owningCharacterIsInGroupObjects)
            {
                if (obj != null)
                    obj.SetActive(currentSocialId > 0);
            }

            foreach (var obj in owningCharacterIsNotInGroupObjects)
            {
                if (obj != null)
                    obj.SetActive(currentSocialId <= 0);
            }

            foreach (var obj in owningCharacterIsLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(OwningCharacterIsLeader());
            }

            foreach (var obj in owningCharacterIsNotLeaderObjects)
            {
                if (obj != null)
                    obj.SetActive(!OwningCharacterIsLeader());
            }

            foreach (var obj in owningCharacterCanKickObjects)
            {
                if (obj != null)
                    obj.SetActive(OwningCharacterCanKick());
            }

            foreach (var obj in owningCharacterCannotKickObjects)
            {
                if (obj != null)
                    obj.SetActive(!OwningCharacterCanKick());
            }
        }

        public override void Show()
        {
            base.Show();
            MemberSelectionManager.eventOnSelect.RemoveListener(OnSelectMember);
            MemberSelectionManager.eventOnSelect.AddListener(OnSelectMember);
            MemberSelectionManager.eventOnDeselect.RemoveListener(OnDeselectMember);
            MemberSelectionManager.eventOnDeselect.AddListener(OnDeselectMember);
            UpdateUIs();
        }

        public override void Hide()
        {
            MemberSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectMember(UISocialCharacter ui)
        {
            if (uiMemberDialog != null)
            {
                uiMemberDialog.selectionManager = MemberSelectionManager;
                uiMemberDialog.Data = ui.Data;
                uiMemberDialog.Show();
            }
        }

        protected void OnDeselectMember(UISocialCharacter ui)
        {
            if (uiMemberDialog != null)
                uiMemberDialog.Hide();
        }
    }
}
