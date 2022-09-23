using System.Collections.Generic;
public class SubData
{
    public int name { get; set; }
    //值数组
    //*100
    public int total { get; set; }

    //*100
    public int self { get; set; }

    public int calls { get; set; }

    //*100
    public int gcalloc { get; set; }

    //*100
    public int timems { get; set; }

    //*100
    public int selfms { get; set; }

    public List<SubData> children { get; set; }
}