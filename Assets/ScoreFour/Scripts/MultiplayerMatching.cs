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

[ExecuteInEditMode]
public class MultiplayerMatching : MonoBehaviour
{
    public SceneTransfrer sceneTransfrer;
    public UnityEngine.UI.InputField inputFieldUserName;
    public UnityEngine.UI.Button buttonStart;
    public UnityEngine.UI.Button buttonCancel;
    public UnityEngine.UI.Text textMessage;
    private volatile bool tryMatching = false;
    private string playerNameFixed;
    private string gameUserId;

    public bool TryMatching { get => tryMatching; set => tryMatching = value; }

    // Start is called before the first frame update
    void Start()
    {
        gameUserId = (string)GameContext.Instance.Context["GameUserId"];
        inputFieldUserName.text = (string)GameContext.Instance.Context["PlayerName"];
    }

    // Update is called once per frame
    void Update()
    {
        if (TryMatching)
        {
            inputFieldUserName.gameObject.SetActive(false);
            buttonStart.gameObject.SetActive(false);
        }
        else
        {
            inputFieldUserName.gameObject.SetActive(true);
            buttonStart.gameObject.SetActive(true);
        }
    }

    public async void StartConnection()
    {
        if (TryMatching || string.IsNullOrWhiteSpace(inputFieldUserName.text))
        {
            return;
        }

        playerNameFixed = inputFieldUserName.text;
        GameContext.Instance.Context["PlayerName"] = playerNameFixed;
        PlayerPrefs.SetString("PlayerName", playerNameFixed);
        PlayerPrefs.Save();

        var lastUserRegister = DateTime.Now - TimeSpan.FromMinutes(100);
        TryMatching = true;
        while (TryMatching)
        {
            if (DateTime.Now - lastUserRegister > TimeSpan.FromSeconds(5))
            {
                await RegisterAsync();
                lastUserRegister = DateTime.Now;
            }
            if (await TryMatchAsync())
            {
                break;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        if (TryMatching)
        {
            this.sceneTransfrer.StartMultiplayerGame();
        }
        else
        {
            this.sceneTransfrer.GoBackToMenu();
        }

    }

    public void CancelConnection()
    {
        this.buttonCancel.enabled = false;
        this.buttonStart.enabled = false;
        this.textMessage.text = "Player matching cancelled";
        if (TryMatching)
        {
            TryMatching = false;
        }
        else
        {
            this.sceneTransfrer.GoBackToMenu();
        }
    }

    private async UniTask RegisterAsync()
    {
        var json = JsonUtility.ToJson(new Player
        {
            gameUserId = gameUserId,
            name = playerNameFixed,
        });

        Exception lastException = null;
        for (int i = 0; i < Settings.NetworkRetry; i++)
        {
            try
            {
                textMessage.text = "Finding game room..";
                var request = UnityWebRequest.Put(new Uri(
                    new Uri(Settings.ServerUrl),
                    $"/api/v1/PlayerMatching/GamePlayer"),
                    json);
                request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                var output = await request.SendWebRequest();

                if (output.isHttpError)
                {
                    textMessage.text = "A http error is occured";
                    throw new Exception("A http error is occured");
                }
                else if (output.isNetworkError)
                {
                    textMessage.text = "A network error is occured";
                    throw new Exception("A network error is occured");
                }
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
        
        Debug.Log($"Error is occured: {lastException?.Message ?? "Unknown"}");
        this.sceneTransfrer.GoBackToMenu();
    }

    private async UniTask<bool> TryMatchAsync()
    {

        textMessage.text = "Finding game room..";
        var output = await UnityWebRequest.Get(new Uri(
            new Uri(Settings.ServerUrl),
            $"/api/v1/PlayerMatching/GamePlayer/{gameUserId}"))
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
