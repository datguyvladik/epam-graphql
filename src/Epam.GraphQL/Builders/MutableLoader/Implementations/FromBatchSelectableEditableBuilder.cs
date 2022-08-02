// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using Epam.GraphQL.Builders.Loader;
using Epam.GraphQL.Configuration;
using Epam.GraphQL.Configuration.Implementations;
using Epam.GraphQL.Configuration.Implementations.Fields;
using Epam.GraphQL.Loaders;

namespace Epam.GraphQL.Builders.MutableLoader.Implementations
{
    internal class FromBatchSelectableEditableBuilder<TField, TSourceType, TReturnType, TExecutionContext> : FromBatchEditableBuilder<TSourceType, TReturnType, TExecutionContext>,
        IHasEditable<TSourceType, TReturnType, TExecutionContext>,
        IHasEditableAndOnWrite<TSourceType, TReturnType, TExecutionContext>,
        IHasEditableAndOnWriteAndMandatoryForUpdate<TSourceType, TReturnType, TExecutionContext>,
        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelect<TSourceType, TReturnType, TExecutionContext>,
        IHasEditableAndOnWriteAndMandatoryForUpdateAndSelectAndReferenceTo<TSourceType, TReturnType, TExecutionContext>
        where TField : FieldBase<TSourceType, TExecutionContext>, IFieldSupportsApplySelect<TSourceType, TReturnType, TExecutionContext>, IFieldSupportsEditSettings<TSourceType, TReturnType, TExecutionContext>
    {
        internal FromBatchSelectableEditableBuilder(RelationRegistry<TExecutionContext> registry, TField field)
            : base(registry, field)
        {
            Field = field;
        }

        protected new TField Field { get; set; }

        public IHasEditableAndOnWriteAndMandatoryForUpdate<TSourceType, TReturnType, TExecutionContext> ReferenceTo<TChildEntity, TChildEntityLoader>(Predicate<TReturnType> isFakePropValue)
            where TChildEntityLoader : Loader<TChildEntity, TExecutionContext>, IIdentifiableLoader, new()
        {
            Registry.RegisterRelationPostponedForSave<TSourceType, TChildEntity, TChildEntityLoader, TReturnType, TReturnType>(Field.Name, isFakePropValue);

            return this;
        }

        public IHasEditableAndOnWriteAndMandatoryForUpdate<TSourceType, TReturnType1, TExecutionContext> Select<TReturnType1>(
            Func<TReturnType, TReturnType1> selector,
            Action<IInlineObjectBuilder<TReturnType1, TExecutionContext>>? build)
        {
            var select = Field.ApplySelect(
                Field.ConfigurationContext.Chain<TReturnType>(nameof(Select))
                    .Argument(selector)
                    .OptionalArgument(build),
                selector,
                build);

            return new FromBatchEditableBuilder<TSourceType, TReturnType1, TExecutionContext>(Registry, select);
        }
    }
}
