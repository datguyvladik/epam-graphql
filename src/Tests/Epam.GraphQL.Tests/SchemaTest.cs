// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Collections.Generic;
using System.Linq;
using Epam.GraphQL.Loaders;
using Epam.GraphQL.Tests.Helpers;
using Epam.GraphQL.Tests.TestData;
using GraphQL;
using GraphQL.Execution;
using NUnit.Framework;

namespace Epam.GraphQL.Tests
{
    [TestFixture]
    public class SchemaTest : BaseTests
    {
        private const string GraphiqlRequest = @"
  query IntrospectionQuery {
    __schema {
      queryType { name }
      mutationType { name }
      subscriptionType { name }
      types {
        ...FullType
      }
      directives {
        name
        description
        locations
        args {
          ...InputValue
        }
      }
    }
  }

  fragment FullType on __Type {
    kind
    name
    description
    fields(includeDeprecated: true) {
      name
      description
      args {
        ...InputValue
      }
      type {
        ...TypeRef
      }
      isDeprecated
      deprecationReason
    }
    inputFields {
      ...InputValue
    }
    interfaces {
      ...TypeRef
    }
    enumValues(includeDeprecated: true) {
      name
      description
      isDeprecated
      deprecationReason
    }
    possibleTypes {
      ...TypeRef
    }
  }

  fragment InputValue on __InputValue {
    name
    description
    type { ...TypeRef }
    defaultValue
  }

  fragment TypeRef on __Type {
    kind
    name
    ofType {
      kind
      name
      ofType {
        kind
        name
        ofType {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
              ofType {
                kind
                name
                ofType {
                  kind
                  name
                }
              }
            }
          }
        }
      }
    }
  }
