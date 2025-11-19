using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubSentence : AegisubWordList, ISentence
    {
        public required string Text { get; set; }
        IReadOnlyList<IWord> IWordList.Words => Words;
    }
}
