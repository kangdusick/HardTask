using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] RectTransform shootingStartPoint;
    [SerializeField] RectTransform point1;
    [SerializeField] RectTransform point2;
    private void Awake()
    {
        TouchManager.Instance.OnTouchIng+= ShootLaser;
    }

    void ShootLaser(Vector2 screenPos)
    {
        var laserDirection = TouchManager.Instance.mouseWorldPos - shootingStartPoint.transform.position;
        laserDirection.z = 0f;
        laserDirection = laserDirection.normalized;
        // 레이저 발사
        var hits = Physics2D.RaycastAll(shootingStartPoint.position, laserDirection, 1000f);

        List<RaycastHit2D> hitsList = new();
        foreach (var hit in hits) 
        {
             // 충돌한 경우
            if (hit.collider != null)
            {
                switch ((ELayers)hit.collider.gameObject.layer)
                {
                    case ELayers.Wall:
                        hitsList.Add(hit);
                        break;
                    case ELayers.HexBlockContainer:
                        var hexBlockContainer = hit.collider.gameObject.GetCashComponent<HexBlockContainer>();
                        if(!ReferenceEquals(hexBlockContainer.hexBlock,null))
                        {
                            hitsList.Add(hit);
                        }
                        break;
                }
            }
        }
        if(hitsList.Count > 0 ) 
        {
            hitsList.OrderByDescending(x => GameUtil.DistanceSquare2D(x.point, shootingStartPoint.position));
            point1.transform.position = hitsList[0].point;
            if (hitsList[0].collider.gameObject.layer == (int)ELayers.Wall)
            {
                // 반사된 레이저 발사
                Vector3 hitPoint = hitsList[0].point;
                Vector3 normal = hitsList[0].normal;
                Debug.Log(normal);
                Vector3 incomingVec = laserDirection;
                Vector3 reflectVec = Vector3.Reflect(incomingVec, normal);

                var ReflectHits = Physics2D.RaycastAll(hitPoint, reflectVec, 1000f);

                List<RaycastHit2D> hitsList2 = new();
                foreach (var hit in ReflectHits)
                {
                    // 충돌한 경우
                    if (hit.collider != null)
                    {
                        switch ((ELayers)hit.collider.gameObject.layer)
                        {
                            case ELayers.HexBlockContainer:
                                var hexBlockContainer = hit.collider.gameObject.GetCashComponent<HexBlockContainer>();
                                if (!ReferenceEquals(hexBlockContainer.hexBlock, null))
                                {
                                    hitsList2.Add(hit);
                                }
                                break;
                        }
                    }
                }

                if (hitsList2.Count > 0)
                {
                    hitsList.OrderByDescending(x => GameUtil.DistanceSquare2D(x.point, hitPoint));
                    point2.transform.position = hitsList2[0].point;
                }

            }
        }
    }
}
