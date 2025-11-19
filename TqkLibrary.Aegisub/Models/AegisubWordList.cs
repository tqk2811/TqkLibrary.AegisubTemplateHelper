using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubWordList : AegisubTime, IAegisubWordList
    {
        public required List<AegisubWord> Words { get; set; }
        IReadOnlyList<IAegisubWord> IAegisubWordList.Words => Words;
    }
}
