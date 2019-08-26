namespace NuKeeper.BitBucket.Models
{
    /// <summary>
    /// Models a page of data in a finite, immutable resource collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListBasedPage<T> : IteratorBasedPage<T>
    {
        public string previous { get; set; }
        public int? page { get; set; }
    }
}
