using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubWordList : AegisubTime, IWordList
    {
        public required List<IWord> Words { get; set; }
        IReadOnlyList<IWord> IWordList.Words => Words;
    }
}
