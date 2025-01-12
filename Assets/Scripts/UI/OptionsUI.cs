using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance{ get; private set; }

    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;

    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button InteractButton;
    [SerializeField] private Button InteractAlternateButton;
    [SerializeField] private Button PauseButton;


    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI InteractText;
    [SerializeField] private TextMeshProUGUI InteractAlternateText;
    [SerializeField] private TextMeshProUGUI PauseText;

    [SerializeField] private Transform pressToRebindKeyTransform;

    private Action onCloseButtonAction;

    private void Awake()
    {
        Instance = this;
        soundEffectsButton.onClick.AddListener(() => {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        musicButton.onClick.AddListener(() =>{
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        closeButton.onClick.AddListener(() =>{
            Hide();
            onCloseButtonAction();
        });

        moveUpButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Move_UP);});
        moveDownButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Move_Down);});
        moveLeftButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Move_Left);});
        moveRightButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Move_Right);});
        InteractButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Interact);});
        InteractAlternateButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.InteractAlternate);});
        PauseButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Pause);});
    }

    private void Start(){
        KitchenGameManager.Instance.OnLocalGameUnpaused += KitchenGameManager_OnGameUnpaused;
        UpdateVisual();

        HidePressToRebindKey();
        Hide();
    }

    private void KitchenGameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual(){
        soundEffectsText.text = "Sound Effects: " + SoundManager.Instance.GetVolume()*10f;
        musicText.text = "Music: " + MusicManager.Instance.GetVolume()*10f;

        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_UP);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        InteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        InteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        PauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
    }

    public void Show(Action onCloseButtonAction){
        this.onCloseButtonAction = onCloseButtonAction;
        gameObject.SetActive(true);

        soundEffectsButton.Select();
    }

    public void Hide(){
        gameObject.SetActive(false);
    }

    private void ShowPressToRebindKey(){
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey(){
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding){
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding,()=>
        {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }
}
