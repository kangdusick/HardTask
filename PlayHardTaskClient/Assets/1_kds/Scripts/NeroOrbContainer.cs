using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NeroOrbContainer : MonoBehaviour
{
    public static NeroOrbContainer Instance;
    [SerializeField] TMP_Text _neroOrbRemainCnt;
    [SerializeField] Image _neroOrbFillImage;
    private int _remainNeroOrbCount;
    public int RemainNeroOrbCount
    {
        get { return _remainNeroOrbCount; }
        set
        {
            _remainNeroOrbCount = value;
            _neroOrbRemainCnt.text = _remainNeroOrbCount.ToString();
            if (_remainNeroOrbCount <= 0)
            {
                MakeNeroOrb();
                RemainNeroOrbCount = Mathf.RoundToInt(Player.Instance.requireBallForNeroOrbDict.FinalValue);
            }
            _neroOrbFillImage.fillAmount = 1f - _remainNeroOrbCount / Player.Instance.requireBallForNeroOrbDict.FinalValue;
        }
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        RemainNeroOrbCount = Mathf.RoundToInt(Player.Instance.requireBallForNeroOrbDict.FinalValue);

    }
    private void MakeNeroOrb()
    {
        BallShooter.Instance.PrefareBall(true);
    }
    public void FillNeroOrbDirectly()
    {
        BallShooter.Instance.FillNeroOrbDirectly();
    }
}
