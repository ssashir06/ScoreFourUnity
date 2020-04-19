using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ScoreFour.Scripts.JsonEntity
{
    [Serializable]
    public class GameRoom
    {
        public string gameRoomId;
        public string name;
        public PlayerLight[] players;
        public string createDate;
    }
}
