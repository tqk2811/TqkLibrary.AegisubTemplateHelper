using System.Reflection;

namespace TqkLibrary.AegisubTemplateHelper
{
    public static class Singleton
    {
        static string CurrentDir { get; } = new FileInfo(Assembly.GetCallingAssembly().Location).Directory!.FullName;
        static string AppDataDir { get; } = Path.Combine(CurrentDir, "AppData");
        public static string AegisubDir { get; set; } = Path.Combine(AppDataDir, "Aegisub-3.4.2-portable");
    }
}
