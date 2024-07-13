using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public enum EColor
{
    none = 0, // 모든 색상과 매치 될 수 없다.
    blue = 100,
    red = 500,
    yellow = 600,
}
public enum EBlockType
{
    Empty = 0,
    normal = 100,
    bomb_Range2_neroOrb = 400,
    attatchPoint = 500,
    attatchPoint_Spawn = 600,
    boss = 700,
    bomb_Range1 = 800,
}
public class HexBlock : MonoBehaviour
{
    public HexBlockContainer hexBlockContainer;
    public int x;
    public int y;
    public bool isItemEffectDone;
    private bool _isDamaged;
    [SerializeField] private Image blockImage;
    public EColor eColor;
    public EBlockType eBlockType;
    private Fairy _attatchedFairy;
    public bool isAttatcfairy = false;
    public bool IsCantDestroyAndMove => (eBlockType == EBlockType.attatchPoint || eBlockType == EBlockType.attatchPoint_Spawn || eBlockType == EBlockType.boss);
    private void Awake()
    {
        if(!ReferenceEquals(BlockEditor.Instance,null))
        {
            transform.SetParent(BlockEditor.Instance.transform);
            transform.rotation = Quaternion.identity;
        }
    }
    public void Init(EColor eColor, EBlockType eBlockType)
    {
        _attatchedFairy = null;
        isItemEffectDone = false;
        _isDamaged = false;
        this.eColor = eColor;
        this.eBlockType = eBlockType;
        switch (eBlockType)
        {
            case EBlockType.Empty:
            case EBlockType.attatchPoint:
            case EBlockType.attatchPoint_Spawn:
            case EBlockType.bomb_Range2_neroOrb:
            case EBlockType.bomb_Range1:
                this.eColor = EColor.none;
                break;
        }
        UpdateBlockImage();
    }
    public void AttatchFairy(float chance)
    {
        if(Random.Range(0f,100f) <= chance)
        {
            _attatchedFairy = PoolableManager.Instance.Instantiate<Fairy>(EPrefab.Fairy);
            _attatchedFairy.Init(this);
        }
    }
    public void ChangeHexBlockContainer(HexBlockContainer hexBlockContainer)
    {
        if (!ReferenceEquals(this.hexBlockContainer, null) && this.hexBlockContainer.hexBlock == this)
        {
            this.hexBlockContainer.hexBlock = null;
        }
        this.hexBlockContainer = hexBlockContainer;
        if(!ReferenceEquals(hexBlockContainer,null))
        {
            hexBlockContainer.hexBlock = this;
            x = hexBlockContainer.x;
            y = hexBlockContainer.y;
        }
    }
    public async UniTask SetHexBlockContainerWithMove(HexBlockContainer hexBlockContainer, float moveSpeed, List<Vector3> movingRoute = null, bool isMoveDirectly = false, bool isTimeBase = false)
    {
        ChangeHexBlockContainer(hexBlockContainer);

        var isMoveDone = false;
        if(!isMoveDirectly)
        {
            if(!ReferenceEquals(movingRoute,null))
            {
                foreach (var item in movingRoute)
                {
                    isMoveDone = false;
                    transform.DOMove(item, moveSpeed).SetEase(Ease.Linear).SetSpeedBased(!isTimeBase).OnComplete(() => isMoveDone = true);
                    await UniTask.WaitWhile(() => !isMoveDone);
                }
            }
            transform.DOMove(hexBlockContainer.transform.position, moveSpeed).SetEase(Ease.Linear).SetSpeedBased(!isTimeBase).OnComplete(() => isMoveDone = true);
        }
        else
        {
            transform.position = hexBlockContainer.transform.position;
            isMoveDone = true;
        }
        await UniTask.WaitWhile(() => !isMoveDone);
    }
    private void UpdateBlockImage()
    {
        var spriteName = $"{(eColor == EColor.none ? string.Empty : eColor.ToString() + "_")}{(eBlockType == EBlockType.Empty ? string.Empty : eBlockType.ToString())}";
        if(eBlockType == EBlockType.attatchPoint || eBlockType == EBlockType.boss || eBlockType == EBlockType.bomb_Range2_neroOrb || eBlockType == EBlockType.bomb_Range1)
        {
            spriteName = ESprite.empty.ToString();
        }
        if (string.IsNullOrEmpty(spriteName))
        {
            blockImage.enabled = false;
        }
        else
        {
            blockImage.enabled = true;
            if (Application.isPlaying)
            {
                blockImage.sprite = SpriteManager.Instance.LoadSprite(spriteName);
            }
            else
            {
                blockImage.sprite = Addressables.LoadAssetAsync<Sprite>(spriteName).WaitForCompletion();
            }
            blockImage.SetNativeSize();
        }
    }
    public void UseFairy()
    {
        if (!ReferenceEquals(_attatchedFairy, null))
        {
            _attatchedFairy.UseFairy();
            _attatchedFairy = null;
        }
    }
    private void BombRangeDestroy(int bombRange)
    {
        var bombRangeList = hexBlockContainer.GetNeighborContainerBlockList(bombRange);
        foreach (var item in bombRangeList)
        {
            if (!ReferenceEquals(item.hexBlock, null))
            {
                item.hexBlock.Damaged();
            }
        }
    }
    public void Damaged()
    {
        if(_isDamaged || IsCantDestroyAndMove)
        {
            return;
        }
        _isDamaged = true;
        UseFairy();
        switch (eBlockType)
        {
            case EBlockType.bomb_Range2_neroOrb:
                BombRangeDestroy(2);
                break;
            case EBlockType.bomb_Range1:
                BombRangeDestroy(1);
                break;
            case EBlockType.normal:
                Color particleColor = Color.blue;
                switch (eColor)
                {
                    case EColor.blue:
                        particleColor = Color.blue;
                        break;
                    case EColor.red:
                        particleColor = Color.red;
                        break;
                    case EColor.yellow:
                        particleColor = Color.yellow;
                        break;
                }
                PoolableManager.Instance.Instantiate<AutoEnableParticleAndDistroyAfterEffectEnd>(EPrefab.NormalDestroyParticleEffect, transform.position, Vector3.one).Init(particleColor);
                break;

        }
        NeroOrbContainer.Instance.RemainNeroOrbCount--;
        Destroy();
    }
    public async UniTask RotateAroundCircle(float startAngle, float endAngle, float time = 0.4f)
    {
        var centerPos = BallShooter.Instance.transform.position;
        float radius = 75f;

        float elapsedTime = 0f;
        float angleDiff = endAngle - startAngle; // 각도 차이를 구함

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            float radian = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radian) * radius;
            float y = Mathf.Sin(radian) * radius;

            transform.position = centerPos + new Vector3(x, y, 0);

            await UniTask.Yield(); // 다음 프레임까지 대기
        }

        // 최종 위치 설정
        float finalRadian = endAngle * Mathf.Deg2Rad;
        float finalX = Mathf.Cos(finalRadian) * radius;
        float finalY = Mathf.Sin(finalRadian) * radius;

        transform.position = centerPos + new Vector3(finalX, finalY, 0);
    }
    private void Destroy()
    {
        if (!ReferenceEquals(this.hexBlockContainer, null) && this.hexBlockContainer.hexBlock == this || IsCantDestroyAndMove)
        {
            this.hexBlockContainer.hexBlock = null;
        }
        this.hexBlockContainer = null;
        PoolableManager.Instance.Destroy(gameObject);
    }
}
