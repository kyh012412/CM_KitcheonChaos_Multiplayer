using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class StoveCounter : BaseCounter,IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs{
        public State state;
    }

    public enum State{
        Idle,
        Frying,
        Fried,
        Burned,
    }
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);

    // 모두가 읽을수 있으며 서버만이 쓰기 기능을 수행할 수 있다.
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO fryingRecipeSO;
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private BurningRecipeSO burningRecipeSO;    

    public override void OnNetworkSpawn(){
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue){
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;

        OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;

        OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
            state = state.Value
        });

        if(state.Value == State.Burned || state.Value == State.Idle){
            OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs{
                progressNormalized = 0f
            });
        }
    }

    private void Update(){
        if (!IsServer) return;
        if (!HasKitchenObject()) return;

        switch(state.Value){
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer.Value += Time.deltaTime;

                if(fryingTimer.Value > fryingRecipeSO.fryingTimerMax){
                    // Fried
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());

                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                    state.Value = State.Fried;
                    burningTimer.Value = 0f;
                    SetBurningRecipeSOClientRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                    );
                }                
                break;
            case State.Fried:            
                burningTimer.Value += Time.deltaTime;

                if(burningTimer.Value > burningRecipeSO.burningTimerMax){
                    // Burned
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());

                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                    state.Value = State.Burned;
                }
                break;
            case State.Burned:
                break;
        }
        Debug.Log(state);
    }

    public override void Interact(Player player)
    {
        if(!HasKitchenObject()){
            // There is no KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())){
                    // Player carrying something that can be Fried
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    SetFryingRecipeSOServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                    );
                }
            }else{
                // Player not carrying anything
            }
        }else{
            // There is KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something                
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    // Player is Holding a Plate
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        // 문제 예정
                        GetKitchenObject().DestroySelf();
                        state.Value = State.Idle;
                        // SetStateIdleServerRpc();
                    }
                }
            } else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc(){
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetFryingRecipeSOServerRpc(int kitchenObjectSOIndex){
        fryingTimer.Value = 0f;
        state.Value = State.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex){
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex){
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }


    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if(fryingRecipeSO == null){
            // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
            Debug.Log("input에 해당하는 fried output이 감지되지 않았음!");
            return null;
        }
        return fryingRecipeSO.output;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach(FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray){
            if(fryingRecipeSO.input == inputKitchenObjectSO){
                return fryingRecipeSO;
            }
        }
        // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
        Debug.Log("input에 해당하는 fried output이 감지되지 않았음!");
        return null;
    }

    // 이하 3개는 Burning에 관한 메서드
    // private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
    //     FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
    //     return fryingRecipeSO != null;
    // }

    // private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
    //     FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
    //     if(fryingRecipeSO == null){
    //         // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
    //         Debug.Log("input에 해당하는 fried output이 감지되지 않았음!");
    //         return null;
    //     }
    //     return fryingRecipeSO.output;
    // }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOArray){
            if(burningRecipeSO.input == inputKitchenObjectSO){
                return burningRecipeSO;
            }
        }
        // Debug.LogError("input에 해당하는 fried output이 감지되지 않았음!");
        Debug.Log("input에 해당하는 Burning output이 감지되지 않았음!");
        return null;
    }

    public bool IsFried(){
        return state.Value == State.Fried;
    }
}
