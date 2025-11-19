using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubWord : AegisubTime, IAegisubWord
    {
        public required string Word { get; set; }
    }
}
