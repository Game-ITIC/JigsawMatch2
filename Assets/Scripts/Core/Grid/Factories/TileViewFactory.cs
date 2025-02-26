using System;
using System.Collections.Generic;
using Core.Grid.Interfaces;
using Core.Grid.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Grid.Factories
{
    [CreateAssetMenu(fileName = nameof(TileViewFactory), menuName = "Game/" + nameof(TileViewFactory))]
    public class TileViewFactory : ScriptableObject, ITileViewFactory
    {
        [System.Serializable]
        public class TileViewMapping
        {
            public string tileId;
            public GameObject prefab;
            public int poolSize = 5;
        }

        [SerializeField] private List<TileViewMapping> tileMappings;

        private Dictionary<string, Queue<GameObject>> _poolDict = new ();

        private Dictionary<string, GameObject> _prefabDict = new();

        private bool _initialized = false;

        public async UniTask WarmUp()
        {
            InitializeFactory();
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
        }

        private void InitializeFactory()
        {
            _poolDict = new Dictionary<string, Queue<GameObject>>();
            _prefabDict = new Dictionary<string, GameObject>();

            if (tileMappings == null) return;

            foreach (var mapping in tileMappings)
            {
                if (string.IsNullOrEmpty(mapping.tileId))
                {
                    Debug.LogWarning("Mapping has not TileId");
                    continue;
                }

                if (mapping.prefab == null)
                {
                    Debug.LogWarning("Mapping has not Prefab");
                    continue;
                }

                _prefabDict[mapping.tileId] = mapping.prefab;
                var queue = new Queue<GameObject>();
                _poolDict[mapping.tileId] = queue;

                for (int i = 0; i < mapping.poolSize; i++)
                {
                    var obj = Instantiate(mapping.prefab);
                    obj.SetActive(false);

                    var tileView = obj.GetComponent<TileView>();
                    if (tileView != null)
                    {
                        tileView.TileId = mapping.tileId;
                    }

                    queue.Enqueue(obj);
                }
            }
        }

        public GameObject GetTileView(string tileId)
        {
            if (!_poolDict.ContainsKey(tileId))
            {
                Debug.LogWarning($"TileViewFactory: no prefab found for tileId = {tileId}. Using fallback.");
                tileId = GetFallbackTileId();
            }

            var queue = _poolDict[tileId];
            if (queue.Count > 0)
            {
                var go = queue.Dequeue();
                go.SetActive(true);
                return go;
            }
            else
            {
                var prefab = _prefabDict[tileId];
                var obj = Instantiate(prefab);
                var tileView = obj.GetComponent<TileView>();
                if (tileView != null)
                {
                    tileView.TileId = tileId;
                }

                return obj;
            }
        }

        public void ReleaseTileView(GameObject tileGameObject)
        {
            if (tileGameObject == null) return;

            var tileView = tileGameObject.GetComponent<TileView>();
            string tileId = tileView != null ? tileView.TileId : GetFallbackTileId();

            tileGameObject.SetActive(false);

            if (!_poolDict.ContainsKey(tileId))
            {
                Debug.LogWarning($"TileViewFactory: no pool for tileId {tileId}. Can't release properly.");

                Destroy(tileGameObject);
                return;
            }

            _poolDict[tileId].Enqueue(tileGameObject);
        }

        private string GetFallbackTileId()
        {
            foreach (var kvp in _prefabDict)
            {
                return kvp.Key;
            }

            return "Unknown";
        }
    }
}