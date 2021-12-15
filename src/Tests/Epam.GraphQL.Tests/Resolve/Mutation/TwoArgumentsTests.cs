// Copyright © 2020 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epam.GraphQL.Builders.Projection;
using Epam.GraphQL.EntityFrameworkCore;
using Epam.GraphQL.Mutation;
using Epam.GraphQL.Tests.Helpers;
using Epam.GraphQL.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace Epam.GraphQL.Tests.Resolve.Mutation
{
    [TestFixtureSource(typeof(FixtureArgCollection))]
    public class TwoArgumentsTests : ResolveMutationTestBase
    {
        private readonly ArgumentType _argumentType;

        public TwoArgumentsTests(ArgumentType argumentType)
        {
            _argumentType = argumentType;
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, IEnumerable<Person>>")]
        public void TestReturnsEnumerable()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, IEnumerable<Person>>((ctx, arg1, arg2) =>
                new[]
                {
                    new Person(ctx.DataContext.GetQueryable<Person>().First())
                    {
                        Id = 0,
                        FullName = arg1 + " " + arg2,
                    },
                });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation);
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            people {{
                                id
                                payload {{
                                    id
                                    fullName
                                }}
                            }}
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            people: [{
                                id: 7,
                                payload: {
                                    id: 7,
                                    fullName: ""Test Test2""
                                }
                            }]
                        }
                    }");

            DataContext
                .Received()
                .AddRange(Arg.Is<IEnumerable<Person>>(p => p.First().Id == 7 && p.First().FullName == "Test Test2"));

            DataContext
                .Received(1)
                .SaveChangesAsync();

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, Task<IEnumerable<Person>>>")]
        public void TestReturnsEnumerableTask()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, Task<IEnumerable<Person>>>(async (ctx, arg1, arg2) =>
            {
                await Task.Yield();
                return new[]
                {
                    new Person(ctx.DataContext.GetQueryable<Person>().First())
                    {
                        Id = 0,
                        FullName = arg1 + " " + arg2,
                    },
                };
            });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation);
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            people {{
                                id
                                payload {{
                                    id
                                    fullName
                                }}
                            }}
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            people: [{
                                id: 7,
                                payload: {
                                    id: 7,
                                    fullName: ""Test Test2""
                                }
                            }]
                        }
                    }");

            DataContext
                .Received(1)
                .AddRange(Arg.Is<IEnumerable<Person>>(p => p.First().Id == 7 && p.First().FullName == "Test Test2"));

            DataContext
                .Received(1)
                .SaveChangesAsync();

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Hook/Field/Func<string, string, IEnumerable<Person>>")]
        public void TestReturnsEnumerableWithHook()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, IEnumerable<Person>>((ctx, arg1, arg2) =>
                ctx.DataContext.GetQueryable<Person>().Take(1));

            var personLoader = CreatePersonLoader();

            var afterSave = FuncSubstitute.For<TestUserContext, IEnumerable<object>, Task<IEnumerable<object>>>((ctx, items) =>
                Task.FromResult<IEnumerable<object>>(ctx.DataContext.GetQueryable<Person>().Skip(1).Take(1)));

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation);
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            people {{
                                id
                                payload {{
                                    id
                                }}
                            }}
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            people: [{
                                id: 1,
                                payload: {
                                    id: 1
                                }
                            },{
                                id: 2,
                                payload: {
                                    id: 2
                                }
                            }]
                        }
                    }",
                afterSave: afterSave);

            DataContext
                .Received(2)
                .SaveChangesAsync();

            afterSave.HasBeenCalledOnce();
            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, IEnumerable<Person>>")]
        public void TestReturnsEnumerableDoNotSaveChanges()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, IEnumerable<Person>>((ctx, arg1, arg2) =>
                new[]
                {
                    new Person(ctx.DataContext.GetQueryable<Person>().First())
                    {
                        Id = 7,
                        FullName = arg1 + " " + arg2,
                    },
                });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, options => options.DoNotAddNewEntitiesToDbContext().DoNotSaveChanges());
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            people {{
                                id
                                payload {{
                                    id
                                }}
                            }}
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            people: [{
                                id: 7,
                                payload: {
                                    id: 7
                                }
                            }]
                        }
                    }");

            DataContext
                .DidNotReceive()
                .AddRange(Arg.Any<IEnumerable<Person>>());

            DataContext
                .DidNotReceive()
                .SaveChangesAsync();

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, Task<IEnumerable<Person>>>")]
        public void TestReturnsEnumerableTaskDoNotSaveChanges()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, Task<IEnumerable<Person>>>(async (ctx, arg1, arg2) =>
            {
                await Task.Yield();
                return new[]
                {
                    new Person(ctx.DataContext.GetQueryable<Person>().First())
                    {
                        Id = 7,
                        FullName = arg1 + " " + arg2,
                    },
                };
            });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, options => options.DoNotAddNewEntitiesToDbContext().DoNotSaveChanges());
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            people {{
                                id
                                payload {{
                                    id
                                }}
                            }}
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            people: [{
                                id: 7,
                                payload: {
                                    id: 7
                                }
                            }]
                        }
                    }");

            DataContext
                .DidNotReceive()
                .AddRange(Arg.Any<IEnumerable<Person>>());

            DataContext
                .DidNotReceive()
                .SaveChangesAsync();

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Hook/Field/Func<string, string, IEnumerable<Person>>")]
        public void TestReturnsEnumerableWithHookDoNotSaveChanges()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, IEnumerable<Person>>((ctx, arg1, arg2) =>
                ctx.DataContext.GetQueryable<Person>().Take(1));

            var personLoader = CreatePersonLoader();

            var afterSave = FuncSubstitute.For<TestUserContext, IEnumerable<object>, Task<IEnumerable<object>>>((ctx, items) =>
                Task.FromResult<IEnumerable<object>>(ctx.DataContext.GetQueryable<Person>().Skip(1).Take(1)));

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, options => options.DoNotAddNewEntitiesToDbContext().DoNotSaveChanges());
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            people {{
                                id
                                payload {{
                                    id
                                }}
                            }}
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            people: [{
                                id: 1,
                                payload: {
                                    id: 1
                                }
                            },{
                                id: 2,
                                payload: {
                                    id: 2
                                }
                            }]
                        }
                    }",
                afterSave: afterSave);

            DataContext
                .DidNotReceive()
                .AddRange(Arg.Any<IEnumerable<Person>>());

            DataContext
                .Received(1)
                .SaveChangesAsync();

            afterSave.HasBeenCalledOnce();
            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, MutationResult<string>>")]
        public void TestReturnsMutationResult()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, MutationResult<string>>((ctx, arg1, arg2) =>
                MutationResult.Create(ctx.DataContext.GetQueryable<Person>().Take(1), arg1 + " " + arg2));

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation);
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            payload {{
                                people {{
                                    id
                                    payload {{
                                        id
                                    }}
                                }}
                            }}
                            data
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            payload: {
                                people: [{
                                    id: 1,
                                    payload: {
                                        id: 1
                                    }
                                }]
                            },
                            data: ""Test Test2""
                        }
                    }");

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, Task<MutationResult<string>>>")]
        public void TestReturnsMutationResultTask()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, Task<MutationResult<string>>>(async (ctx, arg1, arg2) =>
            {
                await Task.Yield();
                return MutationResult.Create(ctx.DataContext.GetQueryable<Person>().Take(1), arg1 + " " + arg2);
            });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation);
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            payload {{
                                people {{
                                    id
                                    payload {{
                                        id
                                    }}
                                }}
                            }}
                            data
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            payload: {
                                people: [{
                                    id: 1,
                                    payload: {
                                        id: 1
                                    }
                                }]
                            },
                            data: ""Test Test2""
                        }
                    }");

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, MutationResult<string>>")]
        public void TestReturnsMutationResultDoNotSaveChanges()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, MutationResult<string>>((ctx, arg1, arg2) =>
                MutationResult.Create(ctx.DataContext.GetQueryable<Person>().Take(1), arg1 + " " + arg2));

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, optionsBuilder => optionsBuilder.DoNotSaveChanges());
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            payload {{
                                people {{
                                    id
                                    payload {{
                                        id
                                    }}
                                }}
                            }}
                            data
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            payload: {
                                people: [{
                                    id: 1,
                                    payload: {
                                        id: 1
                                    }
                                }]
                            },
                            data: ""Test Test2""
                        }
                    }");

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");

            DataContext
                .DidNotReceive()
                .SaveChangesAsync();

            DataContext
                .DidNotReceive()
                .AddRange(Arg.Any<IEnumerable<Person>>());
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, Task<MutationResult<string>>>")]
        public void TestReturnsMutationResultTaskDoNotSaveChanges()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, Task<MutationResult<string>>>(async (ctx, arg1, arg2) =>
            {
                await Task.Yield();
                return MutationResult.Create(ctx.DataContext.GetQueryable<Person>().Take(1), arg1 + " " + arg2);
            });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, optionsBuilder => optionsBuilder.DoNotSaveChanges());
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            payload {{
                                people {{
                                    id
                                    payload {{
                                        id
                                    }}
                                }}
                            }}
                            data
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            payload: {
                                people: [{
                                    id: 1,
                                    payload: {
                                        id: 1
                                    }
                                }]
                            },
                            data: ""Test Test2""
                        }
                    }");

            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test", arg3: s => s == "Test2");

            DataContext
                .DidNotReceive()
                .SaveChangesAsync();

            DataContext
                .DidNotReceive()
                .AddRange(Arg.Any<IEnumerable<Person>>());
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, CustomObject<string>>/Config")]
        public void TestTypeParamReturnsConfigurableResult()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, CustomObject<string>>((ctx, arg1, arg2) =>
                CustomObject.Create(arg1 + " " + arg2));

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, build => build.Field("myField", i => i.TestField));
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            myField
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            myField: ""Test Test2""
                        }
                    }");

            manualMutation.HasBeenCalledOnce();
        }

        [Test(Description = "Manual mutation/Field/Func<string, string, Task<CustomObject<string>>>/Config")]
        public void TestTypeParamReturnsConfigurableResultTask()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, Task<CustomObject<string>>>(async (ctx, arg1, arg2) =>
            {
                await Task.Yield();
                return CustomObject.Create(arg1 + " " + arg2);
            });

            var personLoader = CreatePersonLoader();

            TestMutation(
                queryBuilder: query => query
                    .Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation, build => build.Field("myField", i => i.TestField));
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test", "Test2")}) {{
                            myField
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            myField: ""Test Test2""
                        }
                    }");

            manualMutation.HasBeenCalledOnce();
        }

        [Test]
        public void TestHookReturnsEnumerable()
        {
            var manualMutation = FuncSubstitute.For<TestUserContext, string, string, MutationResult<string>>((ctx, arg1, arg2) =>
                MutationResult.Create(ctx.DataContext.GetQueryable<Person>().Take(1), arg1 + " " + arg2));

            var personLoader = CreatePersonLoader();

            var afterSave = FuncSubstitute.For<TestUserContext, IEnumerable<object>, Task<IEnumerable<object>>>((ctx, items) =>
                Task.FromResult<IEnumerable<object>>(ctx.DataContext.GetQueryable<Person>().Skip(1).Take(1)));

            TestMutation(
                queryBuilder: query => query.Connection(personLoader, "people"),
                mutationBuilder: mutation =>
                {
                    mutation.SubmitField(personLoader, "people");
                    CreateArgumentBuilder<string, string>(mutation)
                        .Resolve(manualMutation);
                },
                query: $@"
                    mutation {{
                        manualMutation({BuildArguments("Test1", "Test2")}) {{
                            payload {{
                                people {{
                                    id
                                    payload {{
                                        id
                                    }}
                                }}
                            }}
                            data
                        }}
                    }}",
                expected: @"
                    {
                        manualMutation: {
                            payload: {
                                people: [{
                                    id: 1,
                                    payload: {
                                        id: 1
                                    }
                                },{
                                    id: 2,
                                    payload: {
                                        id: 2
                                    }
                                }]
                            },
                            data: ""Test1 Test2""
                        }
                    }",
                afterSave: afterSave);

            DataContext
                .Received()
                .SaveChangesAsync();

            afterSave.HasBeenCalledOnce();
            manualMutation.HasBeenCalledOnce(arg2: s => s == "Test1", arg3: s => s == "Test2");
        }

        private string BuildArguments(string arg1, string arg2)
        {
            return _argumentType switch
            {
                ArgumentType.Argument => $"arg1: \"{arg1}\", arg2: \"{arg2}\"",
                ArgumentType.PayloadField => $"payload: {{ arg1: \"{arg1}\", arg2: \"{arg2}\" }}",
                _ => throw new NotSupportedException(),
            };
        }

        private IMutationFieldBuilder<IMutationFieldBuilderBase<TArg1, TArg2, TestUserContext>, TArg1, TArg2, TestUserContext> CreateArgumentBuilder<TArg1, TArg2>(Mutation<TestUserContext> mutation)
        {
            return _argumentType switch
            {
                ArgumentType.Argument => mutation.Field("manualMutation")
                    .Argument<TArg1>("arg1")
                    .Argument<TArg2>("arg2"),
                ArgumentType.PayloadField => mutation.Field("manualMutation")
                    .PayloadField<TArg1>("arg1")
                    .PayloadField<TArg2>("arg2"),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
