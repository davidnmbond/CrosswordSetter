
namespace CrosswordSetter.Interfaces;

public interface ICrosswordDictionaryService
{
	Task<bool> LoadDictionaryAsync(CancellationToken cancellationToken);
	string[] SolveCrossword(string letters);
}
