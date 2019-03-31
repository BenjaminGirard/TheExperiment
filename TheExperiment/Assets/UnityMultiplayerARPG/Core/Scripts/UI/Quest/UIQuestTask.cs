﻿using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIQuestTask : UISelectionEntry<QuestTaskProgressTuple>
    {
        public QuestTask QuestTask { get { return Data.questTask; } }
        public int Progress { get { return Data.progress; } }

        [Header("Generic Info Format")]
        [Tooltip("Kill Monster Task Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}")]
        public string killMonsterTaskFormat = "Kills {0}: {1}/{2}";
        [Tooltip("Kill Monster Task Complete Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}")]
        public string killMonsterTaskCompleteFormat = "Kills {0}: Completed";
        [Tooltip("Collect Item Task Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}")]
        public string collectItemTaskFormat = "Collects {0}: {1}/{2}";
        [Tooltip("Collect Item Task Complete Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}")]
        public string collectItemTaskCompleteFormat = "Collects {0}: Completed";

        [Header("UI Elements")]
        public TextWrapper uiTextTaskDescription;

        protected override void UpdateData()
        {
            var isComplete = false;
            switch (QuestTask.taskType)
            {
                case QuestTaskType.KillMonster:
                    var monsterCharacterAmount = QuestTask.monsterCharacterAmount;
                    var monsterTitle = monsterCharacterAmount.monster == null ? "Unknow" : monsterCharacterAmount.monster.title;
                    var monsterKillAmount = monsterCharacterAmount.amount;
                    isComplete = Progress >= monsterKillAmount;
                    if (uiTextTaskDescription != null)
                        uiTextTaskDescription.text = string.Format(isComplete ? killMonsterTaskCompleteFormat : killMonsterTaskFormat, monsterTitle, Progress.ToString("N0"), monsterKillAmount.ToString("N0"));
                    break;
                case QuestTaskType.CollectItem:
                    var itemAmount = QuestTask.itemAmount;
                    var itemTitle = itemAmount.item == null ? "Unknow" : itemAmount.item.title;
                    var itemCollectAmount = itemAmount.amount;
                    isComplete = Progress >= itemCollectAmount;
                    if (uiTextTaskDescription != null)
                        uiTextTaskDescription.text = string.Format(isComplete ? collectItemTaskCompleteFormat : collectItemTaskFormat, itemTitle, Progress.ToString("N0"), itemCollectAmount.ToString("N0"));
                    break;
            }
        }
    }
}
