// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epam.GraphQL.Builders.Loader;
using Epam.GraphQL.Configuration.Implementations.Descriptors;
using Epam.GraphQL.Configuration.Implementations.Fields.Helpers;
using Epam.GraphQL.Configuration.Implementations.Fields.ResolvableFields;
using Epam.GraphQL.Diagnostics;

namespace Epam.GraphQL.Configuration.Implementations.Fields
{
    internal class Field<TEntity, TExecutionContext> :
        FieldBase<TEntity, TExecutionContext>,
        IUnionableField<TEntity, TExecutionContext>
    {
        public Field(IChainConfigurationContext configurationContext, BaseObjectGraphTypeConfigurator<TEntity, TExecutionContext> parent, string name)
            : base(configurationContext, parent, name)
        {
        }

        public Field(Func<IChainConfigurationContextOwner, IChainConfigurationContext> configurationContextFactory, BaseObjectGraphTypeConfigurator<TEntity, TExecutionContext> parent, string name)
            : base(configurationContextFactory, parent, name)
        {
        }

        public void Resolve<TReturnType>(Func<TExecutionContext, TEntity, TReturnType> resolve, Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build)
        {
            var configurationContext = ConfigurationContext.Chain<TReturnType>(nameof(Resolve))
                .Argument(resolve)
                .OptionalArgument(build);

            var graphType = Parent.GetGraphQLTypeDescriptor(this, build, configurationContext);

            Parent.ApplyResolvedField<TReturnType>(
                configurationContext,
                this,
                graphType,
                ResolvedFieldResolverFactory.Create(Resolvers.ConvertFieldResolver(Name, resolve, Parent.ProxyAccessor)));
        }

        public void Resolve<TReturnType>(Func<TExecutionContext, TEntity, Task<TReturnType>> resolve, Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build)
        {
            var configurationContext = ConfigurationContext.Chain<TReturnType>(nameof(Resolve))
                .Argument(resolve)
                .OptionalArgument(build);

            var graphType = Parent.GetGraphQLTypeDescriptor(this, build, configurationContext);

            Parent.ApplyResolvedField<TReturnType>(
                configurationContext,
                this,
                graphType,
                ResolvedFieldResolverFactory.Create(Resolvers.ConvertFieldResolver(Name, resolve, Parent.ProxyAccessor)));
        }

        public void Resolve<TReturnType>(Func<TExecutionContext, TEntity, IEnumerable<TReturnType>> resolve, Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build)
        {
            var configurationContext = ConfigurationContext.Chain<TReturnType>(nameof(Resolve))
                .Argument(resolve)
                .OptionalArgument(build);

            var graphType = Parent.GetGraphQLTypeDescriptor(this, build, configurationContext).MakeListDescriptor();

            Parent.ApplyResolvedField<IEnumerable<TReturnType>>(
                configurationContext,
                this,
                graphType,
                ResolvedFieldResolverFactory.Create(Resolvers.ConvertFieldResolver(Name, resolve, Parent.ProxyAccessor)));
        }

        public void Resolve<TReturnType>(Func<TExecutionContext, TEntity, Task<IEnumerable<TReturnType>>> resolve, Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build)
        {
            var configurationContext = ConfigurationContext.Chain<TReturnType>(nameof(Resolve))
                .Argument(resolve)
                .OptionalArgument(build);

            var graphType = Parent.GetGraphQLTypeDescriptor(this, build, configurationContext).MakeListDescriptor();

            Parent.ApplyResolvedField<IEnumerable<TReturnType>>(
                configurationContext,
                this,
                graphType,
                ResolvedFieldResolverFactory.Create(Resolvers.ConvertFieldResolver(Name, resolve, Parent.ProxyAccessor)));
        }

        public IUnionableField<TEntity, TExecutionContext> AsUnionOf<TLastElementType>(Action<IInlineObjectBuilder<TLastElementType, TExecutionContext>>? build)
        {
            return And(build);
        }

        public IUnionableField<TEntity, TExecutionContext> And<TLastElementType>(Action<IInlineObjectBuilder<TLastElementType, TExecutionContext>>? build)
        {
            var unionField = UnionField.Create(
                ConfigurationContext.Chain<TLastElementType>(nameof(And))
                    .OptionalArgument(build),
                Parent,
                Name,
                build);
            return Parent.ReplaceField(this, unionField);
        }

        public IUnionableField<TEntity, TExecutionContext> AsUnionOf<TEnumerable, TLastElementType>(Action<IInlineObjectBuilder<TLastElementType, TExecutionContext>>? build)
            where TEnumerable : IEnumerable<TLastElementType>
        {
            return And<TEnumerable, TLastElementType>(build);
        }

        public IUnionableField<TEntity, TExecutionContext> And<TEnumerable, TLastElementType>(Action<IInlineObjectBuilder<TLastElementType, TExecutionContext>>? build)
            where TEnumerable : IEnumerable<TLastElementType>
        {
            return And(build);
        }
    }
}
