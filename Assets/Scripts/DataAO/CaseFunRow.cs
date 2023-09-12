using System;
using System.Collections.Generic;

[ProtoBuf.ProtoContract]
public struct CaseFunRow
{
    //函数hash为主键
    [ProtoBuf.ProtoMember(1)]
    public string name { get; set; }

    [ProtoBuf.ProtoMember(2)]
    public List<CaseFunRowInfo> frames { get; set; }
}
