using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SRF;

public class PopSelectAbility : BasePopup
{
    [SerializeField] List<AbilityCard> _abilityCardList;
    public static HashSet<EAbilityTable> appliedAbiliySet = new();
    private List<EAbilityTable> _allAbilityList = new();
    private bool _isSelectAbility;
    protected override void Awake()
    {
        base.Awake();
        foreach (EAbilityTable item in Enum.GetValues(typeof(EAbilityTable)))
        {
            if(item != EAbilityTable.valueTypeDefine)
            {
                _allAbilityList.Add(item);
            }
        }
    }
    public override void OpenPopup()
    {
        _isSelectAbility = false;
        base.OpenPopup();
        InitAbilityCard();
    }
    private void InitAbilityCard()
    {
        HashSet<EAbilityTable> currentAbilitySet = new();
        foreach (var item in _abilityCardList)
        {
            while (true)
            {
                var randomAbility = _allAbilityList.Random();
                if (!currentAbilitySet.Contains(randomAbility) && !appliedAbiliySet.Contains(randomAbility))
                {
                    item.Init(this, randomAbility);
                    currentAbilitySet.Add(randomAbility);
                    break;
                }
            }
        }
    }
    private void ApplyAbility(AbilityTable abilityTable, StatusDictionary targetDict)
    {
        var nameKey = abilityTable.nameLanguageKey.ParseEnum<ELanguageTable>();
        var typeKey = (EStatusType)abilityTable.statusType;
        targetDict[(nameKey, typeKey)] = abilityTable.amount;
    }
    public void SelectAbility(EAbilityTable eAbilityTable)
    {
        if(_isSelectAbility)
        {
            return;
        }
        appliedAbiliySet.Add(eAbilityTable);
        _isSelectAbility = true;
        var table = TableManager.AbilityTableDict[eAbilityTable];
        switch (eAbilityTable)
        {
            case EAbilityTable.ability1:
                ApplyAbility(table, Boss.Instance.requireBallCntForStunDict);
                break;
            case EAbilityTable.ability2:
                ApplyAbility(table, Player.Instance.requireBallForNeroOrbDict);
                break;
            case EAbilityTable.ability3:
                ApplyAbility(table, Player.Instance.fairyDamageDict);
                break;
            case EAbilityTable.ability4:
                ApplyAbility(table, Player.Instance.fairySpawnChanceDict);
                break;
            case EAbilityTable.ability5:
                ApplyAbility(table, Boss.Instance.attackCooldownDict);
                break;
            case EAbilityTable.ability6:
                ApplyAbility(table, Player.Instance.hpDict);
                break;
            case EAbilityTable.ability7:
                ApplyAbility(table, Player.Instance.stunDurationDict);
                break;
        }
        OnClose();
    }
}
