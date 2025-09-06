using Xunit;

// Disable test parallelization to avoid port binding races in HostCommandTests
[assembly: CollectionBehavior(DisableTestParallelization = true)]
