using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] HpBar _hpBar;

    public Action OnCurrentHpChange;
    public StatusDictionary attackDict = new();
    public StatusDictionary hpDict = new();

    public ObscuredFloat LostHpRate;
    public ObscuredFloat CurrentHpRate;
    private ObscuredFloat _currentHp;
    public virtual ObscuredFloat CurrentHp
    {
        get { return _currentHp; }
        set
        {
            _currentHp = value;
            if (_currentHp > hpDict.FinalValue)
            {
                _currentHp = hpDict.FinalValue;
            }
            CurrentHpRate = _currentHp / hpDict.FinalValue;
            LostHpRate = 1f - CurrentHpRate;
            OnCurrentHpChange?.Invoke();
        }
    }

    private ObscuredFloat beforeFinalHp;
    protected virtual void Awake()
    {
        _currentHp = hpDict.FinalValue;
        beforeFinalHp = 0f;
        hpDict.OnChanged -= SetFinalHp;
        hpDict.OnChanged += SetFinalHp;
        SetFinalHp();
        CurrentHp = hpDict.FinalValue;
        _hpBar.Init(this);
    }

    private void SetFinalHp()
    {
        if (hpDict.FinalValue > beforeFinalHp) //최대 체력이 늘어난 경우
        {
            CurrentHp += (hpDict.FinalValue - beforeFinalHp); //늘어난 최대 체력만큼 현재 체력도 증가
        }
        else //최대 체력이 줄어든 경우
        {
            if (CurrentHp > hpDict.FinalValue)
            {
                CurrentHp = hpDict.FinalValue;
            }
        }
        beforeFinalHp = hpDict.FinalValue;
    }

    public void OnDamaged(float damage)
    {
        CurrentHp -= damage;
        PoolableManager.Instance.InstantiateAsync<DamageText>(EPrefab.DamageText,transform.position + Vector3.up * 50f,parentTransform: GameObject.FindGameObjectWithTag(ETag.WorldCanvas.ToString()).transform).ContinueWithNullCheck(x =>
        {
            x.Init(damage,Color.red);
        });
    }
}
