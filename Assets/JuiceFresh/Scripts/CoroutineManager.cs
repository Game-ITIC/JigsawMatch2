// CoroutineManager.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JuiceFresh.Scripts
{
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager _instance;

        public static CoroutineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineManager");
                    _instance = go.AddComponent<CoroutineManager>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }


        private Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();

        public Coroutine StartManagedCoroutine(string name, IEnumerator routine)
        {
            StopManagedCoroutine(name);

            var coroutine = StartCoroutine(routine);
            _activeCoroutines[name] = coroutine;
            return coroutine;
        }

        
        public void StopManagedCoroutine(string name)
        {
            if (_activeCoroutines.ContainsKey(name))
            {
                if (_activeCoroutines[name] != null)
                    StopCoroutine(_activeCoroutines[name]);

                _activeCoroutines.Remove(name);
            }
        }
        
        public void StopManagedCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);

            // Удалить из словаря
            string keyToRemove = null;
            foreach (var pair in _activeCoroutines)
            {
                if (pair.Value == coroutine)
                {
                    keyToRemove = pair.Key;
                    break;
                }
            }

            if (keyToRemove != null)
                _activeCoroutines.Remove(keyToRemove);
        }

        
        public void StopAllManagedCoroutines()
        {
            StopAllCoroutines();
            _activeCoroutines.Clear();
        }

        
        public bool IsCoroutineRunning(string name)
        {
            return _activeCoroutines.ContainsKey(name) && _activeCoroutines[name] != null;
        }
    }
}