using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace TqkLibrary.AegisubTemplateHelper.DataClasses
{
    public class AssScriptInfoData
    {
        [SetsRequiredMembers]
        public AssScriptInfoData(Size videoSize)
        {
            VideoSize = videoSize;
        }
        public required Size VideoSize { get; set; }
        public override string ToString()
        {
            return @$"[Script Info]
Title: Zoom-Up-Color Karaoke Effect
ScriptType: v4.00+
PlayResX: {VideoSize.Width}
PlayResY: {VideoSize.Height}
WrapStyle: 0
ScaledBorderAndShadow: yes
YCbCr Matrix: TV.709";
        }
    }
}
