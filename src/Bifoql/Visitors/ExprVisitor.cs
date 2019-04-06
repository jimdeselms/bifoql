using Bifoql.Expressions;
using Bifoql.Expressions.Builtins;

namespace Bifoql.Visitors
{
    internal abstract class ExprVisitor
    {
        public virtual void Visit(ArrayExpr expr) {}
        public virtual void Visit(BinaryExpr expr) {}
        public virtual void Visit(ErrorFunctionExpr expr) {}
        public virtual void Visit(IfErrorExpr expr) {}
        public virtual void Visit(TypeExpr expr) {}
        public virtual void Visit(AssignmentExpr expr) {}
        public virtual void Visit(ChainExpr expr) {}
        public virtual void Visit(ErrorExpr expr) {}
        public virtual void Visit(ExpressionExpr expr) {}
        public virtual void Visit(IdentityExpr expr) {}
        public virtual void Visit(IndexedLookupExpr expr) {}
        public virtual void Visit(FilterExpr expr) {}
        public virtual void Visit(FunctionExpr expr) {}
        public virtual void Visit(KeyExpr expr) {}
        public virtual void Visit(LiteralExpr expr) {}
        public virtual void Visit(KeyValuePairExpr expr) {}
        public virtual void Visit(SliceExpr expr) {}
        public virtual void Visit(IndexExpr expr) {}
        public virtual void Visit(MapProjectionExpr expr) {}

        public virtual void Visit(SpreadExpr expr) {}
        public virtual void Visit(TernaryExpr expr) {}
        public virtual void Visit(TypedFunctionCallExpr expr) {}
        public virtual void Visit(UnaryExpr expr) {}
        public virtual void Visit(VariableExpr expr) {}

    }
}