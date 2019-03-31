﻿using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class UpdateSocialMemberMessage : INetSerializable
    {
        public enum UpdateType : byte
        {
            Add,
            Update,
            Remove,
        }
        public UpdateType type;
        public int id;
        public bool isOnline;
        public SocialCharacterData data = new SocialCharacterData();

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            data.id = reader.GetString();
            switch (type)
            {
                case UpdateType.Add:
                case UpdateType.Update:
                    isOnline = reader.GetBool();
                    data.characterName = reader.GetString();
                    data.dataId = reader.GetInt();
                    data.level = reader.GetShort();
                    // Read extra data
                    if (isOnline)
                    {
                        data.currentHp = reader.GetInt();
                        data.maxHp = reader.GetInt();
                        data.currentMp = reader.GetInt();
                        data.maxMp = reader.GetInt();
                    }
                    break;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            writer.Put(data.id);
            switch (type)
            {
                case UpdateType.Add:
                case UpdateType.Update:
                    writer.Put(isOnline);
                    writer.Put(data.characterName);
                    writer.Put(data.dataId);
                    writer.Put(data.level);
                    // Put extra data
                    if (isOnline)
                    {
                        writer.Put(data.currentHp);
                        writer.Put(data.maxHp);
                        writer.Put(data.currentMp);
                        writer.Put(data.maxMp);
                    }
                    break;
            }
        }

        public string CharacterId { get { return data.id; } set { data.id = value; } }
    }
}
