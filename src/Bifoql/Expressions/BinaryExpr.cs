namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class BinaryExpr : Expr
    {
        public Expr LeftHandSide;
        public string Operator;
        public Expr RightHandSide;

        public BinaryExpr(Expr leftHandSide, string @operator, Expr rightHandSide) : base(leftHandSide.Location)
        {
            LeftHandSide = leftHandSide;
            Operator = @operator;
            RightHandSide = rightHandSide;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new BinaryExpr(LeftHandSide.Simplify(variables), Operator, RightHandSide.Simplify(variables));
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var lhs = await LeftHandSide.Apply(context);

            if (Operator == "??")
            {
                if (lhs == null || lhs is IBifoqlNull || lhs is IBifoqlError || lhs is IBifoqlUndefined)
                {
                    return await RightHandSide.Apply(context);
                }
                else
                {
                    return lhs;
                }
            }

            // Propagate error
            if (lhs is IBifoqlError) return lhs;
            
            // Special case. If it's && or ||, then we might not have to evaluate the right hand side.
            if (Operator == "&&" || Operator == "||")
            {
                var lhsBool = lhs as IBifoqlBoolean;
                if (lhsBool == null) return new AsyncError(this.Location, "Can't evaluate boolean operator on non-boolean value");

                var val = await lhsBool.Value;
                if (Operator == "&&" && !val)
                {
                    return new AsyncBoolean(false);
                }
                else if (Operator == "||" && val)
                {
                    return new AsyncBoolean(true);
                }

                // Okay, now we have to do the right hand side
                var rightHandValue = await RightHandSide.Apply(context);
                if (rightHandValue is IBifoqlError) return rightHandValue;
                
                var rhsBool = rightHandValue as IBifoqlBoolean;
                if (rhsBool == null) return new AsyncError(this.Location, "Can't evaluate boolean operator on non-boolean value");

                var rhsVal = await rhsBool.Value;
                return new AsyncBoolean(rhsVal);
            }

            // Another special case; if we have "==" and they are both literally the same object, then it's true.
            if (Operator == "==" && LeftHandSide == RightHandSide) return new AsyncBoolean(true);

            var rhs = await RightHandSide.Apply(context);
            
            // Propagate error
            if (rhs is IBifoqlError) return rhs;

            switch (Operator)
            {
                case "==": return new AsyncBoolean(await lhs.IsEqualTo(rhs));
                case "!=": return new AsyncBoolean(!await lhs.IsEqualTo(rhs));
                case "+":
                case "-":
                case "*":
                case "/": 
                case "%": return await ApplyArithmeticOperator(lhs, rhs, Operator);
                case "<":
                case "<=":
                case ">":
                case ">=": return await ApplyInequalityOperator(lhs, rhs, Operator);
                case "contains": return await ApplyContainsOperator(lhs, rhs);
                case "starts_with": return await ApplyStartsWithEndsWithOperator(lhs, rhs, startsWith: true);
                case "ends_with": return await ApplyStartsWithEndsWithOperator(lhs, rhs, startsWith: false);
            }

            return new AsyncError(this.Location, "Unknown Binary Expression " + Operator);
        }

        private async Task<IBifoqlObject> ApplyInequalityOperator(IBifoqlObject lhs, IBifoqlObject rhs, string @operator)
        {
            var lhsNum = lhs as IBifoqlNumber;
            var rhsNum = rhs as IBifoqlNumber;
            if (lhsNum != null && rhsNum != null)
            {
                var lhsVal = await lhsNum.Value;
                var rhsVal = await rhsNum.Value;
                switch(@operator)
                {
                    case "<": return new AsyncBoolean(lhsVal < rhsVal);
                    case ">": return new AsyncBoolean(lhsVal > rhsVal);
                    case "<=": return new AsyncBoolean(lhsVal <= rhsVal);
                    case ">=": return new AsyncBoolean(lhsVal >= rhsVal);
                    default: return new AsyncError(this.Location, $"Unknown inequality {@operator}");
                }
            }

            var lhsStr = lhs as IBifoqlString;
            var rhsStr = rhs as IBifoqlString;
            if (lhsStr != null && rhsStr != null)
            {
                var lhsVal = await lhsStr.Value;
                var rhsVal = await rhsStr.Value;
                switch (@operator)
                {
                    case "<": return new AsyncBoolean(lhsVal.CompareTo(rhsVal) == -1);
                    case ">": return new AsyncBoolean(lhsVal.CompareTo(rhsVal) == 11);
                    case "<=": return new AsyncBoolean(lhsVal.CompareTo(rhsVal) != 1);
                    case ">=": return new AsyncBoolean(lhsVal.CompareTo(rhsVal) != -1);
                    default: return new AsyncError(this.Location, $"Unknown inequality {@operator}");
                }
            }

            return new AsyncError(this.Location, $"Invalid equality operation");
        }

        private async Task<IBifoqlObject> ApplyArithmeticOperator(IBifoqlObject lhs, IBifoqlObject rhs, string @operator)
        {
            var lhsNum = lhs as IBifoqlNumber;
            var rhsNum = rhs as IBifoqlNumber;
            if (lhsNum != null && rhsNum != null)
            {
                var lhsVal = await lhsNum.Value;
                var rhsVal = await rhsNum.Value;

                switch (@operator)
                {
                    case "+": return new AsyncNumber(lhsVal + rhsVal);
                    case "-": return new AsyncNumber(lhsVal - rhsVal);
                    case "*": return new AsyncNumber(lhsVal * rhsVal);
                    case "/": return rhsVal == 0 ? (IBifoqlObject)new AsyncError(LeftHandSide.Location, "division by zero") : new AsyncNumber(lhsVal / rhsVal);
                    case "%": return new AsyncNumber(lhsVal % rhsVal);
                    case "^": return new AsyncNumber(Math.Pow(lhsVal, rhsVal));
                    default: return new AsyncError(this.Location, "Unknown operator " + @operator);
                }
            }

            var lhsStr = lhs as IBifoqlString;
            var rhsStr = rhs as IBifoqlString;

            // String operations
            if (lhsStr != null && rhsStr != null)
            {
                var lhsVal = await lhsStr.Value;
                var rhsVal = await rhsStr.Value;

                switch (@operator)
                {
                    case "+": return new AsyncString(lhsVal + rhsVal);
                    case "-": return new AsyncString(lhsVal.Replace(rhsVal, ""));
                    default: return new AsyncError(this.Location, $"Can't use operator {@operator} on strings");
                }
            }

            var lhsArray = lhs as IBifoqlArrayInternal;
            // Array operations
            if (lhsArray != null)
            {
                switch (Operator)
                {
                    case "+": return AddArray(lhsArray, rhs);
                    case "-": return await SubtractArray(lhsArray, rhs);
                    default: return new AsyncError(this.Location, "Unknown array operator " + Operator);
                }
            }

            return new AsyncError(this.Location, "Not implemented");
        }

        private IBifoqlObject AddArray(IBifoqlArrayInternal lhsArray, IBifoqlObject rhs)
        {
            var result = new List<Func<Task<IBifoqlObject>>>();

            foreach (var item in lhsArray)
            {
                result.Add(item);
            }

            var rhsArray = rhs as IBifoqlArrayInternal;
            if (rhsArray != null)
            {
                foreach (var item in rhsArray)
                {
                    result.Add(item);
                }
            }
            else
            {
                result.Add(() => Task.FromResult(rhs));
            }
            return new AsyncArray(result);
        }

        private async Task<IBifoqlObject> SubtractArray(IBifoqlArrayInternal lhsArray, IBifoqlObject rhs)
        {
            var rhsArray = rhs as IBifoqlArrayInternal;
            if (rhsArray == null)
            {
                return await SubtractObjectFromArray(lhsArray, rhs);
            }
            else
            {
                return await SubtractArrayFromArray(lhsArray, rhsArray);
            }
        }

        private async Task<IBifoqlObject> SubtractArrayFromArray(IBifoqlArrayInternal lhsArray, IBifoqlArrayInternal rhsArray)
        {
            var result = lhsArray;
            foreach (var item in rhsArray)
            {
                result = await SubtractObjectFromArray(result, await item());
            }
            return result;
        }

        private async Task<IBifoqlArrayInternal> SubtractObjectFromArray(IBifoqlArrayInternal lhsArray, IBifoqlObject rhs)
        {
            var result = new List<Func<Task<IBifoqlObject>>>();
            foreach (var item in lhsArray)
            {
                var curr = await item();
                if (!(await curr.IsEqualTo(rhs)))
                {
                    result.Add(() => Task.FromResult(curr));
                }
            }

            return new AsyncArray(result);
        }

        private async Task<IBifoqlObject> ApplyStartsWithEndsWithOperator(IBifoqlObject subject, IBifoqlObject target, bool startsWith)
        { 
            var subjectAsStringObj = subject as IBifoqlString;
            var targetAsStringObj = target as IBifoqlString;

            if (subjectAsStringObj != null && targetAsStringObj != null)
            {
                var subjectAsString = await subjectAsStringObj.Value;
                var targetAsString = await targetAsStringObj.Value;

                var result = startsWith ? subjectAsString.StartsWith(targetAsString) : subjectAsString.EndsWith(targetAsString);

                return new AsyncBoolean(result);
            }

            return new AsyncError(this.Location, "Don't know how to do starts_with or ends_with on these types");
        }

        private async Task<IBifoqlObject> ApplyContainsOperator(IBifoqlObject subject, IBifoqlObject search)
        {
            var subjectAsStringObj = subject as IBifoqlString;
            if (subjectAsStringObj != null)
            {
                var searchAsStringObj = search as IBifoqlString;
                if (searchAsStringObj == null) return new AsyncBoolean(false);

                var subjectAsString = await subjectAsStringObj.Value;
                var searchAsString = await searchAsStringObj.Value;

                return new AsyncBoolean(subjectAsString.Contains(searchAsString));
            }

            var subjectAsArray = subject as IBifoqlArrayInternal;
            if (subjectAsArray != null)
            {
                foreach (var itemObj in subjectAsArray)
                {
                    var item = await (itemObj());
                    if (await item.IsEqualTo(search))
                    {
                        return new AsyncBoolean(true);
                    }
                }
            }

            return new AsyncBoolean(false);
        }

        public override string ToString()
        {
            return $"{LeftHandSide.ToString()} {Operator} {RightHandSide.ToString()}";
        }

        public override bool NeedsAsync(VariableScope variables) => LeftHandSide.NeedsAsync(variables) || RightHandSide.NeedsAsync(variables);

        public override bool ReferencesRootVariable => LeftHandSide.ReferencesRootVariable || RightHandSide.ReferencesRootVariable;
    }
}