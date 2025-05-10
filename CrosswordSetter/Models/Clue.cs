namespace CrosswordSetter.Models;

public class Clue
{
	public required int Number { get; set; }

	public required Direction Direction { get; set; }

	public required List<int> WordLengths { get; set; }

	public required string Text { get; set; }
}
