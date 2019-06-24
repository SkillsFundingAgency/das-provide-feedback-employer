CREATE TABLE [dbo].[staging_roatp]
(
	[Ukprn] INT NOT NULL, 
    [Name] NCHAR(1000) NOT NULL, 
    [ProviderType] NCHAR(100) NOT NULL, 
    [ParentCompanyGuarantee] BIT NOT NULL, 
    [NewOrganisationWithoutFinancialTrackRecord] BIT NOT NULL, 
    [StartDate] DATETIME2 NOT NULL, 
    [ProviderNotCurrentlyStartingNewApprentices] BIT NULL 
)
