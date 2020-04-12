using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

public class TextNotification : MonoBehaviour
{
    public GameObject textCollection;
    public UnityEngine.UI.Text textNormal;

    // Start is called before the first frame update
    void Start()
    {
        textCollection.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMessage(TimeSpan display, string message)
    {
        StartCoroutine(UniTask.ToCoroutine(
            async () => await this.Display(display, message)));
    }

    private async UniTask Display(TimeSpan display, string message)
    {
        this.textNormal.text = message;
        this.textCollection.SetActive(true);
        await UniTask.Delay(display);
        this.textCollection.SetActive(false);
    }
}
