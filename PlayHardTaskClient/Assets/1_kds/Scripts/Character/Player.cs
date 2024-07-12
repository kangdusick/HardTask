using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBase
{
    public static Player Instance;
    public StatusDictionary fairySpawnChanceDict = new();
    public StatusDictionary fairyDamageDict = new();
    public StatusDictionary directAttackDamageDict = new(); //보스에게 직접 구슬 발사 시 입히는 데미지

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        fairySpawnChanceDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.fairyDefaultChance].FloatValue;
        fairyDamageDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.fairyDefaultDamage].FloatValue;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.playerDefaultHp].FloatValue;
        directAttackDamageDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.playerDirectDamage].FloatValue;
    }
}
