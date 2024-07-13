using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Boss : CharacterBase
{
    public static Boss Instance;
    public StatusDictionary requireBallCntForStunDict = new();
    public StatusDictionary attackCooldown = new();

    private bool _isCanAttack;
    public bool IsCanAttack => !ReferenceEquals(Instance,null) && _isCanAttack;
    [SerializeField] BlockSpawnLine _leftSpawnLine;
    
    [SerializeField] BlockSpawnLine _rightSpawnLine;
    [SerializeField] SkeletonGraphic _skeletonGraphic;

    [SerializeField] TMP_Text _attackCooldownText;
    [SerializeField] TMP_Text _stunText;

    private TrackEntry _spineCharacterTrackEntry;

    private int Stun;
    private int _rmainBallCountForStun;
    private int RmainBallCountForStun;
    private int _remainAttackCooldown;
    private int RemainAttackCooldown
    {
        get { return _remainAttackCooldown; }
        set 
        {
            _remainAttackCooldown = value;
            if(_remainAttackCooldown <=0)
            {
                DoAttack();
                _remainAttackCooldown = Mathf.RoundToInt(attackCooldown.FinalValue);
            }
            _attackCooldownText.text = $"{ELanguageTable.attack}:{_remainAttackCooldown}";
        }
    }

    enum EBossPhase
    {
        Default = 0,
        LeftWing = 1,
        DoubleWing = 2,
        Hide = 3,
        Final = 4
    }
    private EBossPhase _phase;
    private EBossPhase Phase
    {
        get { return _phase; }
        set 
        { 
            _phase = value;
            switch (_phase)
            {
                case EBossPhase.Default:
                    attackCooldown[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown1].FloatValue;
                    RemainAttackCooldown = Mathf.RoundToInt(attackCooldown.FinalValue);
                    break;
                case EBossPhase.LeftWing:
                    SetSkin("LeftWing");
                    SetAnim(EReaperAnim.LeftWingGrow);
                    attackCooldown[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown2].FloatValue;
                    break;
                case EBossPhase.DoubleWing:
                    SetSkin("DoubleWing");
                    SetAnim(EReaperAnim.RightWingGrow);
                    attackCooldown[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown3].FloatValue;
                    break;
                case EBossPhase.Hide:
                    break;
                case EBossPhase.Final:
                    attackCooldown[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown5].FloatValue;
                    break;
            }
        }
    }
    private enum EReaperAnim
    {
        Attack,
        Idle,
        LeftWingGrow,
        RightWingGrow,
        Stun,
    }
    private EReaperAnim _currentIdleAnim;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        Phase = EBossPhase.Default;
        _isCanAttack = true;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossDefaultHp].FloatValue;
        Player.Instance.OnPlayerTurnEnd -= BallSpawnRoutine;
        Player.Instance.OnPlayerTurnEnd += BallSpawnRoutine;
        OnCurrentHpChange -= PhaseChange;
        OnCurrentHpChange += PhaseChange;

        _leftSpawnLine.PushAndSpawnBlocksInLine();
        _rightSpawnLine.PushAndSpawnBlocksInLine();

        _skeletonGraphic.AnimationState.Event -= HandleAnimationStateEvent;
        _skeletonGraphic.AnimationState.Event += HandleAnimationStateEvent;
        _skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;
        _skeletonGraphic.AnimationState.Complete += OnAnimationComplete;
        _currentIdleAnim = EReaperAnim.Idle;
        SetAnim(_currentIdleAnim);
        HexBlock.OnBlockDamaged -= OnBlockDamaged;
        HexBlock.OnBlockDamaged += OnBlockDamaged;
        Player.Instance.OnPlayerTurnEnd -= OnPlayerTurnEnd;
        Player.Instance.OnPlayerTurnEnd += OnPlayerTurnEnd;
    }
    private void OnBlockDamaged()
    {
        RmainBallCountForStun--;
    }
    private void OnPlayerTurnEnd()
    {
        RemainAttackCooldown--;
    }
    private void PhaseChange()
    {
        if(CurrentHpRate<=0.75f && Phase == EBossPhase.Default)
        {
            Phase = EBossPhase.LeftWing;
        }
        if (CurrentHpRate <= 0.5f && Phase == EBossPhase.LeftWing)
        {
            Phase = EBossPhase.DoubleWing;
        }
        if (CurrentHpRate <= 0.25f && Phase == EBossPhase.DoubleWing)
        {
            Phase = EBossPhase.Hide;
        }
    }
    private void SetSkin(string skinName)
    {
        _skeletonGraphic.Skeleton.SetSkin(skinName);
        _skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        _skeletonGraphic.AnimationState.Apply(_skeletonGraphic.Skeleton);
    }
    private void BallSpawnRoutine()
    {
        switch (Phase)
        {
            case EBossPhase.LeftWing:
                _leftSpawnLine.PushAndSpawnBlocksInLine();
                break;
            case EBossPhase.DoubleWing:
                _leftSpawnLine.PushAndSpawnBlocksInLine();
                _rightSpawnLine.PushAndSpawnBlocksInLine();
                break;
            case EBossPhase.Final:
                _leftSpawnLine.PushAndSpawnBlocksInLine();
                _rightSpawnLine.PushAndSpawnBlocksInLine();
                break;
        }
    }
    private void DoAttack()
    {

    }
    private void SetAnim(EReaperAnim eReaperAnim)
    {
        switch (eReaperAnim)
        {
            case EReaperAnim.Idle:
            case EReaperAnim.Stun:
                SetAnim(eReaperAnim, true);
                break;
            default:
                SetAnim(eReaperAnim, false);
                break;
        }
    }
    private void SetAnim(EReaperAnim eSpineAnim, bool isLoop = false, int track = 0)
    {
        _spineCharacterTrackEntry = _skeletonGraphic.AnimationState.SetAnimation(track, eSpineAnim.OriginName(), isLoop);
    }
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (!trackEntry.Animation.Name.Equals(_currentIdleAnim.ToString()))
        {
            SetAnim(_currentIdleAnim);
        }
    }
    private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "Attack":
                break;
        }
    }
}
