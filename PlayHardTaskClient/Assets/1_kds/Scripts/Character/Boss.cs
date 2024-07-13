using Cysharp.Threading.Tasks;
using DG.Tweening;
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
    public StatusDictionary attackCooldownDict = new();

    private bool _isCanAttack;
    public bool IsCanAttack => !ReferenceEquals(Instance, null) && _isCanAttack;
    public bool isBossTurn;
    [SerializeField] RectTransform _weaponEdgeRect;

    [SerializeField] BlockSpawnLine _leftSpawnLine;

    [SerializeField] BlockSpawnLine _rightSpawnLine;
    [SerializeField] SkeletonGraphic _skeletonGraphic;

    [SerializeField] TMP_Text _attackCooldownText;
    [SerializeField] TMP_Text _stunText;

    private TrackEntry _spineCharacterTrackEntry;
    private bool isWhileSpawnBall;

    private int _stun;
    private int Stun
    {
        get { return _stun; }
        set
        {
            _stun = value;
            _stunText.text = $"{ELanguageTable.stun.LocalIzeText()}:{_stun}";
            if(_stun<=0)
            {
                _currentIdleAnim = EReaperAnim.Idle;
                SetAnim(EReaperAnim.Idle);
                RmainBallCountForStun = requireBallCntForStunDict.FinalValue_RoundToInt;
            }
        }
    }
    private int _rmainBallCountForStun;
    private int RmainBallCountForStun
    {
        get { return _rmainBallCountForStun; }
        set
        {
            if(Stun>0)
            {
                return;
            }
            _rmainBallCountForStun = value;
            _stunText.text = $"{ELanguageTable.stun.LocalIzeText()}:{_rmainBallCountForStun}";
            if(_rmainBallCountForStun<=0)
            {
                _currentIdleAnim = EReaperAnim.Stun;
                SetAnim(EReaperAnim.Stun);
                RemainAttackCooldown = attackCooldownDict.FinalValue_RoundToInt;
                Stun = Player.Instance.stunDurationDict.FinalValue_RoundToInt;
            }
        }
    }
    private int _remainAttackCooldown;
    private int RemainAttackCooldown
    {
        get { return _remainAttackCooldown; }
        set
        {
            if (Stun > 0)
            {
                return;
            }
            _remainAttackCooldown = value;
            if (_remainAttackCooldown <= 0)
            {
                DoAttack();
                _remainAttackCooldown = attackCooldownDict.FinalValue_RoundToInt;
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
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown1].FloatValue;
                    RemainAttackCooldown = attackCooldownDict.FinalValue_RoundToInt;
                    break;
                case EBossPhase.LeftWing:
                    SetSkin("LeftWing");
                    SetAnim(EReaperAnim.LeftWingGrow);
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown2].FloatValue;
                    break;
                case EBossPhase.DoubleWing:
                    SetSkin("DoubleWing");
                    SetAnim(EReaperAnim.RightWingGrow);
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown3].FloatValue;
                    break;
                case EBossPhase.Hide:
                    HidePhase();
                    
                    break;
                case EBossPhase.Final:
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown5].FloatValue;
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
        attackDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttack].FloatValue;
        requireBallCntForStunDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.requireBallCntForStun].FloatValue;

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
       

        RmainBallCountForStun = requireBallCntForStunDict.FinalValue_RoundToInt;
    }
    private void Start()
    {
        Player.Instance.OnPlayerTurnEnd -= OnPlayerTurnEnd;
        Player.Instance.OnPlayerTurnEnd += OnPlayerTurnEnd;
        isBossTurn = false;
    }
    private void OnBlockDamaged()
    {
        RmainBallCountForStun--;
    }
    private async void OnPlayerTurnEnd()
    {
        isBossTurn = true;
        if (_phase != EBossPhase.Hide)
        {
            if (_currentIdleAnim == EReaperAnim.Stun)
            {
                Stun--;
            }
            RemainAttackCooldown--;
            await BallSpawnRoutine();
        }
        else
        {
            CurrentHp += hpDict.FinalValue * 0.05f;
        }
        await UniTask.Delay(1000);
        isBossTurn= false;
    }
    private void PhaseChange()
    {
        if (CurrentHpRate <= 0.75f && Phase == EBossPhase.Default)
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
    private async void HidePhase()
    {
        await UniTask.WaitWhile(() => isWhileSpawnBall);
        transform.SetParent(GameManager.Instance.worldCanvas.transform);
        gameObject.SetActive(false);
        Destroy(BlockEditor.Instance.gameObject);
        PoolableManager.Instance.Instantiate<BlockEditor>(EPrefab.BossPhase4, parentTransform: GameManager.Instance.worldCanvas.transform);
    }
    private async UniTask BallSpawnRoutine()
    {
        isWhileSpawnBall = true;
        List<UniTask> tasks = new List<UniTask>();  
        if (Stun > 0)
        {
            return;
        }
        switch (Phase)
        {
            case EBossPhase.LeftWing:
                tasks.Add(_leftSpawnLine.PushAndSpawnBlocksInLine());
                break;
            case EBossPhase.DoubleWing:
                tasks.Add(_leftSpawnLine.PushAndSpawnBlocksInLine());
                tasks.Add(_rightSpawnLine.PushAndSpawnBlocksInLine());
                break;
            case EBossPhase.Final:
                tasks.Add(_leftSpawnLine.PushAndSpawnBlocksInLine());
                tasks.Add(_rightSpawnLine.PushAndSpawnBlocksInLine());
                break;
        }
        await UniTask.WhenAll(tasks);
        isWhileSpawnBall = false;
    }
    private void DoAttack()
    {
        SetAnim(EReaperAnim.Attack);
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
                var slash = PoolableManager.Instance.Instantiate(EPrefab.BossSlash, _weaponEdgeRect.transform.position, parentTransform: GameManager.Instance.worldCanvas.transform);
                slash.transform.SetAsLastSibling();
                slash.transform.DOMove(Player.Instance.transform.position, 0.5f).OnComplete(() =>
                {
                    Player.Instance.OnDamaged(attackDict.FinalValue);
                    PoolableManager.Instance.Destroy(slash);
                });
                break;
        }
    }
}
