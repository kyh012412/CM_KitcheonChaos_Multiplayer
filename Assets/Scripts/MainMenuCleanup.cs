using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    private void Awake() {
        if(NetworkManager.Singleton != null){
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if(KitchenGameMultiplayer.Instance != null){
            Destroy(KitchenGameMultiplayer.Instance.gameObject);
        }
    }
}
