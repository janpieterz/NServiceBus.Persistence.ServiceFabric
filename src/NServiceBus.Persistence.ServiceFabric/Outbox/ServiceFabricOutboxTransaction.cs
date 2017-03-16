﻿namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Outbox;

    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        internal IReliableStateManager StateManager { get; set; }
        internal ITransaction Transaction { get; set; }

        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
            Transaction = stateManager.CreateTransaction();
        }

        public void Dispose()
        {
           Transaction.Dispose();
        }

        public Task Commit()
        {
            return Transaction.CommitAsync();
        }

        public Task Abort()
        {
            Transaction.Abort();

            return TaskEx.CompletedTask;
        }

    }
}