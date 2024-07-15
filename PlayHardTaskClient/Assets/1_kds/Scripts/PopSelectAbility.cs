using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SRF;

public class PopSelectAbility : BasePopup
{
    [SerializeField] List<AbilityCard> _abilityCardList;
    private List<EAbilityTable> _allAbilityList = new(); 
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
                if (!currentAbilitySet.Contains(randomAbility))
                {
                    item.Init(this, randomAbility);
                    currentAbilitySet.Add(randomAbility);
                    break;
                }
            }
        }
    }
    public void SelectAbility()
    {
        OnClose();
    }
}
