namespace Waec.IntegrationTests;

// All tests share the same Docker Compose environment and in-memory service stores.
// Running them serially prevents one test from changing data while another is asserting it.
[CollectionDefinition("WAEC microservices integration tests", DisableParallelization = true)]
public sealed class IntegrationTestCollection
{
    public const string Name = "WAEC microservices integration tests";
}
