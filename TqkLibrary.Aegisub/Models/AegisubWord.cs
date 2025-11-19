using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubWord : AegisubTime, IWord
    {
        public required string Text { get; set; }
        string IWord.Word => Text;
    }
}
