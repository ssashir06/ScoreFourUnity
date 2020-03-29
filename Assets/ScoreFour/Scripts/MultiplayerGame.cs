using Assets.ScoreFour.Scripts.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerGame : MonoBehaviour
{
    public UnityEngine.UI.Text textGuide;
    public MultiplayerState multiPlayerState;
    public SceneTransfrer sceneTransfer;
    public GameRule gameRule;
    public DeploymentOrganizer deploymentOrganizer;

    private DateTimeOffset lastApiCalled = DateTimeOffset.Now;
    private bool pooling = false;
    private bool ended = false;
    private int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (this.runInEditMode)
        {
            return;
        }
        this.multiPlayerState = GameContext.Instance.Context["MultiPlayerState"] as MultiplayerState;
        this.gameRule.AfterMoved += GameRule_AfterMoved;
        this.gameRule.AfterGameOver += GameRule_AfterGameOver;
        this.pooling = this.multiPlayerState.PlayerNumber != 1;
    }

    private void GameRule_AfterGameOver(int winnerPlayerNumber)
    {
        if (winnerPlayerNumber == this.multiPlayerState.PlayerNumber)
        {
            this.textGuide.text = "You win.";
            this.pooling = false;
        }
        else
        {
            this.textGuide.text = "You lose.";
            this.pooling = false;
        }
    }

    private void GameRule_AfterMoved(int playerNumber)
    {
        counter++;
        if (playerNumber == this.multiPlayerState.PlayerNumber)
        {
            this.pooling = true;
        }
        else
        {
            this.pooling = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.multiPlayerState == null)
        {
            this.sceneTransfer.GoBackToMenu();
            return;
        }
        if (this.gameRule.GameOver || this.ended)
        {
            return;
        }


        if (DateTimeOffset.Now - this.lastApiCalled > TimeSpan.FromSeconds(1))
        {
            if (this.IsGameEnded(out var reason))
            {
                this.textGuide.text = $"Game is ended. (reason)";
                this.ended = true;
            }
        }

        if (this.pooling)
        {
            this.textGuide.text = $"Waiting for another player";
            if (DateTimeOffset.Now - this.lastApiCalled > TimeSpan.FromSeconds(1))
            {
                if (this.TryGetEnemyMovement(this.counter, out var result))
                {
                    Debug.Log($"Remote player is deploying: {result.X}-{result.Y}");

                    this.pooling = false;
                    var success = this.deploymentOrganizer.TryDeploy(result.X, result.Y);
                    if (!success)
                    {
                        this.textGuide.text = "Error is occured";
                        this.ended = true;
                        this.gameRule.CanDeploy = false;
                        return;
                    }
                }
            }
        }
        else
        {
            this.textGuide.text = $"Your turn";
        }

        this.gameRule.CanDeploy = !this.gameRule.GameOver && !this.pooling;
        
    }

    private void ReportExit()
    {
        this.lastApiCalled = DateTimeOffset.Now;
        // TODO
    }

    private void ReportMovement(int counter, Movement movement)
    {
        this.lastApiCalled = DateTimeOffset.Now;
        // TODO
    }

    private bool IsGameEnded(out string reason)
    {
        this.lastApiCalled = DateTimeOffset.Now;
        // TODO
        reason = null;
        return false;
    }

    private bool TryGetEnemyMovement(int counter, out Movement movement)
    {
        this.lastApiCalled = DateTimeOffset.Now;
        // TODO
        if (UnityEngine.Random.value > 0.8)
        {
            movement = new Movement
            {
                PlayerNumber = this.multiPlayerState.PlayerNumber == 1 ? 2 : 1,
                X = 1,
                Y = 1,
            };
            return true;
        }
        else
        {
            movement = null;
            return false;
        }
    }

    public class Movement
    {
        public int PlayerNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
