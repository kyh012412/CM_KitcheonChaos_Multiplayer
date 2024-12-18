using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class BaseCounter : MonoBehaviour,IKitchenObjectParent
{
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    // virtual 상속받은 곳에서 재정의 할수도 있게해준다.
    // abstract은 상속받은 곳에서 재정의를 강제한다.
    public virtual void Interact(Player player){
        Debug.LogError("BaseCounter.Interact()");
    }

    public virtual void InteractAlternate(Player player){
        Debug.LogError("BaseCounter.InteractAlternate()");
    }

    
    public Transform GetKitchenObjectFollowTransform(){
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;
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
