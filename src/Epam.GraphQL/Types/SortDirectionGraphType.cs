﻿// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using Epam.GraphQL.Loaders;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Epam.GraphQL.Types
{
    internal class SortDirectionGraphType : ScalarGraphType
    {
        public SortDirectionGraphType()
        {
            Name = nameof(SortDirection);
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is SortDirectionValue sortDirectionValue)
            {
                return ParseValue(sortDirectionValue.Value);
            }

            if (value is EnumValue enumValue)
            {
                return ParseValue(enumValue.Name);
            }

            if (value is StringValue stringValue)
            {
                return ParseValue(stringValue.Value);
            }

            return null;
        }

        public override object ParseValue(object value)
        {
            return ValueConverter.ConvertTo(value, typeof(SortDirection));
        }

        public override object Serialize(object value)
        {
            return ValueConverter.ConvertTo(value, typeof(SortDirection));
        }
    }
}
