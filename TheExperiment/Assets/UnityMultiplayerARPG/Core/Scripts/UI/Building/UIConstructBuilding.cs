﻿namespace MultiplayerARPG
{
    public partial class UIConstructBuilding : UIBase
    {
        public void OnClickConfirmBuild()
        {
            var controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.ConfirmBuild();
        }

        public void OnClickCancelBuild()
        {
            var controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.CancelBuild();
        }
    }
}
