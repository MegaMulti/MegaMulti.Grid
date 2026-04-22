using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaMulti.GenericGridSystem
{
	public class GenericGrid<T>
	{
		public readonly int MaxWidth;
		public readonly int MaxHeight;
		public readonly Vector2 CellSize;
		public readonly Vector3 Offset;

		private readonly Dictionary<Vector2Int, T> cells;
		private readonly Dictionary<T, Vector2Int> reverseCells;

		public GenericGrid(int maxWidth = 0, int maxHeight = 0, Vector2 cellSize = default, Vector3 offset = default)
		{
			if (cellSize == Vector2.zero)
				cellSize = Vector2.one;
			
			if (cellSize.x <= 0 || cellSize.y <= 0)
				throw new ArgumentException("MegaMulti.Grid: cellSize must be positive");
			
			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
			CellSize = cellSize;
			Offset = offset;

			cells = new Dictionary<Vector2Int, T>();
			reverseCells = new Dictionary<T, Vector2Int>();
		}

		#region Helpers
		
		public static Vector3 CalculateWorldPosition(int x, int y, Vector2 cellSize, Vector3 offset) =>
			new Vector3(x * cellSize.x, y * cellSize.y, 0f) + offset;

		public static Vector3 CalculateWorldPosition(Vector2Int gridPosition, Vector2 cellSize, Vector3 offset) =>
			CalculateWorldPosition(gridPosition.x, gridPosition.y, cellSize, offset);
		
		public static Vector2Int CalculateGridPosition(Vector3 worldPosition, Vector2 cellSize, Vector3 offset) =>
			new Vector2Int(
				Mathf.FloorToInt((worldPosition.x - offset.x) / cellSize.x),
				Mathf.FloorToInt((worldPosition.y - offset.y) / cellSize.y)
			);
		
		#endregion


		#region Basic Methods

		public bool TryGetWorldPosition(int x, int y, out Vector3 worldPosition)
		{
			if (!IsPositionValid(x, y))
			{
				worldPosition = default;
				return false;
			}

			worldPosition = CalculateWorldPosition(x, y, CellSize, Offset);
			return true;
		}

		public bool TryGetWorldPosition(Vector2Int gridPosition, out Vector3 worldPosition) =>
			TryGetWorldPosition(gridPosition.x, gridPosition.y, out worldPosition);

		public bool TryGetGridPosition(Vector3 worldPosition, out Vector2Int gridPosition)
		{
			if (!IsPositionValid(worldPosition))
			{
				gridPosition = default;
				return false;
			}

			gridPosition = CalculateGridPosition(worldPosition, CellSize, Offset);
			return true;
		}

		public bool TryGetGridPosition(T value, out Vector2Int gridPosition) =>
			reverseCells.TryGetValue(value, out gridPosition);
		
		public bool TrySet(int x, int y, T value)
		{
			if (!IsPositionValid(x, y))
				return false;

			var pos = new Vector2Int(x, y);

			if (cells.TryGetValue(pos, out T oldValue))
			{
				reverseCells.Remove(oldValue);
			}

			if (reverseCells.TryGetValue(value, out Vector2Int oldPos))
			{
				cells.Remove(oldPos);
			}

			cells[pos] = value;
			reverseCells[value] = pos;

			return true;
		}
		
		public bool TrySet(Vector3 worldPosition, T value) =>
			TrySet(CalculateGridPosition(worldPosition, CellSize, Offset), value);

		public bool TrySet(Vector2Int gridPosition, T value) =>
			TrySet(gridPosition.x, gridPosition.y, value);

		public bool TryGet(int x, int y, out T value)
		{
			if (!IsPositionValid(x, y))
			{
				value = default;
				return false;
			}

			return cells.TryGetValue(new Vector2Int(x, y), out value);
		}
		
		public bool TryGet(Vector3 worldPosition, out T value) =>
			TryGet(CalculateGridPosition(worldPosition, CellSize, Offset), out value);

		public bool TryGet(Vector2Int gridPosition, out T value) =>
			TryGet(gridPosition.x, gridPosition.y, out value);

		public bool TryRemove(int x, int y)
		{
			if (!IsPositionValid(x, y))
				return false;

			var pos = new Vector2Int(x, y);

			if (!cells.Remove(pos, out T value))
				return false;

			reverseCells.Remove(value);

			return true;
		}

		public bool TryRemove(Vector3 worldPosition) =>
			TryRemove(CalculateGridPosition(worldPosition, CellSize, Offset));

		public bool TryRemove(Vector2Int gridPosition) =>
			TryRemove(gridPosition.x, gridPosition.y);

		public bool TryRemove(T value)
		{
			if (!TryGetGridPosition(value, out Vector2Int pos))
				return false;

			return TryRemove(pos);
		}
			

		public bool Contains(int x, int y)
		{
			if (!IsPositionValid(x, y))
			{
				return false;
			}

			return cells.ContainsKey(new Vector2Int(x, y));
		}
		
		public bool Contains(Vector3 worldPosition) =>
			Contains(CalculateGridPosition(worldPosition, CellSize, Offset));

		public bool Contains(Vector2Int gridPosition) =>
			Contains(gridPosition.x, gridPosition.y);

		public bool Contains(T value)
		{
			if (!TryGetGridPosition(value, out Vector2Int pos))
				return false;

			return Contains(pos);
		}

		public bool IsPositionValid(int x, int y)
		{
			bool validX = MaxWidth <= 0 || (x >= 0 && x < MaxWidth);
			bool validY = MaxHeight <= 0 || (y >= 0 && y < MaxHeight);

			return validX && validY;
		}

		public bool IsPositionValid(Vector2Int gridPosition) =>
			IsPositionValid(gridPosition.x, gridPosition.y);

		public bool IsPositionValid(Vector3 worldPosition) =>
			IsPositionValid(CalculateGridPosition(worldPosition, CellSize, Offset));

		#endregion

		#region Move Methods

		public bool TryMove(int fromX, int fromY, int toX, int toY)
		{
			if (!IsPositionValid(fromX, fromY) || !IsPositionValid(toX, toY))
				return false;

			var from = new Vector2Int(fromX, fromY);
			var to = new Vector2Int(toX, toY);

			if (!cells.TryGetValue(from, out T value))
				return false;

			if (from == to)
				return true;

			if (cells.ContainsKey(to))
				return false;

			cells.Remove(from);
			cells[to] = value;
			reverseCells[value] = to;

			return true;
		}

		public bool TryMove(Vector2Int fromGridPosition, Vector2Int toGridPosition) =>
			TryMove(fromGridPosition.x, fromGridPosition.y, toGridPosition.x, toGridPosition.y);

		public bool TryMove(int fromX, int fromY, Vector2Int toGridPosition) =>
			TryMove(fromX, fromY, toGridPosition.x, toGridPosition.y);

		public bool TryMove(Vector2Int fromGridPosition, int toX, int toY) =>
			TryMove(fromGridPosition.x, fromGridPosition.y, toX, toY);

		public bool TryMove(Vector3 fromWorldPosition, Vector3 toWorldPosition) =>
			TryMove(
				CalculateGridPosition(fromWorldPosition, CellSize, Offset),
				CalculateGridPosition(toWorldPosition, CellSize, Offset)
			);

		public bool TryMove(Vector3 fromWorldPosition, Vector2Int toGridPosition) =>
			TryMove(CalculateGridPosition(fromWorldPosition, CellSize, Offset), toGridPosition);

		public bool TryMove(Vector2Int fromGridPosition, Vector3 toWorldPosition) =>
			TryMove(fromGridPosition, CalculateGridPosition(toWorldPosition, CellSize, Offset));

		public bool TryMoveDirection(int fromX, int fromY, int directionX, int directionY) =>
			TryMove(fromX, fromY, fromX + directionX, fromY + directionY);

		public bool TryMoveDirection(int fromX, int fromY, Vector2Int direction) =>
			TryMoveDirection(fromX, fromY, direction.x, direction.y);

		public bool TryMoveDirection(Vector2Int fromGridPosition, Vector2Int direction) =>
			TryMoveDirection(fromGridPosition.x, fromGridPosition.y, direction.x, direction.y);

		public bool TryMoveDirection(Vector3 fromWorldPosition, int directionX, int directionY) =>
			TryMoveDirection(CalculateGridPosition(fromWorldPosition, CellSize, Offset), new Vector2Int(directionX, directionY));

		public bool TryMoveDirection(Vector3 fromWorldPosition, Vector2Int direction) =>
			TryMoveDirection(CalculateGridPosition(fromWorldPosition, CellSize, Offset), direction);

		public bool TryMoveUp(int fromX, int fromY) =>
			TryMoveDirection(fromX, fromY, 0, 1);

		public bool TryMoveUp(Vector2Int fromGridPosition) =>
			TryMoveUp(fromGridPosition.x, fromGridPosition.y);

		public bool TryMoveUp(Vector3 fromWorldPosition) =>
			TryMoveUp(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		public bool TryMoveDown(int fromX, int fromY) =>
			TryMoveDirection(fromX, fromY, 0, -1);

		public bool TryMoveDown(Vector2Int fromGridPosition) =>
			TryMoveDown(fromGridPosition.x, fromGridPosition.y);

		public bool TryMoveDown(Vector3 fromWorldPosition) =>
			TryMoveDown(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		public bool TryMoveRight(int fromX, int fromY) =>
			TryMoveDirection(fromX, fromY, 1, 0);

		public bool TryMoveRight(Vector2Int fromGridPosition) =>
			TryMoveRight(fromGridPosition.x, fromGridPosition.y);

		public bool TryMoveRight(Vector3 fromWorldPosition) =>
			TryMoveRight(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		public bool TryMoveLeft(int fromX, int fromY) =>
			TryMoveDirection(fromX, fromY, -1, 0);

		public bool TryMoveLeft(Vector2Int fromGridPosition) =>
			TryMoveLeft(fromGridPosition.x, fromGridPosition.y);

		public bool TryMoveLeft(Vector3 fromWorldPosition) =>
			TryMoveLeft(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		#endregion

		#region Additional Methods
		
		public IEnumerable<KeyValuePair<Vector2Int, T>> GetAllKeyValuePairs() => cells;

		public IEnumerable<Vector2Int> GetAllPositions() => cells.Keys;

		public IEnumerable<T> GetAllCells() => cells.Values;

		public void Clear()
		{
			cells.Clear();
			reverseCells.Clear();
		}

		#endregion
	}
}
