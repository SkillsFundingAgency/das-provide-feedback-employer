# Employer Feedback

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-provide-feedback-employer?repoName=SkillsFundingAgency%2Fdas-provide-feedback-employer&branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2539&repoName=SkillsFundingAgency%2Fdas-provide-feedback-employer&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-provide-feedback-employer&metric=alert_status)](https://sonarcloud.io/project/overview?id=SkillsFundingAgency_das-provide-feedback-employer)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/browse/QF-79)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3773497345/Employer+Feedback+-+QF)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License) 

This repository represents the code base for the employer feedback service. This is a service that allows employers to provide feedback on their training providers. Employers can give feedback via the ad hoc journey from their employer account. The employer has previously been able to provide feedback via emails, as they would recieve emails prompting them to give feedback. However, this emailing function is in the process of being decommissioned. 

## Developer Setup
### Requirements

* [.NET Core SDK >= 2.1.302](https://www.microsoft.com/net/download/)
* (VS Code Only) [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
* [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

### Environment Setup

* **Database** - Publish the local database from the `ESFA.DAS.EmployerFeedbackEmail.Database` project.
    * Setting up your database - [Setup your database](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3948019848/Employer+Feedback+Setup)      
* **local.settings.json file** - Add the following to the `local.settings.json` file in the `ESFA.DAS.ProviderFeedback.Employer.Functions.Emailer` functions app.

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
    "AppName": "das-provide-feedback-emailer",
    "LogDir": "C:\\Logs\\ESFA\\Provide Feedback\\Employer"
  },
  "ConnectionStrings": {
    "EmployerEmailStoreConnection": "Data Source=(localdb)\\ProjectsV13;Initial Catalog=ESFA.DAS.EmployerFeedbackEmail.Database;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultipleActiveResultSets=true;MultiSubnetFailover=False",
    "Redis": "localhost:6379"
  },
  "CommitmentV2Api": {
    "ApiBaseUrl": "https://******.apprenticeships.education.gov.uk/",
    "IdentifierUri": "https://******.onmicrosoft.com/******"
  }
}
```


### Running
* Start Azurite e.g. using a command `C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator>AzureStorageEmulator.exe start`
* Run the solution
* NB: You will need `das-commitments` running too, specifically the `SFA.DAS.CommitmentsV2.Api` project.
* To start the ad hoc journey: localhost:{port number}/{encoded account ID}/providers (to get the encoded account ID, run the account ID through the encoding service in the solution.)
* Or to start the emailing journey: localhost:{port number}/{unique survey code}

### Tests

This codebase includes unit tests and integration tests. These are each in a seperate project kept in a folder called `Testing`.

#### Unit Tests

The unit tests covering each project are each kept in a folder within the `UnitTests` project in the `Testing` folder. They are built using C#, Moq, FluentAssertions, .NET, MSTest, and XUnit.

#### Integration Tests

There is one integration test project, `IntegrationTests`, and in it one test class.

### Application logs

Application logs are logged to [Elasticsearch](https://www.elastic.co/products/elasticsearch) and can be viewed using [Kibana](https://www.elastic.co/products/kibana) at http://localhost:5601

## License

Licensed under the [MIT license](LICENSE)
