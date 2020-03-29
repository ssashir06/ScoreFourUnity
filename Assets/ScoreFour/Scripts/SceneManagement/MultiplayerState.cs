using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ScoreFour.Scripts.SceneManagement
{
    public class MultiplayerState
    {
        public Guid GameRoomId { get; set; }
        public int PlayerNumber { get; set; }
        public Guid PlayerId { get; set; }
    }
}
