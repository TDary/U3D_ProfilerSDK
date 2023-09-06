using System;
using System.Collections.Generic;

[ProtoBuf.ProtoContract]
public class CaseFunRow
{
    //函数hash为主键
    [ProtoBuf.ProtoMember(1)]
    public int _id { get; set; }

    [ProtoBuf.ProtoMember(2)]
    public List<CaseFunRowInfo> frames { get; set; }
}
