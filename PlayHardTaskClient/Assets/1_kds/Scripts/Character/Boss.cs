using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
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
    public StatusDictionary hpRegenWhenHideDict = new();

    private bool _isCanAttack;
    public bool IsCanAttack => !ReferenceEquals(Instance, null) && _isCanAttack;
    public bool isBossTurn;
    [SerializeField] RectTransform _weaponEdgeRect;

    [SerializeField] BlockSpawnLine _leftSpawnLine;
    [SerializeField] BlockSpawnLine _rightSpawnLine;

    [SerializeField] GameObject _finalPhaseEffect;


    [SerializeField] TMP_Text _attackCooldownText;
    [SerializeField] TMP_Text _stunText;

    private bool isWhileSpawnBall;

    private int _stun;
    private int Stun
    {
        get { return _stun; }
        set
        {
            if (Phase == EBossPhase.Hide)
            {
                return;
            }
            _stun = value;
            _stunText.text = $"{ELanguageTable.stun.LocalIzeText()}:{_stun}";
            if(_stun<=0)
            {
                currentIdleAnim = EReaperAnim.Idle.ToString();
                SetAnim(EReaperAnim.Idle.ToString());
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
            if(Stun> 0 || Phase == EBossPhase.Hide)
            {
                return;
            }
            _rmainBallCountForStun = value;
            _stunText.text = $"{ELanguageTable.stun.LocalIzeText()}:{_rmainBallCountForStun}";
            if(_rmainBallCountForStun<=0)
            {
                currentIdleAnim = EReaperAnim.Stun.ToString();
                SetAnim(EReaperAnim.Stun.ToString());
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
            if (Stun > 0 || Phase == EBossPhase.Hide)
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
                    GameUtil.Instance.ShowToastMessage(ELanguageTable.phaseDesc1);
                    break;
                case EBossPhase.LeftWing:
                    SetSkin("LeftWing");
                    SetAnim(EReaperAnim.LeftWingGrow.ToString());
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown2].FloatValue;
                    GameUtil.Instance.ShowToastMessage(ELanguageTable.phaseDesc2);
                    break;
                case EBossPhase.DoubleWing:
                    SetSkin("DoubleWing");
                    SetAnim(EReaperAnim.RightWingGrow.ToString());
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown3].FloatValue;
                    GameUtil.Instance.ShowToastMessage(ELanguageTable.phaseDesc3);
                    break;
                case EBossPhase.Hide:
                    GameUtil.Instance.ShowToastMessage(ELanguageTable.phaseDesc4);
                    break;
                case EBossPhase.Final:
                    attackCooldownDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossAttackCooldown5].FloatValue;
                    GameUtil.Instance.ShowToastMessage(ELanguageTable.phaseDesc5);
                    break;
            }
            _finalPhaseEffect.SetActive(_phase == EBossPhase.Final);
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

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        Phase = EBossPhase.Default;
        _isCanAttack = true;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossDefaultHp].FloatValue;
        attackDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossDefaultDamage].FloatValue;
        requireBallCntForStunDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.requireBallCntForStun].FloatValue;
        hpRegenWhenHideDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.hpRecoveryEveryTurnWhenHide].FloatValue;

        OnCurrentHpChange -= PhaseChange;
        OnCurrentHpChange += PhaseChange;

        _leftSpawnLine.PushAndSpawnBlocksInLine();
        _rightSpawnLine.PushAndSpawnBlocksInLine();

        _skeletonGraphic.AnimationState.Event -= HandleAnimationStateEvent;
        _skeletonGraphic.AnimationState.Event += HandleAnimationStateEvent;
       
        currentIdleAnim = EReaperAnim.Idle.ToString();

        SetAnim(currentIdleAnim);
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
            if (currentIdleAnim == EReaperAnim.Stun.ToString())
            {
                Stun--;
            }
            RemainAttackCooldown--;
            await BallSpawnRoutine();
        }
        else
        {
            CurrentHp += hpDict.FinalValue * hpRegenWhenHideDict.FinalValue*0.01f;
            bool isAllBallDestroy = true;
            foreach (var item in HexBlockContainer.hexBlockContainerList)
            {
                if (item.hexBlock != null && item.hexBlock.eBlockType != EBlockType.attatchPoint)
                {
                    isAllBallDestroy = false;
                    break;
                }
            }

            if(isAllBallDestroy)
            {
                await ChangeMap(EPrefab.BossPhase5);
                gameObject.SetActive(true);
                transform.SetParent(HexBlockContainer.hexBlockContainerMatrix[9,3].transform, false);
                transform.localPosition = Vector3.zero;
                Phase = EBossPhase.Final;
                Stun = 0;
                RemainAttackCooldown = attackCooldownDict.FinalValue_RoundToInt;
                RmainBallCountForStun = requireBallCntForStunDict.FinalValue_RoundToInt;
                await BallSpawnRoutine();
            }
        }
        await UniTask.Delay(1000);
        isBossTurn= false;
    }
    private async void PhaseChange()
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
            await ChangeMap(EPrefab.BossPhase4);
        }
    }
    private void SetSkin(string skinName)
    {
        _skeletonGraphic.Skeleton.SetSkin(skinName);
        _skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        _skeletonGraphic.AnimationState.Apply(_skeletonGraphic.Skeleton);
    }
    private async UniTask ChangeMap(EPrefab mapPrefab)
    {
        await UniTask.WaitWhile(() => !isBossTurn);
        await UniTask.WaitWhile(() => isWhileSpawnBall);
        transform.SetParent(GameManager.Instance.worldCanvas.transform);
        foreach (var item in BallShooter.Instance.prepareBallList)
        {
            item.transform.SetParent(GameManager.Instance.worldCanvas.transform);
        }
        gameObject.SetActive(false);
        Destroy(BlockEditor.Instance.gameObject);
        await UniTask.Yield();
        var map = PoolableManager.Instance.Instantiate<BlockEditor>(mapPrefab, parentTransform: GameManager.Instance.worldCanvas.transform);
        map.transform.SetAsFirstSibling();
        foreach (var item in BallShooter.Instance.prepareBallList)
        {
            item.transform.SetParent(map.transform);
        }

    }
    private async UniTask BallSpawnRoutine()
    {
        if (Stun > 0)
        {
            return;
        }
        List<UniTask> tasks = new List<UniTask>();
        isWhileSpawnBall = true;
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
                foreach (var item in HexBlockContainer.blockSpawnLineList)
                {
                    tasks.Add(item.PushAndSpawnBlocksInLine());
                }
                break;
        }
        await UniTask.WhenAll(tasks);
        isWhileSpawnBall = false;
    }
    private void DoAttack()
    {
        SetAnim(EReaperAnim.Attack.ToString());
    }
    protected override void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
    {
        base.HandleAnimationStateEvent(trackEntry, e);
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
