using Pulumi;
using System.Text;

namespace Incepted.Infra.Shared;

public class NameGenerator
{
    private static string _prefix = "inpd";

    public static string GetName(string name, string location)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(_prefix);
        stringBuilder.Append("-");
        stringBuilder.Append(location);
        stringBuilder.Append("-");
        
        stringBuilder.Append(name);

        return stringBuilder.ToString();
    }
    public static string GetEnvName(string name, string location)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(_prefix);
        stringBuilder.Append("-");
        stringBuilder.Append(location);
        stringBuilder.Append("-");
        stringBuilder.Append(Deployment.Instance.StackName);
        stringBuilder.Append("-");
        stringBuilder.Append(name);

        return stringBuilder.ToString();
    }
}

public enum Environment
{
    staging,
    prod,
    nonSpecific
}
