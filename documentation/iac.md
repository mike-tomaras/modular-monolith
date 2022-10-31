# Infrastructure as code
We use Pulumi and we have a [Pulumi org](https://app.pulumi.com/incepted). We use [Pulumi Actions](https://github.com/inceptedio/app/actions) for continuous integration. We use one [Pulumi stack] per env

## Initial setup
* [Create an app registration/service principal](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal?view=azure-cli-latest#register-an-application-with-azure-ad-and-create-a-service-principal) in your Azure AD tenant (no web url required) that will be used
* [Add the following roles to the app registration/service principal](app registration/service principal):
    * "Storage Account Contributor"
    * "Web Plan Contributor"
    * "Website Contributor"
    * "DocumentDB Account Contributor"
    * "Key Vault Contributor"
    * A custom role with the following permissions from Microsoft.Resources/subscriptions/resourceGroups
        * Read : Get Resource Group 
        * Write : Create Resource Group 
        * Delete : Delete Resource Group 
    * More TBA as needed, go for least privilege.
* Add the following pulumi configuration in each stack for _per service principal_:
```bash
pulumi config set azure-native:clientId <clientID>
pulumi config set azure-native:clientSecret <clientSecret> --secret
pulumi config set azure-native:tenantId <tenantID> 
pulumi config set azure-native:subscriptionId <subscriptionId>
``` 
* Search for `var config = new Config();` and `config.Require(...)` or `config.RequireSecret(...)` in the project and make sure all those values exist in all the `Pulumi.{env}.yaml`s. If they are not there select the correct "stack" with `pulumi stack select <stack>` and set them with `pulumi config set <name> <value>`.
* Search for the comment `//MANUAL: ...` and do those actions before or after the IaC as needed.

## Pipelines
The [main pipeline](https://github.com/inceptedio/app/blob/main/.github/workflows/main.yml) contains steps to run infra as code for each environment. 
Infra related to security is run via the separate [secops pipeline](https://github.com/inceptedio/app/blob/main/.github/workflows/secops.yml). This was done to decouple the secrets from the main deployment and also because it was slowing down the main pipeline a lot. 

## Devving
You need to login to azure locally if you are to try out pulumi from your dev machine. Do that using `az login` and pulumi will use your authenticated session. 
We've added the infa project to the sln so you can just go in and start coding! See the [docs](https://www.pulumi.com/registry/packages/azure-native/api-docs/web/).

## Notes
* We use one subscription to keep billing in one place.
* We use one appservice per app with multiple slots (one slot per environment).
* There is no pulumi API for AppService Deployment Slot Settings, you'll have to set those manually after the resource is created. See `//MANUAL` comments code for which ones need this.
* In order for pulumi to delete the secrets you have to manually add an extra kv access policy for the pulumi app registration with the delete permission, even though there might already be an access policy for the same app registration. 