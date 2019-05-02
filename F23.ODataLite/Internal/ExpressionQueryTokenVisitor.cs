using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace F23.ODataLite.Internal
{
    internal class ExpressionQueryTokenVisitor : ISyntacticTreeVisitor<Expression>
    {
        private static readonly MethodInfo _hasFlagMethod = typeof(Enum).GetMethod(nameof(Enum.HasFlag), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo _fuzzyEqualsMethod = typeof(ExpressionQueryTokenVisitor).GetMethod(nameof(FuzzyEquals), BindingFlags.Static | BindingFlags.NonPublic);

        private readonly ParameterExpression _parameter;
        private readonly IEnumerable<PropertyInfo> _properties;

        public ExpressionQueryTokenVisitor(ParameterExpression parameterExpression, IEnumerable<PropertyInfo> properties)
        {
            _parameter = parameterExpression;
            _properties = properties;
        }

        private static bool FuzzyEquals(object left, object right)
        {
            return Equals(left, right)
                   || (left != null && right != null && left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        public Expression Visit(AllToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(AnyToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(BinaryOperatorToken tokenIn)
        {
            switch (tokenIn.OperatorKind)
            {
                case BinaryOperatorKind.Or:
                    return Expression.Or(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.And:
                    return Expression.And(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Equal:
                    return GetEqualsExpression(tokenIn);
                case BinaryOperatorKind.NotEqual:
                    return Expression.Not(GetEqualsExpression(tokenIn));
                case BinaryOperatorKind.GreaterThan:
                    return Expression.GreaterThan(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.LessThan:
                    return Expression.LessThan(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.LessThanOrEqual:
                    return Expression.LessThanOrEqual(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Add:
                    return Expression.Add(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Subtract:
                    return Expression.Subtract(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Multiply:
                    return Expression.Multiply(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Divide:
                    return Expression.Divide(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Modulo:
                    return Expression.Modulo(tokenIn.Left.Accept(this), tokenIn.Right.Accept(this));
                case BinaryOperatorKind.Has:
                    return Expression.Call(tokenIn.Left.Accept(this), _hasFlagMethod, tokenIn.Right.Accept(this));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Expression GetEqualsExpression(BinaryOperatorToken tokenIn)
        {
            return Expression.Call(null, _fuzzyEqualsMethod, Expression.Convert(tokenIn.Left.Accept(this), typeof(object)), Expression.Convert(tokenIn.Right.Accept(this), typeof(object)));
        }

        public Expression Visit(InToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(DottedIdentifierToken tokenIn)
        {
            var parts = tokenIn.Identifier.Split('.');

            var propExprs = new List<MemberExpression>();

            var prop = _properties.FirstOrDefault(i => i.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));

            if (prop == null)
                throw new InvalidOperationException($"Property {parts[0]} not found on type {_parameter.Type.Name}");

            var nullExpr = Expression.Constant(null);

            var propExpr = Expression.Property(_parameter, prop);
            propExprs.Add(propExpr);
            
            for (int i = 1; i < parts.Length; i++)
            {
                var parentType = prop.PropertyType;
                var props = parentType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                prop = props.FirstOrDefault(p => p.Name.Equals(parts[i], StringComparison.OrdinalIgnoreCase));

                if (prop == null)
                    throw new InvalidOperationException($"Property {parts[i]} not found on type {parentType.Name}");

                propExpr = Expression.Property(propExpr, prop);
                propExprs.Add(propExpr);
            }

            var finalType = prop.PropertyType;
            Expression expr = propExprs[propExprs.Count - 1];

            if (propExprs.Count > 1)
            {
                for (int i = propExprs.Count - 2; i >= 0; i--)
                {
                    propExpr = propExprs[i];

                    expr = Expression.Condition(
                        Expression.Equal(Expression.Convert(propExpr, typeof(object)), nullExpr),
                        Expression.Default(finalType),
                        expr);
                }
            }

            return expr;
        }

        public Expression Visit(ExpandToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(ExpandTermToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(FunctionCallToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(LambdaToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(LiteralToken tokenIn)
        {
            return Expression.Constant(tokenIn.Value);
        }

        public Expression Visit(InnerPathToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(OrderByToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(EndPathToken tokenIn)
        {
            if (tokenIn.Identifier.Equals("null", StringComparison.OrdinalIgnoreCase))
                return Expression.Constant(null);

            var prop = _properties.FirstOrDefault(i => i.Name.Equals(tokenIn.Identifier, StringComparison.OrdinalIgnoreCase));

            if (prop != null)
                return Expression.Property(_parameter, prop);

            return Expression.Constant(tokenIn.Identifier);
        }

        public Expression Visit(CustomQueryOptionToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(RangeVariableToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(SelectToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(StarToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(UnaryOperatorToken tokenIn)
        {
            switch (tokenIn.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return Expression.Negate(tokenIn.Operand.Accept(this));
                case UnaryOperatorKind.Not:
                    return Expression.Not(tokenIn.Operand.Accept(this));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Expression Visit(FunctionParameterToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(AggregateToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(AggregateExpressionToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(EntitySetAggregateToken tokenIn)
        {
            throw new NotImplementedException();
        }

        public Expression Visit(GroupByToken tokenIn)
        {
            throw new NotImplementedException();
        }
    }
}
