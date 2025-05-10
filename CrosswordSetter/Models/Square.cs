namespace CrosswordSetter.Models;

public class Square
{
	public const string Black = "#";
	public const string Blank = "";
	private string _character = Black;

	public event EventHandler? NotifyChanged;

	public string Character
	{
		get => _character;
		set
		{
			_character = value.ToUpperInvariant();
			NotifyChanged?.Invoke(this, new());
		}
	}

	public bool IsWhite => Character is not Black;

	public int? Number { get; set; }
}
