namespace Bifoql
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using Bifoql.Types;

    internal interface IBifoqlObject
    {
        Task<bool> IsEqualTo(IBifoqlObject o);
        Task<BifoqlType> GetSchema();
    }

    internal interface IBifoqlMap : IBifoqlObject, IReadOnlyDictionary<string, Func<Task<IBifoqlObject>>>
    {
    }

    internal interface IBifoqlArray : IBifoqlObject, IReadOnlyList<Func<Task<IBifoqlObject>>>
    {
    }

    internal interface IBifoqlDeferredQuery : IBifoqlObject
    {
        Task<IBifoqlObject> EvaluateQuery(string query);
    }

    internal interface IBifoqlExpression : IBifoqlObject
    {
        Task<IBifoqlObject> Evaluate(QueryContext context);
    }

    internal interface IBifoqlIndex : IBifoqlObject
    {
        Task<object> Lookup(IndexArgumentList elements);
    }

    internal interface IBifoqlNull : IBifoqlObject
    {
    }

    // Undefined should never bubble up to the caller
    internal interface IBifoqlUndefined : IBifoqlObject
    {
    }

    internal interface IBifoqlString : IBifoqlObject
    {
        Task<string> Value { get; }
    }

    internal interface IBifoqlNumber : IBifoqlObject
    {
        Task<double> Value { get; }
    }

    internal interface IBifoqlBoolean : IBifoqlObject
    {
        Task<bool> Value { get; }
    }

    internal interface IBifoqlError : IBifoqlObject
    {
        string Message { get; }
    }
}