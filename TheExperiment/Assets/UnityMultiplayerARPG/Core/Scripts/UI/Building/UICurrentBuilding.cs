﻿namespace MultiplayerARPG
{
    public partial class UICurrentBuilding : UIBase
    {
        public void OnClickDestroy()
        {
            var controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.DestroyBuilding();
        }

        public void OnClickDeselect()
        {
            var controller = BasePlayerCharacterController.Singleton;
            if (controller != null)
                controller.DeselectBuilding();
        }
    }
}
