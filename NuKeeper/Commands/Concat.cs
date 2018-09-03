namespace NuKeeper.Commands
{
    public static class Concat
    {
        public static string FirstValue(string value1, string value2)
        {
            if (!string.IsNullOrWhiteSpace(value1))
            {
                return value1;
            }

            return value2;
        }
    }
}
