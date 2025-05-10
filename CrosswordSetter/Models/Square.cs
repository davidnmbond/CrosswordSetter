namespace CrosswordSetter.Models;

public class Square
{
	public const string Black = "#";
	public const string Blank = "";
	private string _character = Black;

	public string Character
	{
		get => _character;
		set => _character = value.ToUpperInvariant();
	}

	public bool IsWhite => Character is not Black;

	public int? Number { get; set; }
}
