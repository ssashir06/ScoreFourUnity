using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentOrganizer : MonoBehaviour
{
    public GameObject blockerPiecePlayer1;
    public GameObject blockerPiecePlayer2;
    public GameObject blockerPieceCollector;
    public GameRule gameMaster;

    void Start()
    {
        
    }
    void Update()
    {

    }

    public bool TryDeploy(int x, int y)
    {
        var player = gameMaster.TurnedPlayer;

        if (gameMaster.GameOver)
        {
            return false;
        }

        if (!gameMaster.TryMove(x, y))
        {
            return false;
        }

        Debug.Log($"Graphical deploy: Player {player} ({x}-{y})");


        var srcGameObject = player == 1
            ? blockerPiecePlayer1
            : blockerPiecePlayer2;
        var copyGameObject = Object.Instantiate(srcGameObject);
        copyGameObject.transform.SetParent(blockerPieceCollector.transform, false);
        copyGameObject.transform.localPosition = new Vector3(x, 0, y);
        copyGameObject.SetActive(true);
        
        return true;
    }

    public void RemoveAll()
    {
        for (var i = 0; i < blockerPieceCollector.transform.childCount; i++)
        {
            var gameObject = blockerPieceCollector.transform.GetChild(i);
            Object.Destroy(gameObject.gameObject);
        }
    }
}
