using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Aegisub.Interfaces
{
    public interface IAegisubSentence : IAegisubTime, IAegisubWordList
    {
        string Text { get; }
    }
}
