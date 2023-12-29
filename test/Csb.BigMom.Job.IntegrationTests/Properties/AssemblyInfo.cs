// Disable tests parallelization because it hangs on Gitlab CI.

using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]