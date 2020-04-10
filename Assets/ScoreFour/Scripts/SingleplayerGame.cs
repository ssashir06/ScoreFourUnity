using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using UnityEngine;

[ExecuteInEditMode]
public class SingleplayerGame : MonoBehaviour
{
    public GameRule gameRule;

    // Start is called before the first frame update
    void Start()
    {
        this.gameRule.OnGameOverAsObservable
            .RepeatUntilDestroy(this)
            .Subscribe(tuple => {
                this.gameRule.CanDeploy = false;
            });
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
