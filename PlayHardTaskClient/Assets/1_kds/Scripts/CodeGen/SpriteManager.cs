//ToolSpriteManagerGen 의해 자동으로 생성된 스크립트입니다..
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Threading;
public enum ESprite
{
    [Description("blue_normal")]
	blue_normal = -240477941,
	[Description("yellow_normal")]
	yellow_normal = -883872501,
	[Description("attatchPoint_Spawn")]
	attatchPoint_Spawn = -919027796,
	[Description("empty")]
	empty = 172708490,
	[Description("red_normal")]
	red_normal = 1623121414,
	
}



public class SpriteManager : MonoBehaviour
{
    private static SpriteManager _instance;
    public static SpriteManager Instance
    {
       
        get
        {
            if (ReferenceEquals(_instance ,null))
            {
                GameObject spriteManagerGameObject = new GameObject("SpriteManager");
                _instance = spriteManagerGameObject.AddComponent<SpriteManager>();
            }
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Dictionary<ESprite, Sprite> spriteDict = new();
    public Sprite LoadSprite(string eSprite)
    {
        return LoadSprite(eSprite.ParseEnum<ESprite>());
    }
    public Sprite LoadSprite(ESprite eSprite)
    {
        Sprite sprite;
        if (spriteDict.ContainsKey(eSprite))
        {
            sprite = spriteDict[eSprite];
        }
        else
        {
            var go = Addressables.LoadAssetAsync<Sprite>(eSprite.OriginName()).WaitForCompletion();
            spriteDict[eSprite] = go;
            sprite = spriteDict[eSprite];
        }
        return sprite;
    }
    public async UniTask<Sprite> LoadSpriteAsync(ESprite eSprite)
    {
        Sprite sprite;
        if (spriteDict.ContainsKey(eSprite))
        {
            sprite = spriteDict[eSprite];
        }
        else
        {
            try
            {
                var go = await Addressables.LoadAssetAsync<Sprite>(eSprite.OriginName()).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
                spriteDict[eSprite] = go;
                sprite = spriteDict[eSprite];
            }
            catch (System.Exception e)
            {
                Debug.Log("Loading of" +eSprite.OriginName() +"was cancelled.");

                return null;
            }
        }

        return sprite;
    }
    private void ReleaseAsset(ESprite eSprite) 
    {
        if(spriteDict.ContainsKey(eSprite))
        {
            Addressables.Release(spriteDict[eSprite]);
            spriteDict.Remove(eSprite);
        }
    }
    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        var dictkeyList = spriteDict.Keys.ToList();
        foreach (var eSprite in dictkeyList)
        {
            ReleaseAsset(eSprite);
        }
        spriteDict.Clear();
        Instance = null;
    }
}