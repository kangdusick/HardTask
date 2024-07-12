using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : CharacterBase
{
    public static Boss Instance;
    private bool _isCanAttack;
    public bool IsCanAttack => !ReferenceEquals(Instance,null) && _isCanAttack;
    
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        _isCanAttack = true;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossDefaultHp].FloatValue;
    }
}
