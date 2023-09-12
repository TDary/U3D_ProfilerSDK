using System.Collections.Generic;

[ProtoBuf.ProtoContract]
public struct Simple
{
    [ProtoBuf.ProtoMember(1)]
    public string field { get; set; }
    [ProtoBuf.ProtoMember(2)]
    public List<float> values { get; set; }
}
