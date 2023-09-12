using System;

[ProtoBuf.ProtoContract]
public struct CaseFlame
{
    [ProtoBuf.ProtoMember(1)]
    public int frame_id { get; set; }

    [ProtoBuf.ProtoMember(2)]
    public SubData flame { get; set; }
}