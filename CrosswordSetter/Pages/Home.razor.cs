using CrosswordSetter.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CrosswordSetter.Pages;

public partial class Home
{
	[Inject]
	private HttpClient _httpClient { get; set; } = default!;

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

	private bool _isGridLocked = false;
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
		// Load the dictionary file from ./wwwroot/dictionary.txt using the HttpClient
		// and populate the _dictionary list with its contents
		var response = await _httpClient.GetAsync("dictionaries/en.txt");
		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			_dictionary.AddRange(content
				.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
				// Exclude entries that contain characters other than standard roman letters
				.Where(x => x.All(c => char.IsLetter(c) && c < 128))
				);
			if (_dictionary.Count > 1000)
			{
				_isDictionaryLoaded = true;
			}
		}
		else
		{
			Console.WriteLine("Failed to load dictionary file.");
		}
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

		_clues.Clear(); // Clear the clue list when initializing
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
						// Calculate down word length
						var wordLength = 1;
						var downWordRowIndex = rowIndex;
						while (true)
						{
							downWordRowIndex++;
							if (downWordRowIndex == _gridSize || _crosswordGrid[downWordRowIndex, colIndex].Character == Square.Black)
							{
								break;
							}
						}

						_clues.Add(new Clue
						{
							Number = clueNumber,
							Direction = Direction.Down,
							WordLengths = [wordLength],
							Text = "Placeholder"
						});
					}

					if (isAcrossStart)
					{
						// Calculate across word length
						var wordLength = 1;
						var acrossWordColIndex = colIndex;
						while (true)
						{
							acrossWordColIndex++;
							if (acrossWordColIndex == _gridSize || _crosswordGrid[rowIndex, acrossWordColIndex].Character == Square.Black)
							{
								break;
							}
						}

						_clues.Add(new Clue
						{
							Number = clueNumber,
							Direction = Direction.Across,
							WordLengths = [wordLength],
							Text = "Placeholder"
						});
					}

					clueNumber++;
				}
			}
		}
	}
	private void FillGrid(MouseEventArgs args)
	{
		throw new NotImplementedException();
	}
}
