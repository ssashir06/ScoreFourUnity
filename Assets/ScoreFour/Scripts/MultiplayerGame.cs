using Assets.ScoreFour.Scripts;
using Assets.ScoreFour.Scripts.Enums;
using Assets.ScoreFour.Scripts.JsonEntity;
using Assets.ScoreFour.Scripts.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class MultiplayerGame : MonoBehaviour
{
    public GameRoom gameRoom;
    public TextNotification textNotification;
    public SceneTransfrer sceneTransfer;
    public GameRule gameRule;
    public DeploymentOrganizer deploymentOrganizer;
    public UnityEngine.UI.Text textGuide;
    public UnityEngine.UI.Text textPlayer1Name;
    public UnityEngine.UI.Text textPlayer2Name;
    public UnityEngine.UI.Button buttonLeaveButton;
    private Guid gameUserId;

    private int counter;
    private int playerNumber;
    private bool isEnded;

    private bool IsMyturn => counter % 2 == this.playerNumber - 1;
    private bool Pooling => !IsMyturn;

    // Start is called before the first frame update
    void Start()
    {

        this.textGuide.text = "Game started";

        this.gameRoom = (GameRoom)GameContext.Instance.Context["GameRoom"];
        this.gameUserId = Guid.Parse((string)GameContext.Instance.Context["GameUserId"]);
        this.buttonLeaveButton.enabled = true;
        this.isEnded = false;
        this.counter = 0;
        this.playerNumber = Guid.Parse(this.gameRoom.players[0].gameUserId) == this.gameUserId ? 1
            : Guid.Parse(this.gameRoom.players[1].gameUserId) == this.gameUserId ? 2
            : throw new Exception("Invalid user id");

        this.textPlayer1Name.text = this.playerNumber == 1 ? "You" : this.gameRoom.players[0].name;
        this.textPlayer2Name.text = this.playerNumber == 2 ? "You" : this.gameRoom.players[1].name;

        this.gameRule.OnMoveAsObservable
            .RepeatUntilDestroy(this)
            .Subscribe(async tuple =>
            {
                var playerNumber = tuple.Item1;
                var movement = tuple.Item2;
                if (playerNumber == this.playerNumber)
                {
                    if (IsMyturn)
                    {
                        movement.playerNumber = this.playerNumber;
                        await this.ReportMovementAsync(movement);
                    }
                }
                else
                {
                    counter++;
                }
            });
        this.gameRule.OnGameOverAsObservable
            .RepeatUntilDestroy(this)
            .Subscribe(async (winnerPlayerNumber) => await GameOverAsync(winnerPlayerNumber));
        StartCoroutine(MovementUpdateLoopAsync().ToCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameRoom == null)
        {
            this.sceneTransfer.GoBackToMenu();
            return;
        }

        if (gameRule.GameOver || isEnded)
        {
            this.deploymentOrganizer.SetActive(false);
        }
        else
        {
            if (IsMyturn)
            {
                this.deploymentOrganizer.SetActive(true);
                this.textGuide.text = "Your turn.";
            }
            else
            {
                this.deploymentOrganizer.SetActive(false);
                this.textGuide.text = "Waiting for response..";
            }
        }
    }

    public void LeaveAndGoToMenu()
    {
        StartCoroutine(UniTask.ToCoroutine(LeaveAndGoToMenuAsync));
    }

    private async UniTask LeaveAndGoToMenuAsync()
    {
        if (!isEnded)
        {
            this.buttonLeaveButton.enabled = false;

            Exception lastException = null;
            for (var i = 0; i < Settings.NetworkRetry; i++)
            {
                try
                {
                    var json = JsonUtility.ToJson(new
                    {
                        playerNumber,
                    });
                    var request = UnityWebRequest.Put(new Uri(
                        new Uri(Settings.ServerUrl), $"/api/v1/GameManager/{gameRoom.gameRoomId}/Leave"
                        ), json);
                    request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                    var output = await request.SendWebRequest();
                    if (output.isHttpError)
                    {
                        Debug.Log("A http error is occured");
                        throw new Exception("A http error is occured");
                    }
                    else if (output.isNetworkError)
                    {
                        Debug.Log("A network error is occured");
                        throw new Exception("A network error is occured");
                    }

                    sceneTransfer.GoBackToMenu();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
            this.enabled = true;
            this.textGuide.text = $"Error: ({lastException?.Message ?? "Unknown"})";
            this.buttonLeaveButton.enabled = true;
            throw lastException;
        }
        else
        {
            sceneTransfer.GoBackToMenu();
        }

    }

    private async UniTask GameOverAsync(int winnerPlayerNumber)
    {
        if (winnerPlayerNumber == this.playerNumber)
        {
            this.textGuide.text = "You win.";
            this.textNotification.ShowMessage(
                TimeSpan.FromSeconds(5),
                "You win");
            this.isEnded = true;

            await ReportWinner(winnerPlayerNumber);
        }
        else
        {
            this.textGuide.text = "You lose.";
            this.textNotification.ShowMessage(
                TimeSpan.FromSeconds(5),
                "You lose");
            this.isEnded = true;
        }
    }

    private async UniTask ReportWinner(int winnerPlayerNumber)
    {
        Exception lastException = null;
        for (var i = 0; i < Settings.NetworkRetry; i++)
        {
            try
            {
                var json = JsonUtility.ToJson(new
                {
                    playerNumber = winnerPlayerNumber,
                });
                var request = UnityWebRequest.Put(new Uri(
                    new Uri(Settings.ServerUrl), $"/api/v1/GameManager/{gameRoom.gameRoomId}/Winner"
                    ), json);
                request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                var output = await request.SendWebRequest();
                if (output.isHttpError)
                {
                    Debug.Log("A http error is occured");
                    throw new Exception("A http error is occured");
                }
                else if (output.isNetworkError)
                {
                    Debug.Log("A network error is occured");
                    throw new Exception("A network error is occured");
                }
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        this.enabled = true;
        this.textGuide.text = $"Error: ({lastException?.Message ?? "Unknown"})";
        throw lastException;
    }

    private async UniTask MovementUpdateLoopAsync()
    {
        while (true)
        {
            if (gameRule.GameOver || isEnded)
            {
                break;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            await UpdateGameAsync();
        }

    }

    private async UniTask UpdateGameAsync()
    {

        if (this.Pooling && !this.isEnded)
        {
            var movement = await this.GetMovementAsync(this.counter);
            if (movement != null)
            {
                Debug.Log($"Remote player is deploying: {movement.x}-{movement.y}");

                var success = this.deploymentOrganizer.TryDeploy(movement.x, movement.y);
                if (!success)
                {
                    this.textGuide.text = "An error is occured";
                    this.isEnded = true;
                    this.gameRule.CanDeploy = false;
                    return;
                }
            }
        }

        if (await this.IsGameEndedAsync())
        {
            this.textGuide.text = $"Game is ended.";
            this.isEnded = true;
            return;
        }

        this.gameRule.CanDeploy = !this.gameRule.GameOver && !this.Pooling;

    }

    private async UniTask ReportMovementAsync(Movement movement)
    {
        counter++;
        await UniTask.Yield();

        Exception lastException = null;
        for (var i = 0; i < Settings.NetworkRetry; i++)
        {
            try
            {
                var json = JsonUtility.ToJson(movement);
                var request = UnityWebRequest.Put(new Uri(
                    new Uri(Settings.ServerUrl), $"/api/v1/GameManager/{gameRoom.gameRoomId}/Movement"
                    ), json);
                request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                var output = await request.SendWebRequest();
                if (output.isHttpError)
                {
                    Debug.Log("A http error is occured");
                    throw new Exception("A http error is occured");
                }
                else if (output.isNetworkError)
                {
                    Debug.Log("A network error is occured");
                    throw new Exception("A network error is occured");
                }
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        this.enabled = true;
        this.textGuide.text = $"Error: ({lastException?.Message ?? "Unknown"})";
        throw lastException;

    }

    private async Task<bool> IsGameEndedAsync()
    {
        Exception lastException = null;
        for (var i = 0; i < Settings.NetworkRetry; i++)
        {
            try
            {
                var output = await UnityWebRequest.Get(new Uri(
                    new Uri(Settings.ServerUrl),
                    $"/api/v1/GameManager/{this.gameRoom.gameRoomId}/Status"
                    )).SendWebRequest();

                if (output.isHttpError)
                {
                    throw new Exception("A http error is occured");
                }
                else if (output.isNetworkError)
                {
                    throw new Exception("A network error is occured");
                }

                switch (output.responseCode)
                {
                    default:
                        throw new Exception($"Unexpected responce ({output.responseCode})");
                    case 200:
                        {
                            var json = output.downloadHandler.text;
                            var status = (GameRoomStatus)int.Parse(json);
                            return new[]{
                                GameRoomStatus.GameOver,
                                GameRoomStatus.Left,
                                GameRoomStatus.TimedOut,
                            }.Contains(status);
                        }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;

            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        this.enabled = true;
        this.textGuide.text = $"Error: ({lastException?.Message ?? "Unknown"})";
        throw lastException;
    }

    private async Task<Movement> GetMovementAsync(int counter)
    {
        Exception lastException = null;
        for (var i = 0; i < Settings.NetworkRetry; i++)
        {
            try
            {
                var output = await UnityWebRequest.Get(new Uri(
                    new Uri(Settings.ServerUrl),
                    $"/api/v1/GameManager/{this.gameRoom.gameRoomId}/Movement/{counter}"))
                    .SendWebRequest();

                if (output.isNetworkError)
                {
                    throw new Exception("A network error is occured");
                }

                switch (output.responseCode)
                {
                    default:
                        throw new Exception($"Unexpected responce ({output.responseCode})");
                    case 404:
                        // No movement yet
                        return null;
                    case 200:
                        {
                            var json = output.downloadHandler.text;
                            var movement = JsonUtility.FromJson<Movement>(json);
                            return movement;
                        }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;

            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        this.enabled = true;
        this.textGuide.text = $"Error: ({lastException?.Message ?? "Unknown"})";
        throw lastException;
    }
}
