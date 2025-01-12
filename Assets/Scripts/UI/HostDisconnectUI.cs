using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;

    private void Awake() {
        playAgainButton.onClick.AddListener(() =>{
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId) {
            // Server is shutting down
            Show();
        }
    }

    private void Show(){
        gameObject.SetActive(true);
    }

    private void Hide(){
        gameObject.SetActive(false);
    }

    /*
    // 농협캐피탈 초봉 4800 / 성과급 별도 채용전환형 인턴
    // 서류 이후
    // 375문제 40분 인적성검사
    // 시작 30분러시 (호재)
    // 직무능력검사
    // 시험일이 3개가 겹처서 1/3 추정
    // 필기합격발표 1월 14일
    // 되면 면접 1월 말
    // 되면 2월 7일 출근
    // 00명
    // 35문제 품 / 70문제
    // 찍기 5번라인
    */

    /*
        우리금융
        600:3
        서류x 면접
    */
}
