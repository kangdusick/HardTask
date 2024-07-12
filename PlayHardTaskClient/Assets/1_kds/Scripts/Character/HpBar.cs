using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] TMP_Text _hpText;
    [SerializeField] Slider _hpSlider;
    CharacterBase _characterBase;

    public void Init(CharacterBase characterBase)
    {
        _characterBase = characterBase;
        _characterBase.OnCurrentHpChange -= OnCurrentHpChanged;
        _characterBase.OnCurrentHpChange += OnCurrentHpChanged;
    }
    private void OnCurrentHpChanged()
    {
        _hpSlider.value = _characterBase.CurrentHpRate;
        _hpText.text = $"{_characterBase.CurrentHp}/{_characterBase.hpDict.FinalValueDescription}";
    }
}
