﻿namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_multiple_workers_retrieve_same_saga : SagaPersisterTests
    {
        [Test]
        public async Task Persister_returns_different_instance_of_saga_data()
        {
            var saga = new TestSagaData { Id = Guid.NewGuid() };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            TestSagaData returnedSaga1;
            var readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstance(readContextBag, new TestSaga(), new TestSagaData());

                returnedSaga1 = await persister.Get<TestSagaData>(saga.Id, readSession, readContextBag);

                await readSession.CompleteAsync();
            }

            TestSagaData returnedSaga2;
            readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstance(readContextBag, new TestSaga(), new TestSagaData());

                returnedSaga2 = await persister.Get<TestSagaData>("Id", saga.Id, readSession, readContextBag);

                await readSession.CompleteAsync();
            }

            Assert.AreNotSame(returnedSaga2, returnedSaga1);
            Assert.AreNotSame(returnedSaga1, saga);
            Assert.AreNotSame(returnedSaga2, saga);
        }

        [Test]
        public async Task Save_fails_when_data_changes_between_read_and_update()
        {
            var sagaId = Guid.NewGuid();
            var saga = new TestSagaData { Id = sagaId, SomeId = sagaId.ToString() };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
            var record = await persister.Get<TestSagaData>(saga.Id, winningSaveSession, winningContext);

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            var staleRecord = await persister.Get<TestSagaData>("SomeId", sagaId.ToString(), losingSaveSession, losingContext);

            record.DateTimeProperty = DateTime.Now;
            await persister.Update(record, winningSaveSession, winningContext);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            await persister.Update(staleRecord, losingSaveSession, losingContext);
            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
        }

        [Test]
        public async Task Save_fails_when_data_changes_between_read_and_update_on_same_thread()
        {
            var sagaId = Guid.NewGuid();
            var saga = new TestSagaData { Id = sagaId, SomeId = sagaId.ToString() };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
            var record = await persister.Get<TestSagaData>(saga.Id, winningSaveSession, winningContext);

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            var staleRecord = await persister.Get<TestSagaData>("SomeId", sagaId.ToString(), losingSaveSession, losingContext);

            record.DateTimeProperty = DateTime.Now;
            await persister.Update(record, winningSaveSession, winningContext);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            await persister.Update(staleRecord, losingSaveSession, losingContext);
            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
        }

        [Test]
        public async Task Save_fails_when_writing_same_data_twice()
        {
            var sagaId = Guid.NewGuid();
            var saga = new TestSagaData { Id = sagaId , SomeId = sagaId.ToString()};

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            TestSagaData returnedSaga1;
            var readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstance(readContextBag, new TestSaga(), new TestSagaData());

                returnedSaga1 = await persister.Get<TestSagaData>(saga.Id, readSession, readContextBag);

                await readSession.CompleteAsync();
            }

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);

            returnedSaga1.DateTimeProperty = DateTime.Now;
            await persister.Update(returnedSaga1, winningSaveSession, readContextBag);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            await persister.Update(returnedSaga1, losingSaveSession, readContextBag);

            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
        }

        [Test]
        public async Task Save_process_is_repeatable()
        {
            var sagaId = Guid.NewGuid();
            var saga = new TestSagaData { Id = sagaId, SomeId = sagaId.ToString() };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            var winningContext1 = configuration.GetContextBagForSagaStorage();
            var winningSaveSession1 = await configuration.SynchronizedStorage.OpenSession(winningContext1);
            var record1 = await persister.Get<TestSagaData>(saga.Id, winningSaveSession1, winningContext1);

            var losingContext1 = configuration.GetContextBagForSagaStorage();
            var losingSaveSession1 = await configuration.SynchronizedStorage.OpenSession(losingContext1);
            var staleRecord1 = await persister.Get<TestSagaData>("SomeId", sagaId.ToString(), losingSaveSession1, losingContext1);

            record1.DateTimeProperty = DateTime.Now;
            await persister.Update(record1, winningSaveSession1, winningContext1);
            await winningSaveSession1.CompleteAsync();
            winningSaveSession1.Dispose();

            await persister.Update(staleRecord1, losingSaveSession1, losingContext1);
            Assert.That(async () => await losingSaveSession1.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
            losingSaveSession1.Dispose();

            var winningContext2 = configuration.GetContextBagForSagaStorage();
            var winningSaveSession2 = await configuration.SynchronizedStorage.OpenSession(winningContext2);
            var record2 = await persister.Get<TestSagaData>(saga.Id, winningSaveSession2, winningContext2);

            var losingContext2 = configuration.GetContextBagForSagaStorage();
            var losingSaveSession2 = await configuration.SynchronizedStorage.OpenSession(losingContext2);
            var staleRecord2 = await persister.Get<TestSagaData>("SomeId", sagaId.ToString(), losingSaveSession2, losingContext2);

            record2.DateTimeProperty = DateTime.Now;
            await persister.Update(record2, winningSaveSession2, winningContext2);
            await winningSaveSession2.CompleteAsync();
            winningSaveSession2.Dispose();

            await persister.Update(staleRecord2, losingSaveSession2, losingContext2);
            Assert.That(async () => await losingSaveSession2.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
            losingSaveSession2.Dispose();
        }
    }
}