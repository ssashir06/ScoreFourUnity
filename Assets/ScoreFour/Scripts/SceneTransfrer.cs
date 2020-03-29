using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.ScoreFour.Scripts.SceneManagement;
using System;

public class SceneTransfrer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMultiPlayerGame()
    {
        GameContext.Instance.Context["MultiPlayerState"] = new MultiplayerState
        {
            GameRoomId = Guid.NewGuid(),
            PlayerNumber = 2,
        };
        SceneManager.LoadScene("MultiMain");
        LoadAfterTime();
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    private IEnumerator LoadAfterTime()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene("StartMenu");
    }
}
