
public class Word
{
    public int Index;
    public int MaxLetterLength;
    public int StartLetterCount;
    public int EndLetterCount;
    public int WordLength;

    public Word(int index, int wordLength, int maxLetterLength, int startLetterCount, int endLetterCount)
    {
        Index = index;
        MaxLetterLength = maxLetterLength;
        StartLetterCount = startLetterCount;
        EndLetterCount = endLetterCount;
        WordLength = wordLength;
    }
    public Word(string value, int index, int wordLength, int maxLetterLength)
    {
        Index = index;
        MaxLetterLength = maxLetterLength;
        WordLength = wordLength;
    }
}