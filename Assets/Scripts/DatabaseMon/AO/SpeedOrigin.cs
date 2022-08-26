using KProfilerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SpeedOrigin
{
    public string case_uuid { get; set; }
    public string threadname { get; set; }
    public List<KTimeLineData> origindata { get; set; }
}
