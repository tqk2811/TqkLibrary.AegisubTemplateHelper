using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AegisubTemplateHelper.Interfaces
{
    public interface ISentence
    {
        string Text { get; }
        TimeSpan Start { get; }
        TimeSpan End { get; }
        IReadOnlyList<IWord> Words { get; }
    }
}
