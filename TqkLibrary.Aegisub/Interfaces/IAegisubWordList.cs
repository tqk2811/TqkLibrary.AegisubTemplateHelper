namespace TqkLibrary.Aegisub.Interfaces
{
    public interface IAegisubWordList : IAegisubTime
    {
        IReadOnlyList<IAegisubWord> Words { get; }
    }
}
