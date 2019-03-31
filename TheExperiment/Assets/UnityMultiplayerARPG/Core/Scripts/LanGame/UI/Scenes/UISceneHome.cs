﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UISceneHome : UIHistory
    {
        public UILanConnection uiLanConnection;
        public UICharacterList uiCharacterList;
        public UICharacterCreate uiCharacterCreate;

        public void OnClickSinglePlayer()
        {
            var networkManager = BaseGameNetworkManager.Singleton as LanRpgNetworkManager;
            networkManager.startType = LanRpgNetworkManager.GameStartType.SinglePlayer;
            Next(uiCharacterList);
        }

        public void OnClickMultiplayer()
        {
            Next(uiLanConnection);
        }

        public void OnClickJoin()
        {
            var networkManager = BaseGameNetworkManager.Singleton as LanRpgNetworkManager;
            networkManager.startType = LanRpgNetworkManager.GameStartType.Client;
            networkManager.networkAddress = uiLanConnection.NetworkAddress;
            Next(uiCharacterList);
        }

        public void OnClickHost()
        {
            var networkManager = BaseGameNetworkManager.Singleton as LanRpgNetworkManager;
            networkManager.startType = LanRpgNetworkManager.GameStartType.Host;
            Next(uiCharacterList);
        }

        public void OnClickExit()
        {
            Application.Quit();
        }
    }
}
