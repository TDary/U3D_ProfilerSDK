using System.Collections.Generic;

public class CaseFunRow
{
    //函数hash为主键
    public int _id { get; set; }
    public List<CaseFunRowInfo> frames { get; set; }
}
