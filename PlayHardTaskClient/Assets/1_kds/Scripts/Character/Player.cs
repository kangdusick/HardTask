using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : CharacterBase
{
    public static Player Instance;
  
    public StatusDictionary fairySpawnChanceDict = new();
    public StatusDictionary fairyDamageDict = new();
    public StatusDictionary directAttackDamageDict = new(); //보스에게 직접 구슬 발사 시 입히는 데미지
    public StatusDictionary neroDirectAttackDamageDict = new(); //보스에게 직접 네로구슬 발사 시 입히는 데미지
    public StatusDictionary stunDurationDict = new();
    public StatusDictionary requireBallForNeroOrbDict = new(); //네로 오브를 얻기 위해 필요한 구슬 파괴 개수
    public StatusDictionary smallBombSpawnChanceDict = new();

    public Action OnPlayerTurnEnd;

    public override ObscuredFloat CurrentHp
    { 
        get => base.CurrentHp;
        set
        {
            base.CurrentHp = value;
            if(isStatusDictInit && base.CurrentHp<=0f)
            {
                PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon).OpenPopup(ELanguageTable.lose.LocalIzeText(), ELanguageTable.gameEndDesc.LocalIzeText(), () => 
                {
                    GameUtil.Instance.LoadScene("Load");
                });
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        SetStatusDict();
        ChangeIdleAnim(EGangAnimation.GangDungeon_AnubisIdle);
    }
    protected override void SetStatusDict()
    {
        fairySpawnChanceDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.fairyDefaultChance].FloatValue;
        fairyDamageDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.fairyDefaultDamage].FloatValue;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.playerDefaultHp].FloatValue;
        directAttackDamageDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.playerDirectDamage].FloatValue;
        requireBallForNeroOrbDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.requireBallForNeroOrb].FloatValue;
        smallBombSpawnChanceDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.smallBombSpawnChance].FloatValue;
        neroDirectAttackDamageDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.neroBallDirectDamage].FloatValue;
        stunDurationDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.stunDuration].FloatValue;
        base.SetStatusDict();
    }
    public void ChangeIdleAnim(EGangAnimation idleAnim)
    {
        currentIdleAnim = idleAnim.OriginName();
        SetAnim(idleAnim);
    }
    public void TurnEnd()
    {
        OnPlayerTurnEnd?.Invoke();
    }
   
}
