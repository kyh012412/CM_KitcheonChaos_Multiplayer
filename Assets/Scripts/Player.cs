using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : NetworkBehaviour ,IKitchenObjectParent
{
    // public static Player Instance{ get; private set; }

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    // [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private void Awake() {
        // if (Instance != null){
        //     Debug.LogError("There is more than one Player instance");
        // }

        // Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        
        if(selectedCounter !=null){
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if(selectedCounter !=null){
            selectedCounter.Interact(this);
        }
    }

    private void Update(){
        if(!IsOwner){
            return;
        }

        // HandleMovementServerAuth(); // server Auth 방법으로 client는 딜레이가 있는것처럼 느껴지며 서버가 소스의 원천이 된다.
        HandleMovement(); // Client 에서 각자 움직이며 상대방화면에서 늦게 반영이 된다.
        HandleInteractions();
    }

    public bool IsWalking(){
        return isWalking;
    }

    private void HandleMovementServerAuth(){
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        HandleMovementServerRpc(inputVector);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector2 inputVector){
        
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDir,moveDistance);

        if(!canMove){
            // Cannot move towards moveDir

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDirX,moveDistance);

            if (canMove){
                // Can move only on the X
                moveDir = moveDirX;
            }else{
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDirZ,moveDistance);
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

    private void HandleMovement(){
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDir,moveDistance);

        if(!canMove){
            // Cannot move towards moveDir

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDirX,moveDistance);

            if (canMove){
                // Can move only on the X
                moveDir = moveDirX;
            }else{
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius,moveDirZ,moveDistance);
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
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if(moveDir != Vector3.zero){
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;

        // bool canInteract = Physics.Raycast(transform.position, moveDir,out RaycastHit raycastHit, interactDistance);
        if(Physics.Raycast(transform.position, lastInteractDir ,out RaycastHit raycastHit, interactDistance, countersLayerMask)){
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) { //반환하는 자료형은 bool
                // Has ClearCounter 
                if(baseCounter != selectedCounter){
                    SetSelectedCounter(baseCounter);
                }
            }else{
                SetSelectedCounter(null);
            }
        }else{
            SetSelectedCounter(null);
        }
        // Debug.Log(selectedCounter); 
    }

    private void SetSelectedCounter(BaseCounter selectedCounter){
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform(){
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        
        this.kitchenObject = kitchenObject;
        if(kitchenObject != null){
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void ClearKitchenObject(){
        kitchenObject = null;
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }
}
