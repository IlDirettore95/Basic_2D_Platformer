using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.Utility
{
    public class Grid<T>
    {
        public Vector2[,] CellsPositions { get; }
        public Vector2Int GridSize { get; }
        public Vector2 CellSize { get; }
        public Vector2 GridPosition { get; }

        private Cell<T>[,] cells;

        public Grid(Vector2Int gridSize, Vector2 cellSize, Vector2 gridPosition)
        {
            GridSize = gridSize;
            CellSize = cellSize;
            GridPosition = gridPosition;

            cells = new Cell<T>[gridSize.y, gridSize.x];
            CellsPositions = new Vector2[gridSize.y, gridSize.x];

            float yTranslation = (gridSize.y - 1) * cellSize.y / 2;
            float xTranslation = (gridSize.x - 1) * cellSize.x / 2;

            for (int i = 0; i < gridSize.y; i++)
            {
                for (int j = 0; j < gridSize.x; j++)
                {
                    Vector2 cellPosition = new Vector2(j * cellSize.x - xTranslation, i * cellSize.y - yTranslation) + gridPosition;
                    cells[i, j] = new Cell<T>(cellSize, cellPosition, new Vector2Int(i, j));
                    cells[i, j].Content = default(T);
                    CellsPositions[i, j] = cellPosition;
                }
            }
        }

        public Vector2 GetPosition(Vector2Int indicies)
        {
            if (indicies.y < 0 || indicies.x < 0 || indicies.y > GridSize.y - 1 || indicies.x > GridSize.x - 1)
            {
                throw new ArgumentException();
            }

            return CellsPositions[indicies.y, indicies.x];
        }

        public Vector2 GetPosition(int i, int j)
        {
            if (i < 0 || j < 0 || i > GridSize.y - 1 || j > GridSize.x - 1)
            {
                throw new ArgumentException();
            }

            return CellsPositions[i, j];
        }

        public Vector2Int GetIndicies(Vector2 position)
        {
            for (int i = 0; i < CellsPositions.GetLength(0); i++)
            {
                for (int j = 0; j < CellsPositions.GetLength(1); j++)
                {
                    if (CellsPositions[i, j] == position) return new Vector2Int(i, j);
                }
            }

            return new Vector2Int(-1, -1);
        }

        public T GetElement(int i, int j)
        {
            if (i < 0 || j < 0 || i > GridSize.y - 1 || j > GridSize.x - 1)
            {
                throw new ArgumentException();
            }

            return cells[i, j].Content;
        }

        public T GetElement(Vector2Int indicies)
        {
            if (indicies.y < 0 || indicies.x < 0 || indicies.y > GridSize.y - 1 || indicies.x > GridSize.x - 1)
            {
                throw new ArgumentException();
            }

            return cells[indicies.y, indicies.x].Content;
        }

        public void PlaceElement(int i, int j, T content)
        {
            if (i < 0 || j < 0 || i > GridSize.y - 1 || j > GridSize.x - 1)
            {
                throw new ArgumentException();
            }

            cells[i, j].Content = content;
        }

        public void PlaceElement(Vector2Int indicies, T content)
        {
            if (indicies.y < 0 || indicies.x < 0 || indicies.y > GridSize.y - 1 || indicies.x > GridSize.x - 1)
            {
                throw new ArgumentException();
            }

            cells[indicies.y, indicies.x].Content = content;
        }

        public void Draw()
        {
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    cells[i, j].Draw();
                }
            }
        }

        public void DrawContent(GameObject parent, int fontSize, Func<T, Color> colorHeuristic, Func<T, string> stringHeuristic)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject.Destroy(parent.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < GridSize.y; i++)
            {
                for (int j = 0; j < GridSize.x; j++)
                {
                    cells[i, j].DrawContent(parent, fontSize, colorHeuristic, stringHeuristic);
                }
            }
        }

        private class Cell<S>
        {
            public Vector2 Size;
            public Vector2 PositionInWorld;
            public Vector2 PositionInGrid;
            public S Content;

            public Cell(Vector2 positionInWorld, Vector2Int positionInGrid)
            {
                Size = new Vector2(1, 1);
                PositionInWorld = positionInWorld;
                PositionInGrid = positionInGrid;
            }

            public Cell(Vector2 size, Vector2 positionInWorld, Vector2Int positionInGrid)
            {
                Size = size;
                PositionInWorld = positionInWorld;
                PositionInGrid = positionInGrid;
            }

            public void Draw()
            {
                Debug.DrawLine(PositionInWorld + new Vector2(-Size.x / 2, Size.y / 2), PositionInWorld + new Vector2(Size.x / 2, Size.y / 2), Color.black);
                Debug.DrawLine(PositionInWorld + new Vector2(Size.x / 2, Size.y / 2), PositionInWorld + new Vector2(Size.x / 2, -Size.y / 2), Color.black);
                Debug.DrawLine(PositionInWorld + new Vector2(Size.x / 2, -Size.y / 2), PositionInWorld + new Vector2(-Size.x / 2, -Size.y / 2), Color.black);
                Debug.DrawLine(PositionInWorld + new Vector2(-Size.x / 2, -Size.y / 2), PositionInWorld + new Vector2(-Size.x / 2, Size.y / 2), Color.black);
            }

            public void DrawContent(GameObject parent, int fontSize, Func<S, Color> colorHeuristic, Func<S, string> stringHeuristic)
            {
                Color color = colorHeuristic.Invoke(Content);
                string text = stringHeuristic.Invoke(Content);
                TextUtility.CreateWorldText(text, fontSize, PositionInWorld, Size, color, parent.transform);
            }
        }
    }
}