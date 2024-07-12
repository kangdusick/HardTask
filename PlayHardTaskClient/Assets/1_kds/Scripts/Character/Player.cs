using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBase
{
    public static Player Instance;
    public StatusDictionary fairySpawnChanceDict = new();
    public StatusDictionary fairyDamageDict = new();

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        fairySpawnChanceDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.fairyDefaultChance].FloatValue;
        fairyDamageDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.fairyDefaultDamage].FloatValue;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.playerDefaultHp].FloatValue;
    }
}
