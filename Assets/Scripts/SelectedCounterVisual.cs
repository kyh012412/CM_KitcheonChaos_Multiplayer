using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private ClearCounter clearCounter;
    [SerializeField] private GameObject visualGameObject; // 실제 선택된 대상 효과를 나타내는 조금 더 큰 게임오브젝트
    private void Start() {
        Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == clearCounter){
            Show();
        }else{
            Hide();
        }
    }

    private void Show(){
        visualGameObject.SetActive(true);
    }

    private void Hide(){
        visualGameObject.SetActive(false);
    }
}
