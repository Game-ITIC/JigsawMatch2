using System;
using System.Collections.Generic;
using Core.Grid.Interfaces;
using Core.Grid.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Grid.Factories
{
    [CreateAssetMenu(fileName = nameof(CellViewFactory), menuName = "Game/" + nameof(CellViewFactory))]
    public class CellViewFactory : ScriptableObject
    {
        [SerializeField] private CellView cellViewPrefab;

        public GameObject CreateCellView(ICell cell)
        {
            var cellView = Instantiate(cellViewPrefab);
            
            cellView.LogicalCell = cell;
            cellView.UpdateVisual();
            
            return cellView.gameObject;
        }
    }
}