using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDestroyer : MonoBehaviour
{
    public static BallDestroyer Instance;
    private void Awake()
    {
        Instance = this;
    }
    public async UniTask DestroyWithBallDestroyer(List<HexBlock> hexBlockList)
    {
        int remainDestroyCount = hexBlockList.Count;
        foreach (var item in hexBlockList)
        {
            item.transform.DOMove(transform.position, 1f).OnComplete(() =>
            {
                item.Damaged();
                remainDestroyCount--;
            });
        }
        await UniTask.WaitWhile(() => remainDestroyCount != 0);
    }
}
