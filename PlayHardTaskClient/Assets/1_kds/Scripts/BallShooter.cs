using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SRF;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] private RectTransform _shootingStartPoint;
    [SerializeField] private RectTransform point1;
    [SerializeField] private RectTransform point2;
    private HexBlockContainer _destineHexBlockContainer = null;
    private List<Vector2> shootingBallMovingRoute = new();
    private const float shootingBallSpeed = 1000f;
    private void Awake()
    {
        TouchManager.Instance.OnTouchIng+= SetDestineHexBlockContainer;
        TouchManager.Instance.OnTouchUp += ShootingBall;
    }

    void SetDestineHexBlockContainer(Vector2 screenPos) //마우스 클릭 중에 조준선이 활성화되며 구슬이 어디에 도착할지 표시해준다.
    {
        shootingBallMovingRoute.Clear();
        if (!ReferenceEquals(_destineHexBlockContainer, null))
        {
            _destineHexBlockContainer.EnableHintEffect(false);
            _destineHexBlockContainer = null;
        }

        var laserDirection = TouchManager.Instance.mouseWorldPos - _shootingStartPoint.transform.position;
        laserDirection.z = 0f;

        var hit = GetNearestRaycastHit(_shootingStartPoint.transform.position, laserDirection,new List<ELayers>() { ELayers.Wall,ELayers.HexBlockContainer});
        if(!ReferenceEquals(hit.collider,null)) 
        {
            shootingBallMovingRoute.Add(hit.point);
            point1.transform.position = hit.point;
            if (hit.collider.gameObject.layer == (int)ELayers.Wall) //벽과 충돌한 경우 한번 반사된다.
            {
                // 반사된 레이저 발사
                Vector3 hitPoint = hit.point;
                Vector3 normal = hit.normal;
                Vector3 incomingVec = laserDirection;
                Vector3 reflectVec = Vector3.Reflect(incomingVec, normal);

                var hitByReflect = GetNearestRaycastHit(hitPoint, reflectVec, new List<ELayers>() { ELayers.HexBlockContainer });

                if (!ReferenceEquals(hitByReflect.collider,null))
                {
                    point2.transform.position = hitByReflect.point;
                    shootingBallMovingRoute.Add(hitByReflect.point);
                    _destineHexBlockContainer = GetBallDestineContainer(hitByReflect);
                }
            }
            else //구슬이 있는 블럭인 경우 해당 구슬과 인접한 가장 가까운 빈 구역을 표시해준다.
            {
                _destineHexBlockContainer = GetBallDestineContainer(hit);
            }
        }

        if(!ReferenceEquals(_destineHexBlockContainer,null))
        {
            shootingBallMovingRoute.Add(_destineHexBlockContainer.transform.position);
            _destineHexBlockContainer.EnableHintEffect(true);
        }
    }
    private async void ShootingBall(Vector2 screenPos)
    {
        if(!ReferenceEquals(_destineHexBlockContainer,null))
        {
            var shootedBall = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.HexBlock, _shootingStartPoint.position);
            shootedBall.Init(HexBlockContainer.EColorList.Random(), EBlockType.normal);
            shootedBall.SetHexBlockContainerWithMove(_destineHexBlockContainer, shootingBallSpeed, shootingBallMovingRoute);
        }
    }
    private HexBlockContainer GetBallDestineContainer(RaycastHit2D raycastHit2D)
    {
        var detectedContainer = raycastHit2D.collider.gameObject.GetCashComponent<HexBlockContainer>();
        foreach (var item in HexBlockContainer.GetNeighborContainerBlockList(detectedContainer))
        {
            if (ReferenceEquals(item.hexBlock,null) &&
                GameUtil.DistanceSquare2D(item.transform.position,raycastHit2D.point) <= (HexBlockContainer.hexHeight/2f) * (HexBlockContainer.hexHeight / 2f))
            {
                return item;
            }
        }
        return null;
    }
    private RaycastHit2D GetNearestRaycastHit(Vector3 from, Vector3 dir, List<ELayers> detectLayerList)
    {
        var hits = Physics2D.RaycastAll(from, dir.normalized, 1000f);

        List<RaycastHit2D> hitsList = new();
        foreach (var hit in hits)
        {
            // 충돌한 경우
            if (hit.collider != null)
            {
                var eLayer = (ELayers)hit.collider.gameObject.layer;
                switch (eLayer)
                {
                    case ELayers.Wall:
                        if(detectLayerList.Contains(eLayer))
                        {
                            hitsList.Add(hit);
                        }
                        break;
                    case ELayers.HexBlockContainer:
                        if (detectLayerList.Contains(eLayer))
                        {
                            var hexBlockContainer = hit.collider.gameObject.GetCashComponent<HexBlockContainer>();
                            if (!ReferenceEquals(hexBlockContainer.hexBlock, null))
                            {
                                hitsList.Add(hit);
                            }
                        }
                        break;
                }
            }
        }
        hitsList.OrderByDescending(x => GameUtil.DistanceSquare2D(x.point, from));
        return hitsList.FirstOrDefault();
    }
}
