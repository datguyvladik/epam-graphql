// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Linq.Expressions;
using Epam.GraphQL.Loaders;

namespace Epam.GraphQL.Builders.Projection
{
    public interface IMutationArgumentBuilder<out TArgType, TExecutionContext> :
        IMutationFieldBuilder<IMutationFieldBuilderBase<TArgType, TExecutionContext>, TArgType, TExecutionContext>
    {
        IMutationArgumentBuilder<TArgType, TType, TExecutionContext> Argument<TType>(string name);

        IMutationArgumentBuilder<TArgType, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TProjection, TEntity>(string name)
            where TProjection : Projection<TEntity, TExecutionContext>
            where TEntity : class;

        IMutationArgumentBuilder<TArgType, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TEntity>(Type projectionType, string name)
            where TEntity : class;
    }

    public interface IMutationArgumentBuilder<out TArgType1, out TArgType2, TExecutionContext> :
        IMutationFieldBuilder<IMutationFieldBuilderBase<TArgType1, TArgType2, TExecutionContext>, TArgType1, TArgType2, TExecutionContext>
    {
        IMutationArgumentBuilder<TArgType1, TArgType2, TType, TExecutionContext> Argument<TType>(string name);

        IMutationArgumentBuilder<TArgType1, TArgType2, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TProjection, TEntity>(string name)
            where TProjection : Projection<TEntity, TExecutionContext>
            where TEntity : class;

        IMutationArgumentBuilder<TArgType1, TArgType2, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TEntity>(Type projectionType, string name)
            where TEntity : class;
    }

    public interface IMutationArgumentBuilder<out TArgType1, out TArgType2, out TArgType3, TExecutionContext> :
        IMutationFieldBuilder<IMutationFieldBuilderBase<TArgType1, TArgType2, TArgType3, TExecutionContext>, TArgType1, TArgType2, TArgType3, TExecutionContext>
    {
        IMutationArgumentBuilder<TArgType1, TArgType2, TArgType3, TType, TExecutionContext> Argument<TType>(string name);

        IMutationArgumentBuilder<TArgType1, TArgType2, TArgType3, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TProjection, TEntity>(string name)
            where TProjection : Projection<TEntity, TExecutionContext>
            where TEntity : class;

        IMutationArgumentBuilder<TArgType1, TArgType2, TArgType3, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TEntity>(Type projectionType, string name)
            where TEntity : class;
    }

    public interface IMutationArgumentBuilder<out TArgType1, out TArgType2, out TArgType3, out TArgType4, TExecutionContext> :
        IMutationFieldBuilder<IMutationFieldBuilderBase<TArgType1, TArgType2, TArgType3, TArgType4, TExecutionContext>, TArgType1, TArgType2, TArgType3, TArgType4, TExecutionContext>
    {
        IMutationArgumentBuilder<TArgType1, TArgType2, TArgType3, TArgType4, TType, TExecutionContext> Argument<TType>(string name);

        IMutationArgumentBuilder<TArgType1, TArgType2, TArgType3, TArgType4, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TProjection, TEntity>(string name)
            where TProjection : Projection<TEntity, TExecutionContext>
            where TEntity : class;

        IMutationArgumentBuilder<TArgType1, TArgType2, TArgType3, TArgType4, Expression<Func<TEntity, bool>>, TExecutionContext> FilterArgument<TEntity>(Type projectionType, string name)
            where TEntity : class;
    }

    public interface IMutationArgumentBuilder<out TArgType1, out TArgType2, out TArgType3, out TArgType4, out TArgType5, TExecutionContext> :
        IMutationFieldBuilder<IMutationFieldBuilderBase<TArgType1, TArgType2, TArgType3, TArgType4, TArgType5, TExecutionContext>, TArgType1, TArgType2, TArgType3, TArgType4, TArgType5, TExecutionContext>
    {
    }
}
