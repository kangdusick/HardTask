using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public static Boss Instance;
    public StatusDictionary hpDict= new();
    private bool _isCanAttack;
    public bool IsCanAttack => !ReferenceEquals(Instance,null) && _isCanAttack;
    private void Awake()
    {
        Instance = this;
        _isCanAttack = true;
    }
}
