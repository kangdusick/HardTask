using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCard : MonoBehaviour
{
    private PopSelectAbility _popSelectAbility;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _descText;
    private AbilityTable _abilityTable;
    public void Init(PopSelectAbility popSelectAbility, EAbilityTable eAbilityTable)
    {
        _popSelectAbility = popSelectAbility;
        _abilityTable = TableManager.AbilityTableDict[eAbilityTable];
        _titleText.text = _abilityTable.nameLanguageKey.LocalIzeText();
        _descText.text = _abilityTable.descLanguageKey.LocalIzeText();
    }
    public void OnClickSelect()
    {
        _popSelectAbility.SelectAbility();
    }
}
