// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Epam.GraphQL.Builders.Loader;

namespace Epam.GraphQL.Builders.MutableLoader
{
    public interface IHasFromBatch<TEntity, TExecutionContext>
    {
        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TReturnType>(
            Func<TExecutionContext, IEnumerable<TEntity>, IDictionary<TEntity, TReturnType>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<TExecutionContext, IEnumerable<TKeyType>, IDictionary<TKeyType, TReturnType>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TReturnType>(
            Func<IEnumerable<TEntity>, IDictionary<TEntity, TReturnType>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<IEnumerable<TKeyType>, IDictionary<TKeyType, TReturnType>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TReturnType>(
            Func<TExecutionContext, IEnumerable<TEntity>, IDictionary<TEntity, IEnumerable<TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<TExecutionContext, IEnumerable<TKeyType>, IDictionary<TKeyType, IEnumerable<TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TReturnType>(
            Func<IEnumerable<TEntity>, IDictionary<TEntity, IEnumerable<TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<IEnumerable<TKeyType>, IDictionary<TKeyType, IEnumerable<TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TReturnType>(
            Func<TExecutionContext, IEnumerable<TEntity>, Task<IDictionary<TEntity, TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<TExecutionContext, IEnumerable<TKeyType>, Task<IDictionary<TKeyType, TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TReturnType>(
            Func<IEnumerable<TEntity>, Task<IDictionary<TEntity, TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, TReturnType, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<IEnumerable<TKeyType>, Task<IDictionary<TKeyType, TReturnType>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TReturnType>(
            Func<TExecutionContext, IEnumerable<TEntity>, Task<IDictionary<TEntity, IEnumerable<TReturnType>>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<TExecutionContext, IEnumerable<TKeyType>, Task<IDictionary<TKeyType, IEnumerable<TReturnType>>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TReturnType>(
            Func<IEnumerable<TEntity>, Task<IDictionary<TEntity, IEnumerable<TReturnType>>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);

        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceToAndAndFromBatch<TEntity, IEnumerable<TReturnType>, TExecutionContext> FromBatch<TKeyType, TReturnType>(
            Expression<Func<TEntity, TKeyType>> keySelector,
            Func<IEnumerable<TKeyType>, Task<IDictionary<TKeyType, IEnumerable<TReturnType>>>> batchFunc,
            Action<IInlineObjectBuilder<TReturnType, TExecutionContext>>? build = null);
    }
}
