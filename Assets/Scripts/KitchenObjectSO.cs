using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()] // 공백은 기본값 사용
public class KitchenObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;


}
