using Assets.ScoreFour.Scripts.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameContext.Instance.Context["GameUserId"] = ReadOrConfigureUserInfo(
            "GameUserId", Guid.NewGuid().ToString("D"));
        GameContext.Instance.Context["ClientId"] = Guid.NewGuid().ToString("D");
        GameContext.Instance.Context["PlayerName"] = ReadOrConfigureUserInfo(
            "PlayerName", $"Player {(UInt32)(UnityEngine.Random.value * UInt32.MaxValue)}");
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private string ReadOrConfigureUserInfo(string key, string defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetString(key);
        }
        else
        {
            PlayerPrefs.SetString(key, defaultValue);
            return defaultValue;
        }
    }
}
