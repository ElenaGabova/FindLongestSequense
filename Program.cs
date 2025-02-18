using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

class Program
{

    /// <summary>
    /// Список слов для поиска самой длинной последовательности повторяющихся символов.
    /// </summary>
    private static string[] words;

    static async Task Main()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        long memoryBefore = GC.GetTotalMemory(true); // Память до выполнения

        words = ReadWords();
        var result = FindLongestSequence(words);
        Console.WriteLine("Максимальная длина подстроки из одного символа ({0}): {1}", result.Key, result.Value);
        words = await ReadWordsFromFile();
        result = FindLongestSequence(words);
        Console.WriteLine("Максимальная длина подстроки из одного символа ({0}): {1}", result.Key, result.Value);

        long memoryAfter = GC.GetTotalMemory(true); // Память после выполнения
        stopwatch.Stop();
        Console.WriteLine($"Использовано памяти: {(double)((memoryAfter - memoryBefore) / 1000000)} мегабайт");
        Console.WriteLine($"Время выполнения: {(stopwatch.ElapsedMilliseconds)} мс");
        Console.ReadLine();
    }

    /// <summary>
    /// Читаем данные из массива.
    /// </summary>
    /// <returns></returns>
    static string[] ReadWords()
    {
        string[] input = { "caa", "aac", "ccc", "bbbb", "fff", "faf", "ddd", "da", "ccd" };
        return input;
    }

    /// <summary>
    /// Читаем данные из файла.
    /// </summary>
    /// <returns></returns>
    static async Task<string[]> ReadWordsFromFile()
    {
        List<string> input = new List<string>();
        using (StreamReader reader = new StreamReader("Data (1).txt", true))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line != null)
                    input.Add(line);
            }
        }
        return input.ToArray();
    }

    /// <summary>
    /// Поиск самой длинной последовательности одинаковых символов.
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    private static KeyValuePair<string, int> FindLongestSequence(string[] words)
    {
        var groupWords = GetGroupWordsDiconary(words);
        var result = GetGroupWordsLongestSequenceLength(groupWords);

        return new KeyValuePair<string, int>(result.Key, result.Value);
    }

    /// <summary>
    /// Группируем слова и получаем словарь, где ключ - символ, значение -> слова, в которых есть этот символ, и количество символов.
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    private static Dictionary<char, List<Word>> GetGroupWordsDiconary(string[] words)
    {
        int index = 0;

        //Список, ключом является буква, значением где она встречается, и сколько раз она встречается.
        Dictionary<char, List<Word>> groupWords = new Dictionary<char, List<Word>>();
        foreach (string word in words)
        {
            foreach (char c in word.Distinct())
            {
                int startLetterCount = 0;
                int endLetterCount = 0;
                bool firstPrioritry = false;

                if (word.StartsWith(c))
                    startLetterCount = CountLeadingRepeatingCharacters(word, c);

                if (word.EndsWith(c))
                    endLetterCount = CountLeadingRepeatingCharactersEndWord(word, c);

                if (startLetterCount > 0 && endLetterCount > 0)
                {
                    if (startLetterCount >= endLetterCount)
                        firstPrioritry = true;
                }

                int maxLetterLength = MaxLetterLength(word, c);

                var keyValuePair = new Word(index, word.Length, maxLetterLength, startLetterCount, endLetterCount);
                if (groupWords.ContainsKey(c))
                    groupWords[c].Add(keyValuePair);
                else
                    groupWords.Add(c, new List<Word>() { keyValuePair });
            }

            index++;
        }
        return groupWords;
    }

    /// <summary>
    /// Получаем длину максимальной последовательности одинаковых символов
    /// </summary>
    /// <param name="groupWords"></param>
    /// <returns></returns>
    private static KeyValuePair<string, int> GetGroupWordsLongestSequenceLength(Dictionary<char, List<Word>> groupWords)
    {
        int maxLength = 0;
        string maxSequence = null;
        KeyValuePair<string, int> result;

        foreach (var groupWord in groupWords)
        {
            char letter = groupWord.Key;
            var words = groupWord.Value;

            //Сортируем слова по первому и последнему символу, чтобы правильно соединять слова
            result = FindMaxSymbolPermutation(words, letter);

            if (result.Value > maxLength)
            {
                maxLength = result.Value;
                maxSequence = result.Key;
            }
        }
        return new KeyValuePair<string, int>(maxSequence, maxLength);
    }

    private static KeyValuePair<string, int> FindMaxSymbolPermutation(IEnumerable<Word> wordsWithSymbol, char ch)
    {
        //Находим самую длинную подстроку, которая заканчивается с ch
        var firstWord = wordsWithSymbol.Where(w => w.EndLetterCount >= w.StartLetterCount &&
                                                   w.EndLetterCount > 0 &&
                                                   w.WordLength != w.MaxLetterLength)
            .OrderByDescending(w => w.EndLetterCount).FirstOrDefault();

        //Находим самую длинную подстроку, которая начинается  с ch
        var lastWord = wordsWithSymbol.Where(w => w.StartLetterCount >= w.EndLetterCount &&
                                                  w.StartLetterCount > 0 &&
                                                  w.WordLength != w.MaxLetterLength &&
                                                  w.Index != firstWord?.Index)
            .OrderByDescending(w => w.StartLetterCount)
            .FirstOrDefault();

        int maxRepeat = 0;

        //Находим все остальные строки (все символы совпдают с ch)
        var middleWordsCount = wordsWithSymbol
            .Where(w => w.WordLength == w.MaxLetterLength)
            .Select(w => w.WordLength)
            .Sum();

        int maxLength = 0;
        string maxSequence = string.Empty;
        int result = middleWordsCount;

        if (firstWord?.EndLetterCount != null)
            result += firstWord.EndLetterCount;
        if (lastWord?.EndLetterCount != null)
            result += lastWord.StartLetterCount;
        if (result > maxLength)
        {
            maxLength = result;
            maxSequence = MergeWords(firstWord, new string(ch, middleWordsCount), lastWord);
        }

        //Находим самую длинную строку повторяющихся сивмолов.
        var maxLetterLength = wordsWithSymbol.OrderByDescending(w => w.MaxLetterLength).FirstOrDefault();
        if (maxLetterLength.MaxLetterLength > maxLength)
            return new KeyValuePair<string, int>(words[maxLetterLength.Index],
                                                 maxLetterLength.MaxLetterLength);

        return new KeyValuePair<string, int>(maxSequence, maxLength);
    }

    /// <summary>
    /// Объединяем слова для конкретного символа, учитывая возможные стыки одинаковых символов.
    /// </summary>
    private static string MergeWords(Word firstWord, string middleWords, Word lastWord)
    {
        StringBuilder mergedWords = new StringBuilder();
        if (firstWord != null)
            mergedWords.Append(words[firstWord.Index]);

        if (middleWords != null)
            mergedWords.Append(middleWords);

        if (lastWord != null)
            mergedWords.Append(words[lastWord.Index]);

        return mergedWords.ToString();
    }

    private static int MaxLetterLength(string str, char letter)
    {
        if (string.IsNullOrEmpty(str)) return 0;

        int maxLen = 0, currentLen = 0;
        ReadOnlySpan<char> span = str.AsSpan();

        foreach (char c in span)
        {
            if (c == letter)
            {
                currentLen++;
                maxLen = Math.Max(maxLen, currentLen);
            }
            else
            {
                currentLen = 0;
            }
        }
        return maxLen;
    }

    /// <summary>
    /// Количество повторяющихся символов char в начале строки.
    /// </summary>
    /// <param name="word">строка символов</param>
    /// <param name="letter">повторяющейся символ</param>
    /// <returns></returns>
    private static int CountLeadingRepeatingCharacters(string word, char letter)
    {
        if (string.IsNullOrEmpty(word))
            return 0;

        int count = 1;

        for (int i = 1; i < word.Length; i++)
        {
            if (word[i] == letter)
                count++;
            else
                break;
        }
        return count;
    }

    /// <summary>
    /// Количество повторяющихся символов char в конце строки.
    /// </summary>
    /// <param name="word">строка символов</param>
    /// <param name="letter">повторяющейся символ</param>
    /// <returns></returns>
    private static int CountLeadingRepeatingCharactersEndWord(string word, char letter)
    {
        if (string.IsNullOrEmpty(word))
            return 0;

        int count = 1;

        for (int i = word.Length - 2; i > 1; i--)
        {
            if (word[i] == letter)
                count++;
            else
                break;
        }
        return count;
    }
}