using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : CharacterBase
{
    public static Boss Instance;
    private bool _isCanAttack;
    public bool IsCanAttack => !ReferenceEquals(Instance,null) && _isCanAttack;
    [SerializeField] BlockSpawnLine _leftSpawnLine;
    [SerializeField] BlockSpawnLine _rightSpawnLine;
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
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        Phase = EBossPhase.Default;
        _isCanAttack = true;
        hpDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = TableManager.ConfigTableDict[EConfigTable.bossDefaultHp].FloatValue;
        BallShooter.Instance.OnPlayerTurnEnd -= BallSpawnRoutine;
        BallShooter.Instance.OnPlayerTurnEnd += BallSpawnRoutine;
        OnCurrentHpChange -= PhaseChange;
        OnCurrentHpChange += PhaseChange;

        _leftSpawnLine.PushAndSpawnBlocksInLine();
        _rightSpawnLine.PushAndSpawnBlocksInLine();
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
            Phase = EBossPhase.Default;
        }
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
}
