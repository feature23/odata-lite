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
        private readonly Dictionary<BinaryOperatorKind, Func<Expression, Expression, Expression>> EqualityExprs =
            new Dictionary<BinaryOperatorKind, Func<Expression, Expression, Expression>>
            {
                [BinaryOperatorKind.Equal] = (left, right) => Expression.Equal(left, right),
                [BinaryOperatorKind.NotEqual] = (left, right) => Expression.NotEqual(left, right),
                [BinaryOperatorKind.GreaterThan] = (left, right) => Expression.GreaterThan(left, right),
                [BinaryOperatorKind.GreaterThanOrEqual] = (left, right) => Expression.GreaterThanOrEqual(left, right),
                [BinaryOperatorKind.LessThan] = (left, right) => Expression.LessThan(left, right),
                [BinaryOperatorKind.LessThanOrEqual] = (left, right) => Expression.LessThanOrEqual(left, right),
            };

        private static readonly MethodInfo _hasFlagMethod = typeof(Enum).GetMethod(nameof(Enum.HasFlag), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo _toStringMethod = typeof(object).GetMethod(nameof(ToString), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo _fuzzyEqualsMethod = typeof(ExpressionQueryTokenVisitor).GetMethod(nameof(FuzzyEquals), BindingFlags.Static | BindingFlags.NonPublic);

        private readonly ParameterExpression _parameter;
        private readonly IEnumerable<PropertyInfo> _properties;
        private readonly bool _inMemoryEvaluation;
        private readonly bool _supportDecimalType;

        public ExpressionQueryTokenVisitor(ParameterExpression parameterExpression,
            IEnumerable<PropertyInfo> properties,
            bool inMemoryEvaluation,
            bool supportDecimalType)
        {
            _parameter = parameterExpression;
            _properties = properties;
            _inMemoryEvaluation = inMemoryEvaluation;
            _supportDecimalType = supportDecimalType;
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

                case BinaryOperatorKind.Equal:
                case BinaryOperatorKind.NotEqual:
                case BinaryOperatorKind.GreaterThan:
                case BinaryOperatorKind.GreaterThanOrEqual:
                case BinaryOperatorKind.LessThan:
                case BinaryOperatorKind.LessThanOrEqual:
                    return GetEqualityExpression(tokenIn);

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public Expression Visit(SelectTermToken tokenIn)
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

        private Expression GetEqualityExpression(BinaryOperatorToken tokenIn)
        {
            var left = tokenIn.Left.Accept(this);
            var right = tokenIn.Right.Accept(this);

            if (tokenIn.OperatorKind == BinaryOperatorKind.Equal && _inMemoryEvaluation)
            {
                // TODO.JB - Do we need to apply this logic for other operators with in-memory eval?
                return Expression.Call(
                    null,
                    _fuzzyEqualsMethod,
                    Expression.Convert(left, typeof(object)),
                    Expression.Convert(right, typeof(object))
                );
            }

            (left, right) = ConvertTypesIfNeeded(left, right);

            return EqualityExprs[tokenIn.OperatorKind](left, right);
        }

        private (Expression, Expression) ConvertTypesIfNeeded(Expression left, Expression right)
        {
            var targetType = left.Type;

            if (right is ConstantExpression expr)
            {
                if (_inMemoryEvaluation && targetType.IsAssignableFrom(expr.Type))
                {
                    // Attempt automatic type conversion if we're evaluating
                    // in-memory and .NET can to the conversion.
                    right = Expression.Convert(right, targetType);
                }
                else if (typeof(IConvertible).IsAssignableFrom(targetType) &&
                         expr.Value is IConvertible convertible)
                {
                    if (targetType == typeof(decimal) && !_supportDecimalType)
                    {
                        left = Expression.Convert(left, typeof(double));
                        right = Expression.Constant(convertible.ToType(typeof(double), null));
                    }
                    else
                    {
                        right = Expression.Constant(convertible.ToType(targetType, null));
                    }
                }
                else if (targetType.IsNullableType())
                {
                    var underlyingTargetType = targetType.GetGenericArguments().First();

                    (left, right) = AttemptManualTypeConversion(left, expr, underlyingTargetType);

                    right = Expression.Convert(right, targetType);
                }
                else
                {
                    (left, right) = AttemptManualTypeConversion(left, expr, targetType);
                }
            }

            return (left, right);
        }

        private (Expression left, Expression right) AttemptManualTypeConversion(Expression left, Expression right, Type targetType)
        {
            // Attempt manual type conversion.
            if (right is ConstantExpression expr && expr.Value is string @string)
            {
                if (targetType == typeof(Guid))
                {
                    right = Expression.Constant(Guid.Parse(@string));
                }
                else if (typeof(Enum).IsAssignableFrom(targetType))
                {
                    var enumValue = Enum.Parse(targetType, @string);
                    right = Expression.Convert(Expression.Constant(enumValue), targetType);
                }
            }

            // Unknown conversion. Subsequent query will likely fail.

            return (left, right);
        }
    }
}
