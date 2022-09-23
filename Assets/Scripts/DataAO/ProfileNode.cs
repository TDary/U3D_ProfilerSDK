using System.Collections.Generic;
public class ProfileNode
{
    public string type { get; set; }
    public string name { get; set; }
    public string unit { get; set; }
    public decimal startValue { get; set; }
    public decimal endValue { get; set; }
    public List<EvenetNode> events { get; set; }

    public ProfileNode()
    {
        this.type = "evented";
        this.unit = "milliseconds";
        this.events = new List<EvenetNode>();
    }
}
