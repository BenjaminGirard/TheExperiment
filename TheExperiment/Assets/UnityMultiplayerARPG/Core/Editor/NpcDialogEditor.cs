﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(NpcDialog))]
    [CanEditMultipleObjects]
    public class NpcDialogEditor : BaseCustomEditor
    {
        private static NpcDialog cacheNpcDialog;
        protected override void SetFieldCondition()
        {
            if (cacheNpcDialog == null)
                cacheNpcDialog = CreateInstance<NpcDialog>();
            // Normal
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Normal.ToString(), cacheNpcDialog.GetMemberName(a => a.menus));
            // Quest
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Quest.ToString(), cacheNpcDialog.GetMemberName(a => a.quest));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Quest.ToString(), cacheNpcDialog.GetMemberName(a => a.questAcceptedDialog));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Quest.ToString(), cacheNpcDialog.GetMemberName(a => a.questDeclinedDialog));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Quest.ToString(), cacheNpcDialog.GetMemberName(a => a.questAbandonedDialog));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Quest.ToString(), cacheNpcDialog.GetMemberName(a => a.questCompletedDailog));
            // Shop
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.Shop.ToString(), cacheNpcDialog.GetMemberName(a => a.sellItems));
            // Craft Item
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.CraftItem.ToString(), cacheNpcDialog.GetMemberName(a => a.itemCraft));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.CraftItem.ToString(), cacheNpcDialog.GetMemberName(a => a.craftDoneDialog));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.CraftItem.ToString(), cacheNpcDialog.GetMemberName(a => a.craftNotMeetRequirementsDialog));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.CraftItem.ToString(), cacheNpcDialog.GetMemberName(a => a.craftCancelDialog));
            // Save Spawn Point
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.SaveRespawnPoint.ToString(), cacheNpcDialog.GetMemberName(a => a.menus));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.SaveRespawnPoint.ToString(), cacheNpcDialog.GetMemberName(a => a.saveRespawnMap));
            ShowOnEnum(cacheNpcDialog.GetMemberName(a => a.type), NpcDialogType.SaveRespawnPoint.ToString(), cacheNpcDialog.GetMemberName(a => a.saveRespawnPosition));
        }
    }
}
