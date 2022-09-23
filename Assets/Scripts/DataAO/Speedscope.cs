using System.Collections.Generic;

public class Speedscope
{
    public string schema = "https://www.speedscope.app/file-format-schema.json";
    public string case_uuid { get; set; }
    public string threadname { get; set; }
    public int frame_id { get; set; }
    public int activeProfileIndex { get; set; }
    public Shared shared { get; set; }
    public List<ProfileNode> profiles { get; set; }

    public Speedscope(int activeProfileIndex, string uuid, string threadname, int frame_id)
    {
        this.case_uuid = uuid;
        this.frame_id = frame_id;
        this.threadname = threadname;
        this.activeProfileIndex = activeProfileIndex;
        this.shared = new Shared();
        this.profiles = new List<ProfileNode>();
    }
}
