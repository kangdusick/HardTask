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
            if(!IsEnable)
            {
                return;
            }
            _remainNeroOrbCount = value;
            _neroOrbRemainCnt.text = _remainNeroOrbCount.ToString();
            if (_remainNeroOrbCount <= 0)
            {
                MakeNeroOrb();
            }
            else
            {
                _neroOrbFillImage.fillAmount = 1f - _remainNeroOrbCount / Player.Instance.requireBallForNeroOrbDict.FinalValue;
            }
        }
    }
    public bool IsEnable;
    private void Awake()
    {
        Instance = this;
        HexBlock.OnBlockDamaged += () => { RemainNeroOrbCount--; };
    }
    private void Start()
    {
        EnableNeroOrbContainer(true);
    }
    private void MakeNeroOrb()
    {
        BallShooter.Instance.PrefareBall(true);
        EnableNeroOrbContainer(false);
    }
    public void FillNeroOrbDirectly()
    {
        BallShooter.Instance.FillNeroOrbDirectly();
    }
    public void EnableNeroOrbContainer(bool isEnable)
    {
        this.IsEnable = isEnable;
        gameObject.SetActive(isEnable);
        if(isEnable)
        {
            RemainNeroOrbCount = Mathf.RoundToInt(Player.Instance.requireBallForNeroOrbDict.FinalValue);
        }
    }
}
