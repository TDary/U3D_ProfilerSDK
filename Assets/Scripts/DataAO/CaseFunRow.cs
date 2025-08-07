using System;
using System.Collections.Generic;

[ProtoBuf.ProtoContract]
public struct CaseFunRow
{
    //函数名
    [ProtoBuf.ProtoMember(1)]
    public string name { get; set; }

    [ProtoBuf.ProtoMember(2)]
    public string unityobject { get; set; }

    [ProtoBuf.ProtoMember(3)]
    public List<CaseFunRowInfo> frames { get; set; }
}
