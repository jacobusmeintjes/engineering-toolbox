using EnterpriseTestFramework.LoadTests;

// Separate console entry point rather than an xUnit [Fact] — load runs are long-lived,
// resource-intensive, and belong on their own CI cadence (see docs/adr/0011-nbomber-load-testing.md).
// Exit code propagates threshold failures to the nightly pipeline.
return UserApiLoadTests.Run(args);
