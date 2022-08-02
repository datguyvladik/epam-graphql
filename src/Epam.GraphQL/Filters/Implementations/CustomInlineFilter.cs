// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Linq.Expressions;
using Epam.GraphQL.Diagnostics;
using Epam.GraphQL.Helpers;
using GraphQL;

namespace Epam.GraphQL.Filters.Implementations
{
    internal class CustomInlineFilter<TEntity, TValueType, TExecutionContext> : IInlineFilter<TExecutionContext>, IChainConfigurationContextOwner
    {
        private readonly Func<TExecutionContext, TValueType, Expression<Func<TEntity, bool>>> _filterPredicateFactory;

        public CustomInlineFilter(
            Func<IChainConfigurationContextOwner, IChainConfigurationContext> configurationContextFactory,
            string name,
            Func<TValueType, Expression<Func<TEntity, bool>>> filterPredicateFactory)
            : this(configurationContextFactory, name, (context, value) => filterPredicateFactory(value))
        {
        }

        public CustomInlineFilter(
            Func<IChainConfigurationContextOwner, IChainConfigurationContext> configurationContextFactory,
            string name,
            Func<TExecutionContext, TValueType, Expression<Func<TEntity, bool>>> filterPredicateFactory)
        {
            ConfigurationContext = configurationContextFactory(this);
            FieldName = name.ToCamelCase();
            _filterPredicateFactory = filterPredicateFactory;
        }

        public Type FieldType => typeof(TValueType);

        public string FieldName { get; }

        public Type FilterType => typeof(TValueType);

        public IChainConfigurationContext ConfigurationContext { get; set; }

        LambdaExpression IInlineFilter<TExecutionContext>.BuildExpression(TExecutionContext context, object? filter)
        {
            return BuildExpression(context, (TValueType?)filter);
        }

        public LambdaExpression BuildExpression(TExecutionContext context, TValueType? filter)
        {
            if (filter != null)
            {
                return _filterPredicateFactory(context, filter);
            }

            return FuncConstants<TEntity>.TrueExpression;
        }

        public override bool Equals(object obj) => Equals(obj as CustomInlineFilter<TEntity, TValueType, TExecutionContext>);

        public bool Equals(IInlineFilter<TExecutionContext> obj) => Equals(obj as CustomInlineFilter<TEntity, TValueType, TExecutionContext>);

        public bool Equals(CustomInlineFilter<TEntity, TValueType, TExecutionContext>? other)
        {
            if (other == null)
            {
                return false;
            }

            return FieldName.Equals(other.FieldName, StringComparison.Ordinal)
                && _filterPredicateFactory == other._filterPredicateFactory;
        }

        public override int GetHashCode()
        {
            var hashCode = default(HashCode);
            hashCode.Add(FieldName, StringComparer.Ordinal);
            hashCode.Add(_filterPredicateFactory);

            return hashCode.ToHashCode();
        }
    }
}
