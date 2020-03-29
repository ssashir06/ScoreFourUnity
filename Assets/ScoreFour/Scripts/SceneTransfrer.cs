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

    public void StartMultiplayMatching()
    {
        SceneManager.LoadScene("MultiLoading");
    }

    public void StartMultiplayerGame()
    {
        SceneManager.LoadScene("MultiMain");
    }

    public void StartOfflineGame()
    {
        SceneManager.LoadScene("SingleMain");
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

}
