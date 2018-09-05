namespace NuKeeper.Configuration
{
    public class FileSettings
    {
        public string Age { get; set; }
        public string Api { get; set; }
        public string Include { get; set; }
        public string Exclude { get; set; }

        public static FileSettings Empty()
        {
            return new FileSettings();
        }
    }
}
