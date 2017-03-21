﻿namespace NServiceBus.Persistence.ServiceFabric.AcceptanceTests.SagaDataStorage
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    [TestFixture]
    public class When_storing_saga_with_default_collection_mapping : NServiceBusAcceptanceTest, INeed<IReliableStateManager>
    {
        IReliableStateManager stateManager;

        [Test]
        public async Task Should_use_fullname_as_collection_name()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SagaEndpoint>(b => b.When(session => session.SendLocal(new StartSaga { SomeId = Guid.NewGuid() })))
                .Done(c => c.SagaId.HasValue)
                .Run(TimeSpan.FromSeconds(20));

            var collection = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(typeof(SagaEndpoint.ConventionBasedSaga.ConventionBasedSagaData).Name);

            using (var tx = stateManager.CreateTransaction())
            {
                var conditionalValue = await collection.TryGetValueAsync(tx, context.SagaId.Value);

                Assert.True(conditionalValue.HasValue);
            }
        }

        public void Need(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public class Context : ScenarioContext
        {
            public Guid? SagaId { get; set; }
        }

        public class SagaEndpoint : EndpointConfigurationBuilder
        {
            public SagaEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class ConventionBasedSaga : Saga<ConventionBasedSaga.ConventionBasedSagaData>, IAmStartedByMessages<StartSaga>
            {
                public Context TestContext { get; set; }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ConventionBasedSagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSaga>(m => m.SomeId).ToSaga(d => d.Id);
                }

                public Task Handle(StartSaga message, IMessageHandlerContext context)
                {
                    TestContext.SagaId = Data.Id;

                    return Task.FromResult(0);
                }

                public class ConventionBasedSagaData : ContainSagaData
                {
                }
            }
        }

        public class StartSaga : IMessage
        {
            public Guid SomeId { get; set; }
        }
    }
}