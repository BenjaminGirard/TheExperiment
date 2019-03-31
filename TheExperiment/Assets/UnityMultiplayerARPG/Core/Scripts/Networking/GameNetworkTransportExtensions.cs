﻿using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static partial class GameNetworkTransportExtensions
    {
        private static void Send(TransportHandler transportHandler, long? connectionId, ushort msgType, INetSerializable message)
        {
            if (!connectionId.HasValue)
                transportHandler.ClientSendPacket(SendOptions.ReliableOrdered, msgType, message.Serialize);
            else
                transportHandler.ServerSendPacket(connectionId.Value, SendOptions.ReliableOrdered, msgType, message.Serialize);
        }

        public static void SendEnterChat(this TransportHandler transportHandler, long? connectionId, ushort msgType, ChatChannel channel, string message, string senderName, string receiverName, int channelId)
        {
            var netMessage = new ChatMessage();
            netMessage.channel = channel;
            netMessage.message = message;
            netMessage.sender = senderName;
            netMessage.receiver = receiverName;
            netMessage.channelId = channelId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendAddSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, string characterName, int dataId, short level)
        {
            var netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Add;
            netMessage.id = id;
            netMessage.CharacterId = characterId;
            netMessage.data = new SocialCharacterData()
            {
                id = characterId,
                characterName = characterName,
                dataId = dataId,
                level = level,
            };
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendUpdateSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            var netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Update;
            netMessage.id = id;
            netMessage.CharacterId = characterId;
            netMessage.isOnline = isOnline;
            netMessage.data = new SocialCharacterData()
            {
                id = characterId,
                characterName = characterName,
                dataId = dataId,
                level = level,
                currentHp = currentHp,
                maxHp = maxHp,
                currentMp = currentMp,
                maxMp = maxMp,
            };
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendRemoveSocialMember(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var netMessage = new UpdateSocialMemberMessage();
            netMessage.type = UpdateSocialMemberMessage.UpdateType.Remove;
            netMessage.id = id;
            netMessage.CharacterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendCreateParty(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem, string characterId)
        {
            var netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Create;
            netMessage.id = id;
            netMessage.shareExp = shareExp;
            netMessage.shareItem = shareItem;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendChangePartyLeader(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.ChangeLeader;
            netMessage.id = id;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendPartySetting(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, bool shareExp, bool shareItem)
        {
            var netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Setting;
            netMessage.id = id;
            netMessage.shareExp = shareExp;
            netMessage.shareItem = shareItem;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendPartyTerminate(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            var netMessage = new UpdatePartyMessage();
            netMessage.type = UpdatePartyMessage.UpdateType.Terminate;
            netMessage.id = id;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendCreateGuild(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string guildName, string characterId)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.Create;
            netMessage.id = id;
            netMessage.guildName = guildName;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendChangeGuildLeader(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.ChangeLeader;
            netMessage.id = id;
            netMessage.characterId = characterId;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMessage(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string message)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMessage;
            netMessage.id = id;
            netMessage.guildMessage = message;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildRole(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildRole;
            netMessage.id = id;
            netMessage.guildRole = guildRole;
            netMessage.roleName = roleName;
            netMessage.canInvite = canInvite;
            netMessage.canKick = canKick;
            netMessage.shareExpPercentage = shareExpPercentage;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendSetGuildMemberRole(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, string characterId, byte guildRole)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetGuildMemberRole;
            netMessage.id = id;
            netMessage.characterId = characterId;
            netMessage.guildRole = guildRole;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendGuildTerminate(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.Terminate;
            netMessage.id = id;
            Send(transportHandler, connectionId, msgType, netMessage);
        }

        public static void SendGuildLevelExpSkillPoint(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, short level, int exp, short skillPoint)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.LevelExpSkillPoint;
            netMessage.id = id;
            netMessage.level = level;
            netMessage.exp = exp;
            netMessage.skillPoint = skillPoint;
            Send(transportHandler, connectionId, msgType, netMessage);
        }
        
        public static void SendSetGuildSkillLevel(this TransportHandler transportHandler, long? connectionId, ushort msgType, int id, int dataId, short level)
        {
            var netMessage = new UpdateGuildMessage();
            netMessage.type = UpdateGuildMessage.UpdateType.SetSkillLevel;
            netMessage.id = id;
            netMessage.dataId = dataId;
            netMessage.level = level;
            Send(transportHandler, connectionId, msgType, netMessage);
        }
    }
}
