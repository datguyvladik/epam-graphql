// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using Epam.GraphQL.Configuration;
using Epam.GraphQL.Configuration.Implementations;
using Epam.GraphQL.Loaders;
using GraphQL.Types;

namespace Epam.GraphQL.Types
{
    internal class EdgeGraphType<TChildLoader, TChildEntity, TExecutionContext> : ObjectGraphType<object>
        where TChildLoader : Loader<TChildEntity, TExecutionContext>, new()
    {
        public EdgeGraphType(RelationRegistry<TExecutionContext> registry)
        {
            var typeName = registry.GetProjectionTypeName<TChildLoader, TChildEntity>(false);

            Name = $"{typeName}Edge";

            Description = $"An edge in a connection from an object to another object of type `{typeName}`.";

            Field<NonNullGraphType<StringGraphType>>()
                .Name("cursor")
                .Description("A cursor for use in pagination");

            Field(registry.GetEntityGraphType<TChildLoader, TChildEntity>(), "node", "The item at the end of the edge");
        }
    }

    internal class EdgeGraphType<TReturnType, TExecutionContext> : ObjectGraphType<object>
    {
        public EdgeGraphType(IGraphTypeDescriptor<TReturnType, TExecutionContext> graphType)
        {
            var typeName = graphType.Name;

            Name = $"{typeName}Edge";

            Description = $"An edge in a connection from an object to another object of type `{typeName}`.";

            Field<NonNullGraphType<StringGraphType>>()
                .Name("cursor")
                .Description("A cursor for use in pagination");

            AddField(new FieldType
            {
                Type = graphType.Type,
                ResolvedType = graphType.GraphType,
                Name = "node",
                Description = "The item at the end of the edge",
            });
        }
    }
}
