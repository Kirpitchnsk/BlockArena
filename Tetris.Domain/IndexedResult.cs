namespace BlockArena.Domain
{
    public class IndexedResult<T>
    {
        public int Index { get; set; }
        public T Result { get; set; }
    }
}