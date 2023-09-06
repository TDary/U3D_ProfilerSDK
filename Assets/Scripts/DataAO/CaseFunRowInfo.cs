using System;
[ProtoBuf.ProtoContract]
public class CaseFunRowInfo
{
    [ProtoBuf.ProtoMember(1)]
    public int frame { get; set; }  //frame

    [ProtoBuf.ProtoMember(2)]
    public int total { get; set; }  //total

    //*100
    [ProtoBuf.ProtoMember(3)]
    public int self { get; set; }  //self

    [ProtoBuf.ProtoMember(4)]
    public int calls { get; set; }  //calls

    //*100
    [ProtoBuf.ProtoMember(5)]
    public int gcalloc { get; set; }  //gcalloc

    //*100
    [ProtoBuf.ProtoMember(6)]
    public int timems { get; set; }  //timems

    //*100
    [ProtoBuf.ProtoMember(7)]
    public int selfms { get; set; } //selfms
}
