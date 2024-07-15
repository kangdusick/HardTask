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
    private EAbilityTable _eAbilityTable;
    public void Init(PopSelectAbility popSelectAbility, EAbilityTable eAbilityTable)
    {
        _eAbilityTable = eAbilityTable;
        _popSelectAbility = popSelectAbility;
        var abilityTable = TableManager.AbilityTableDict[eAbilityTable];
        _titleText.text = abilityTable.nameLanguageKey.LocalIzeText();
        _descText.text = abilityTable.descLanguageKey.LocalIzeText(StatusDictionary.GetDescriptionValue(abilityTable.amount, abilityTable.statusType, true));
    }
    public void OnClickSelect()
    {
        _popSelectAbility.SelectAbility(_eAbilityTable);
    }
}
