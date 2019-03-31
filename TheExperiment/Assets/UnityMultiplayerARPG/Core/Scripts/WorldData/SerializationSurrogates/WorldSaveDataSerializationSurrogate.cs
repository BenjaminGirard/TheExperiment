﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class WorldSaveDataSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var data = (WorldSaveData)obj;
        info.AddListValue("buildings", data.buildings);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        WorldSaveData data = (WorldSaveData)obj;
        data.buildings = new List<BuildingSaveData>(info.GetListValue<BuildingSaveData>("buildings"));
        obj = data;
        return obj;
    }
}
