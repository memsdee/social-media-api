using System.Linq.Expressions;
using MongoDB.Driver;

namespace be.Infrastructure.Helper;

public static class ReadCursorPagiFilterHelper
{
    public static FilterDefinition<T> BuildCursorFilter<T, TSelector>(
        Expression<Func<T, TSelector>> selectorExpr,
        Expression<Func<T, short>> idSelector,
        TSelector? cursorSelector,
        short? cursorId)
        where TSelector : struct, IComparable<TSelector>
    {
        if (cursorSelector is null)
            return Builders<T>.Filter.Empty;

        var ltSelector = Builders<T>.Filter.Lt(selectorExpr, cursorSelector.Value);

        if (!cursorId.HasValue)
            return ltSelector;

        return Builders<T>.Filter.Or(
            ltSelector,
            Builders<T>.Filter.And(
                Builders<T>.Filter.Eq(selectorExpr, cursorSelector.Value),
                Builders<T>.Filter.Lt(idSelector, cursorId.Value)
            )
        );
    }
}