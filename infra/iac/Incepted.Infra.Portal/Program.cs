using Pulumi;

namespace Incepted.Infra;

class Program
{
    static Task<int> Main() => Deployment.RunAsync<InceptedPortalStack>();
}
