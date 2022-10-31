# Authentication and authorization
We use [Auth0](https://manage.auth0.com/dashboard/eu/incepted-dev/applications) for client and API authetication and authorization. 

## Setup
* There is an Auth0 tenant per environment (dev, staging, prod) named `incepted-{env}`.
* Each tentant has one Auth0 Application for the client and one Auth0 API for the back end.
    * There is only one API project with a controller sub folder per "service". The bounded context services are `csproj`s with a programmatic API exposed in HTTP via the one HTTP API `Incepted.API`.
* [Tutorial on how to autheticate client and API with Auth0](https://auth0.com/blog/securing-blazor-webassembly-apps/)
* No custom domain or SMTP provider for now. The _Universal Login_ is branded with: 
 * Logo: https://inpduksassets.blob.core.windows.net/logos/logo-icon.png
 * Primary Color: #0093DD
 * Page Background Color: #FFFFFF
* No socials or passwordless configured for now. 

## Permissions and Roles structure
There are three main roles:
* Broker
* Insurer
* Admin

These roles group the [permissions needed](https://manage.auth0.com/dashboard/eu/incepted-dev/apis/62484d74d41fe60047fc4418/permissions) (this is the dev env but the same permissions are to be replicated in staging and prod). 

No permissions are to be ever set directly on Users, always use Roles and group permissions in those. Because the API authorizes by role, if any permissions are set directly on the user they will not be ckecked for creating a security flaw. 

## Adding the roles to the access token
Create an [Auth0 Custom Action](https://manage.auth0.com/dashboard/eu/incepted-dev/actions/library) and add it to the [Log In flow](https://manage.auth0.com/dashboard/eu/incepted-dev/actions/flows). Add this snippet to the custom action:
```javascript
/**
* Handler that will be called during the execution of a PostLogin flow.
/**
 * @param {Event} event - Details about the user and the context in which they are logging in.
 * @param {PostLoginAPI} api - Interface whose methods can be used to change the behavior of the login.
 */
exports.onExecutePostLogin = async (event, api) => {
  const namespace = 'https://incepted.co.uk';
  if (event.authorization) {
    //add roles
    api.idToken.setCustomClaim(`${namespace}/roles`, event.authorization.roles);
    api.accessToken.setCustomClaim(`${namespace}/roles`, event.authorization.roles);
    //add user_metadata
    api.idToken.setCustomClaim(`${namespace}/user_metadata`, event.user.user_metadata);
    api.accessToken.setCustomClaim(`${namespace}/user_metadata`, event.user.user_metadata);
  }
}
```