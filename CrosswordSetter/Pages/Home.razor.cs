using CrosswordSetter.Interfaces;
using CrosswordSetter.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace CrosswordSetter.Pages;

public partial class Home
{
	[Inject]
	private ICrosswordDictionaryService DictionaryService { get; set; } = default!;

	// Default grid size
	private const int DefaultGridSize = 15;

	// Default grid size
	private int _gridSize = DefaultGridSize;

	// Default grid size
	private Square[,] _crosswordGrid = new Square[DefaultGridSize, DefaultGridSize];

	// Clue list
	private readonly List<Clue> _clues = [];

	// Dictionary
	private readonly List<string> _dictionary = [];

	private bool _isGridLocked;
	private bool _isDictionaryLoaded;
	private int _whiteSquareCount = 0;

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

	protected override async Task OnInitializedAsync()
	{
		InitializeGrid();
		_isDictionaryLoaded = await DictionaryService.LoadDictionaryAsync(default);
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
				var newSquare = new Square
				{
					Character = Square.Black
				};
				newSquare.NotifyChanged += SquareUpdated;

				_crosswordGrid[i, j] = newSquare;
			}
		}

		_clues.Clear(); // Clear the clue list when initializing
	}

	private void SquareUpdated(object? sender, EventArgs e)
	{
		if (sender is not Square _)
		{
			return;
		}

		// Update the clue list when a square is updated
		UpdateClues();
		StateHasChanged();
	}


	private void ToggleSquareColor(int row, int col)
	{
		// Ensure the square at (row, col) is white
		var character = _crosswordGrid[row, col].IsWhite
			? Square.Black
			: Square.Blank;

		// Make the square at (row, col) white
		_crosswordGrid[row, col].Character = character;

		// Calculate and make the other three rotationally symmetric squares white
		int rotated90Row = col;
		int rotated90Col = _gridSize - 1 - row;
		_crosswordGrid[rotated90Row, rotated90Col].Character = character; // 90-degree rotation

		int rotated180Row = _gridSize - 1 - row;
		int rotated180Col = _gridSize - 1 - col;
		_crosswordGrid[rotated180Row, rotated180Col].Character = character; // 180-degree rotation

		int rotated270Row = _gridSize - 1 - col;
		int rotated270Col = row;
		_crosswordGrid[rotated270Row, rotated270Col].Character = character; // 270-degree rotation

		_whiteSquareCount += character switch
		{
			Square.Black => -4,
			Square.Blank => 4,
			_ => throw new InvalidOperationException("Invalid character")
		};

		// Update the little numbers in the top right
		UpdateClues();

		// Notify the UI to re-render
		StateHasChanged();
	}

	private void UpdateClues()
	{
		// Clear existing clues
		_clues.Clear();

		int clueNumber = 1;
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
						? clueNumber
						: null;

					if (isDownStart)
					{
						Process(clueNumber, rowIndex, colIndex, Direction.Down);
					}

					if (isAcrossStart)
					{
						Process(clueNumber, rowIndex, colIndex, Direction.Across);
					}

					clueNumber++;
				}
			}
		}
	}

	private void Process(
		int clueNumber,
		int rowIndex,
		int colIndex,
		Direction direction)
	{
		// Calculate down word length
		var answerStringBuilder = new StringBuilder();
		while (true)
		{
			answerStringBuilder.Append(_crosswordGrid[rowIndex, colIndex].Character);
			switch (direction)
			{
				case Direction.Across:
					colIndex++;
					break;
				case Direction.Down:
					rowIndex++;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}

			if (rowIndex == _gridSize || colIndex == _gridSize || _crosswordGrid[rowIndex, colIndex].Character == Square.Black)
			{
				break;
			}
		}

		var answer = answerStringBuilder.ToString();

		_clues.Add(new Clue
		{
			Answer = answer,
			Number = clueNumber,
			StartPosition = new Position
			{
				Row = rowIndex,
				Column = colIndex
			},
			Direction = direction,
			WordLengths = [answer.Length],
			Text = "Placeholder"
		});
	}

	private void FillGrid(MouseEventArgs args)
	{
		foreach (var clue in _clues)
		{
			// Write to console for debugging
			Console.WriteLine($"Filling grid for clue {clue.Number} {clue.Direction}");

			// Get the start position of the clue
			var startRow = clue.StartPosition.Row;
			var startCol = clue.StartPosition.Column;
			var direction = clue.Direction;

			// Read the grid in the direction of the clue for the length of the clue

			var totalLength = clue.WordLengths.Sum();
			var answerStringBuilder = new StringBuilder();
			for (int i = 0; i < totalLength; i++)
			{
				string letter;
				if (direction == Direction.Across)
				{
					letter = _crosswordGrid[startRow, startCol + i].Character;
				}
				else
				{
					letter = _crosswordGrid[startRow + i, startCol].Character;
				}

				answerStringBuilder.Append(letter switch
				{
					Square.Blank => ' ',
					_ => letter
				});
			}

			var possibleWords = DictionaryService.SolveCrossword(answerStringBuilder.ToString());

			if (possibleWords.Length > 0)
			{
				// Fill the grid with the first possible word
				var word = possibleWords[0];
				for (int i = 0; i < totalLength; i++)
				{
					if (direction == Direction.Across)
					{
						_crosswordGrid[startRow, startCol + i].Character = word[i].ToString();
					}
					else
					{
						_crosswordGrid[startRow + i, startCol].Character = word[i].ToString();
					}
				}
			}
			else
			{
				Console.WriteLine($"No words found for clue {clue.Number}");
			}
		}
	}
}
