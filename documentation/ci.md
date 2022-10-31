[![CI](https://github.com/inceptedio/app/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/inceptedio/app/actions/workflows/main.yml)

# Authentication and authorization
We use [Github Actions](https://github.com/inceptedio/app/actions) for continuous integration. 

## Main pipeline
The [main pipeline](https://github.com/inceptedio/app/blob/main/.github/workflows/main.yml) is used for building testing and deploying all changes to the `main` branch. See the file for the steps. 

## Secrets
We use [Github Actions secrets](https://github.com/inceptedio/app/settings/secrets/actions) for managing the secrets of the deployment. There is a possibility to move to `azure keyvault` in the future.
We need the following secrets for this pipeline: 
* `AZURE_WEBAPP_PUBLISH_PROFILE_PROD` (download publish profile of the appservice defined in the main.yml -> `AZURE_WEBAPP_NAME`)
* `PULUMI_ACCESS_TOKEN` (created by the pulumi ci helper into my account, can always generate new one and replace it)