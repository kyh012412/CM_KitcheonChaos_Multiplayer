using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour{

    public event EventHandler OnInteractionAction;
    private PlayerInputActions playerInputActions;

    private void  Awake(){
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
    }//2 41 07

    private void Interact_performed(InputAction.CallbackContext obj){
        if(OnInteractionAction != null){
            OnInteractionAction?.Invoke(this, EventArgs.Empty);
        }
        // Debug.Log(obj);
        // throw new System.NotImplementedException();
    }
    
    public Vector2 GetMovementVectorNormalized(){
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        // Debug.Log(inputVector);

        return inputVector;
    }
}
