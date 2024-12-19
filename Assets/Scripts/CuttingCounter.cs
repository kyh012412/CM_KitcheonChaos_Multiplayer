using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : EventArgs{
        public float progressNormalized;
    }

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
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgress = 0;

                    
                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs
                    {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
                    });
                }
            }else{
                // Player not carrying anything
            }
        }else{
            // There is KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something
            } else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){
            // There is a KitchenObject here AND it can be cut
            cuttingProgress++;

            OnCut?.Invoke(this, EventArgs.Empty);
            
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });
            
            if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
                KitchenObjectSO outputKitchenObjectSO = cuttingRecipeSO.output;

                GetKitchenObject().DestroySelf();

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
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