";

        [Test]
        public void GraphiqlQueryTest()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(person => person.Id);
                    loader.Field(person => person.FullName);
                    loader.Field(person => person.Salary);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            var securityPersonLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(person => person.Id);
                    loader.Field(person => person.FullName);
                    loader.Field(person => person.Salary);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, query) => query.Where(p => p.ManagerId == 1));

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(onConfigure: query =>
            {
                query.Connection(personLoaderType, "people");
                query.Connection(securityPersonLoaderType, "securityPeople");
            });

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, GraphiqlRequest);
            var actualQuery = actualResult.Query;
            var actualErrorsCount = actualResult.Errors?.Count ?? 0;

            const int expectedErrorsCount = 0;

            Assert.AreEqual(expectedErrorsCount, actualErrorsCount);
            Assert.AreEqual(GraphiqlRequest, actualQuery);
        }

        [Test(Description = "Empty config")]
        public void ShouldThrowOnQueryEmptyConfig()
        {
            // TODO message should be "Query should have one field at least"
            TestHelpers.TestQueryError(
                query => { },
                typeof(InvalidOperationException),
                "Type `object` should have one readable field at least.",
                string.Empty);
        }

        [Test(Description = "Query:Field<int>(...) without resolver")]
        public void ShouldThrowOnQueryFieldWithoutResolver()
        {
            TestHelpers.TestQueryError(
                query => query
                    .Field("test"),
                typeof(InvalidOperationException),
                "Field `test` must have resolver.",
                string.Empty);
        }

        [Test(Description = "Query:Field<int>(null)/Resolve(...)")]
        public void ShouldThrowOnNullQueryFieldName()
        {
            TestHelpers.TestQueryError(
                query => query
                    .Field(null)
                    .Resolve(_ => 0),
                typeof(InvalidOperationException),
                "Field name cannot be null or empty.",
                string.Empty);
        }

        [Test(Description = "Query:Field<int>(\"\")/Resolve(...)")]
        public void ShouldThrowOnEmptyQueryFieldName()
        {
            TestHelpers.TestQueryError(
                query => query
                    .Field(string.Empty)
                    .Resolve(_ => 0),
                typeof(InvalidOperationException),
                "Field name cannot be null or empty.",
                string.Empty);
        }

        [Test(Description = "Query:Duplicate field")]
        public void ShouldThrowOnDuplicateQueryField()
        {
            static void Builder(Query<TestUserContext> query)
            {
                query.Field("test")
                    .Resolve(_ => 0);
                query.Field("test")
                    .Resolve(_ => 0);
            }

            TestHelpers.TestQueryError(
                Builder,
                typeof(InvalidOperationException),
                "A field with the name `test` is already registered.",
                string.Empty);
        }

        [Test(Description = "Query:Connection<object>(...)")]
        public void ShouldThrowOnQueryConnectionFromNonLoader()
        {
            TestHelpers.TestQueryError(
                query => query
                    .Connection<object>("test"),
                typeof(ArgumentException),
                "Cannot find the corresponding base type for loader: System.Object",
                string.Empty);
        }

        [Test(Description = "Loader:Field<int>(...) without resolver")]
        public void ShouldThrowOnLoaderFieldWithoutResolver()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader => loader.Field("test"),
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            void Builder(Query<TestUserContext> query)
            {
                query
                    .Connection(personLoaderType, "people");
            }

            TestHelpers.TestQueryError(
                Builder,
                typeof(InvalidOperationException),
                "Field `test` must have resolver.",
                string.Empty);
        }

        [Test(Description = "Loader:Field(p => 2 * p.Id)")]
        public void ShouldThrowOnLoaderFieldWithExpressionAndNoName()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader => loader.Field(p => 2 * p.Id),
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            void Builder(Query<TestUserContext> query)
            {
                query
                    .Connection(personLoaderType, "people");
            }

            TestHelpers.TestQueryError(
                Builder,
                typeof(InvalidOperationException),
                "Expression (p => (2 * p.Id)), provided for field is not a property. Consider to give a name to field explicitly.",
                string.Empty);
        }

        [Test]
        public void ShouldThrowOnFieldReturningValueTypeWithoutConfig()
        {
            static void Builder(Query<TestUserContext> query)
            {
                query
                    .Field("people")
                    .Resolve(_ => ("abc", "abc"));
            }

            TestHelpers.TestQueryError(
                Builder,
                typeof(ArgumentOutOfRangeException),
                "The type: ValueTuple`2 cannot be coerced effectively to a GraphQL type (Parameter 'type')",
                string.Empty);
        }

        /// <summary>
        /// Test for https://git.epam.com/epm-ppa/epam-graphql/-/issues/22 (EF Query fails for loaders having instance methods in field expressions).
        /// </summary>
        [Test]
        public void ShouldThrowOnFieldThatUsesInstanceMember()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader => loader.Field("instance", p => GetPersonName(p)),
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            void Builder(Query<TestUserContext> query)
            {
                query
                    .Connection(personLoaderType, "people");
            }

            TestHelpers.TestQueryError(
                Builder,
                typeof(InvalidOperationException),
                "Client projection (value(Epam.GraphQL.Tests.SchemaTest).GetPersonName(p)) contains a call of instance method 'GetPersonName' of type 'SchemaTest'. This could potentially cause memory leak. Consider making the method static so that it does not capture constant in the instance.",
                string.Empty);
        }

        [Test(Description = "Query:Connection(...)/WithFilter<object>()")]
        public void ShouldThrowOnQueryConnectionWithFilterFromNonFilter()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader => loader.Field(p => p.Id),
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            void Builder(Query<TestUserContext> query)
            {
                query
                    .Connection(personLoaderType, "people")
                    .WithFilter<object>();
            }

            TestHelpers.TestQueryError(
                Builder,
                typeof(ArgumentException),
                "Cannot find the corresponding base type for filter: System.Object",
                string.Empty);
        }

        [Test(Description = "ApplySecurityFilter() based on another query")]
        public void ShouldThrowIfApplySecurityFilterReturnsQueryNotBasedPassedQuery()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader => loader.Field(p => p.Id),
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            TestHelpers.TestQueryError(
                query => query
                    .Connection(personLoaderType, "people"),
                typeof(ExecutionError),
                "PersonLoader.ApplySecurityFilter:  You cannot query data from anywhere (e.g. using context), please use passed `query` parameter.",
                @"
                query {
                    people {
                        items {
                            id
                        }
                    }
                }
                ");
        }

        [Test]
        public void ShouldThrowIfFromBatchSourceTypeDoesNotContainFields()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("error")
                        .FromBatch(people => people.ToDictionary(p => p, p => new Empty()), null);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            TestHelpers.TestQueryError(
                query => query
                    .Connection(personLoaderType, "people"),
                typeof(InvalidOperationException),
                "Type `Empty` should have one readable field at least.",
                string.Empty);
        }

        [Test]
        public void ShouldNotExposeIndexedFieldFromBatch()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("error")
                        .FromBatch(people => people.ToDictionary(p => p, p => new WithIndexedProperty()), null);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            TestHelpers.TestQueryError(
                query => query
                    .Connection(personLoaderType, "people"),
                typeof(InvalidOperationException),
                "Type `WithIndexedProperty` should have one readable field at least.",
                string.Empty);
        }

        [Test]
        public void ShouldExposeDifferentTypesForLoaderAndInlineObject()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("manager")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("AutoPerson", fieldTypeName);
        }

        [Test]
        public void ShouldExposeDifferentTypesForLoaderAndConfiguredInlineObject()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("manager")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager), build => build.Field(p => p.Id));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("PersonManager", fieldTypeName);
        }

        [Test]
        public void ShouldExposeDifferentTypesForTwoIdenticalyConfiguredInlineObjects()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("manager1")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager), build => build.Field(p => p.Id));
                    loader.Field("manager2")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager), build => build.Field(p => p.Id));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fields = GetFieldValue(actualResult.Data, "__type", "fields") as IEnumerable<object>;

            var manager1Field = fields.Where(f => (string)GetFieldValue(f, "name") == "manager1");
            var manager2Field = fields.Where(f => (string)GetFieldValue(f, "name") == "manager2");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("PersonManager1", GetFieldValue(manager1Field, "type", "name"));
            Assert.AreEqual("PersonManager2", GetFieldValue(manager2Field, "type", "name"));
        }

        [Test]
        public void ShouldExposeFieldNameForUnionInlineObjects()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("fieldName")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager))
                        .AndFromBatch(people => people.ToDictionary(p => p, p => p.Unit));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                ofType {
                                    name
                                }
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "ofType", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("PersonFieldName", fieldTypeName);
        }

        [Test]
        public void ShouldExposeFieldNameTypeForUnionInlineObjectsWithBuilder()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("field")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager), build => build.Field(p => p.Id))
                        .AndFromBatch(people => people.ToDictionary(p => p, p => p.Unit));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                ofType {
                                    name
                                }
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "ofType", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("PersonField", fieldTypeName);
        }

        [Test]
        public void ShouldExposeTheSameTypesForLoaderAndInlineObjectConfiguredFromTheSameLoader()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("manager")
                        .FromBatch(people => people.ToDictionary(p => p, p => p.Manager), build => build.ConfigureFrom(loader.GetType()));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("Person", fieldTypeName);
        }

        [Test]
        public void ShouldExposeTheSameTypesForLoaderAndNestedLoader()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("manager")
                        .FromLoader<Person>(loader.GetType(), (p, m) => p.ManagerId == m.Id)
                        .FirstOrDefault();
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));
            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);
            Assert.AreEqual("Person", fieldTypeName);
        }

        [Test]
        public void ShouldExposeDifferentTypesForTwoLoaders()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var person2LoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable(),
                typeName: "UserLoader");

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "people");
                q.Connection(person2LoaderType, "users");
            });

            const string query1 = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);

            const string query2 = @"
                query {
                    __type(name: ""User"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("User", loaderEntityTypeName);
        }

        [Test]
        public void ShouldExposeDifferentTypesForTwoConnectionsOfDifferentLoadersOfTheSameEntities()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var person2LoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable(),
                typeName: "UserLoader");

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "people");
                q.Connection(person2LoaderType, "users");
            });

            const string query1 = @"
                query {
                    __type(name: ""PersonConnection"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("PersonConnection", loaderEntityTypeName);

            const string query2 = @"
                query {
                    __type(name: ""UserConnection"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("UserConnection", loaderEntityTypeName);

            const string query3 = @"
                query {
                    __type(name: ""PersonEdge"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query3);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("PersonEdge", loaderEntityTypeName);

            const string query4 = @"
                query {
                    __type(name: ""UserEdge"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query4);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("UserEdge", loaderEntityTypeName);
        }

        [Test]
        public void ShouldExposeTheSameTypesForLoaderAndAllItsDescendants()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var person2LoaderType = GraphQLTypeBuilder.CreateInheritedLoaderType<Person, TestUserContext>(
                personLoaderType,
                typeName: "Person2Loader");

            var person3LoaderType = GraphQLTypeBuilder.CreateInheritedLoaderType<Person, TestUserContext>(
                person2LoaderType,
                typeName: "Person3Loader");

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "people");
                q.Connection(person2LoaderType, "people2");
                q.Connection(person3LoaderType, "people3");
            });

            const string query1 = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("Person", loaderEntityTypeName);

            const string query2 = @"
                query {
                    __type(name: ""Person2"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(loaderEntityTypeName);

            const string query3 = @"
                query {
                    __type(name: ""Person3"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query3);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(loaderEntityTypeName);
        }

        [Test]
        public void ShouldExposeTheSameInputTypesForLoaderAndAllItsDescendants()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateMutableLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var person2LoaderType = GraphQLTypeBuilder.CreateInheritedLoaderType<Person, TestUserContext>(
                personLoaderType,
                typeName: "Person2Loader");

            var person3LoaderType = GraphQLTypeBuilder.CreateInheritedLoaderType<Person, TestUserContext>(
                person2LoaderType,
                typeName: "Person3Loader");

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "people");
                q.Connection(person2LoaderType, "people2");
                q.Connection(person3LoaderType, "people3");
            });

            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(q =>
            {
                q.SubmitField(personLoaderType, "people");
                q.SubmitField(person2LoaderType, "people2");
                q.SubmitField(person3LoaderType, "people3");
            });

            const string query1 = @"
                query {
                    __type(name: ""InputPerson"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("InputPerson", loaderEntityTypeName);

            const string query2 = @"
                query {
                    __type(name: ""InputPerson2"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query2);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(loaderEntityTypeName);

            const string query3 = @"
                query {
                    __type(name: ""InputPerson3"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query3);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(loaderEntityTypeName);
        }

        [Test]
        public void ShouldExposeQueryTypeNameForQuery()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"), typeName: "MyQuery");

            const string query1 = @"
                query {
                    __type(name: ""MyQuery"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("MyQuery", loaderEntityTypeName);
        }

        [Test]
        public void ShouldExposeMutationTypeNameForQuery()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"), typeName: "MyMutation");

            const string query1 = @"
                query {
                    __type(name: ""MyMutation"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("MyMutation", loaderEntityTypeName);
        }

        [Test]
        public void ShouldNotGenerateFilterTypeIfNoFiltersApplied()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));

            const string query1 = @"
                query {
                    __type(name: ""PersonFilter"") {
                        name
                        fields {
                            type {
                                name
                                ofType {
                                    name
                                }
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(filterTypeName);
        }

        [Test]
        public void ShouldGenerateFilterTypeIfInlineFilterApplied()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id).Filterable();
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people"));

            const string query1 = @"
                query {
                    __type(name: ""InputPersonFilter"") {
                        name
                        fields {
                            type {
                                name
                                ofType {
                                    name
                                }
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("InputPersonFilter", filterTypeName);
        }

        [Test]
        public void ShouldNotGenerateInlineFilterTypeIfWithFilterApplied()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var filterType = GraphQLTypeBuilder.CreateLoaderFilterType<Person, PersonFilter, TestUserContext>((context, q, filter) => q);

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people").WithFilter(filterType));

            const string query1 = @"
                query {
                    __type(name: ""InputPersonFilter"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("InputPersonFilter", filterTypeName);

            const string query2 = @"
                query {
                    __type(name: ""InputPersonFilter1"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(filterTypeName);
        }

        [Test]
        public void ShouldNotGenerateBothInputTypeAndTypeIfFilterIsUsedForLoaderField()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field("filter").FromBatch(people => people.ToDictionary(p => p, p => new PersonFilter()));
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var filterType = GraphQLTypeBuilder.CreateLoaderFilterType<Person, PersonFilter, TestUserContext>((context, q, filter) => q);

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Connection(personLoaderType, "people").WithFilter(filterType));

            const string query1 = @"
                query {
                    __type(name: ""PersonFilter"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("PersonFilter", filterTypeName);
            var fieldName = GetFieldValue(actualResult.Data, "__type", "fields", "name");
            Assert.AreEqual("id", fieldName);

            const string query2 = @"
                query {
                    __type(name: ""InputPersonFilter"") {
                        name
                        inputFields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("InputPersonFilter", filterTypeName);
            fieldName = GetFieldValue(actualResult.Data, "__type", "inputFields", "name");
            Assert.AreEqual("id", fieldName);
        }

        [Test]
        public void ShouldGeneratePayloadTypeForQuery()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var filterType = GraphQLTypeBuilder.CreateLoaderFilterType<Person, PersonFilter, TestUserContext>((context, q, filter) => q);

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Field("myQuery").PayloadField<int>("arg").Resolve((_, arg) => arg));

            const string query = @"
                query {
                    __type(name: ""InputMyQueryPayload"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);
            var filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("InputMyQueryPayload", filterTypeName);
        }

        [Test]
        public void ShouldGeneratePayloadTypeForMutation()
        {
            var filterType = GraphQLTypeBuilder.CreateLoaderFilterType<Person, PersonFilter, TestUserContext>((context, q, filter) => q);

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q => q.Field("myQuery").PayloadField<int>("arg").Resolve((_, arg) => arg));
            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(q => q.Field("myMutation").PayloadField<int>("arg").Resolve((_, arg) => arg));

            const string query = @"
                query {
                    __type(name: ""InputMyMutationPayload"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query);
            var filterTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("InputMyMutationPayload", filterTypeName);
        }

        [Test]
        public void ShouldExposeLoaderTypeOnQueryField()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Field("myQuery").Resolve(_ => FakeData.HannieEveritt, builder => builder.ConfigureFrom(personLoaderType));
            });

            const string query = @"
                query {
                    __type(name: ""Query"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "fields", "type", "name");
            Assert.AreEqual("Person", fieldTypeName);
        }

        [Test]
        public void ShouldNotThrowForEnumAndModelBothHavingTheSameNames()
        {
            var loaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => 1),
                applyNaturalThenBy: q => q.OrderBy(p => 1),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, q) => q);

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(loaderType, "people");
                q.Field("people2").Resolve(ctx => TestData.Extra.Person.None);
            });

            const string query1 = @"
                query {
                    __type(name: ""Person"") {
                        kind
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "kind");
            Assert.AreEqual("OBJECT", fieldTypeName);

            const string query2 = @"
                query {
                    __schema {
                        types {
                            name
                            kind
                        }
                    }
                    __type(name: ""Person1"") {
                        kind
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            fieldTypeName = GetFieldValue(actualResult.Data, "__type", "kind");
            Assert.AreEqual("ENUM", fieldTypeName);
        }

        [Test]
        public void ShouldNotThrowForEnumAndAutoModelBothHavingTheSameNames()
        {
            var loaderType = GraphQLTypeBuilder.CreateLoaderType<Unit, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                    loader.Field("people")
                        .FromIQueryable(people => FakeData.People.AsQueryable(), (unit, person) => unit.Id == person.UnitId);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => 1),
                applyNaturalThenBy: q => q.OrderBy(p => 1),
                getBaseQuery: _ => FakeData.Units.AsQueryable(),
                applySecurityFilter: (_, q) => q);

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(loaderType, "units");
                q.Field("person").Resolve(ctx => TestData.Extra.Person.None);
            });

            const string query1 = @"
                query {
                    __type(name: ""Person1"") {
                        kind
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "kind");
            Assert.AreEqual("ENUM", fieldTypeName);

            const string query2 = @"
                query {
                    __schema {
                        types {
                            name
                            kind
                        }
                    }
                    __type(name: ""Person"") {
                        kind
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            fieldTypeName = GetFieldValue(actualResult.Data, "__type", "kind");
            Assert.AreEqual("OBJECT", fieldTypeName);
        }

        [Test]
        public void ShouldExposeLoaderTypeWithProvidedName()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Name = "MyPerson";
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "myQuery");
            });

            const string query = @"
                query {
                    __type(name: ""MyPerson"") {
                        name
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("MyPerson", fieldTypeName);
        }

        [Test]
        public void ShouldExposeLoaderTypeWithProvidedInputName()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateMutableLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.InputName = "InputMyPerson";
                    loader.Field(p => p.Id).Editable();
                },
                getBaseQuery: _ => FakeData.People.AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "myQuery");
            });
            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(q =>
            {
                q.SubmitField(personLoaderType, "person");
            });

            const string query = @"
                query {
                    __type(name: ""InputMyPerson"") {
                        name
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query);
            var fieldTypeName = GetFieldValue(actualResult.Data, "__type", "name");
            Assert.AreEqual("InputMyPerson", fieldTypeName);
        }

        [Test]
        public void ShouldExposeNonNullableField()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id);
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.ThenBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable());

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "myQuery");
            });

            const string query = @"
                query {
                    __type(name: ""Person"") {
                        name
                        fields {
                            name
                            type {
                                kind
                                name
                            }
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query);

            var fieldTypeKind = GetFieldValue(actualResult.Data, "__type", "fields", "type", "kind");

            Assert.AreEqual("NON_NULL", fieldTypeKind);
        }

        [Test]
        public void ShouldExposeTheSameTypesForFiltersForLoaderAndAllItsDescendants()
        {
            var personLoaderType = GraphQLTypeBuilder.CreateLoaderType<Person, TestUserContext>(
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id).Filterable();
                },
                applyNaturalOrderBy: q => q.OrderBy(p => p.Id),
                applyNaturalThenBy: q => q.OrderBy(p => p.Id),
                getBaseQuery: _ => FakeData.People.AsQueryable(),
                applySecurityFilter: (_, __) => Array.Empty<Person>().AsQueryable());

            var person2LoaderType = GraphQLTypeBuilder.CreateInheritedLoaderType<Person, TestUserContext>(
                personLoaderType,
                typeName: "Person2Loader",
                onConfigure: loader =>
                {
                    loader.Field(p => p.Id).Filterable();
                    loader.Field(p => p.FullName);
                });

            var person3LoaderType = GraphQLTypeBuilder.CreateInheritedLoaderType<Person, TestUserContext>(
                person2LoaderType,
                typeName: "Person3Loader");

            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(q =>
            {
                q.Connection(personLoaderType, "people");
                q.Connection(person2LoaderType, "people2");
                q.Connection(person3LoaderType, "people3");
            });

            const string query1 = @"
                query {
                    __type(name: ""InputPersonFilter"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, query1);
            var loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.AreEqual("InputPersonFilter", loaderEntityTypeName);

            const string query2 = @"
                query {
                    __type(name: ""InputPersonFilter1"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query2);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(loaderEntityTypeName);

            const string query3 = @"
                query {
                    __type(name: ""InputPersonFilter2"") {
                        name
                        fields {
                            type {
                                name
                            }
                            name
                        }
                    }
                }
            ";

            actualResult = ExecuteHelpers.ExecuteQuery(queryType, query3);
            loaderEntityTypeName = GetFieldValue(actualResult.Data, "__type", "name");

            Assert.IsNull(loaderEntityTypeName);
        }

        /// <summary>
        /// Test for https://git.epam.com/epm-ppa/epam-graphql/-/issues/103.
        /// </summary>
        [Test]
        public void Issue103TwoArgs()
        {
            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(query =>
            {
                query.Field("test").Resolve(_ => 1);
            });
            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(mutation =>
            {
                mutation.Field("test")
                    .PayloadField<WithId>("arg1")
                    .PayloadField<int>("arg2")
                    .Resolve((ctx, arg1, arg2) => 1);
            });

            const string query = @"
                query {
                    __type(name: ""InputTestPayload"") {
                        name
                        inputFields {
                            name
                            type {
                                kind
                                name
                            }
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query);

            var arg1FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "0", "type", "name");
            Assert.AreEqual("InputWithId", arg1FieldTypeName);

            var arg2FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "1", "type", "name");
            Assert.AreEqual("Int", arg2FieldTypeName);
        }

        /// <summary>
        /// Test for https://git.epam.com/epm-ppa/epam-graphql/-/issues/103.
        /// </summary>
        [Test]
        public void Issue103ThreeArgs()
        {
            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(query =>
            {
                query.Field("test").Resolve(_ => 1);
            });
            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(mutation =>
            {
                mutation.Field("test")
                    .PayloadField<WithId>("arg1")
                    .PayloadField<int>("arg2")
                    .PayloadField<int>("arg3")
                    .Resolve((ctx, arg1, arg2, arg3) => 1);
            });

            const string query = @"
                query {
                    __type(name: ""InputTestPayload"") {
                        name
                        inputFields {
                            name
                            type {
                                kind
                                name
                            }
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query);

            var arg1FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "0", "type", "name");
            Assert.AreEqual("InputWithId", arg1FieldTypeName);

            var arg2FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "1", "type", "name");
            Assert.AreEqual("Int", arg2FieldTypeName);

            var arg3FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "2", "type", "name");
            Assert.AreEqual("Int", arg3FieldTypeName);
        }

        /// <summary>
        /// Test for https://git.epam.com/epm-ppa/epam-graphql/-/issues/103.
        /// </summary>
        [Test]
        public void Issue103FourArgs()
        {
            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(query =>
            {
                query.Field("test").Resolve(_ => 1);
            });
            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(mutation =>
            {
                mutation.Field("test")
                    .PayloadField<WithId>("arg1")
                    .PayloadField<int>("arg2")
                    .PayloadField<int>("arg3")
                    .PayloadField<int>("arg4")
                    .Resolve((ctx, arg1, arg2, arg3, arg4) => 1);
            });

            const string query = @"
                query {
                    __type(name: ""InputTestPayload"") {
                        name
                        inputFields {
                            name
                            type {
                                kind
                                name
                            }
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query);

            var arg1FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "0", "type", "name");
            Assert.AreEqual("InputWithId", arg1FieldTypeName);

            var arg2FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "1", "type", "name");
            Assert.AreEqual("Int", arg2FieldTypeName);

            var arg3FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "2", "type", "name");
            Assert.AreEqual("Int", arg3FieldTypeName);

            var arg4FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "3", "type", "name");
            Assert.AreEqual("Int", arg4FieldTypeName);
        }

        /// <summary>
        /// Test for https://git.epam.com/epm-ppa/epam-graphql/-/issues/103.
        /// </summary>
        [Test]
        public void Issue103FiveArgs()
        {
            var queryType = GraphQLTypeBuilder.CreateQueryType<TestUserContext>(query =>
            {
                query.Field("test").Resolve(_ => 1);
            });
            var mutationType = GraphQLTypeBuilder.CreateMutationType<TestUserContext>(mutation =>
            {
                mutation.Field("test")
                    .PayloadField<WithId>("arg1")
                    .PayloadField<int>("arg2")
                    .PayloadField<int>("arg3")
                    .PayloadField<int>("arg4")
                    .PayloadField<int>("arg5")
                    .Resolve((ctx, arg1, arg2, arg3, arg4, arg5) => 1);
            });

            const string query = @"
                query {
                    __type(name: ""InputTestPayload"") {
                        name
                        inputFields {
                            name
                            type {
                                kind
                                name
                            }
                        }
                    }
                }
            ";

            var actualResult = ExecuteHelpers.ExecuteQuery(queryType, mutationType, query);

            var arg1FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "0", "type", "name");
            Assert.AreEqual("InputWithId", arg1FieldTypeName);

            var arg2FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "1", "type", "name");
            Assert.AreEqual("Int", arg2FieldTypeName);

            var arg3FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "2", "type", "name");
            Assert.AreEqual("Int", arg3FieldTypeName);

            var arg4FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "3", "type", "name");
            Assert.AreEqual("Int", arg4FieldTypeName);

            var arg5FieldTypeName = GetFieldValue(actualResult.Data, "__type", "inputFields", "4", "type", "name");
            Assert.AreEqual("Int", arg5FieldTypeName);
        }

        private static object GetFieldValue(object data, params string[] fieldNames)
        {
            if (data is ExecutionNode executionNode)
            {
                data = executionNode.ToValue();
            }

            if (data is not IDictionary<string, object> asDictionary)
            {
                if (data is not IEnumerable<object> asCollection || !asCollection.Any())
                {
                    return null;
                }

                if (int.TryParse(fieldNames[0], out var skip))
                {
                    return asCollection
                        .Select(item => GetFieldValue(item, fieldNames.Skip(1).ToArray()))
                        .Skip(skip)
                        .FirstOrDefault(item => item != null);
                }

                return asCollection
                    .Select(item => GetFieldValue(item, fieldNames))
                    .FirstOrDefault(item => item != null);
            }

            var fieldValue = asDictionary[fieldNames[0]];
            if (fieldNames.Length == 1)
            {
                return fieldValue;
            }

            return GetFieldValue(fieldValue, fieldNames.Skip(1).ToArray());
        }

#pragma warning disable CA1822 // Mark members as static
        private string GetPersonName(Person person) => person.FullName;
#pragma warning restore CA1822 // Mark members as static

        public class Empty
        {
        }

        public class WithId
        {
            public int Id { get; set; }
        }

        public class WithIndexedProperty
        {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
            public int this[int index] => throw new NotImplementedException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
        }

        public class PersonFilter : Input
        {
            public int Id { get; set; }
        }
    }
}
