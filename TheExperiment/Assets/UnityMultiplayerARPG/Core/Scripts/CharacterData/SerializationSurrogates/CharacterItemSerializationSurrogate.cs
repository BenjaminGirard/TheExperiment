﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MultiplayerARPG;

public class CharacterItemSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterItem data = (CharacterItem)obj;
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("amount", data.amount);
        info.AddValue("durability", data.durability);
        info.AddValue("exp", data.exp);
        info.AddValue("lockRemainsDuration", data.lockRemainsDuration);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterItem data = (CharacterItem)obj;
        data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.amount = info.GetInt16("amount");
        // Backward compatible
        try
        {
            data.durability = info.GetSingle("durability");
            data.exp = info.GetInt32("exp");
            data.lockRemainsDuration = info.GetSingle("lockRemainsDuration");
        }
        catch { }
        obj = data;
        return obj;
    }
}
