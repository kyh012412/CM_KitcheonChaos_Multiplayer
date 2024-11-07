using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask; 

    private bool isWalking;
    private Vector3 lastInteractDir;

    private void Start()
    {
        gameInput.OnInteractionAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if(moveDir != Vector3.zero){
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;

        // bool canInteract = Physics.Raycast(transform.position, moveDir,out RaycastHit raycastHit, interactDistance);
        if(Physics.Raycast(transform.position, lastInteractDir ,out RaycastHit raycastHit, interactDistance, countersLayerMask)){
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)) { //반환하는 자료형은 bool
                // Has ClearCounter 
                clearCounter.Interact();
            }
        }
    }

    private void Update(){
        HandleMovement();
        HandleInteractions();

    }

    public bool IsWalking(){
        return isWalking;
    }

    private void HandleMovement(){
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDir,moveDistance);

        if(!canMove){
            // Cannot move towards moveDir

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDirX,moveDistance);

            if (canMove){
                // Can move only on the X
                moveDir = moveDirX;
            }else{
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDirZ,moveDistance);
                if(canMove){
                    // Can move only on the Z
                    moveDir = moveDirZ;
                }else{
                    // Cannot move in any direction
                }
            }
        }
        if(canMove){
            transform.position += moveDir * moveSpeed * Time.deltaTime ;
        }

        // transform.rotation 이것을 쓰게되면 Quaternion어쩌고 를 계산해야하고 복잡하므로 아래 방법을 사용
        // transform.forward = moveDir;

        isWalking = moveDir != Vector3.zero;
        // 위 처럼 코드 작성시 너무 180회전이 스무스하지 않으므로 아래 코드 사용
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    private void HandleInteractions(){        
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if(moveDir != Vector3.zero){
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;

        // bool canInteract = Physics.Raycast(transform.position, moveDir,out RaycastHit raycastHit, interactDistance);
        if(Physics.Raycast(transform.position, lastInteractDir ,out RaycastHit raycastHit, interactDistance, countersLayerMask)){
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)) { //반환하는 자료형은 bool
                // Has ClearCounter 
                // clearCounter.Interact();
            }
        }

    }

}
