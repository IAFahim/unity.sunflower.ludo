using System;
using UnityEngine;
using VirtueSky.ObjectPooling;

namespace Pools
{
    public class PoolSetup : MonoBehaviour
    {
        public PoolData poolData;
        private void OnEnable()
        {
            Pool.InitPool();
            poolData.PreSpawn();
        }

        private void OnDisable()
        {
            Pool.DestroyAll();
        }
    }
}