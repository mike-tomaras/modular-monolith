# Secrets

## App secrets
All are managed by az keyvault and referenced securely by the app service configuration. They are deployed via the [secops pipeline](https://github.com/inceptedio/app/blob/main/.github/workflows/secops.yml).

## Secrets in Github actions
We use [Github Actions secrets](https://github.com/inceptedio/app/settings/secrets/actions) for managing the secrets of the deployment. We need the following secrets for this pipeline: 
* `AZURE_WEBAPP_PUBLISH_PROFILE_PROD` (download publish profile of the appservice defined in the main.yml -> `AZURE_WEBAPP_NAME`)
* `PULUMI_ACCESS_TOKEN` (created by the pulumi ci helper into my account, can always generate new one and replace it)

## Secrets in Pulumi
There are some secrets saved in the [Pulumi service](https://app.pulumi.com/incepted/portal/prod) ("Stack" tab -> "Confguration" panel) that pulumi uses to deploy to each stack (Azure AD Pulumi App registration stuff).
Pulumi also holds the system secrets that are saved in Az Keyvaults and used by the appservices (az kv references in the apservice site config).