[ProtoBuf.ProtoContract]
public class CaseRenderInfo
{
    [ProtoBuf.ProtoMember(1)]
    public int frame { get; set; }
    [ProtoBuf.ProtoMember(2)]
    public float timems { get; set; }
    [ProtoBuf.ProtoMember(3)]
    public float selfms { get; set; }
}
