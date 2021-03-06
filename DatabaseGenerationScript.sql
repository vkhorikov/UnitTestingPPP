CREATE DATABASE [IntegrationTests]
GO
USE [IntegrationTests]
GO
CREATE TABLE [dbo].[Company](
	[DomainName] [nvarchar](50) NOT NULL,
	[NumberOfEmployees] [int] NOT NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[Type] [int] NOT NULL,
	[IsEmailConfirmed] [bit] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT [dbo].[Company] ([DomainName], [NumberOfEmployees]) VALUES (N'mycorp.com', 0)
GO
