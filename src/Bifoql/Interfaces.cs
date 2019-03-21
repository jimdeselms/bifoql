namespace Bifoql
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using Bifoql.Types;

    public interface IBifoqlObject
    {
        Task<bool> IsEqualTo(IBifoqlObject o);
        Task<BifoqlType> GetSchema();
    }

    public interface ISyncBifoqlArray
    {
        IReadOnlyList<Func<object>> Items { get; }
    }

    public interface ISyncBifoqlMap
    {
        IReadOnlyDictionary<string, Func<object>> Items { get; }
    }

    public interface IAsyncBifoqlArray
    {
        IReadOnlyList<Func<Task<object>>> Items { get; }
    }

    public interface IAsyncBifoqlMap
    {
        IReadOnlyDictionary<string, Func<Task<object>>> Items { get; }
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