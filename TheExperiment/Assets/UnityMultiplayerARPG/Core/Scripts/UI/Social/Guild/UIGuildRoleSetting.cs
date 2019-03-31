﻿using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildRoleSetting : UIBase
    {
        [Header("UI Elements")]
        public InputFieldWrapper inputFieldRoleName;
        public Toggle toggleCanInvite;
        public Toggle toggleCanKick;
        public InputFieldWrapper inputFieldShareExpPercentage;

        private byte guildRole;

        public void Show(byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            base.Show();

            this.guildRole = guildRole;
            if (inputFieldRoleName != null)
            {
                inputFieldRoleName.unityInputField.contentType = InputField.ContentType.Standard;
                inputFieldRoleName.text = roleName;
            }
            if (toggleCanInvite != null)
                toggleCanInvite.isOn = canInvite;
            if (toggleCanKick != null)
                toggleCanKick.isOn = canKick;
            if (inputFieldShareExpPercentage != null)
            {
                inputFieldShareExpPercentage.unityInputField.contentType = InputField.ContentType.IntegerNumber;
                inputFieldShareExpPercentage.text = shareExpPercentage.ToString("N0");
            }
        }

        public void OnClickSetting()
        {
            byte shareExpPercentage;
            if (inputFieldRoleName == null ||
                string.IsNullOrEmpty(inputFieldRoleName.text))
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Warning", "Role name must not empty");
                return;
            }
            if (inputFieldShareExpPercentage == null || 
                !byte.TryParse(inputFieldShareExpPercentage.text, out shareExpPercentage))
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Warning", "Share exp percentage must be number");
                return;
            }
            BasePlayerCharacterController.OwningCharacter.RequestSetGuildRole(
                guildRole,
                inputFieldRoleName.text,
                toggleCanInvite != null && toggleCanInvite.isOn,
                toggleCanKick != null && toggleCanKick.isOn,
                shareExpPercentage);
            Hide();
        }
    }
}
