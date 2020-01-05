using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public static class PhysicsCaster
    {
        const int HitMax = 16;

        /// <summary>
        /// 当たり判定配列
        /// </summary>
        public static readonly RaycastHit[] hits = new RaycastHit[HitMax];

        /// <summary>
        /// MapCollisionのGetMaskの値
        /// </summary>
        public static int MapCollisionLayer { get; private set; }
        /// <summary>
        /// MapCollider, MapTrigger, NaeのGetMaskの値
        /// </summary>
        public static int MapLayer { get; private set; }
        public const string GroundTag = "Ground";
        public const string DeadZoneTag = "DeadZone";

        public static void Init()
        {
            MapCollisionLayer = LayerMask.GetMask("MapCollision");
            MapLayer = LayerMask.GetMask("MapCollision", "MapTrigger", "Nae");
        }

        /// <summary>
        /// 指定の座標の真下にある地面を返します。ない場合はnullを返します。
        /// </summary>
        /// <param name="origin">調査開始座標</param>
        /// <param name="distance">チェックする距離</param>
        /// <returns>地面のオブジェクト。何もなければnull</returns>
        public static GameObject GetGround(Vector3 origin, float distance)
        {
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, distance, MapCollisionLayer);
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.CompareTag(GroundTag))
                {
                    return hits[i].collider.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定の座標の真下にある地面や水を返します。ない場合はnullを返します。
        /// </summary>
        /// <param name="origin">調査開始座標</param>
        /// <param name="distance">チェックする距離</param>
        /// <returns>地面のオブジェクト。何もなければnull</returns>
        public static GameObject GetGroundWater(Vector3 origin, float distance)
        {
            return GetGroundWater(origin, Vector3.down, distance);
        }

        /// <summary>
        /// 方向を指定して、地面と水面を探して返します。
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static GameObject GetGroundWater(Vector3 origin, Vector3 dir, float distance)
        {
            int hitCount = Physics.RaycastNonAlloc(origin, dir, hits, distance, MapCollisionLayer);
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.CompareTag(GroundTag)
                    || hits[i].collider.CompareTag(DeadZoneTag))
                {
                    return hits[i].collider.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// キャラクターコントローラーのキャストをします。
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="queryTriggerInteraction"></param>
        /// <returns></returns>
        public static int CharacterControllerCast(CharacterController chr, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction= QueryTriggerInteraction.UseGlobal)
        {
            float h = chr.height * 0.5f - chr.radius;
            return Physics.CapsuleCastNonAlloc(
                chr.bounds.center + Vector3.up * h,
                chr.bounds.center + Vector3.down * h,
                chr.radius,
                direction, hits, maxDistance, layerMask, queryTriggerInteraction);
        }
    }
}