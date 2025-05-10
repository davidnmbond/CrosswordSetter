using CrosswordSetter.Models;
using Microsoft.AspNetCore.Components.Web;

namespace CrosswordSetter.Pages;

public partial class Home
{
	// Default grid size
	private const int DefaultGridSize = 15;

	// Default grid size
	private int _gridSize = DefaultGridSize;

	// Default grid size
	private Square[,] _crosswordGrid = new Square[DefaultGridSize, DefaultGridSize];

	// Clue list
	private readonly List<Clue> _clueList = [];

	private int GridSize
	{
		get => _gridSize;
		set
		{
			if (value == _gridSize)
			{
				return;
			}

			_gridSize = value;
			InitializeGrid();
		}
	}

	protected override void OnInitialized()
	{
		InitializeGrid();
	}

	private void InitializeGrid()
	{
		if (_gridSize != _crosswordGrid.Length)
		{
			_crosswordGrid = new Square[_gridSize, _gridSize];
		}

		// Fill all squares with black
		for (int i = 0; i < _gridSize; i++)
		{
			for (int j = 0; j < _gridSize; j++)
			{
				_crosswordGrid[i, j] = new Square
				{
					Character = Square.Black
				};
			}
		}

		_clueList.Clear(); // Clear the clue list when initializing
	}


	private void EnsureSquareWhite(int row, int col)
	{
		// Ensure the square at (row, col) is white
		if (_crosswordGrid[row, col].IsWhite)
		{
			return;
		}

		// Make the square at (row, col) white
		_crosswordGrid[row, col].Character = Square.Blank;

		// Calculate and make the other three rotationally symmetric squares white
		int rotated90Row = col;
		int rotated90Col = _gridSize - 1 - row;
		_crosswordGrid[rotated90Row, rotated90Col].Character = Square.Blank; // 90-degree rotation

		int rotated180Row = _gridSize - 1 - row;
		int rotated180Col = _gridSize - 1 - col;
		_crosswordGrid[rotated180Row, rotated180Col].Character = Square.Blank; // 180-degree rotation

		int rotated270Row = _gridSize - 1 - col;
		int rotated270Col = row;
		_crosswordGrid[rotated270Row, rotated270Col].Character = Square.Blank; // 270-degree rotation

		// Update the little numbers in the top right
		UpdateNumbers();

		// Notify the UI to re-render
		StateHasChanged();
	}

	private void UpdateNumbers()
	{
		int number = 1;
		for (int rowIndex = 0; rowIndex < _gridSize; rowIndex++)
		{
			for (int colIndex = 0; colIndex < _gridSize; colIndex++)
			{
				if (_crosswordGrid[rowIndex, colIndex].IsWhite)
				{
					var isfirstRow = rowIndex == 0;
					var isLastRow = rowIndex == _gridSize - 1;
					var isfirstCol = colIndex == 0;
					var isLastCol = colIndex == _gridSize - 1;
					var isDownStart = (isfirstRow || _crosswordGrid[rowIndex - 1, colIndex].Character == Square.Black) &&
						(!isLastRow && _crosswordGrid[rowIndex + 1, colIndex].Character == Square.Blank);
					var isAcrossStart = (isfirstCol || _crosswordGrid[rowIndex, colIndex - 1].Character == Square.Black) &&
					(!isLastCol && _crosswordGrid[rowIndex, colIndex + 1].Character == Square.Blank);

					_crosswordGrid[rowIndex, colIndex].Number = isDownStart || isAcrossStart
						? number++
						: null;
				}
			}
		}
	}

	private void SaveAsJson(MouseEventArgs args)
	{
		throw new NotImplementedException();
	}

	private void SaveAsPng(MouseEventArgs args)
	{
		throw new NotImplementedException();
	}

	private void SaveAsSvg(MouseEventArgs args)
	{
		throw new NotImplementedException();
	}
}
