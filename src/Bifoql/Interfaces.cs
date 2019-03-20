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

    public interface IBifoqlMap : IBifoqlObject, IReadOnlyDictionary<string, Func<Task<IBifoqlObject>>>
    {
    }

    public interface IBifoqlArray : IBifoqlObject, IReadOnlyList<Func<Task<IBifoqlObject>>>
    {
    }

    public interface IBifoqlDeferredQuery : IBifoqlObject
    {
        Task<IBifoqlObject> EvaluateQuery(string query);
    }

    public interface IBifoqlExpression : IBifoqlObject
    {
        Task<IBifoqlObject> Evaluate(QueryContext context);
    }

    public interface IBifoqlIndex : IBifoqlObject
    {
        Task<object> Lookup(IndexArgumentList elements);
    }

    public interface IBifoqlNull : IBifoqlObject
    {
    }

    public interface IBifoqlString : IBifoqlObject
    {
        Task<string> Value { get; }
    }

    public interface IBifoqlNumber : IBifoqlObject
    {
        Task<double> Value { get; }
    }

    public interface IBifoqlBoolean : IBifoqlObject
    {
        Task<bool> Value { get; }
    }

    public interface IBifoqlError : IBifoqlObject
    {
        string Message { get; }
    }
}