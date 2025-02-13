using System.Collections.Generic;
using System.Text;

partial class Program
{

    /// <summary>
    /// Список слов для поиска самой длинной последовательности повторяющихся символов.
    /// </summary>
    private static List<string> words;

    static async Task Main()
    {
        words = ReadWords();
        var result = FindLongestSequence(words);
        Console.WriteLine("Максимальная длина подстроки из одного символа ({0}): {1}", result.Key, result.Value);
        words = await ReadWordsFromFile();
        result = FindLongestSequence(words);
        Console.WriteLine("Максимальная длина подстроки из одного символа ({0}): {1}", result.Key, result.Value);
        Console.ReadLine();
    }
    /// <summary>
    /// Читаем данные из массива.
    /// </summary>
    /// <returns></returns>
    static List<string> ReadWords()
    {
        string[] input = { "caa", "aac", "ccc", "bbbb", "fff", "faf", "ddd", "da", "ccd" };
        return input.ToList();
    }
    /// <summary>
    /// Читаем данные из файла.
    /// </summary>
    /// <returns></returns>
    static async Task<List<string>> ReadWordsFromFile()
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
        return input;
    }

    /// <summary>
    /// Поиск самой длинной последовательности одинаковых символов.
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    private static KeyValuePair<string, int> FindLongestSequence(List<string> words)
    {
        var groupWords = GetGroupWordsDiconary(words);
        return GetGroupWordsLongestSequenceLength(groupWords);
    }

    /// <summary>
    /// Группируем слова и получаем словарь, где ключ - символ, значение -> слова, в которых есть этот символ, и количество символов.
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    private static Dictionary<char, List<Word>> GetGroupWordsDiconary(List<string> words)
    {
        int index = 0;

        //Список, ключом является буква, значением где она встречается, и сколько раз она встречается.
        Dictionary<char, List<Word>> groupWords = new Dictionary<char, List<Word>>();
        foreach (string word in words)
        {
            foreach (char c in word.Distinct())
            {
                var keyValuePair = new Word(word, index, MaxLetterLength(word, c));
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
        string maxSequence = string.Empty;
        KeyValuePair<string, int> result;

        foreach (var groupWord in groupWords)
        {
            char letter = groupWord.Key;
            var words = groupWord.Value;
            //Сортируем слова по первому и последнему символу, чтобы правильно соединять слова
            words = words.OrderByDescending(w => w.Value).ToList();
            result = FindMaxSymbolPermutation(words, letter);

            if (result.Value > maxLength)
            {
                maxLength = result.Value;
                maxSequence = result.Key;
            }
        }
        return new KeyValuePair<string, int>(maxSequence, maxLength);
    }

    private static KeyValuePair<string, int> FindMaxSymbolPermutation(List<Word> words, char ch)
    {
        //Находим самую длинную подстроку, которая заканчивается с ch
        var firstWord = words.Where(w => w.Value.EndsWith(ch) && w.Value.Length != w.MaxLetterLength)
            .OrderByDescending(w =>
                CountLeadingRepeatingCharactersEndWord(w.Value, ch))
            .FirstOrDefault();

        //Находим самую длинную подстроку, которая начинается  с ch
        var lastWord = words.Where(w => w.Value.StartsWith(ch) &&
                                        w.Value.Length != w.MaxLetterLength &&
                                        w.Index != firstWord?.Index)
            .OrderByDescending(w =>
                CountLeadingRepeatingCharacters(w.Value, ch))
            .FirstOrDefault();

        //Находим все остальные строки (все символы совпдают с ch)
        var middleWords = words.Where(w => w.Value.Length == w.MaxLetterLength).Select(w => w.Value).ToList();

        int maxLength = 0;
        string maxSequence = string.Empty;
        int result = 0;

        var mergeWords = MergeWords(firstWord?.Value, middleWords, lastWord?.Value);
        result = MaxLetterLength(mergeWords, ch);
        if (result > maxLength)
        {
            maxLength = result;
            maxSequence = mergeWords;
        }

        //Находим самую длинную строку повторяющихся сивмолов.
        var maxLetterLength = words.OrderByDescending(w => w.MaxLetterLength).FirstOrDefault();
        if (maxLetterLength.MaxLetterLength > maxLength)
            return new KeyValuePair<string, int>(maxLetterLength.Value,
                                                 maxLetterLength.MaxLetterLength);

        return new KeyValuePair<string, int>(maxSequence, maxLength);
    }

    /// <summary>
    /// Объединяем слова для конкретного символа, учитывая возможные стыки одинаковых символов.
    /// </summary>
    private static string MergeWords(string firstWord, List<string> middleWords, string lastWord)
    {
        StringBuilder mergedWords = new StringBuilder();
        if (firstWord != null)
            mergedWords.Append(firstWord);

        foreach (var item in middleWords)
            mergedWords.Append(item);

        if (lastWord != null)
            mergedWords.Append(lastWord);
        return mergedWords.ToString();
    }

    /// <summary>
    /// Проходим по строке,  ищем самую длинную повторяющаяся последовательность символа char
    /// </summary>
    /// <param name="str">строка символов</param>
    /// <param name="letter">повторяющейся символ</param>
    /// <returns>Максимальное количество повторяющихся символов letter</returns>
    private static int MaxLetterLength(string str, char letter)
    {
        int maxLen = 0;
        int currentLen = 0;
        foreach (char c in str)
        {
            if (c.Equals(letter))
                currentLen++;
            else
                currentLen = 0;
            maxLen = Math.Max(maxLen, currentLen);
        }
        return maxLen;
    }

    /// <summary>
    /// Количество повторяющихся символов char в начале строки.
    /// </summary>
    /// <param name="word">строка символов</param>
    /// <param name="letter">повторяющейся символ</param>
    /// <returns></returns>
    static int CountLeadingRepeatingCharacters(string word, char letter)
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
    static int CountLeadingRepeatingCharactersEndWord(string word, char letter)
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