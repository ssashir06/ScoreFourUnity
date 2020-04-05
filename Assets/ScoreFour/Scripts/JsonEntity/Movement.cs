using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ScoreFour.Scripts.JsonEntity
{
    [Serializable]
    public class Movement
    {
        public int x;
        public int y;
        public int counter;
        public int playerNumber;
        public string createDate;
    }
}
