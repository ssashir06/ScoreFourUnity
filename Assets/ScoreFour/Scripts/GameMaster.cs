using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public UnityEngine.UI.Text textGuide;
    private int turnedPlayer = 1;
    private bool gameOver = false;
    private string guide = "";
    private int[,,] matrix;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        textGuide.text = $"Player {turnedPlayer} {guide}";
    }

    public void Initialize()
    {
        guide = "Please start your game.";
        turnedPlayer = 1;
        gameOver = false;
        matrix = new int[4, 4, 4];
    }

    public bool TryMove(int x, int y)
    {
        if (gameOver)
        {
            return false;
        }

        var line = new[] {
            matrix[x - 1, y - 1, 0],
            matrix[x - 1, y - 1, 1],
            matrix[x - 1, y - 1, 2],
            matrix[x - 1, y - 1, 3],
        };
        var height = 4 - line.Count(v => v == 0);
        if (height == 4)
        {
            guide = "Something is wrong.";
            return false;
        }
        matrix[x - 1, y - 1, height] = turnedPlayer;

        var winner = GetWinnerPlayer();

        if (winner != null)
        {
            guide = "Game.";
            gameOver = true;
            return true;
        }

        guide = "Next, your turn.";
        turnedPlayer = turnedPlayer == 1 ? 2 : 1;
        return true;
    }

    private int? GetWinnerPlayer()
    {
        for (int directionNumber = 1; directionNumber < 8; directionNumber++)
        {
            var xChecks = (directionNumber & 1) == 0;
            var yChecks = (directionNumber & 2) == 0;
            var zChecks = (directionNumber & 4) == 0;
            Func<int, int, int, string> fnKeyGenerator = (x, y, z) => {
                var sb = new StringBuilder();
                if (xChecks)
                {
                    sb.Append($"{x}");
                }
                if (yChecks)
                {
                    sb.Append($"{y}");
                }
                if (zChecks)
                {
                    sb.Append($"{z}");
                }
                return sb.ToString();
            };
            var positions =
                from x in xChecks ? new[] { 0, 1, 2, 3 } : new[] { 0 }
                from y in yChecks ? new[] { 0, 1, 2, 3 } : new[] { 0 }
                from z in zChecks ? new[] { 0, 1, 2, 3 } : new[] { 0 }
                select new { x, y, z }
                ;
            foreach (var xyz in positions)
            {
                var count = new[] { 0, 0, 0 };
                var x = xyz.x;
                var y = xyz.y;
                var z = xyz.z;

                for (int i = 0; i < 4; i++)
                {
                    var player = matrix[x, y, z];
                    count[player]++;
                    x += xChecks ? 0 : 1;
                    y += yChecks ? 0 : 1;
                    z += zChecks ? 0 : 1;
                }
                foreach (var player in new[] { 1, 2 })
                {
                    if (count[player] == 4)
                    {
                        return player;
                    }
                }
            }
        }
        return null;
    }

    public int TurnedPlayer => turnedPlayer;
    public bool GameOver => gameOver;
}
