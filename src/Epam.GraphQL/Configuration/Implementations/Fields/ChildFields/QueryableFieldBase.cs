// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Epam.GraphQL.Configuration.Implementations.FieldResolvers;
using Epam.GraphQL.Extensions;
using Epam.GraphQL.Filters;
using Epam.GraphQL.Helpers;
using Epam.GraphQL.Loaders;
using Epam.GraphQL.Search;
using Epam.GraphQL.Sorters;
using Epam.GraphQL.Sorters.Implementations;
using Epam.GraphQL.Types;
using GraphQL;
using GraphQL.Types;

#nullable enable

namespace Epam.GraphQL.Configuration.Implementations.Fields.ChildFields
{
    internal abstract class QueryableFieldBase<TEntity, TReturnType, TExecutionContext> : EnumerableFieldBase<TEntity, TReturnType, TExecutionContext>
        where TEntity : class
    {
        protected QueryableFieldBase(
            RelationRegistry<TExecutionContext> registry,
            BaseObjectGraphTypeConfigurator<TEntity, TExecutionContext> parent,
            string name,
            Func<IResolveFieldContext, IQueryable<TReturnType>> query,
            Func<IResolveFieldContext, IQueryable<TReturnType>, IQueryable<TReturnType>> transform,
            Expression<Func<TEntity, TReturnType, bool>>? condition,
            IGraphTypeDescriptor<TReturnType, TExecutionContext> elementGraphType,
            IObjectGraphTypeConfigurator<TReturnType, TExecutionContext> configurator,
            LazyQueryArguments? arguments,
            ISearcher<TReturnType, TExecutionContext>? searcher,
            Func<IQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? orderBy,
            Func<IOrderedQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? thenBy)
            : this(
                  registry,
                  parent,
                  name,
                  CreateResolver(
                      fieldName: name,
                      query,
                      transform,
                      condition,
                      searcher,
                      orderBy,
                      thenBy,
                      outerProxyAccessor: parent.ProxyAccessor,
                      configurator),
                  elementGraphType,
                  configurator,
                  arguments,
                  searcher,
                  orderBy,
                  thenBy)
        {
        }

        protected QueryableFieldBase(
            RelationRegistry<TExecutionContext> registry,
            BaseObjectGraphTypeConfigurator<TEntity, TExecutionContext> parent,
            string name,
            IQueryableResolver<TEntity, TReturnType, TExecutionContext> resolver,
            IGraphTypeDescriptor<TReturnType, TExecutionContext> elementGraphType,
            IObjectGraphTypeConfigurator<TReturnType, TExecutionContext>? configurator,
            LazyQueryArguments? arguments,
            ISearcher<TReturnType, TExecutionContext>? searcher,
            Func<IQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? orderBy,
            Func<IOrderedQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? thenBy)
            : base(
                  registry,
                  parent,
                  name,
                  resolver,
                  elementGraphType)
        {
            Arguments = arguments;
            ObjectGraphTypeConfigurator = configurator;
            OrderBy = orderBy;
            ThenBy = thenBy;

            if (HasFilter)
            {
                Argument("filter", () => new QueryArgument(Registry.GenerateInputGraphType(ObjectGraphTypeConfigurator?.CreateInlineFilters().FilterType))
                {
                    Name = "filter",
                });
            }

            var sortableFields = ObjectGraphTypeConfigurator?.Sorters.Select(f => f.Name).ToArray();
            if (sortableFields != null && sortableFields.Any())
            {
                Argument("sorting", new ListGraphType(new SortingOptionGraphType(typeof(TReturnType).Name, sortableFields)));
            }

            Searcher = searcher;
            if (searcher != null)
            {
                Argument("search", typeof(string));
            }
        }

        public virtual bool HasFilter => ObjectGraphTypeConfigurator?.HasInlineFilters ?? false;

        protected Func<IQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? OrderBy { get; }

        protected Func<IOrderedQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? ThenBy { get; }

        protected ISearcher<TReturnType, TExecutionContext>? Searcher { get; private set; }

        protected IObjectGraphTypeConfigurator<TReturnType, TExecutionContext>? ObjectGraphTypeConfigurator { get; private set; }

        protected virtual IQueryableResolver<TEntity, TReturnType, TExecutionContext> QueryableFieldResolver => QueryableFieldResolverBase.Reorder(ApplySort(ObjectGraphTypeConfigurator?.Sorters, Searcher, OrderBy, ThenBy));

        protected IQueryableResolver<TEntity, TReturnType, TExecutionContext> QueryableFieldResolverBase => (IQueryableResolver<TEntity, TReturnType, TExecutionContext>)EnumerableFieldResolver;

        public virtual QueryableFieldBase<TEntity, TReturnType, TExecutionContext> ApplyFilter<TLoaderFilter, TFilter>()
            where TLoaderFilter : Filter<TReturnType, TFilter, TExecutionContext>
            where TFilter : Input
        {
            if (HasFilter)
            {
                throw new InvalidOperationException("Cannot apply filter twice.");
            }

            Registry.RegisterInputAutoObjectGraphType<TFilter>();
            var loaderFilterType = typeof(TLoaderFilter);
            var filter = Registry.ResolveFilter<TReturnType>(loaderFilterType);

            Argument("filter", filter.FilterType);
            return ApplyField(ReplaceResolver(QueryableFieldResolverBase.Select(GetFilteredQuery(filter))));
        }

        public QueryableFieldBase<TEntity, TReturnType, TExecutionContext> ApplySearch<TSearcher>()
        {
            var searcherBaseType = TypeHelpers.FindMatchingGenericBaseType(typeof(TSearcher), typeof(Searcher<,>));

            if (searcherBaseType == null)
            {
                throw new ArgumentException($"Cannot find the corresponding base type for filter: {typeof(TSearcher)}");
            }

            if (Searcher != null)
            {
                throw new InvalidOperationException("Cannot apply search twice.");
            }

            Searcher = Registry.ResolveSearcher<TReturnType>(typeof(TSearcher));
            Argument("search", typeof(string));
            return ApplyField(ReplaceResolver(QueryableFieldResolverBase.Select(GetSearchQuery(Searcher))));
        }

        public abstract QueryableFieldBase<TEntity, TReturnType, TExecutionContext> ApplyConnection(Expression<Func<IQueryable<TReturnType>, IOrderedQueryable<TReturnType>>> order);

        protected abstract QueryableFieldBase<TEntity, TReturnType, TExecutionContext> ReplaceResolver(IQueryableResolver<TEntity, TReturnType, TExecutionContext> resolver);

        protected override EnumerableFieldBase<TEntity, TReturnType, TExecutionContext> CreateWhere(Expression<Func<TReturnType, bool>> predicate)
        {
            var queryableField = ReplaceResolver(QueryableFieldResolverBase.Where(predicate));
            return queryableField;
        }

        private static IQueryableResolver<TEntity, TReturnType, TExecutionContext> CreateResolver(
            string fieldName,
            Func<IResolveFieldContext, IQueryable<TReturnType>> query,
            Func<IResolveFieldContext, IQueryable<TReturnType>, IQueryable<TReturnType>> transform,
            Expression<Func<TEntity, TReturnType, bool>>? condition,
            ISearcher<TReturnType, TExecutionContext>? searcher,
            Func<IQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? orderBy,
            Func<IOrderedQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? thenBy,
            IProxyAccessor<TEntity, TExecutionContext> outerProxyAccessor,
            IObjectGraphTypeConfigurator<TReturnType, TExecutionContext> configurator)
        {
            var sorters = configurator.Sorters;

            if (condition == null)
            {
                return new QueryableFuncResolver<TEntity, TReturnType, TExecutionContext>(
                    configurator.ProxyAccessor,
                    GetQuery(configurator, query),
                    transform,
                    ApplySort(sorters, searcher, orderBy, thenBy));
            }

            return new QueryableAsyncFuncResolver<TEntity, TReturnType, TExecutionContext>(
                fieldName,
                GetQuery(configurator, query),
                condition,
                transform,
                ApplySort(sorters, searcher, orderBy, thenBy),
                outerProxyAccessor,
                configurator.ProxyAccessor);

            static Func<IResolveFieldContext, IQueryable<TReturnType>> GetQuery(
                IObjectGraphTypeConfigurator<TReturnType, TExecutionContext> configurator,
                Func<IResolveFieldContext, IQueryable<TReturnType>> queryFactory)
            {
                return context =>
                {
                    var filter = configurator.HasInlineFilters ? configurator.CreateInlineFilters() : null;

                    var query = queryFactory(context);

                    if (filter != null)
                    {
                        var listener = context.GetListener();
                        var ctx = context.GetUserContext<TExecutionContext>();
                        query = filter.All(listener, query, ctx, context.GetFilterValue(filter.FilterType), context.GetFilterFieldNames());
                    }

                    return query;
                };
            }
        }

        private static Func<IResolveFieldContext, IQueryable<TReturnType>, IQueryable<TReturnType>> GetFilteredQuery(IFilter<TReturnType, TExecutionContext> filter)
        {
            return (context, query) =>
            {
                var listener = context.GetListener();
                return filter.All(listener, query, context.GetUserContext<TExecutionContext>(), context.GetFilterValue(filter.FilterType), context.GetFilterFieldNames());
            };
        }

        private static Func<IResolveFieldContext, IQueryable<TReturnType>, IQueryable<TReturnType>> GetSearchQuery(ISearcher<TReturnType, TExecutionContext> searcher)
        {
            return (context, query) =>
            {
                var result = query;
                if (!string.IsNullOrEmpty(context.GetSearch()))
                {
                    result = searcher.All(result, context.GetUserContext<TExecutionContext>(), context.GetSearch());
                }

                return result;
            };
        }

        private static Func<IResolveFieldContext, IQueryable<TReturnType>, IOrderedQueryable<TReturnType>> ApplySort(
            IReadOnlyList<ISorter<TExecutionContext>>? sorters,
            ISearcher<TReturnType, TExecutionContext>? searcher,
            Func<IQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? orderBy,
            Func<IOrderedQueryable<TReturnType>, IOrderedQueryable<TReturnType>>? thenBy)
        {
            return (context, children) => SortingHelpers.ApplySort(
                context,
                children,
                sorters,
                searcher as IOrderedSearcher<TReturnType, TExecutionContext>,
                orderBy,
                thenBy);
        }
    }
}