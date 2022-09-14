using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyze
{
    internal class AnalyzeSDK
    {
        private static AnalyzeSDK _AnalyzeSDK = null;

        string RawPath;
        string CsvPath;
        string FunRowjsonPath;
        string FunRenderRowjsonPath;
        string FunHashPath;
        string FunJsonPath;
        string Index;
        int renderindex;
        string ServerUrl;
        bool shieldSwitch;
        static AnalyzeSDK()
        {
            _AnalyzeSDK = new AnalyzeSDK();
        }

        public static AnalyzeSDK instance()
        {
            return _AnalyzeSDK;
        }


    }
}
