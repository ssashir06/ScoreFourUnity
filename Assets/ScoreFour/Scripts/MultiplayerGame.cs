﻿using Assets.ScoreFour.Scripts;
using Assets.ScoreFour.Scripts.JsonEntity;
using Assets.ScoreFour.Scripts.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class MultiplayerGame : MonoBehaviour
{
    public UnityEngine.UI.Text textGuide;
    public GameRoom gameRoom;
    private Guid gameUserId;
    public SceneTransfrer sceneTransfer;
    public GameRule gameRule;
    public DeploymentOrganizer deploymentOrganizer;

    private int counter;
    private int playerNumber;
    private bool ended;
    private bool updating;

    private bool IsMyturn => counter % 2 == this.playerNumber - 1;
    private bool Pooling => !IsMyturn;

    // Start is called before the first frame update
    void Start()
    {

        this.textGuide.text = "Game started";

        this.gameRoom = (GameRoom)GameContext.Instance.Context["GameRoom"];
        this.gameUserId = (Guid)GameContext.Instance.Context["GameUserId"];
        this.ended = false;
        this.counter = 0;
        this.playerNumber = Guid.Parse(this.gameRoom.players[0].gameUserId) == this.gameUserId ? 1
            : Guid.Parse(this.gameRoom.players[1].gameUserId) == this.gameUserId ? 2
            : throw new Exception("Invalid user id");

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
                    movement.gameRoomId = this.gameRoom.gameRoomId;
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
            .Subscribe(playerNumber =>
        {
            if (playerNumber == this.playerNumber)
            {
                this.textGuide.text = "You win.";
                this.ended = true;
            }
            else
            {
                this.textGuide.text = "You lose.";
                this.ended = true;
            }

        });
        StartCoroutine(MovementUpdateLoopAsync().ToCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update");
        if (this.gameRoom == null)
        {
            this.sceneTransfer.GoBackToMenu();
            return;
        }

        if (gameRule.GameOver || ended || updating)
        {
            this.deploymentOrganizer.SetActive(false);
        }
        else
        {
            if (IsMyturn)
            {
                Debug.Log("Deploy = true");
                this.deploymentOrganizer.SetActive(true);
                this.textGuide.text = "Your turn.";
            }
            else
            {
                Debug.Log("Deploy = false");
                this.deploymentOrganizer.SetActive(false);
                this.textGuide.text = "Waiting for response..";
            }
        }
    }

    async UniTask MovementUpdateLoopAsync()
    {
        while (true)
        {
            if (gameRule.GameOver || ended)
            {
                break;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            await UpdateGameAsync();
        }

    }

    private async UniTask UpdateGameAsync()
    {

        if (await this.IsGameEndedAsync())
        {
            this.textGuide.text = $"Game is ended.";
            this.ended = true;
            return;
        }

        if (this.Pooling && !this.ended)
        {
            var movement = await this.GetMovementAsync(this.counter);
            if (movement != null)
            {
                Debug.Log($"Remote player is deploying: {movement.x}-{movement.y}");

                var success = this.deploymentOrganizer.TryDeploy(movement.x, movement.y);
                if (!success)
                {
                    this.textGuide.text = "An error is occured";
                    this.ended = true;
                    this.gameRule.CanDeploy = false;
                    return;
                }
            }
        }

        this.gameRule.CanDeploy = !this.gameRule.GameOver && !this.Pooling;

    }

    private async UniTask ReportMovementAsync(Movement movement)
    {
        counter++;
        await UniTask.Yield();

        var json = JsonUtility.ToJson(movement);
        var request = UnityWebRequest.Put(new Uri(
            new Uri(Settings.ServerUrl), $"/GameManager/Movement"
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

    }

    private async Task<bool> IsGameEndedAsync()
    {
        var output = await UnityWebRequest.Get(new Uri(
            new Uri(Settings.ServerUrl),
            $"/GameManager/IsEnded?gameRoomId={this.gameRoom.gameRoomId}"
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
                    var ended = bool.Parse(json);
                    return ended;
                }
        }
    }

    private async Task<Movement> GetMovementAsync(int counter)
    {
        var output = await UnityWebRequest.Get(new Uri(
            new Uri(Settings.ServerUrl),
            $"/GameManager/Movement?gameRoomId={this.gameRoom.gameRoomId}&counter={counter}"))
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
}
