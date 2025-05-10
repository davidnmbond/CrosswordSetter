namespace CrosswordSetter.Models;

public class Square
{
	public const char Black = '#';
	public const char Blank = ' ';
	public char Character { get; set; } = Black;

	public bool IsWhite => Character is Blank;

	public int? Number { get; set; }
}
