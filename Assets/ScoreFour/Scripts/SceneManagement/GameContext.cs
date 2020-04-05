using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ScoreFour.Scripts.SceneManagement
{
    public class GameContext
    {
        private static Lazy<GameContext> lazySceneManager = new Lazy<GameContext>(() => new GameContext());
        private GameContext() { }
        public static GameContext Instance => lazySceneManager.Value;
        public Dictionary<string, object> Context { get; } = new Dictionary<string, object>();
    }
}
