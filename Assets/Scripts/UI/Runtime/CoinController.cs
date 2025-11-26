using TMPro;
using UnityEngine;
using VirtueSky.Variables;

namespace DefaultNamespace
{
    public class CoinController : MonoBehaviour
    {
        [SerializeField] IntegerVariable coinVariable;

        [Header("GameObject")]
        [SerializeField] TMP_Text coinText;

        private void OnEnable()
        {
            coinVariable.AddListener(CoinVariableOnRaised);
        }

        private void Start()
        {
            CoinVariableOnSet(coinVariable.Value);
        }

        private void CoinVariableOnSet(int obj)
        {
            coinText.text = obj.ToString();
        }
        
        private void CoinVariableOnRaised(int obj)
        {
            coinText.text = obj.ToString();
        }

        private void OnDisable()
        {
            coinVariable.RemoveListener(CoinVariableOnRaised);
        }
    }
}