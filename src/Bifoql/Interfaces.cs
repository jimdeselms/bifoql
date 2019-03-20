namespace Bifoql
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using Bifoql.Types;

    public interface IAsyncObject
    {
        Task<bool> IsEqualTo(IAsyncObject o);
        Task<BifoqlType> GetSchema();
    }

    public interface IAsyncMap : IAsyncObject, IReadOnlyDictionary<string, Func<Task<IAsyncObject>>>
    {
    }

    public interface IAsyncArray : IAsyncObject, IReadOnlyList<Func<Task<IAsyncObject>>>
    {
    }

    public interface IAsyncDeferredQuery : IAsyncObject
    {
        Task<IAsyncObject> EvaluateQuery(string query);
    }

    public interface IAsyncExpression : IAsyncObject
    {
        Task<IAsyncObject> Evaluate(QueryContext context);
    }

    public interface IAsyncIndex : IAsyncObject
    {
        Task<object> Lookup(IndexArgumentList elements);
    }

    public interface IAsyncNull : IAsyncObject
    {
    }

    public interface IAsyncString : IAsyncObject
    {
        Task<string> Value { get; }
    }

    public interface IAsyncNumber : IAsyncObject
    {
        Task<double> Value { get; }
    }

    public interface IAsyncBoolean : IAsyncObject
    {
        Task<bool> Value { get; }
    }

    public interface IAsyncError : IAsyncObject
    {
        string Message { get; }
    }
}