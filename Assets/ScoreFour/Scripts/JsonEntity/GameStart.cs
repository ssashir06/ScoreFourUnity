﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ScoreFour.Scripts.JsonEntity
{
    [Serializable]
    public class GameStart
    {
        public GameRoom gameRoom;
        public string token;
    }
}
