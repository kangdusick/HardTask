using Cysharp.Threading.Tasks;
using DG.Tweening;
using SRF;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
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
    top = 200,
    item_fairy = 300,
    item_bomb = 400,
    attatchPoint = 500,
    attatchPoint_Spawn = 600,
}
public class HexBlock : MonoBehaviour
{
    public HexBlockContainer hexBlockContainer;
    public int x;
    public int y;
    public bool isItemEffectDone;
    private bool _isDamaged;
    [SerializeField] private Image blockImage;
    public Canvas canvas;
    public EColor eColor;
    public EBlockType eBlockType;
    private void Awake()
    {
        if(!ReferenceEquals(BlockEditor.Instance,null))
        {
            transform.SetParent(BlockEditor.Instance.transform);
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
    public void Init(EColor eColor, EBlockType eBlockType)
    {
        isItemEffectDone = false;
        _isDamaged = false;
        this.eColor = eColor;
        this.eBlockType = eBlockType;
        switch (eBlockType)
        {
            case EBlockType.top:
            case EBlockType.Empty:
            case EBlockType.attatchPoint:
            case EBlockType.attatchPoint_Spawn:
                this.eColor = EColor.none;
                break;
        }
        UpdateBlockImage();
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
  
    public async UniTask Damaged()
    {
        if(_isDamaged)
        {
            return;
        }
        _isDamaged = true;
        switch (eBlockType)
        {
            case EBlockType.item_fairy:
            case EBlockType.item_bomb:
                break;
            case EBlockType.top:
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
        Destroy();
    }
    public void Destroy()
    {
        if (!ReferenceEquals(this.hexBlockContainer, null) && this.hexBlockContainer.hexBlock == this)
        {
            this.hexBlockContainer.hexBlock = null;
        }
        this.hexBlockContainer = null;
        PoolableManager.Instance.Destroy(gameObject);
    }
}
