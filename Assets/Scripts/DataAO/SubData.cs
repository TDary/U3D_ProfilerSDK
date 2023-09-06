using System;
using System.Collections.Generic;
[ProtoBuf.ProtoContract]
public class SubData
{
    [ProtoBuf.ProtoMember(1)]
    public int name { get; set; }
    //值数组
    //*100
    [ProtoBuf.ProtoMember(2)]
    public int total { get; set; }

    //*100
    [ProtoBuf.ProtoMember(3)]
    public int self { get; set; }

    [ProtoBuf.ProtoMember(4)]
    public int calls { get; set; }

    //*100
    [ProtoBuf.ProtoMember(5)]
    public int gcalloc { get; set; }

    //*100
    [ProtoBuf.ProtoMember(6)]
    public int timems { get; set; }

    //*100
    [ProtoBuf.ProtoMember(7)]
    public int selfms { get; set; }

    [ProtoBuf.ProtoMember(8)]
    public List<SubData> children { get; set; }
}