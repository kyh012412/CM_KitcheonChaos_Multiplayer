using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    public static event EventHandler OnAnyCut;
    new public static void ResetStaticData(){
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] CuttingRecipeSOArray;

    private int cuttingProgress;
    public override void Interact(Player player){
        if(!HasKitchenObject()){
            // There is no KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())){
                    // Player carrying something that can be Cut
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc();
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
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            } else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(){
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc(){        
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    public override void InteractAlternate(Player player)
    {
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){
            // There is a KitchenObject here AND it can be cut
            CutObjectServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc(){
        CutObjectClientRpc();
        TestCuttingProgressDoneServerRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc(){
        cuttingProgress++;

        OnCut?.Invoke(this, EventArgs.Empty);
        // Debug.Log(OnAnyCut.GetInvocationList().Length);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
        
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc(){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
            KitchenObjectSO outputKitchenObjectSO = cuttingRecipeSO.output;

            // 이 부분은 서버에서만 일어남
            // 2개의 클라이언트가 연결되었을때는 이요청이 2번 들어오게 되면서 문제가 발생
            KitchenObject.DestroyKitchenObject(GetKitchenObject());

            KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if(cuttingRecipeSO == null){
            // Debug.LogError("input에 해당하는 sliced output이 감지되지 않았음!");
            Debug.Log("input에 해당하는 sliced output이 감지되지 않았음!");
            return null;
        }
        return cuttingRecipeSO.output;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
                foreach(CuttingRecipeSO cuttingRecipeSO in CuttingRecipeSOArray){
            if(cuttingRecipeSO.input == inputKitchenObjectSO){
                return cuttingRecipeSO;
            }
        }
        // Debug.LogError("input에 해당하는 sliced output이 감지되지 않았음!");
        Debug.Log("input에 해당하는 sliced output이 감지되지 않았음!");
        return null;
    }
}
