using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using UnityEngine;

[ExecuteInEditMode]
public class SingleplayerGame : MonoBehaviour
{
    public GameRule gameRule;
    public TextNotification textNotification;

    // Start is called before the first frame update
    void Start()
    {
        this.gameRule.OnGameOverAsObservable
            .RepeatUntilDestroy(this)
            .Subscribe(winnerPlayerNumber => {
                this.gameRule.CanDeploy = false;
                this.textNotification.ShowMessage(
                    TimeSpan.FromSeconds(5),
                    $"Player {winnerPlayerNumber} win"
                    );
            });
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
