namespace NuKeeper.Configuration
{
    public class FileSettings
    {
        public string Include { get; set; }
        public string Exclude { get; set; }

        public static FileSettings Empty()
        {
            return new FileSettings();
        }
    }
}
