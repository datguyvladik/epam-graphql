﻿// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Reflection;
using Epam.GraphQL.Extensions;

namespace Epam.GraphQL.Loaders
{
    internal static class FieldChange
    {
        private static MethodInfo _create;

        public static IFieldChange<TEntity, TExecutionContext> Create<TEntity, TExecutionContext>(Type valueType, TExecutionContext context, TEntity entity, object previousValue, object nextValue)
        {
            return Create<TEntity, TExecutionContext>(valueType).InvokeAndHoistBaseException<IFieldChange<TEntity, TExecutionContext>>(null, context, entity, previousValue, nextValue);
        }

        public static IFieldChange<TEntity, T, TExecutionContext> Create<TEntity, T, TExecutionContext>(TExecutionContext context, TEntity entity, T previousValue, T nextValue)
        {
            return new FieldChange<TEntity, T, TExecutionContext>()
            {
                Context = context,
                Entity = entity,
                PreviousValue = previousValue,
                NextValue = nextValue,
            };
        }

        private static MethodInfo Create<TEntity, TExecutionContext>(Type value)
        {
            return (_create ??= new Func<TExecutionContext, TEntity, object, object, IFieldChange<TEntity, TExecutionContext>>(Create).GetMethodInfo()!.GetGenericMethodDefinition()).MakeGenericMethod(typeof(TEntity), value, typeof(TExecutionContext));
        }
    }

    internal class FieldChange<TEntity, T, TExecutionContext> : IFieldChange<TEntity, T, TExecutionContext>, IFieldChange<TEntity, TExecutionContext>
    {
        public TExecutionContext Context { get; set; }

        public TEntity Entity { get; set; }

        public T PreviousValue { get; set; }

        public T NextValue { get; set; }

        object IFieldChange<TEntity, TExecutionContext>.PreviousValue => PreviousValue;

        object IFieldChange<TEntity, TExecutionContext>.NextValue => NextValue;

        object IFieldChange<TExecutionContext>.Entity => Entity;
    }
}
