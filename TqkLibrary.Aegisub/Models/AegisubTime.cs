using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubTime : ITime
    {
        public required TimeSpan Start { get; set; }
        public required TimeSpan End { get; set; }
    }
}
