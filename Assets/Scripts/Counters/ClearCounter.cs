using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ClearCounter : BaseCounter
{
    // 참조 0개 필요없는 코드?
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player){
        if(!HasKitchenObject()){
            // There is no KitchenObject here
            if(player.HasKitchenObject()){
                // Player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this);
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
                }else{
                    // Player is not carrying Plate but something else
                    if(GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        // Counter is holding a plate
                        if(plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO())){
                            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                        }
                    }
                }
            } else {
                // Player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}
