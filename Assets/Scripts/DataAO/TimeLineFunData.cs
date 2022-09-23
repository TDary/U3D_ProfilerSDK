using System.Collections.Generic;

public class TimeLineFunData
{
    public TimeLineFunData() { }

    public void init()
    {
        children.Clear();
    }

    public TimeLineFunData(int name, string funname, float beginTime, float durTime)
    {
        this.name = name;
        this.funname = funname;
        this.beginTime = (decimal)beginTime;
        this.durTime = (decimal)durTime;
        this.children = new List<TimeLineFunData>();
    }
    public int name { get; set; }
    public string funname { get; set; }
    public decimal beginTime { get; set; }
    public decimal durTime { get; set; }
    public List<TimeLineFunData> children { get; set; }
}
