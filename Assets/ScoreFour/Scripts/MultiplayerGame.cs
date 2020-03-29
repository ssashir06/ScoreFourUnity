using Assets.ScoreFour.Scripts.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private bool updating = false;
    private bool forceRefresh = false;

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
        this.gameRule.CanDeploy = this.multiPlayerState.PlayerNumber == 1;
        this.textGuide.text = "Game is started";
        this.forceRefresh= true;
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
            this.gameRule.CanDeploy = false;
        }
        else
        {
            this.pooling = false;
            this.gameRule.CanDeploy = true;
        }
    }

    // Update is called once per frame
    async void Update()
    {
        if (this.runInEditMode)
        {
            return;
        }
        if (this.multiPlayerState == null)
        {
            this.sceneTransfer.GoBackToMenu();
            return;
        }
        if (gameRule.GameOver || ended || updating || DateTimeOffset.Now - this.lastApiCalled < TimeSpan.FromSeconds(1))
        {
            return;
        }
        if (this.forceRefresh)
        {
            this.forceRefresh = false;
            return;
        }

        Debug.Log("Update start");
        this.updating = true;
        try
        {
            await UpdateGame();
        }
        finally
        {
            this.updating = false;
        }
        Debug.Log("Update end");
        
    }

    private async Task UpdateGame()
    {

        if (await this.IsGameEnded())
        {
            this.textGuide.text = $"Game is ended.";
            this.ended = true;
            return;
        }

        if (this.pooling)
        {
            this.textGuide.text = $"Waiting for another player";
            var movement = await this.GetEnemyMovement(this.counter);
            if (movement != null)
            {
                Debug.Log($"Remote player is deploying: {movement.X}-{movement.Y}");

                this.pooling = false;
                var success = this.deploymentOrganizer.TryDeploy(movement.X, movement.Y);
                if (!success)
                {
                    this.textGuide.text = "Error is occured";
                    this.ended = true;
                    this.gameRule.CanDeploy = false;
                    return;
                }
            }
        }
        else
        {
            this.textGuide.text = $"Your turn";
        }

        this.gameRule.CanDeploy = !this.gameRule.GameOver && !this.pooling;

    }

    private async void ReportExit()
    {
        // TODO
        await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.value * 10));
        this.lastApiCalled = DateTimeOffset.Now;
    }

    private async Task ReportMovement(int counter, Movement movement)
    {
        // TODO
        await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.value * 1));
        this.lastApiCalled = DateTimeOffset.Now;
    }

    private async Task<bool> IsGameEnded()
    {
        // TODO
        await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.value * 1));
        this.lastApiCalled = DateTimeOffset.Now;
        return false;
    }

    private async Task<Movement> GetEnemyMovement(int counter)
    {
        this.lastApiCalled = DateTimeOffset.Now;
        // TODO
        await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.value * 1));
        if (UnityEngine.Random.value > 0.5)
        {
            return new Movement
            {
                PlayerNumber = this.multiPlayerState.PlayerNumber == 1 ? 2 : 1,
                X = 1,
                Y = 1,
            };
        }
        else
        {
            return null;
        }
    }

    public class Movement
    {
        public int Counter { get; set; }
        public int PlayerNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
