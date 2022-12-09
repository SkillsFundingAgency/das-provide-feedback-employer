# Employer Feedback

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-provide-feedback-employer?repoName=SkillsFundingAgency%2Fdas-provide-feedback-employer&branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2539&repoName=SkillsFundingAgency%2Fdas-provide-feedback-employer&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-provide-feedback-employer&metric=alert_status)](https://sonarcloud.io/project/overview?id=SkillsFundingAgency_das-provide-feedback-employer)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/browse/QF-79)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3773497345/Employer+Feedback+-+QF)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)                                                                                                                                                        | ![Build badge](https://sfa-gov-uk.visualstudio.com/_apis/public/build/definitions/c39e0c0b-7aff-4606-b160-3566f3bbce23/1090/badge) |

This repository represents the code base for the employer feedback service. This is a service that allows employers to provide feedback on their training providers. Employers can give feedback via the ad hoc journey from their employer account. The employer has previously been able to provide feedback via emails, as they would recieve emails prompting them to give feedback. However, this emailing function is in the process of being decommissioned. 

## Developer Setup
### Requirements

* [.NET Core SDK >= 2.1.302](https://www.microsoft.com/net/download/)
* [Docker for X](https://docs.docker.com/install/#supported-platforms) (not required for emailer functions)
* [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) (not required for emailer functions)
* (VS Code Only) [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
* [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

### Environment Setup

* **Database** - Publish the local databse from the `ESFA.DAS.EmployerFeedbackEmail.Database` project.
    * Setup your database - You will need your `SFA.DAS.Commitments.Database` database setup with data before following these steps:
        * 
* **Docker** - The default development environment uses docker containers to host the following dependencies.
    * Redis
    * Elasticsearch
    * Logstash

    On first setup run the following command from _**/setup/containers/**_ to create the docker container images:
    `docker-compose build`

    To start the containers run:
    `docker-compose up -d`

    You can view the state of the running containers using:
    `docker ps -a`

##### Add local.settings.json to ESFA.DAS.ProviderFeedback.Employer.Functions.Emailer

Please note all the connection string and secrets to API have been removed. This will need updating.

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "ASPNETCORE_ENVIRONMENT": "DEV",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "FUNCTIONS_EXTENSION_VERSION": "2.0.12673.0",
    "AppInsights_InstrumentationKey": "",
    "FeedbackSiteBaseUrl": "localhost:5030",
    "EmailBatchSize": "500",
    "NotificationApiBaseUrl": "https://at-notifications.apprenticeships.sfa.bis.gov.uk",
    "ClientToken": "",
    "InviteEmailerSchedule": "0 */10 10-11 * * MON-FRI",
    "ReminderEmailerSchedule": "0 3/10 10-11 * * MON-FRI",
    "DataRefreshSchedule": "0 0 3 * * MON-FRI",
    "AppName": "das-provide-feedback-emailer",
    "LogDir": "C:\\Logs\\ESFA\\Provide Feedback\\Employer",
    "ServiceBusConnection": "",
    "RetrieveFeedbackAccountsQueueName": "retrieve-employer-accounts",
    "ProcessActiveFeedbackQueueName": "process-active-feedback",
    "GenerateSurveyInviteMessageQueueName": "generate-survey-invite",
    "AccountRefreshQueueName": "refresh-account",
    "RetrieveProvidersQueueName": "retrieve-providers"
  },

  "EmailSettings": {
    "BatchSize": 5,
    "FeedbackSiteBaseUrl": "localhost:5030",
    "ReminderDays": 14,
    "InviteCycleDays": 30
  },
  "ConnectionStrings": {
    "EmployerEmailStoreConnection": "Data Source=(localdb)\\ProjectsV13;Initial Catalog=ESFA.DAS.EmployerFeedbackEmail.Database;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultipleActiveResultSets=true;MultiSubnetFailover=False",
    "Redis": "localhost:6379"
  },
  "NotificationApi": {
    "BaseUrl": "https://at-notifications.apprenticeships.sfa.bis.gov.uk",
    "ClientToken": "abc123"
  },
  "AccountApi": {
    "ApiBaseUrl": "https://test-accounts.apprenticeships.education.gov.uk",
    "ClientId": "",
    "ClientSecret": "",
    "IdentifierUri": "https://citizenazuresfabisgov.onmicrosoft.com/eas-api",
    "Tenant": "citizenazuresfabisgov.onmicrosoft.com"
  },
  "CommitmentApi": {
    "BaseUrl": "https://test-commitments.apprenticeships.education.gov.uk/",
    "ClientToken": ""
  },
  "ProviderApi": {
    "BaseUrl": "https://test-fatapi.apprenticeships.education.gov.uk/"
  },
"CommitmentV2Api": {
    "ApiBaseUrl": "https://test-commitments.apprenticeships.education.gov.uk/",
    "IdentifierUri": "https://citizenazuresfabisgov.onmicrosoft.com/eas-api"
  }
}
```

##### Publish database (ESFA.DAS.EmployerFeedbackEmail.Database) to sql server.

- Set the connection string to the database in the local.settings.json file under `ConnectionStrings.EmployerEmailStoreConnection`

##### Setting up service bus

- Create a service bus account on azure subscription
- Set the connection string to service bus account in the local.settings.json file under `Values.ServiceBusConnection`
- Create following queues on the service bus account
  - retrieve-employer-accounts
  - process-active-feedback
  - generate-survey-invite
  - refresh-account
  - retrieve-providers

### Application logs

Application logs are logged to [Elasticsearch](https://www.elastic.co/products/elasticsearch) and can be viewed using [Kibana](https://www.elastic.co/products/kibana) at http://localhost:5601

## License

Licensed under the [MIT license](LICENSE)
