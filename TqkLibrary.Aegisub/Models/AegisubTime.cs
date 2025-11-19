using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubTime : IAegisubTime
    {
        public required TimeSpan Start { get; set; }
        public required TimeSpan End { get; set; }
    }
}
