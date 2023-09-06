using System;
[Serializable]
public class CaseFunRowInfo
{
    public int frame { get; set; }  //frame
    public int total { get; set; }  //total

    //*100
    public int self { get; set; }  //self

    public int calls { get; set; }  //calls

    //*100
    public int gcalloc { get; set; }  //gcalloc

    //*100
    public int timems { get; set; }  //timems

    //*100
    public int selfms { get; set; } //selfms
}
