using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fairy : MonoBehaviour
{
    public void Init(HexBlock hexBlock)
    {
        transform.SetParent(hexBlock.transform);
        transform.position = hexBlock.transform.position;
        transform.DOShakePosition(10, new Vector3(25, 25, 0f), 1, 90, false, false).SetLoops(-1);
    }
    public async UniTask UseFairy()
    {
        bool isMoveDone = false;
        transform.DOKill();
        transform.SetParent(BlockEditor.Instance.transform);
        if(Boss.Instance.IsCanAttack)
        {
            transform.DOMove(Boss.Instance.transform.position, 500f).SetSpeedBased().SetEase(Ease.InCubic).OnComplete(() => 
            {
                Boss.Instance.OnDamaged(Player.Instance.fairyDamageDict.FinalValue);
                isMoveDone = true;
            });
        }
        else
        {

        }
        await UniTask.WaitWhile(() => !isMoveDone);
        PoolableManager.Instance.Destroy(gameObject);
    }
}