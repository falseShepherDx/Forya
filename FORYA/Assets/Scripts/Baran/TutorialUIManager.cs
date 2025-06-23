using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIManager : NetworkBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] TextMeshProUGUI countdown;

    private const int countdownSecond = 7;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(Countdown());
        }
    }

    IEnumerator Countdown()
    {
        for (int i = countdownSecond; i > 0; i--)
        {
            UpdateCountdownClientRpc(i);
            yield return new WaitForSeconds(1);
            
        }

        HideCanvasClientRpc();
        RunAfterCanvas();
    }


    [ClientRpc]
    void UpdateCountdownClientRpc(int secondsLeft)
    {
        if (countdown != null)
        {
            countdown.text = secondsLeft.ToString();
        }
    }

    [ClientRpc]
    void HideCanvasClientRpc()
    {
        if (canvas != null)
        {
            canvas.SetActive(false);    
        }

    }

    [Obsolete("Obsolete")]
    void RunAfterCanvas()
    {
        CharacterSpawner.instance.SpawnPlayers();
        if (FindObjectOfType<CannonSpawner>() != null)
        {
            FindObjectOfType<CannonSpawner>().StartSpawning();
        }
        
    }
}
