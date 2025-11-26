using UnityEngine;
using VirtueSky.Ads;
using VirtueSky.Inspector;

namespace DefaultNamespace
{
    public class AdFire : MonoBehaviour
    {
        public AdmobRewardVariable admobRewardVariable;

        private void Awake()
        {
            admobRewardVariable.Init();
        }

        public void Start()
        {
            admobRewardVariable.Load();
        }

        [Button]
        public void Show()
        {
            admobRewardVariable.Show();
        }
    }
}