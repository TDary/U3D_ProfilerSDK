using System.Collections.Generic;

[ProtoBuf.ProtoContract]
public class TimeLineFunData
{
    //public void init()
    //{
    //    children.Clear();
    //}
    [ProtoBuf.ProtoMember(1)]
    public string name { get; set; }
    [ProtoBuf.ProtoMember(2)]
    public string funname { get; set; }
    [ProtoBuf.ProtoMember(3)]
    public decimal beginTime { get; set; }
    [ProtoBuf.ProtoMember(4)]
    public decimal durTime { get; set; }
    [ProtoBuf.ProtoMember(5)]
    public List<TimeLineFunData> children { get; set; }
    public TimeLineFunData(string name, string funname, float beginTime, float durTime)
    {
        this.name = name;
        this.funname = funname;
        this.beginTime = (decimal)beginTime;
        this.durTime = (decimal)durTime;
        this.children = new List<TimeLineFunData>();
    }
    public TimeLineFunData() { }
}
