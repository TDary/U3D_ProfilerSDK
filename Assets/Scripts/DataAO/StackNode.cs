public class StackNode
{
    public TimeLineFunData timefundata { get; set; }
    public EvenetNode eventData { get; set; }

    public StackNode(TimeLineFunData subRenderData, EvenetNode eventData)
    {
        this.timefundata = subRenderData;
        this.eventData = eventData;
    }
}
