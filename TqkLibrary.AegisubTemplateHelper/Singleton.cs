using System.Reflection;

namespace TqkLibrary.AegisubTemplateHelper
{
    public static class Singleton
    {
        public static string CurrentDir { get; set; } = new FileInfo(Assembly.GetCallingAssembly().Location).Directory!.FullName;
        public static string AppDataDir { get; set; } = Path.Combine(CurrentDir, "AppData");
        public static string AegisubDir { get; set; } = Path.Combine(AppDataDir, "Aegisub-3.4.2-portable");
    }
}
