using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Aegisub.Interfaces
{
    public interface ISentence : ITime
    {
        string Text { get; }
        IReadOnlyList<IWord> Words { get; }
    }
}
