using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ScoreFour.Scripts
{
    public class Settings
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public static string ServerUrl = "https://localhost:44339";
#else
        public static string ServerUrl = "https://scorefourserverdev.azurewebsites.net/";
#endif
        public static int NetworkRetry = 5;
    }
}
