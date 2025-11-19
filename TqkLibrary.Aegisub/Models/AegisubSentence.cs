using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubSentence : AegisubWordList, IAegisubSentence
    {
        public required string Text { get; set; }
        IReadOnlyList<IAegisubWord> IAegisubWordList.Words => Words;
        public override string ToString()
        {
            return $"{Start:hh\\:mm\\:ss\\.fff} - {End:hh\\:mm\\:ss\\.fff}: {Text}";
        }
    }
}
