partial class Program
{
    public class Word
    {
        public string Value;
        public int Index;
        public int MaxLetterLength;

        public Word(string value, int index, int maxLetterLength)
        {
            Value = value;
            Index = index;
            MaxLetterLength = maxLetterLength;
        }
    }
}