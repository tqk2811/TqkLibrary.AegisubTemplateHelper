using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubWord : AegisubTime, IAegisubWord
    {
        public required string Word { get; set; }
        public override string ToString()
        {
            return $"{Start:hh\\:mm\\:ss\\.fff} - {End:hh\\:mm\\:ss\\.fff}: {Word}";
        }
    }
}
