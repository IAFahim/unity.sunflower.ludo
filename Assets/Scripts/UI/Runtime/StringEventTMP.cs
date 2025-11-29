using TMPro;
using UnityEngine;
using VirtueSky.Events;

namespace DefaultNamespace
{
    public class StringEventTMP : MonoBehaviour
    {
        public StringEvent stringEvent;
        public TMP_Text text;
        
        private void OnEnable()
        {
            stringEvent.AddListener(OnTestChange);
        }

        private void OnTestChange(string str)
        {
            text.text = str;
        }


        private void OnDisable()
        {
            stringEvent.RemoveListener(OnTestChange);
        }
    }
}