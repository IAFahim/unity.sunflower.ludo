using UnityEngine;

namespace ElProfesorKudo.Firebase.Common
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType(typeof(T)) as T;

                }
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString();
                }
                return _instance;
            }
        }
        protected virtual void Awake()
        {
            if (_instance == null)
                _instance = this as T;
            else if (_instance != this)
                Destroy(gameObject);
        }
    }
}
