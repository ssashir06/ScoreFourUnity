using Assets.ScoreFour.Scripts;
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

public class MultiplayerMatching : MonoBehaviour
{
    public SceneTransfrer sceneTransfrer;
    public UnityEngine.UI.InputField inputFieldUserName;
    public UnityEngine.UI.Button buttonStart;
    public UnityEngine.UI.Text textMessage;
    private bool matching = false;
    private bool started = false;
    private string playerNameFixed;
    private Guid gameUserId;

    // Start is called before the first frame update
    void Start()
    {
        gameUserId = Guid.NewGuid();
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            inputFieldUserName.enabled = false;
            buttonStart.enabled = false;
        }
        else
        {
            inputFieldUserName.enabled = true;
            buttonStart.enabled = true;
        }
    }

    public async void StartConnection()
    {
        if (started
            || string.IsNullOrWhiteSpace(inputFieldUserName.text))
        {
            return;
        }
        playerNameFixed = inputFieldUserName.text;

        textMessage.text = "Adding user info..";
        started = true;
        await RegisterAsync();
        textMessage.text = "Finding game room ...";
        while (true)
        {
            if (await TryMatchAsync())
            {
                break;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        this.sceneTransfrer.StartMultiplayerGame();

    }

    public void CancelConnection()
    {
        started = false;
        textMessage.text = "";
    }

    private async UniTask RegisterAsync()
    {
        var json = JsonUtility.ToJson(new Player
        {
            gameUserId = gameUserId.ToString("D"),
            name = playerNameFixed,
        });
        var request = UnityWebRequest.Put(new Uri(
            new Uri(Settings.ServerUrl),
            $"/PlayerMatching/GamePlayer"),
            json);
        request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
        var output = await request.SendWebRequest();

        if (output.isHttpError)
        {
            textMessage.text = "A http error is occured";
            return;
        } else if (output.isNetworkError)
        {
            textMessage.text = "A network error is occured";
            return;
        }
    }

    private async UniTask<bool> TryMatchAsync()
    {

        var output = await UnityWebRequest.Get(new Uri(
            new Uri(Settings.ServerUrl),
            $"/PlayerMatching/GameRoom?gameUserid={gameUserId}"))
            .SendWebRequest();

        if (output.isHttpError)
        {
            textMessage.text = "A http error is occured";
            return false;
        } else if (output.isNetworkError)
        {
            textMessage.text = "A network error is occured";
            return false;
        }

        switch (output.responseCode)
        {
            default:
                textMessage.text = $"Unexpected responce ({output.responseCode})";
                return false;
            case 202:
                // No user found.
                textMessage.text = "Finding game room..";
                return false;
            case 200:
                {
                    var json = output.downloadHandler.text;
                    var gameRoom = JsonUtility.FromJson<GameRoom>(json);
                    GameContext.Instance.Context["GameUserId"] = gameUserId;
                    GameContext.Instance.Context["GameRoom"] = gameRoom;
                    return true;
                }
        }
    }
}
