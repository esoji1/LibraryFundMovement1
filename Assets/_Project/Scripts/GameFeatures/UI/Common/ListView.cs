using System.Collections.Generic;
using UnityEngine;

namespace _Project.GameFeatures.UI.Common
{
    public class ListView<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private T _itemPrefab;
        [SerializeField] private Transform _container;
        
        private readonly List<T> _items = new();
        private readonly Queue<T> _freeList = new();

        public T SpawnElement()
        {
            if (this._freeList.TryDequeue(out var item))
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item = Instantiate(this._itemPrefab, this._container);
            }
            
            this._items.Add(item);
            return item;
        }

        public void DespawnElement(T item)
        {
            if (item != null && this._items.Remove(item))
            {
                item.gameObject.SetActive(false);
                this._freeList.Enqueue(item);
            }
        }
        
        public void Clear()
        {
            for (int i = 0, count = this._items.Count; i < count; i++)
            {
                T item = this._items[i];
                item.gameObject.SetActive(false);
                this._freeList.Enqueue(item);
            }
            
            this._items.Clear();
        }
    }
}