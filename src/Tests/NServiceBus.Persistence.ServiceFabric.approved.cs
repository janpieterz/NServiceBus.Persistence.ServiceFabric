﻿[assembly: System.CLSCompliantAttribute(true)]
[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute(@"AcceptanceTests, PublicKey=00240000048000009400000006020000002400005253413100040000010001007f16e21368ff041183fab592d9e8ed37e7be355e93323147a1d29983d6e591b04282e4da0c9e18bd901e112c0033925eb7d7872c2f1706655891c5c9d57297994f707d16ee9a8f40d978f064ee1ffc73c0db3f4712691b23bf596f75130f4ec978cf78757ec034625a5f27e6bb50c618931ea49f6f628fd74271c32959efb1c5")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute(@"Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001007f16e21368ff041183fab592d9e8ed37e7be355e93323147a1d29983d6e591b04282e4da0c9e18bd901e112c0033925eb7d7872c2f1706655891c5c9d57297994f707d16ee9a8f40d978f064ee1ffc73c0db3f4712691b23bf596f75130f4ec978cf78757ec034625a5f27e6bb50c618931ea49f6f628fd74271c32959efb1c5")]
[assembly: System.Runtime.InteropServices.ComVisibleAttribute(false)]
[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.5.2", FrameworkDisplayName=".NET Framework 4.5.2")]

namespace NServiceBus.Persistence.ServiceFabric
{
    
    public interface IServiceFabricStorageSession
    {
        Microsoft.ServiceFabric.Data.IReliableStateManager StateManager { get; }
        Microsoft.ServiceFabric.Data.ITransaction Transaction { get; }
    }
    [System.Runtime.Serialization.DataContractAttribute(Name="SagaEntry", Namespace="NServiceBus.Persistence.ServiceFabric")]
    public sealed class SagaEntry : System.Runtime.Serialization.IExtensibleDataObject
    {
        public SagaEntry(string data, System.Version sagaTypeVersion, System.Version persistenceVersion) { }
        [System.Runtime.Serialization.DataMemberAttribute(Name="Data", Order=0)]
        public string Data { get; }
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="PersistenceVersion", Order=1)]
        public System.Version PersistenceVersion { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="SagaTypeVersion", Order=2)]
        public System.Version SagaTypeVersion { get; set; }
    }
    public class SagaSettings
    {
        public void JsonSettings(Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings) { }
        public void ReaderCreator(System.Func<System.IO.TextReader, Newtonsoft.Json.JsonReader> readerCreator) { }
        public void WriterCreator(System.Func<System.Text.StringBuilder, Newtonsoft.Json.JsonWriter> writerCreator) { }
    }
    public class ServiceFabricPersistence : NServiceBus.Persistence.PersistenceDefinition { }
    public class static ServiceFabricPersistenceStorageSessionExtensions
    {
        public static NServiceBus.Persistence.ServiceFabric.IServiceFabricStorageSession ServiceFabricSession(this NServiceBus.Persistence.SynchronizedStorageSession session) { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Class | System.AttributeTargets.All, Inherited=false)]
    public sealed class ServiceFabricSagaAttribute : System.Attribute
    {
        public string CollectionName;
        public string SagaDataName;
        public ServiceFabricSagaAttribute() { }
    }
    [System.Runtime.Serialization.DataContractAttribute(Name="StoredOutboxMessage", Namespace="NServiceBus.Persistence.ServiceFabric")]
    public sealed class StoredOutboxMessage : System.Runtime.Serialization.IExtensibleDataObject
    {
        public StoredOutboxMessage(string messageId, NServiceBus.Persistence.ServiceFabric.StoredTransportOperation[] transportOperations) { }
        [System.Runtime.Serialization.DataMemberAttribute(Name="Dispatched", Order=1)]
        public bool Dispatched { get; }
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="Id", Order=0)]
        public string Id { get; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="StoredAt", Order=2)]
        public System.DateTimeOffset StoredAt { get; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="TransportOperations", Order=3)]
        public NServiceBus.Persistence.ServiceFabric.StoredTransportOperation[] TransportOperations { get; }
        public NServiceBus.Persistence.ServiceFabric.StoredOutboxMessage CloneAndMarkAsDispatched() { }
        public override bool Equals(object obj) { }
        public override int GetHashCode() { }
    }
    [System.Runtime.Serialization.DataContractAttribute(Name="StoredOutboxMessage", Namespace="NServiceBus.Persistence.ServiceFabric")]
    public sealed class StoredTransportOperation : System.Runtime.Serialization.IExtensibleDataObject
    {
        public StoredTransportOperation(string messageId, System.Collections.Generic.Dictionary<string, string> options, byte[] body, System.Collections.Generic.Dictionary<string, string> headers) { }
        [System.Runtime.Serialization.DataMemberAttribute(Name="Body", Order=2)]
        public byte[] Body { get; }
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="Headers", Order=3)]
        public System.Collections.Generic.Dictionary<string, string> Headers { get; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="MessageId", Order=0)]
        public string MessageId { get; }
        [System.Runtime.Serialization.DataMemberAttribute(Name="Options", Order=1)]
        public System.Collections.Generic.Dictionary<string, string> Options { get; }
    }
}
namespace NServiceBus
{
    
    public class static ServiceFabricOutboxSettingsExtensions
    {
        public static void SetFrequencyToRunDeduplicationDataCleanup(this NServiceBus.Outbox.OutboxSettings configuration, System.TimeSpan frequencyToRunDeduplicationDataCleanup) { }
        public static void SetTimeToKeepDeduplicationData(this NServiceBus.Outbox.OutboxSettings configuration, System.TimeSpan timeToKeepDeduplicationData) { }
    }
    public class static ServiceFabricPersistenceConfig
    {
        public static NServiceBus.Persistence.ServiceFabric.SagaSettings SagaSettings(this NServiceBus.PersistenceExtensions<NServiceBus.Persistence.ServiceFabric.ServiceFabricPersistence> configuration) { }
        public static void StateManager(this NServiceBus.PersistenceExtensions<NServiceBus.Persistence.ServiceFabric.ServiceFabricPersistence> configuration, Microsoft.ServiceFabric.Data.IReliableStateManager stateManager) { }
    }
}