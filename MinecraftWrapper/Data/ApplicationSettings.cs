namespace MinecraftWrapper.Data
{
    public class ApplicationSettings
    {
        //string exePath, string startDirectory, bool restartOnFailure, int maxOutputRetained
        public string ExePath { get; set; }
        public string StartDirectory { get; set; }
        public bool RestartOnFailure { get; set; }
        public int MaxOutputRetained { get; set; }
    }
}