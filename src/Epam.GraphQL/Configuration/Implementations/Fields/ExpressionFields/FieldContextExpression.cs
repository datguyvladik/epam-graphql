// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Epam.GraphQL.Diagnostics;
using Epam.GraphQL.Extensions;
using Epam.GraphQL.Helpers;
using Epam.GraphQL.Sorters;
using GraphQL;

namespace Epam.GraphQL.Configuration.Implementations.Fields.ExpressionFields
{
    internal class FieldContextExpression<TEntity, TReturnType, TExecutionContext> : IFieldExpression<TEntity, TReturnType, TExecutionContext>
    {
        private readonly ExpressionField<TEntity, TReturnType, TExecutionContext> _field;
        private readonly Expression<Func<TExecutionContext, TEntity, TReturnType>> _expression;
        private readonly Lazy<Func<TExecutionContext, TEntity, TReturnType>> _resolver;
        private readonly Lazy<Func<object, object?>> _proxyResolver;

        public FieldContextExpression(ExpressionField<TEntity, TReturnType, TExecutionContext> field, Expression<Func<TExecutionContext, TEntity, TReturnType>> expression)
        {
            _expression = expression;
            _field = field;
            _resolver = new Lazy<Func<TExecutionContext, TEntity, TReturnType>>(_expression.Compile);
            _proxyResolver = new Lazy<Func<object, object?>>(() => _field.Parent.ProxyAccessor.BaseProxyType.GetPropertyDelegate(Name));
        }

        public bool IsGroupable => _field.IsGroupable;

        public LambdaExpression ContextedExpression => _expression;

        public LambdaExpression OriginalExpression => _expression;

        public bool IsReadOnly => true;

        public PropertyInfo? PropertyInfo => null;

        public string Name => _field.Name;

        public IChainConfigurationContext ConfigurationContext => _field.ConfigurationContext;

        public void ValidateExpression()
        {
            ExpressionValidator.Validate(_expression);
        }

        public TReturnType? Resolve(IResolveFieldContext context)
        {
            try
            {
                Guards.AssertIfNull(context.Source);

                return context.Source is Proxy<TEntity> proxy
                    ? (TReturnType?)_proxyResolver.Value(proxy)
                    : _resolver.Value(context.GetUserContext<TExecutionContext>(), (TEntity)context.Source);
            }
            catch (Exception e)
            {
                throw context.CreateFieldExecutionError(e);
            }
        }

        public LambdaExpression BuildOriginalExpression(TExecutionContext context) => _expression;

        public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
        {
            return new ValueTask<object?>(Resolve(context));
        }

        LambdaExpression ISorter<TExecutionContext>.BuildExpression(TExecutionContext context) => Expression.Lambda<Func<TEntity, TReturnType>>(
            _expression.Body.ReplaceParameter(_expression.Parameters[0], Expression.Constant(context, typeof(TExecutionContext))),
            _expression.Parameters[1]);
    }
}
