namespace Csb.BigMom.Api.Models
{
    public struct RangeSearchFilter<T> : ISearchFilter
    {
        public T Start { get; init; }

        public T End { get; init; }
    }
}
