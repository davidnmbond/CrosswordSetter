namespace CrosswordSetter.Pages;

public partial class Home
{
	private record Clue(int StartRow, int StartCol, int EndRow, int EndCol, Direction Direction);
}
