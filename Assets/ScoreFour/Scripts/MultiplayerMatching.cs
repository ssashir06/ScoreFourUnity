using Assets.ScoreFour.Scripts.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MultiplayerMatching : MonoBehaviour
{
    public SceneTransfrer sceneTransfrer;
    private volatile bool matching = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    async void Update()
    {
        if (!matching)
        {
            matching = true;
            await Match();
        }
        
    }

    private async Task Match()
    {
        await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.value * 10));

        GameContext.Instance.Context["MultiPlayerState"] = new MultiplayerState
        {
            GameRoomId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            PlayerNumber = 1,
        };

        this.sceneTransfrer.StartMultiplayerGame();
    }
}
