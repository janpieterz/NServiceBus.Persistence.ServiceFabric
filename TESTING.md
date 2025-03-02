## How to run the tests

* Install the Service Fabric SDK (https://azure.microsoft.com/en-us/downloads/)
* Ensure you have an environment variable `AzureServiceBusTransport.ConnectionString` containing a valid Azure ServiceBus connection string 
* Start the "Service Fabric Local Cluster Manager"
* via SF Cluster Manager tray icon, select "Start a local Cluster"
* In the "NServiceBus.Persistence.ServiceFabric" solution, go to the "Tests" project and select open the class representing the test suite you want to run (e.g. in case of the acceptance tests, open the `AcceptanceTests.cs` file
* Run the test in the opened file. This may take a while to connect to the SF cluster but once running, all executed tests will be shown in the test runner.


## How the test execution works

With Service Fabric code that needs to access infrastructure like the reliable state manager has to be run inside the cluster. This applies to integration tests as well. A classical integration test (here as an example against SQL Server) might look like this

```
[TestFixture]
public class IntegrationTest 
{
    SqlConnection connection;

    [SetUp]
    public async Task SetUp() 
    {
        connection = //.. somehow acquire connection
        // delete test database if existant
        // potentially deploy test database or spawn transaction
    }

    [TearDown]
    public async Task TearDown() 
    {
        // close connection
        // rollback transaction or delete database
    }

    [Test]
    public async Task SomeTest() 
    {
        // do test and asserts
    }
}
```

The above code assumes that there is either a remote or a local Sql Server it can connect to. There are no specific requirements other than that to be able to execute the test. Now let's look at an integration test with Service Fabric (here as an example against reliable collections)

```
[TestFixture]
public class IntegrationTest 
{
    IReliableStateManager stateManager;

    [SetUp]
    public async Task SetUp() 
    {
        stateManager = //.. somehow acquire state manager
        // spawn transaction
    }

    [TearDown]
    public async Task TearDown() 
    {
        // rollback transaction
    }

    [Test]
    public async Task SomeTest() 
    {
        var state = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state", TimeSpan.FromSeconds(30)).ConfigureAwait(false);
        Assert.AreEqual("urn:state", state.Name.ToString());
    }
}
```

In order to be able to access the reliable state manager in the above test we have to deploy and run the Integration Test above into the cluster as an application with a stateful service. There are a few unique challenges:

- An application is needed to package and deploy the service containing the tests
- The application needs to be deployed and executed every time the tests are executed either on the build server or locally
- The build server or the local test runner needs to be able to report the current state of the tests
- Components such as the reliable state manager need to be "injected" into the tests

At first glance it might seems simple to just run the build server agent inside the Service Fabric cluster. Unfortunately this has a number of drawbacks:

- Number of build agents that need to be licensed
- Build agents might make assumptions about local disk access, permissions etc. that might not be true inside the cluster
- Build agents need to report information back to the build server which requires infrastructure or configuration
- An integration tests has no clear way of interacting with the agent in order to get automagically the reliable state manager and other infrastructure provided by Service Fabric

### Solution

NServiceBus.Persistence.TestRunner project contains

- `AbstractTestRunner` which acts as a stateful test runner service
- `CommunicationListener` which inspects the service library for tests, exposes them and has the ability to run individual tests
- `INeed` interface to allow dependency injection into integration tests
- `StatefulServiceProviderListener` which injects the stateful service into the `TestContext`

Service Fabric remoting is used as a communication channel and therefore the tests need to be run on the same machine that contains the Service Fabric cluster.

Tests project contains

- A base class called R which acts as a remoting client to the TestRunner service


### Usage

Let's assume a new test library called NServiceBus.Persistence.ServiceFabric.ComponentTests needs to be created. These are the steps required:

- Create a service fabric application called `NServiceBus.Persistence.ServiceFabric.ComponentTestsApplication`
- Create a new stateful service library called `NServiceBus.Persistence.ServiceFabric.ComponentTests`
- Reference `NServiceBus.Persistence.TestRunner`
- Add `NUnitLite` as a nuget package dependency
- Implement Test Service in the following way

```
    sealed class Tests : AbstractTestRunner<Tests>
    {
        public Tests(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override Tests Self => this;
    }
```
- Application manifest should contain

```
    <Service Name="Tests">
      <StatefulService ServiceTypeName="TestsType" TargetReplicaSetSize="[Tests_TargetReplicaSetSize]" MinReplicaSetSize="[Tests_MinReplicaSetSize]">
        <SingletonPartition />
      </StatefulService>
    </Service>
```
- Edit `NServiceBus.Persistence.ServiceFabric.ComponentTestApplication.sfproj` to contain
```
  <Target Name="ReleaseAfterBuild" AfterTargets="AfterBuild">
    <MSBuild Projects="$(ProjectPath)" Targets="Package" />
  </Target>
```

- Add to the Tests project the following test

```
    public class ComponentTests : R<Tests>
    {
    }
```

- Run tests by executing the `ComponentTest.cs`

#### Environment variables

It is possible to promote environment variables automatically. The ServiceManifest needs to declare the environment variable.
```
...
  <CodePackage Name="Code" Version="1.0.0">
...
    <EnvironmentVariables>
      <EnvironmentVariable Name="MyEnvironmentVariable" Value=""/>
    </EnvironmentVariables>
  </CodePackage>
```

In the application manifest a parameter for the environment variable needs to be defined (Convention: `Tests_{VariableName}`)

```
<ApplicationManifest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ApplicationTypeName="AcceptanceTestsType" ApplicationTypeVersion="1.0.0" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
...
    <Parameter Name="Tests_MyEnvironmentVariable" DefaultValue="" />
  </Parameters>
  ...
</ApplicationManifest>  
```

In the test define the name of the actual environment variable.

```
    public class ComponentTests : R<Tests>
    {
        public static string[] EnvironmentVariables { get; set; } = { "MyEnvironmentVariable" };
    }
```

The deployment test will automatically read the value and promote when deploying. 
