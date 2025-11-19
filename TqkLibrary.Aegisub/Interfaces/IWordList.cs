namespace TqkLibrary.Aegisub.Interfaces
{
    public interface IWordList : ITime
    {
        IReadOnlyList<IWord> Words { get; }
    }
}
