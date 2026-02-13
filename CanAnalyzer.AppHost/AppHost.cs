var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CanAnalyzer>("cananalyzer");

builder.Build().Run();
