var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(false);

var ui = builder
    .AddJavaScriptApp("bff-angular", "../ui", runScriptName: "start")
    .WithNpm(installCommand: "install", installArgs: ["--force"])
    .WithHttpsEndpoint(env: "PORT")
    .WithAnnotation(new ContainerFilesSourceAnnotation() { SourcePath = "/app/dist/ui/browser" })
    .PublishAsDockerFile();

var server = builder.AddProject<Projects.BffMicrosoftEntraID_Server>("bff-server")
    .WithExternalHttpEndpoints()
    .PublishWithContainerFiles(ui, "./wwwroot")
    .PublishAsDockerComposeService((_, _) => { });

// In publish mode the Angular app is embedded in the server Docker image.
// The dev-time frontend process is only needed when running locally.
if (!builder.ExecutionContext.IsPublishMode)
{
    server
      .WithReference(ui)
      .WaitFor(ui)
      .WithChildRelationship(ui);
}

builder.Build().Run();
