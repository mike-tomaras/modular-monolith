using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Incepted.API.Tests.Integration.TestClasses;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]//https://github.com/serilog/serilog-aspnetcore/issues/289#issuecomment-1062384826
public class BaseIntegrationTest
{
    private WebApplicationFactory<Program> _application;    
    public HttpClient Client { get; private set; }
    public IServiceProvider Services { get; private set; }

    [SetUp]
    public void BaseSetup()
    {
        _application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        Services = _application.Services;

        var clientOptions = new WebApplicationFactoryClientOptions();
        clientOptions.AllowAutoRedirect = true;
        clientOptions.BaseAddress = new Uri("http://localhost");
        clientOptions.HandleCookies = true;
        clientOptions.MaxAutomaticRedirections = 7;

        Client = _application.CreateClient(clientOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    public async Task<string> GetTestFilePathAsync()
    {
        string localPath = "./testdata/";
        string fileName = "testfile.txt";
        string localFilePath = Path.Combine(localPath, fileName);

        // Write text to the file
        Directory.CreateDirectory(localPath);
        await File.WriteAllTextAsync(localFilePath, "Hello, World!");

        return localFilePath;
    }
    public void DeleteTestFile(string path)
    {
        File.Delete(path);
    }
}


