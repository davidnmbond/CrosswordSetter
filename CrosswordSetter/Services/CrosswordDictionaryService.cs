
using CrosswordSetter.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace CrosswordSetter.Services;

public class CrosswordDictionaryService(
	HttpClient _httpClient) : ICrosswordDictionaryService
{
	private readonly List<string> _wordList = [];

	public async Task<bool> LoadDictionaryAsync(CancellationToken cancellationToken)
	{
		// Load the dictionary file from ./wwwroot/dictionary.txt using the HttpClient
		// and populate the _dictionary list with its contents
		var response = await _httpClient.GetAsync("dictionaries/en.txt", cancellationToken);
		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			_wordList.AddRange(content
				.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
				// Exclude entries that contain characters other than standard roman letters
				.Where(x => x.All(c => char.IsLetter(c) && c < 128))
				);
			if (_wordList.Count > 1000)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Takes input like "fi**ed" and returns a list of all dictionary words that match.
	/// In this example, it would return "fitted", "fished", etc.
	/// </summary>
	/// <param name="word"></param>
	public string[] SolveCrossword(string letters)
	{
		if (string.IsNullOrEmpty(letters))
		{
			return [];
		}

		if (_wordList.Count == 0)
		{
			throw new InvalidOperationException("Dictionary not loaded.");
		}

		var pattern = new StringBuilder();
		foreach (var c in letters)
		{
			if (c == '*')
			{
				pattern.Append("[a-zA-Z]");
				continue;
			}

			pattern.Append(c);
		}

		var randomStartIndex = Random.Shared.Next(0, _wordList.Count - 1);
		var result = new List<string>();
		for (var wordIndex = 0; wordIndex < _wordList.Count; wordIndex++)
		{
			var word = _wordList[(randomStartIndex + wordIndex) % _wordList.Count];
			if (Regex.IsMatch(word, pattern.ToString(), RegexOptions.IgnoreCase))
			{
				result.Add(word);

				if (result.Count > 10)
				{
					break;
				}
			}
		}

		return [.. result];
	}
}
