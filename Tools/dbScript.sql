USE [Budget]
GO
/****** Object:  UserDefinedFunction [dbo].[GetAccountCreditFunc]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetAccountCreditFunc](@accountID INT)
RETURNS NVARCHAR(29)
AS
BEGIN
	DECLARE @firstDay INT SET @firstDay = (SELECT TOP 1 FirstDay FROM dbo.Accounts WHERE ID = @accountID)
	IF @firstDay IS NULL SET @firstDay = 1
	
	DECLARE @todayYear INT SET @todayYear = YEAR(GETDATE())
	DECLARE @todayMonth INT SET @todayMonth = MONTH(GETDATE())
	DECLARE @todayDay INT SET @todayDay = DAY(GETDATE())
	
	DECLARE @stDate DATETIME, @finDate DATETIME --начало и конец расчетного периода
	
	IF @todayDay >= @firstDay
	BEGIN
		SET @stDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @todayDay - 1)
	END
	ELSE
	BEGIN
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)  --текущего месяца
		SET @stDate =  DATEADD(month, -1, @finDate) --предыдущего месяца
	END
	
	DECLARE @realAmount MONEY
	SET @realAmount = (SELECT SUM(o.Amount)
					   FROM dbo.Operations AS o
					   JOIN dbo.OperDay AS od ON o.OperDay_ID = od.ID
					   WHERE (Credit_ID = @accountID) AND (Credit_ID IS NOT NULL) AND (od.[Date] BETWEEN @stDate AND @finDate))
	IF @realAmount IS NULL RETURN NULL
	DECLARE @planAmount MONEY SET @planAmount = (SELECT TOP 1 [Plan] FROM dbo.Accounts WHERE ID = @accountID)
	IF @planAmount IS NULL SET @planAmount = -1
	
	DECLARE @result FLOAT SET @result = (@realAmount/@planAmount * 100)
	IF (@result < 0) SET @result = NULL
	RETURN CONVERT(NVARCHAR(20), @realAmount) + '*' + CONVERT(NVARCHAR(9), @result)
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetAccountDebetFunc]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetAccountDebetFunc](@accountID INT)
RETURNS NVARCHAR(30)
AS
BEGIN
	DECLARE @firstDay INT SET @firstDay = (SELECT TOP 1 FirstDay FROM dbo.Accounts WHERE ID = @accountID)
	IF @firstDay IS NULL SET @firstDay = 1
	
	DECLARE @todayYear INT SET @todayYear = YEAR(GETDATE())
	DECLARE @todayMonth INT SET @todayMonth = MONTH(GETDATE())
	DECLARE @todayDay INT SET @todayDay = DAY(GETDATE())
	
	DECLARE @stDate DATETIME, @finDate DATETIME --начало и конец расчетного периода
	
	IF @todayDay >= @firstDay
	BEGIN
		SET @stDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @todayDay - 1)
	END
	ELSE
	BEGIN
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)  --текущего месяца
		SET @stDate =  DATEADD(month, -1, @finDate) --предыдущего месяца
	END
	
	DECLARE @realAmount MONEY
	SET @realAmount = (SELECT SUM(o.Amount)
					   FROM dbo.Operations AS o
					   JOIN dbo.OperDay AS od ON o.OperDay_ID = od.ID
					   WHERE (Debet_ID = @accountID) AND (Debet_ID IS NOT NULL) AND (od.[Date] BETWEEN @stDate AND @finDate))
	IF @realAmount IS NULL RETURN NULL
	DECLARE @limitAmount MONEY SET @limitAmount = (SELECT TOP 1 [Limit] FROM dbo.Accounts WHERE ID = @accountID)
	IF @limitAmount IS NULL SET @limitAmount = -1
	
	DECLARE @result FLOAT SET @result = (@realAmount/@limitAmount * 100)
	IF (@result < 0) SET @result = NULL
	RETURN CONVERT(NVARCHAR(20), @realAmount) + '*' + CONVERT(NVARCHAR(9), @result)
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetCategoryCreditFunc]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetCategoryCreditFunc](@categoryID INT)
RETURNS NVARCHAR(29)
AS
BEGIN
	DECLARE @firstDay INT SET @firstDay = (SELECT TOP 1 FirstDay FROM dbo.Categories WHERE ID = @categoryID)
	IF @firstDay IS NULL SET @firstDay = 1
	
	DECLARE @todayYear INT SET @todayYear = YEAR(GETDATE())
	DECLARE @todayMonth INT SET @todayMonth = MONTH(GETDATE())
	DECLARE @todayDay INT SET @todayDay = DAY(GETDATE())
	
	DECLARE @stDate DATETIME, @finDate DATETIME --начало и конец расчетного периода
	
	IF @todayDay >= @firstDay
	BEGIN
		SET @stDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @todayDay - 1)
	END
	ELSE
	BEGIN
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)  --текущего месяца
		SET @stDate =  DATEADD(month, -1, @finDate) --предыдущего месяца
	END
	
	DECLARE @realAmount MONEY
	SET @realAmount = (SELECT SUM(o.Amount)
					   FROM dbo.Operations AS o
					   JOIN dbo.OperDay AS od ON o.OperDay_ID = od.ID
					   WHERE (Category_ID = @categoryID) AND (Credit_ID IS NOT NULL) AND (od.[Date] BETWEEN @stDate AND @finDate))
	IF @realAmount IS NULL RETURN NULL
	DECLARE @planAmount MONEY SET @planAmount = (SELECT TOP 1 [Plan] FROM dbo.Categories WHERE ID = @categoryID)
	IF @planAmount IS NULL SET @planAmount = -1
	
	DECLARE @result FLOAT SET @result = (@realAmount/@planAmount * 100)
	IF (@result < 0) SET @result = NULL
	RETURN CONVERT(NVARCHAR(20), @realAmount) + '*' + CONVERT(NVARCHAR(9), @result)
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetCategoryDebetFunc]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetCategoryDebetFunc](@categoryID INT)
RETURNS NVARCHAR(29)
AS
BEGIN
	DECLARE @firstDay INT SET @firstDay = (SELECT TOP 1 FirstDay FROM dbo.Categories WHERE ID = @categoryID)
	IF @firstDay IS NULL SET @firstDay = 1
	
	DECLARE @todayYear INT SET @todayYear = YEAR(GETDATE())
	DECLARE @todayMonth INT SET @todayMonth = MONTH(GETDATE())
	DECLARE @todayDay INT SET @todayDay = DAY(GETDATE())
	
	DECLARE @stDate DATETIME, @finDate DATETIME --начало и конец расчетного периода
	
	IF @todayDay >= @firstDay
	BEGIN
		SET @stDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @todayDay - 1)
	END
	ELSE
	BEGIN
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)  --текущего месяца
		SET @stDate =  DATEADD(month, -1, @finDate) --предыдущего месяца
	END
	
	DECLARE @realAmount MONEY
	SET @realAmount = (SELECT SUM(o.Amount)
					   FROM dbo.Operations AS o
					   JOIN dbo.OperDay AS od ON o.OperDay_ID = od.ID
					   WHERE (Category_ID = @categoryID) AND (Debet_ID IS NOT NULL) AND (od.[Date] BETWEEN @stDate AND @finDate))
	IF @realAmount IS NULL RETURN NULL
	DECLARE @limitAmount MONEY SET @limitAmount = (SELECT TOP 1 [Limit] FROM dbo.Categories WHERE ID = @categoryID)
	IF @limitAmount IS NULL SET @limitAmount = -1
	
	DECLARE @result FLOAT SET @result = (@realAmount/@limitAmount * 100)
	IF (@result < 0) SET @result = NULL
	RETURN CONVERT(NVARCHAR(20), @realAmount) + '*' + CONVERT(NVARCHAR(9), @result)
END
GO
/****** Object:  Table [dbo].[Acc_Rests]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Acc_Rests](
	[Account_ID] [int] NOT NULL,
	[OperDay_ID] [int] NOT NULL,
	[Rest] [money] NOT NULL,
	[Credit] [money] NOT NULL,
	[Debet] [money] NOT NULL,
 CONSTRAINT [PK_Acc_Rests] PRIMARY KEY CLUSTERED 
(
	[Account_ID] ASC,
	[OperDay_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountAssistant]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountAssistant](
	[Assistant_ID] [int] NOT NULL,
	[Account_ID] [int] NOT NULL,
 CONSTRAINT [PK_AccountAssistant] PRIMARY KEY CLUSTERED 
(
	[Assistant_ID] ASC,
	[Account_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Accounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Rest] [money] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UserID] [int] NOT NULL,
	[Order] [int] NULL,
	[Limit] [money] NULL,
	[Plan] [money] NULL,
	[FirstDay] [int] NULL,
	[AssistNeeded] [bit] NOT NULL,
	[IsMinusAllowed] [bit] NULL,
	[dtc] [datetime] NOT NULL,
 CONSTRAINT [PK_Checks] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountsCategoriesRating]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountsCategoriesRating](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Rating] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Banners]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Banners](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsPicture] [bit] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Link] [nvarchar](256) NOT NULL,
	[Url] [nvarchar](256) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[CreditRating] [int] NOT NULL,
	[DebetRating] [int] NOT NULL,
	[TransferRating] [int] NULL,
	[UserID] [int] NOT NULL,
	[Limit] [money] NULL,
	[Plan] [money] NULL,
	[FirstDay] [int] NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comments]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeID] [int] NOT NULL,
	[Comment] [nvarchar](max) NOT NULL,
	[Answer] [nvarchar](max) NOT NULL,
	[UserID] [int] NOT NULL,
	[dtc] [datetime] NOT NULL,
	[IsVisible] [bit] NOT NULL,
 CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommentTypes]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentTypes](
	[ID] [int] NOT NULL,
	[Type] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_CommentTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hints]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hints](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Header] [nvarchar](256) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Alias] [nvarchar](128) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Log]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[src] [nvarchar](150) NULL,
	[msg] [nvarchar](550) NULL,
	[err] [int] NULL,
	[dtc] [datetime] NOT NULL,
 CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogAction]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogAction](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[src] [nvarchar](150) NULL,
	[msg] [nvarchar](550) NULL,
	[dtc] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogError]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogError](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[src] [nvarchar](150) NULL,
	[msg] [nvarchar](550) NULL,
	[dtc] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Operation_Statuses]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Operation_Statuses](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Operation_Statuses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Operations]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Operations](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Amount] [money] NOT NULL,
	[Category_ID] [int] NULL,
	[Debet_ID] [int] NULL,
	[Credit_ID] [int] NULL,
	[OperDay_ID] [int] NOT NULL,
	[DateCreate] [datetime] NOT NULL,
	[DateEdit] [datetime] NOT NULL,
	[UserName] [nvarchar](64) NULL,
	[Status] [int] NOT NULL,
	[Transfer_ID] [int] NULL,
 CONSTRAINT [PK_Operations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OperationsImages]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OperationsImages](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[OperationID] [int] NULL,
	[Image] [image] NULL,
	[FileName] [nvarchar](256) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OperDay]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OperDay](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
 CONSTRAINT [PK_OperDay] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PredefinedAccounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PredefinedAccounts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_PredefinedAccounts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PredefinedCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PredefinedCategories](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_PredefinedCategories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sessions]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sessions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TimeIn] [datetime] NOT NULL,
	[TimeOut] [datetime] NULL,
	[Duration] [decimal](18, 0) NULL,
	[UserID] [int] NOT NULL,
	[IP] [nvarchar](15) NULL,
	[Version] [varchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](256) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NULL,
	[Login] [nvarchar](64) NOT NULL,
	[Password] [nvarchar](47) NOT NULL,
	[Online] [bit] NOT NULL,
	[dtc] [datetime] NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[LastSessionID] [int] NULL,
	[IP] [nvarchar](15) NULL,
	[Version] [nvarchar](10) NULL,
	[Rating] [int] NULL,
	[ParentID] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [dbo].[OpersPer]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[OpersPer](@AccID int, @DTS date,@DTPO date)
returns table 
as return
(

select Operations.*
from dbo.Operations 
where (cast(Operations.DateEdit as DATE) between @DTS and @DTPO) and (Debet_ID = @AccID)
)
GO
/****** Object:  UserDefinedFunction [dbo].[SaldoByGroup]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[SaldoByGroup](@UserID bigint, @DTS date, @DTPO date)
returns table
as
return(
select Accounts.ID, Accounts.Name, 
	ROUND(IsNull(DT.Sum_DT,0),2) as SumDT, 
	ROUND(IsNull(KT.Sum_Kt,0),2) as SumKD, 
	ROUND(IsNull(DT.Sum_DT,0) - IsNull(KT.Sum_Kt,0),2) as Saldo
from dbo.Accounts 
left join (			--сумма дебета по счетам
	select Accounts.ID, Accounts.Name, SUM(Operations.Amount) as Sum_DT
	from dbo.Accounts inner join dbo.Operations on Accounts.ID = Operations.Debet_ID
	where (Accounts.UserID = @UserID) and (CAST(Operations.DateEdit as Date) between @DTS and @DTPO)
	group by Accounts.ID, Accounts.Name
	)DT on Accounts.ID = DT.ID
left join (			--сумма кредита по счетам
	select Accounts.ID, Accounts.Name, SUM(Operations.Amount) as Sum_Kt
	from dbo.Accounts inner join dbo.Operations on Accounts.ID = Operations.Credit_ID
	where (Accounts.UserID = @UserID) and (CAST(Operations.DateEdit as Date) between @DTS and @DTPO)
	group by Accounts.ID, Accounts.Name
	) KT on Accounts.ID = KT.ID
where Accounts.UserID = @UserID and Accounts.IsDeleted = 0
group by Accounts.ID, Accounts.Name, IsNull(DT.Sum_DT,0), IsNull(KT.Sum_Kt,0)
)
GO
/****** Object:  UserDefinedFunction [dbo].[SaldoByGroupPer]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[SaldoByGroupPer](@UserID bigint, @DTS date, @DTPO date)
returns table
as
return(
select Accounts.ID, Accounts.[Name], 
	ROUND(IsNull(DT.Sum_DT,0),2) as SumDT, 
	ROUND(IsNull(KT.Sum_Kt,0),2) as SumKD, 
	ROUND(IsNull(DT.Sum_DT,0) - IsNull(KT.Sum_Kt,0),2) as Saldo
from dbo.Accounts 
left join (			--сумма дебета по счетам
	select Accounts.ID, Accounts.Name, CAST(SUM(Operations.Amount) as numeric(16,2)) as Sum_DT
	from dbo.Accounts inner join dbo.Operations on Accounts.ID = Operations.Debet_ID
	where (Accounts.UserID = @UserID) and (CAST(Operations.DateEdit as Date) between @DTS and @DTPO)
	group by Accounts.ID, Accounts.Name
	)DT on Accounts.ID = DT.ID
left join (			--сумма кредита по счетам
	select Accounts.ID, Accounts.Name, CAST(SUM(Operations.Amount) as numeric(16,2)) as Sum_Kt
	from dbo.Accounts inner join dbo.Operations on Accounts.ID = Operations.Credit_ID
	where (Accounts.UserID = @UserID) and (CAST(Operations.DateEdit as Date) between @DTS and @DTPO)
	group by Accounts.ID, Accounts.Name
	) KT on Accounts.ID = KT.ID
where Accounts.UserID = @UserID
group by Accounts.ID, Accounts.[Name], IsNull(DT.Sum_DT,0), IsNull(KT.Sum_Kt,0)
--having not DT.[Name] is null
)
GO
ALTER TABLE [dbo].[Acc_Rests] ADD  CONSTRAINT [DF_Acc_Rests_Credit]  DEFAULT ((0)) FOR [Credit]
GO
ALTER TABLE [dbo].[Acc_Rests] ADD  CONSTRAINT [DF_Acc_Rests_Debet]  DEFAULT ((0)) FOR [Debet]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_AssistNeeded]  DEFAULT ((0)) FOR [AssistNeeded]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_IsMinusAllowed]  DEFAULT ((0)) FOR [IsMinusAllowed]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_dtc]  DEFAULT (getdate()) FOR [dtc]
GO
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [DF_Categories_CreditRating]  DEFAULT ((0)) FOR [CreditRating]
GO
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [DF_Categories_DebetRating]  DEFAULT ((0)) FOR [DebetRating]
GO
ALTER TABLE [dbo].[Log] ADD  CONSTRAINT [DF_Log_dtc]  DEFAULT (getdate()) FOR [dtc]
GO
ALTER TABLE [dbo].[LogAction] ADD  CONSTRAINT [DF_LogAction_dtc]  DEFAULT (getdate()) FOR [dtc]
GO
ALTER TABLE [dbo].[LogError] ADD  CONSTRAINT [DF_LogError_dtc]  DEFAULT (getdate()) FOR [dtc]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Online]  DEFAULT ((0)) FOR [Online]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_dtc]  DEFAULT (getdate()) FOR [dtc]
GO
ALTER TABLE [dbo].[Acc_Rests]  WITH CHECK ADD  CONSTRAINT [FK_Acc_Rests_Accounts] FOREIGN KEY([Account_ID])
REFERENCES [dbo].[Accounts] ([ID])
GO
ALTER TABLE [dbo].[Acc_Rests] CHECK CONSTRAINT [FK_Acc_Rests_Accounts]
GO
ALTER TABLE [dbo].[Acc_Rests]  WITH CHECK ADD  CONSTRAINT [FK_Acc_Rests_OperDay] FOREIGN KEY([OperDay_ID])
REFERENCES [dbo].[OperDay] ([ID])
GO
ALTER TABLE [dbo].[Acc_Rests] CHECK CONSTRAINT [FK_Acc_Rests_OperDay]
GO
ALTER TABLE [dbo].[AccountAssistant]  WITH CHECK ADD  CONSTRAINT [FK_AccountAssistant_Accounts] FOREIGN KEY([Account_ID])
REFERENCES [dbo].[Accounts] ([ID])
GO
ALTER TABLE [dbo].[AccountAssistant] CHECK CONSTRAINT [FK_AccountAssistant_Accounts]
GO
ALTER TABLE [dbo].[AccountAssistant]  WITH CHECK ADD  CONSTRAINT [FK_AccountAssistant_Users] FOREIGN KEY([Assistant_ID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[AccountAssistant] CHECK CONSTRAINT [FK_AccountAssistant_Users]
GO
ALTER TABLE [dbo].[Accounts]  WITH CHECK ADD  CONSTRAINT [FK_Accounts_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Accounts] CHECK CONSTRAINT [FK_Accounts_Users]
GO
ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [FK_Categories_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [FK_Categories_Users]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_CommentTypes] FOREIGN KEY([TypeID])
REFERENCES [dbo].[CommentTypes] ([ID])
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_CommentTypes]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_Categories] FOREIGN KEY([Category_ID])
REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_Categories]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_Checks] FOREIGN KEY([Credit_ID])
REFERENCES [dbo].[Accounts] ([ID])
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_Checks]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_Checks1] FOREIGN KEY([Debet_ID])
REFERENCES [dbo].[Accounts] ([ID])
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_Checks1]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_Operation_Statuses] FOREIGN KEY([Status])
REFERENCES [dbo].[Operation_Statuses] ([ID])
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_Operation_Statuses]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_OperDay] FOREIGN KEY([OperDay_ID])
REFERENCES [dbo].[OperDay] ([ID])
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_OperDay]
GO
/****** Object:  StoredProcedure [dbo].[AddAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AddAccount] 
	@Name NVARCHAR(MAX),
	@UserID INT,
	@Limit MONEY = NULL,
	@Plan MONEY = NULL,
	@FirstDay INT = NULL,
	@IsMinusAllowed bit = 0
AS
BEGIN

	insert LogAction(src, msg) values('[AddAccount]', @Name + ' User - ' + STR(@UserID))

	DECLARE @AccID INT
	INSERT INTO Accounts (Name, Rest, IsDeleted, UserID, [Order], Limit, [Plan], FirstDay, IsMinusAllowed)
	VALUES (@Name, 0.00, 0, @UserID, 1, @Limit, @Plan, @FirstDay, @IsMinusAllowed)
	SET @AccID = @@IDENTITY

	EXEC OperDayClose @AccID, @UserID, NULL, NULL

	EXEC IncRating @UserID, 2  --повысить рейтинг на 2

	SELECT @AccID

END
GO
/****** Object:  StoredProcedure [dbo].[AddAccountAssistant]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AddAccountAssistant] 
	@userId int,
	@accountId int								   
AS
BEGIN

	insert LogAction(src, msg) values('[AddAccountAssistant]', 
		' User - ' + STR(@UserID) + '. Account - ' + STR(@accountId))

	INSERT INTO [dbo].[AccountAssistant]
			   ([Assistant_ID]
			   ,[Account_ID])
		 VALUES
			   (@userId
			   ,@accountId)

END
GO
/****** Object:  StoredProcedure [dbo].[AddCategory]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[AddCategory] 
	@Name NVARCHAR(MAX),
	@UserID INT
AS
BEGIN
	
	insert LogAction(src, msg) values('[AddCategory]', @Name + ' User - ' + STR(@UserID))

	DECLARE @CategID INT

	INSERT INTO Categories (Name, CreditRating, DebetRating, TransferRating, UserID, Limit, [Plan], FirstDay)
	VALUES  (@name, 0, 0, 0, @UserID, NULL, NULL, NULL)
	
	SET @CategID = @@IDENTITY
	
	EXEC IncRating @UserID, 1  --повысить рейтинг на 1

	
	SELECT @CategID
	
END


GO
/****** Object:  StoredProcedure [dbo].[AddComment]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AddComment] @typeID INT,
								   @comment NVARCHAR(MAX),
								   @userID INT
AS
BEGIN

INSERT INTO dbo.Comments(TypeID, Comment, Answer, UserID, dtc, IsVisible)
VALUES(@typeID, @comment, '', @userID, GETDATE(), 0)

EXEC IncRating @userID, 500  --повысить рейтинг на 500

EXEC IdeaTracker.dbo.site_AddIdea @comment, N'', @typeID, 1, 1

END
GO
/****** Object:  StoredProcedure [dbo].[am_ClearAllUserData]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[am_ClearAllUserData]
	@UserID int
as
begin

	delete Acc_Rests where Account_ID in (select ID from Accounts where UserID = @UserID)
	delete Operations where Debet_ID in (select ID from Accounts where UserID = @UserID)
	delete Operations where Credit_ID in (select ID from Accounts where UserID = @UserID)
	delete Categories where UserID = @UserID
	delete Accounts where UserID = @UserID

end

GO
/****** Object:  StoredProcedure [dbo].[am_CloseAccOperDay]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[am_CloseAccOperDay] 
	--AM: 25-10-2022
	--AM: 04.11.2022 Процедура используется для пересчета остатков, название не соответствует
	@accountID INT,
	@beginDate DATE = null, 
	@endDate DATE = null,
	@NeedResult bit = 0
AS
BEGIN
  
	insert LogAction(src, msg) values('am_CloseAccOperDay - BEGIN', '@accountID = '+str(@accountID))

	declare @minDate date, @minOperDay_ID int

	--> Получить дату первой операции и её ID для заданного счета
	SELECT @minDate = MIN(od.[Date]) FROM OperDay od 
	JOIN dbo.Operations op ON od.ID = op.OperDay_ID 
	JOIN dbo.Accounts a ON a.ID in (op.Credit_ID, op.Debet_ID) WHERE a.ID = @accountID
	
	select @minOperDay_ID = d.ID from OperDay d where @minDate = d.[Date]
	--<

	-- В день первой операции на счету входящий остаток должен быть 0
	update Acc_Rests set Rest = 0 where Account_ID = @accountID and OperDay_ID = @minOperDay_ID

	-- Если не задан диапазон дат, ставим зн. по умолч.
	if @endDate is null set @endDate = GETDATE() 
	if @beginDate is null set @beginDate = @minDate

	-- Удалить остатки до первой операции для заданного счета  		
	DELETE ar FROM dbo.Acc_Rests ar	JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID
	WHERE od.[Date] < @minDate AND ar.Account_ID = @accountID
  		
	--Обновить таблицу Acc_Rests--------------
	DECLARE @operDayID INT
	DECLARE @credit MONEY    -- приход за ОперДень
	DECLARE @debet MONEY     -- расход за ОперДень
	DECLARE @lastRest MONEY  -- последний остаток за предыдущий день, тоесть это входящий за выбр-й ОперДень
	--Подсчитать остаток на начало рассматриваемого периода-------------------
	SELECT @credit = SUM(Amount) FROM Operations o join OperDay d on d.ID = o.OperDay_ID WHERE d.Date < @beginDate AND Credit_ID = @accountID and Status > 3
	SELECT @debet = SUM(Amount) FROM Operations o join OperDay d on d.ID = o.OperDay_ID WHERE d.Date < @beginDate AND Debet_ID = @accountID and Status > 3
	SET @lastRest = ISNULL(@credit, 0) - ISNULL(@debet, 0) --входящий остаток заданного периода
	--------------------------------------------------------------------------
		
	--Создаем курсор для каждого дня заданного диапазона дат
	DECLARE operDayCursor CURSOR FOR									-- в порядке возрастания дат
		SELECT od.ID FROM OperDay od WHERE od.[Date] BETWEEN @beginDate AND @endDate order by od.Date	
	OPEN operDayCursor
	FETCH NEXT FROM operDayCursor INTO @operDayID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--Для каждого дня заданного диапазона считаем сумму приходов и расходов по операциям
		SELECT @credit = IsNull(SUM(Amount), 0) FROM Operations WHERE OperDay_ID = @operDayID AND Credit_ID = @accountID and Status > 3
		SELECT @debet = IsNull(SUM(Amount), 0) FROM Operations WHERE OperDay_ID = @operDayID AND Debet_ID = @accountID and Status > 3
		
		--если в остатках нет записи для этого дня, создаем
		IF NOT EXISTS(SELECT * FROM dbo.Acc_Rests WHERE Account_ID = @accountID AND OperDay_ID = @operDayID)
			INSERT INTO dbo.Acc_Rests VALUES(@accountID, @operDayID, 0, 0, 0)
		
		--обновим остаток и приход, расход за этот день для заданного счета
		UPDATE dbo.Acc_Rests SET Rest = @lastRest, Credit = @credit, Debet = @debet
		WHERE Account_ID = @accountID AND OperDay_ID = @operDayID
		
		--считаем входящий остаток для следующего дня
		SET @lastRest = @lastRest + @credit - @debet
		
		FETCH NEXT FROM operDayCursor
		INTO @operDayID
	END
	
	CLOSE operDayCursor
	DEALLOCATE operDayCursor
	
	-- Подсчитать остаток счета на текущую дату и проверка
	DECLARE @restRest MONEY, @restOper MONEY, @today date set @today = getdate()
	declare @creOper money, @debOper money, @creRest money, @debRest money
	-- Сумма по остаткам
	SELECT @creRest = ISNULL(SUM(r.Credit), 0),
		   @debRest = ISNULL(SUM(r.Debet), 0) FROM Acc_Rests r join OperDay d on r.OperDay_ID = d.ID WHERE r.Account_ID = @accountID and d.Date < @today
	SELECT @creOper = ISNULL(SUM(Amount), 0) FROM Operations o join OperDay d on d.ID = o.OperDay_ID WHERE d.Date < @today AND Credit_ID = @accountID and o.Status > 3
	SELECT @debOper = ISNULL(SUM(Amount), 0) FROM Operations o join OperDay d on d.ID = o.OperDay_ID WHERE d.Date < @today AND Debet_ID = @accountID and o.Status > 3
	
	SELECT @restOper = @creOper - @debOper
	SELECT @restRest = @creRest - @debRest
	SELECT @lastRest = r.Rest FROM Acc_Rests r join OperDay d on r.OperDay_ID = d.ID WHERE r.Account_ID = @accountID and d.Date = @today
	
	declare @res int			set @res = 0
	if @creRest  <> @creOper	set @res = @res + 1
	if @debRest  <> @debOper	set @res = @res + 2
	if @restRest <> @restOper	set @res = @res + 4
	if @restRest <> @lastRest	set @res = @res + 8
	if @restOper <> @lastRest	set @res = @res + 16

	insert LogAction(src, msg) values('am_CloseAccOperDay - Проверка остатков', 'Res = ' + str(@res))
	
	if @res <> 0
	  insert LogError(src, msg) values('am_CloseAccOperDay - Остатки не сошлись', 
	  str(@creRest)+' - '+str(@creOper)+' - '+str(@restRest)+' - '+str(@lastRest))


	update Accounts set Rest = @lastRest WHERE ID = @accountID
	
	if @NeedResult = 1	select @res

END


GO
/****** Object:  StoredProcedure [dbo].[am_CreateModelClassFromTable]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[am_CreateModelClassFromTable]
	@TableName sysname = 'Задай имя таблицы и получишь код класса'
as
begin
	if @TableName like 'Зад%' RAISERROR('Имя таблицы не задано', 16, 1)

	declare @Result varchar(max) = 'public class ' + @TableName + '
	{'

	select @Result = @Result + '
		public ' + ColumnType + NullableSign + ' ' + ColumnName + ' { get; set; }'
	from
	(
		select
			replace(col.name, ' ', '_') ColumnName,
			column_id ColumnId,
			case typ.name
				when 'bigint' then 'long'
				when 'binary' then 'byte[]'
				when 'bit' then 'bool'
				when 'char' then 'string'
				when 'date' then 'DateTime'
				when 'datetime' then 'DateTime'
				when 'datetime2' then 'DateTime'
				when 'datetimeoffset' then 'DateTimeOffset'
				when 'decimal' then 'decimal'
				when 'float' then 'double'
				when 'image' then 'byte[]'
				when 'int' then 'int'
				when 'money' then 'decimal'
				when 'nchar' then 'string'
				when 'ntext' then 'string'
				when 'numeric' then 'decimal'
				when 'nvarchar' then 'string'
				when 'real' then 'float'
				when 'smalldatetime' then 'DateTime'
				when 'smallint' then 'short'
				when 'smallmoney' then 'decimal'
				when 'text' then 'string'
				when 'time' then 'TimeSpan'
				when 'timestamp' then 'long'
				when 'tinyint' then 'byte'
				when 'uniqueidentifier' then 'Guid'
				when 'varbinary' then 'byte[]'
				when 'varchar' then 'string'
				else 'UNKNOWN_' + typ.name
			end ColumnType,
			case
				when col.is_nullable = 1 and typ.name in ('bigint', 'bit', 'date', 'datetime', 'datetime2', 'datetimeoffset', 'decimal', 'float', 'int', 'money', 'numeric', 'real', 'smalldatetime', 'smallint', 'smallmoney', 'time', 'tinyint', 'uniqueidentifier')
				then '?'
				else ''
			end NullableSign
		from sys.columns col
			join sys.types typ on
				col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
		where object_id = object_id(@TableName)
	) t
	order by ColumnId

	set @Result = @Result  + '
	}'

	print @Result

end
GO
/****** Object:  StoredProcedure [dbo].[am_GetBalans]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[am_GetBalans]
  @userID int, 
  @OperDayID int, 
  @Amount money out 
as
	 
    select @Amount = SUM(r.Rest+r.Credit-r.Debet) from Acc_Rests r 
    inner join Accounts a on a.ID = r.Account_ID
    where a.ID in (386, 407, 397, 403, 388, 393, 396, 417, 455, 677)
    and r.OperDay_ID = @OperDayID
    and a.UserID = @userID
GO
/****** Object:  StoredProcedure [dbo].[BackupCheckAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupCheckAccount] @accountID INT, @name NVARCHAR(MAX), @isDeleted BIT
AS
BEGIN

IF NOT EXISTS (SELECT TOP 1 ID FROM dbo.Accounts WHERE ID = @accountID)
	SELECT 2  --аккаунт удален
ELSE
BEGIN
	IF EXISTS (SELECT TOP 1 ID FROM dbo.Accounts WHERE ID = @accountID AND Name = @name AND IsDeleted = @isDeleted)
		SELECT 1  --аккаунт не модифицирован
	ELSE
		SELECT 0  --аккаунт модифицирован
END
	
END
GO
/****** Object:  StoredProcedure [dbo].[BackupCheckCategory]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupCheckCategory] @categoryID INT, @name NVARCHAR(MAX)
AS
BEGIN

IF NOT EXISTS (SELECT TOP 1 ID FROM dbo.Categories WHERE ID = @categoryID)
	SELECT 2  --категория удалена
ELSE
BEGIN
	IF EXISTS (SELECT TOP 1 ID FROM dbo.Categories WHERE ID = @categoryID AND Name = @name)
		SELECT 1  --категория не модифицирована
	ELSE
		SELECT 0  --категория модифицирована
END
	
END
GO
/****** Object:  StoredProcedure [dbo].[BackupGetAccounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupGetAccounts] @userID INT
AS
BEGIN

SELECT ID, Name, Rest, IsDeleted, UserID, [Order]
FROM dbo.Accounts
WHERE UserID = @userID
	
END
GO
/****** Object:  StoredProcedure [dbo].[BackupGetAccountsCategoriesRating]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupGetAccountsCategoriesRating] @userID INT
AS
BEGIN

SELECT acr.ID, acr.AccountID, acr.CategoryID, acr.Rating
FROM dbo.AccountsCategoriesRating AS acr
JOIN dbo.Accounts AS a ON acr.AccountID = a.ID
WHERE a.UserID = @userID

END
GO
/****** Object:  StoredProcedure [dbo].[BackupGetCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupGetCategories] @userID INT
AS
BEGIN

SELECT ID, Name, CreditRating, DebetRating, TransferRating, UserID
FROM dbo.Categories
WHERE UserID = @userID
	
END
GO
/****** Object:  StoredProcedure [dbo].[BackupGetOperations]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupGetOperations] @userID INT
AS
BEGIN

(
SELECT o.ID, o.[Description], o.Amount, o.Category_ID, o.Debet_ID, o.Credit_ID, o.OperDay_ID, o.DateCreate, o.DateEdit, o.[Status]
FROM dbo.Operations AS o
JOIN dbo.Accounts AS a ON o.Credit_ID = a.ID
WHERE a.UserID = @userID
)
UNION
(
SELECT o.ID, o.[Description], o.Amount, o.Category_ID, o.Debet_ID, o.Credit_ID, o.OperDay_ID, o.DateCreate, o.DateEdit, o.[Status]
FROM dbo.Operations AS o
JOIN dbo.Accounts AS a ON o.Debet_ID = a.ID
WHERE a.UserID = @userID
)
	
END
GO
/****** Object:  StoredProcedure [dbo].[BackupSetAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupSetAccount] 
-- AM: 04.11.2022 - в этом виде проца полная чепуха
	@accountID INT,
	@name NVARCHAR(MAX),
	@rest MONEY,
	@isDeleted BIT,
	@userID INT,
	@order INT
AS
BEGIN

IF EXISTS (SELECT * FROM dbo.Accounts WHERE ID = @accountID)
BEGIN
	UPDATE dbo.Accounts
	SET Name = @name, Rest = @rest, IsDeleted = @isDeleted, UserID = @userID, [Order] = @order
	WHERE ID = @accountID
END
ELSE
BEGIN
	INSERT INTO dbo.Accounts(Name, Rest, IsDeleted, UserID, [Order])
	VALUES(@name, @rest, @isDeleted, @userID, @order)
END

END
GO
/****** Object:  StoredProcedure [dbo].[BackupSetAccountsCategoriesRating]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupSetAccountsCategoriesRating] @acRatingID INT,
													 @accountID INT,
													 @categoryID INT,
													 @rating INT
AS
BEGIN

IF EXISTS (SELECT * FROM dbo.AccountsCategoriesRating WHERE ID = @acRatingID)
BEGIN
	UPDATE dbo.AccountsCategoriesRating
	SET AccountID = @accountID, CategoryID = @categoryID, Rating = @rating
	WHERE ID = @acRatingID
END
ELSE
BEGIN
	INSERT INTO dbo.AccountsCategoriesRating(AccountID, CategoryID, Rating)
	VALUES(@accountID, @categoryID, @rating)
END

END
GO
/****** Object:  StoredProcedure [dbo].[BackupSetCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupSetCategories] @categoryID INT,
										@name NVARCHAR(MAX),
										@creditRating INT,
										@debetRating INT,
										@transferRating INT,
										@userID INT
AS
BEGIN

IF EXISTS (SELECT * FROM dbo.Categories WHERE ID = @categoryID)
BEGIN
	UPDATE dbo.Categories
	SET Name = @name, CreditRating = @creditRating, DebetRating = @debetRating, TransferRating = @transferRating, UserID = @userID
	WHERE ID = @categoryID
END
ELSE
BEGIN
	INSERT INTO dbo.Categories(Name, CreditRating, DebetRating, TransferRating, UserID)
	VALUES(@name, @creditRating, @debetRating, @transferRating, @userID)
END

END
GO
/****** Object:  StoredProcedure [dbo].[BackupSetOperations]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[BackupSetOperations] @operationID INT,
										@description NVARCHAR(MAX),
										@amount MONEY,
										@category_ID INT,
										@debet_ID INT,
										@credit_ID INT,
										@operDay_ID INT,
										@dateCreate DATE,
										@dateEdit DATE,
										@status INT
AS
BEGIN

IF EXISTS (SELECT * FROM dbo.Operations WHERE ID = @operationID)
BEGIN
	UPDATE dbo.Operations
	SET [Description] = @description, Amount = @amount, Category_ID = @category_ID, Debet_ID = @debet_ID, Credit_ID = @credit_ID, OperDay_ID = @operDay_ID, DateCreate = @dateCreate, DateEdit = @dateEdit, [Status] = @status
	WHERE ID = @operationID
END
ELSE
BEGIN
	INSERT INTO dbo.Operations([Description], Amount, Category_ID, Debet_ID, Credit_ID, OperDay_ID, DateCreate, DateEdit, [Status])
	VALUES(@description, @amount, @category_ID, @debet_ID, @credit_ID, @operDay_ID, @dateCreate, @dateEdit, @status)
END

END
GO
/****** Object:  StoredProcedure [dbo].[CarryCreditOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[CarryCreditOperation] @operationId INT
AS
BEGIN
	UPDATE Operations
	SET [Status] = 8
	WHERE ID = @operationId
END
GO
/****** Object:  StoredProcedure [dbo].[CarryDebetOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[CarryDebetOperation] @operationId INT
AS
BEGIN
	UPDATE Operations
	SET [Status] = 8
	WHERE ID = @operationId
END
GO
/****** Object:  StoredProcedure [dbo].[ChangeOperationStatus]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ChangeOperationStatus] 
-- AM: 04.11.2022
	@operationID INT,
	@newStatusID INT
AS
BEGIN

	declare @oldStatusID int 
	select @oldStatusID = [Status] from Operations where ID = @operationID
	UPDATE dbo.Operations SET [Status] = @newStatusID WHERE ID = @operationID

	-- Пересчет остатков если статус меняемся с 8 на 0, 1 или наоборот
	if @oldStatusID = 8 and @newStatusID in (0, 1)
	OR @newStatusID = 8 and @oldStatusID in (0, 1)	
	begin
		declare @accountId int, @beginDate date, @endDate date set @endDate = getdate()
		
		select @beginDate = DATEADD(day, -1, d.Date) 
		from Operations o join OperDay d on d.ID = o.OperDay_ID where o.ID = @operationID

		select @accountId = Credit_ID from Operations where ID = @operationID
		if @accountId is not null
			exec am_CloseAccOperDay @accountId, @beginDate, @endDate
		
		select @accountId = Debet_ID from Operations where ID = @operationID
		if @accountId is not null
			exec am_CloseAccOperDay @accountId, @beginDate, @endDate

	end

END
GO
/****** Object:  StoredProcedure [dbo].[CheckAddOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CheckAddOperation] @accountID INT,
										  @userID INT,
										  @beginDate DATE,
										  @minus BIT OUTPUT
AS
BEGIN
	--------------------------------------------------------------------------
	--Определить min и max ID дат--------------------------------------------
	DECLARE @minDate DATETIME
	DECLARE @endDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date])
					FROM dbo.OperDay AS od
					JOIN dbo.Operations AS op ON od.ID = op.OperDay_ID
					LEFT JOIN dbo.Accounts AS aC ON op.Credit_ID = aC.ID
					LEFT JOIN dbo.Accounts AS aD ON op.Debet_ID = aD.ID
					WHERE aC.UserID = @userID OR aD.UserID = @userID)
					 
	SET @endDate = (SELECT MAX(od.[Date])
					FROM dbo.OperDay AS od
					JOIN dbo.Operations AS op ON od.ID = op.OperDay_ID
					LEFT JOIN dbo.Accounts AS aC ON op.Credit_ID = aC.ID
					LEFT JOIN dbo.Accounts AS aD ON op.Debet_ID = aD.ID
					WHERE aC.UserID = @userID OR aD.UserID = @userID)
	
	IF ((@beginDate IS NULL) OR (@beginDate < @minDate))
		SET @beginDate = @minDate
	--------------------------------------------------------------------------
	--------------------------------------------------------------------------
		
	--------------------------------------------------------------------------
	--Заполнить временную таблицу tempRests-----------------------------------
	CREATE TABLE tempRests(OperDay_ID INT, Rest MONEY, Credit MONEY, Debet MONEY)
	
	INSERT INTO dbo.tempRests
	SELECT od.ID, 0, 0, 0
	FROM dbo.OperDay AS od
	WHERE od.[Date] BETWEEN @beginDate AND @endDate
	--------------------------------------------------------------------------
	--------------------------------------------------------------------------
	
	--------------------------------------------------------------------------
	--------------------------------------------------------------------------
	DECLARE @credit MONEY    --прибыль за ОперДень
	DECLARE @debet MONEY     --расходы за ОперДень
	DECLARE @lastRest MONEY  --последний остаток за предыдущий день
	--Подсчитать остаток на начало рассматриваемого периода-------------------
	SET @lastRest = (SELECT TOP 1 Rest FROM dbo.Acc_Rests AS ar JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID WHERE od.[Date] = @beginDate AND ar.Account_ID = @accountID)
	--------------------------------------------------------------------------
		
	DECLARE @operDayID	INT
	
	DECLARE operDayCursor CURSOR FOR
	SELECT od.ID
	FROM dbo.OperDay AS od
	INNER JOIN tempRests AS tr ON tr.OperDay_ID = od.ID
	WHERE (od.[Date] >= @beginDate) AND (od.[Date] <= @endDate)
	
	OPEN operDayCursor

	FETCH NEXT FROM operDayCursor
	INTO @operDayID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @credit = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Credit_ID = @accountID)
		SET @debet = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Debet_ID = @accountID)
		IF @credit IS NULL SET @credit = 0
		IF @debet IS NULL SET @debet = 0
		IF @lastRest IS NULL SET @lastRest = 0
		
		UPDATE tempRests
		SET Rest = @lastRest, Credit = @credit, Debet = @debet
		WHERE OperDay_ID = @operDayID
		
		SET @lastRest = @lastRest + @credit - @debet
		
		FETCH NEXT FROM operDayCursor
		INTO @operDayID
	END
	
	CLOSE operDayCursor
	DEALLOCATE operDayCursor
	--------------------------------------------------------------------------
	--------------------------------------------------------------------------
	
	--------------------------------------------------------------------------
	-- Проверить наличие отрицательных остатков ------------------------------
	IF EXISTS (SELECT r.Rest FROM tempRests AS r WHERE r.Rest < 0)
		SET @minus = 1
	ELSE
		SELECT @minus = 0
	--------------------------------------------------------------------------
	--------------------------------------------------------------------------
	
	DROP TABLE tempRests
END
GO
/****** Object:  StoredProcedure [dbo].[CheckCategoryReferences]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[CheckCategoryReferences] @category_id INT
AS
BEGIN
	IF EXISTS (SELECT ID FROM dbo.Operations WHERE Category_ID = @category_id)
		SELECT 1  --существуют
	ELSE
		SELECT 0 -- не существуют
END
GO
/****** Object:  StoredProcedure [dbo].[CloseAllOperDays]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CloseAllOperDays] 
  @userID INT
AS
BEGIN
	DECLARE @id INT
	
	DECLARE accountsCursor CURSOR FOR
	SELECT ID
	FROM dbo.Accounts
	WHERE UserID = @userID
	
	OPEN accountsCursor

	FETCH NEXT FROM accountsCursor
	INTO @id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--EXEC OperDayClose @id, @userID, NULL, NULL
		exec am_CloseAccOperDay @id
		
		FETCH NEXT FROM accountsCursor
		INTO @id
	END
	
	CLOSE accountsCursor
	DEALLOCATE accountsCursor
END
GO
/****** Object:  StoredProcedure [dbo].[CloseAllOperDaysWithDates]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CloseAllOperDaysWithDates] 
  @userID INT,
	@fromDate DATE,
	@toDate DATE
AS
BEGIN
	DECLARE @aid INT
	
	DECLARE accountsCursor CURSOR FOR
	SELECT ID
	FROM dbo.Accounts
	WHERE UserID = @userID
	
	OPEN accountsCursor

	FETCH NEXT FROM accountsCursor
	INTO @aid
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--EXEC OperDayClose @aid, @userID, @fromDate, @toDate
		exec am_CloseAccOperDay @aid, @fromDate, @toDate
		
		FETCH NEXT FROM accountsCursor
		INTO @aid
	END
	
	CLOSE accountsCursor
	DEALLOCATE accountsCursor
END
GO
/****** Object:  StoredProcedure [dbo].[DailyDebitReport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[DailyDebitReport] 
	@userID INT,
	@accountID INT,
	@startDate DATE, 
	@endDate DATE
AS
BEGIN
	
	select [Date] [Day], sum(Amount) [Amount] from Operations o join OperDay d on o.OperDay_ID=d.ID
	where @accountID in (Debet_ID, Credit_ID) and d.Date between @startDate and @endDate
	group by [Date]

END

GO
/****** Object:  StoredProcedure [dbo].[DailyRestReport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[DailyRestReport] 
	@userID INT,
	@accountID INT,
	@startDate DATE, 
	@endDate DATE
AS
BEGIN
	select d.Date [Day], Rest [Amount] from Acc_Rests r join OperDay d on r.OperDay_ID=d.ID
	where d.Date between @startDate and @endDate and r.Account_ID=@accountID
END

GO
/****** Object:  StoredProcedure [dbo].[DeleteAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[DeleteAccount] @accountID INT
AS
BEGIN
	UPDATE dbo.Accounts
	SET IsDeleted = 1
	WHERE ID = @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteAccountAssistant]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteAccountAssistant] 
	@userId int,
	@accountId int								   
AS
BEGIN

	insert LogAction(src, msg) values('[DeleteAccountAssistant]', 
		' User - ' + STR(@UserID) + '. Account - ' + STR(@accountId))

	DELETE FROM [dbo].[AccountAssistant]
      WHERE [Assistant_ID] = @userId and [Account_ID] = @accountId

END
GO
/****** Object:  StoredProcedure [dbo].[DeleteCategory]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteCategory] @categoryID INT
AS
BEGIN
	DELETE dbo.Categories
	WHERE ID = @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteInfoByUserID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[DeleteInfoByUserID] @userID INT
AS
BEGIN

DELETE 
FROM dbo.AccountsCategoriesRating
WHERE dbo.AccountsCategoriesRating.AccountID IN (SELECT ID FROM dbo.Accounts WHERE UserID = @userID)
	
DELETE
FROM dbo.Operations
WHERE dbo.Operations.Debet_ID IN (SELECT ID FROM dbo.Accounts WHERE UserID = @userID)
	
DELETE
FROM dbo.Operations
WHERE dbo.Operations.Credit_ID IN (SELECT ID FROM dbo.Accounts WHERE UserID = @userID)
	
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[DeleteOperation] 
  @operationID INT,
  @clientID INT = null
AS
BEGIN
	DECLARE @debetAccount INT, @creditAccount INT, @userID INT, @operDate DATETIME
	SET @debetAccount = (SELECT TOP 1 Debet_ID FROM dbo.Operations WHERE ID = @operationID)
	SET @creditAccount = (SELECT TOP 1 Credit_ID FROM dbo.Operations WHERE ID = @operationID)
	SET @operDate = (SELECT TOP 1 od.[Date] FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID WHERE op.ID = @operationID)
	
	DELETE dbo.Operations
	WHERE ID = @operationID
	
	IF (@debetAccount IS NOT NULL)
	BEGIN
		SET @userID = (SELECT TOP 1 a.UserID FROM dbo.Accounts AS a WHERE a.ID = @debetAccount)
		EXEC am_CloseAccOperDay @debetAccount, @operDate, NULL
	END
	IF (@creditAccount IS NOT NULL)
	BEGIN
		SET @userID = (SELECT TOP 1 a.UserID FROM dbo.Accounts AS a WHERE a.ID = @creditAccount)
		EXEC am_CloseAccOperDay @creditAccount, @operDate, NULL
	END
END
GO
/****** Object:  StoredProcedure [dbo].[DelImage]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[DelImage] @userID INT,
						    @operationID INT
AS
BEGIN
	DELETE
	FROM dbo.OperationsImages
	WHERE UserID = @userID AND OperationID = @operationID
END
GO
/****** Object:  StoredProcedure [dbo].[DoLogAction]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [dbo].[DoLogAction]
	@src nvarchar(150),
	@msg nvarchar(max)
as

	insert LogAction(src, msg) values(@src, @msg)
GO
/****** Object:  StoredProcedure [dbo].[DuplicateOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[DuplicateOperation] @operationID INT
AS
BEGIN
	DECLARE @userID INT, @type INT, @amount MONEY, @account1 INT, @account2 INT, @description NVARCHAR(MAX), @categoryID INT, @operDate DATETIME, @operDay_ID INT
	
	SELECT TOP 1 @description = o.[Description], @amount = o.Amount, @categoryID = o.Category_ID, @operDay_ID = o.OperDay_ID, @account1 = o.Debet_ID, @account2 = o.Credit_ID
	FROM dbo.Operations AS o
	WHERE ID = @operationID
	
	SET @userID = (SELECT TOP 1 a.UserID FROM dbo.Accounts AS a LEFT JOIN dbo.Operations AS do ON a.ID = do.Debet_ID LEFT JOIN dbo.Operations AS co ON a.ID = co.Credit_ID WHERE do.ID = @operationID OR co.ID = @operationID)
	SET @operDate = (SELECT TOP 1 [Date] FROM dbo.OperDay WHERE ID = @operDay_ID)
	
	IF (@account1 IS NULL AND @account2 IS NOT NULL)  --зачисление
		SET @type = 1
	ELSE
		SET @type = 2
	
	EXEC dbo.SetOperation @userID, @type, @amount, NULL, @account1, @account2, @description, @categoryID, @operDate
END
GO
/****** Object:  StoredProcedure [dbo].[EditOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[EditOperation]
	@id int,
	@description nvarchar(MAX),
	@amount money,
	@category int,
	@operDate DATE
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	DECLARE @operDay_ID INT
	IF EXISTS (SELECT ID FROM OperDay WHERE [Date] = @operDate)
		SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @operDate)
	ELSE
	BEGIN
		IF @operDate > @maxDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @maxDate)
		ELSE IF @operDate < @minDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @minDate)
	END

	UPDATE Operations
	SET Amount = @amount,[Description] = @description, Category_ID = @category, OperDay_ID = @operDay_ID
	WHERE ID = @id
END
GO
/****** Object:  StoredProcedure [dbo].[ExistOperationsInAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExistOperationsInAccount] @accountID INT
AS
BEGIN

IF EXISTS (SELECT *
		   FROM dbo.Operations AS op
		   LEFT OUTER JOIN dbo.Accounts AS ac1 ON op.Credit_ID = ac1.ID
		   LEFT OUTER JOIN dbo.Accounts AS ac2 ON op.Debet_ID = ac2.ID
		   WHERE (ac1.ID = @accountID OR ac2.ID = @accountID)
		   )
	SELECT 1  --есть операции
ELSE
	SELECT 2  --нет операций
	
END
GO
/****** Object:  StoredProcedure [dbo].[FillAccRests]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[FillAccRests] @minDate DATE,  --мин ID даты по операциям
							    @maxDate DATE,  --макс ID даты по операциям
							    @userID INT
AS
BEGIN
	DECLARE @restMinDate DATETIME, @restMaxDate DATETIME
	
	DECLARE @accountID INT
	
	DECLARE AccountsCursor CURSOR FOR
	SELECT a.ID
	FROM dbo.Accounts AS a
	WHERE a.UserID = @userID
	
	OPEN AccountsCursor

	FETCH NEXT FROM AccountsCursor
	INTO @accountID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS (SELECT * FROM dbo.Acc_Rests AS ar WHERE ar.Account_ID = @accountID)
			INSERT INTO dbo.Acc_Rests VALUES(@accountID, (SELECT TOP 1 od.ID FROM dbo.OperDay AS od WHERE od.[Date] = @minDate), 0, 0, 0)
	
		SET @restMinDate = (SELECT MIN(od.[Date]) FROM dbo.Acc_Rests AS ar JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID WHERE ar.Account_ID = @accountID)
		SET @restMaxDate = (SELECT MAX(od.[Date]) FROM dbo.Acc_Rests AS ar JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID WHERE ar.Account_ID = @accountID)
		
		IF(@restMinDate > @minDate) EXEC OperDayClose @accountID, @userID, @minDate, @restMinDate
		IF(@restMaxDate < @maxDate) EXEC OperDayClose @accountID, @userID, @restMaxDate, @maxDate
		
		FETCH NEXT FROM AccountsCursor
		INTO @accountID
	END
	
	CLOSE AccountsCursor
	DEALLOCATE AccountsCursor
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccount] @accountID INT
AS
BEGIN
	SELECT TOP 1 Name, Limit, [Plan], FirstDay, IsMinusAllowed
	FROM dbo.Accounts
	WHERE ID = @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountNameByID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountNameByID] @accountID INT
AS
BEGIN
	SELECT TOP 1 Name
	FROM dbo.Accounts
	WHERE ID = @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountsForExport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountsForExport] @userID INT
AS
BEGIN
	SELECT a.ID, a.Name, a.Rest, a.IsDeleted, a.UserID, a.[Order], a.Limit, a.[Plan], a.FirstDay
	FROM Accounts AS a
	WHERE a.IsDeleted = 0 AND a.UserID = @userID
	ORDER BY a.[Order] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountsList]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountsList] @userID INT
AS
BEGIN
	SELECT ID, Name
	FROM dbo.Accounts
	WHERE IsDeleted = 0 AND UserID = @userID
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountsListButCurrent]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountsListButCurrent] @accountID INT,
											 @userID INT
AS
BEGIN
	SELECT ID, Name
	FROM dbo.Accounts
	WHERE ID != @accountID AND IsDeleted = 0 AND UserID = @userID
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountsListForAdding]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountsListForAdding] 
  @accountID INT,
	@userID INT
AS
BEGIN

	declare @tempTable table(ID INT, Name NVARCHAR(MAX)) 

	INSERT INTO @tempTable(ID, Name)
	VALUES(-1, 'Без счета')

	INSERT INTO @tempTable(ID, Name)
	SELECT ID, Name	FROM Accounts
	WHERE ID != @accountID AND IsDeleted = 0 AND UserID = @userID
	
	SELECT ID, Name	FROM @tempTable
	

END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountTable]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountTable] 
	@beginDate DATE, 
	@endDate DATE,
	@userID INT
AS
BEGIN

	DECLARE 
	@parentID INT,
	@minDate DATETIME, 
	@maxDate DATETIME,
	@beginDateID INT,
	@endDateID INT

	select @ParentID = ParentID from Users where ID = @UserID 
	if @ParentID is not null
		begin -- Дата первой и последней операции клиента
			SELECT @minDate = MIN(od.[Date]), @maxDate = MAX(od.[Date]) 
			  FROM OperDay od JOIN Operations op ON od.ID = op.OperDay_ID 
			  LEFT OUTER JOIN Accounts aC ON op.Credit_ID = aC.ID 
			  LEFT OUTER JOIN Accounts aD ON op.Debet_ID = aD.ID 
			WHERE aC.UserID = @ParentID OR aD.UserID = @ParentID
		end
	else
		begin
			SELECT @minDate = MIN(od.[Date]), @maxDate = MAX(od.[Date]) 
			  FROM OperDay od JOIN Operations op ON od.ID = op.OperDay_ID 
			  LEFT OUTER JOIN Accounts aC ON op.Credit_ID = aC.ID 
			  LEFT OUTER JOIN Accounts aD ON op.Debet_ID = aD.ID 
			WHERE aC.UserID = @UserID OR aD.UserID = @UserID
		end

	-- Корректировка параметров под мин макс даты
	IF (@beginDate < @minDate) 
		SET @beginDate = @minDate	
	IF (@endDate > @maxDate) 
		SET @endDate = @maxDate
	
	-- Вместо дат дольше берем номера(не очень конечно)
	SELECT @beginDateID = ID FROM OperDay WHERE datediff(dd, [Date], @beginDate) = 0
	select @endDateID = ID FROM OperDay WHERE datediff(dd, [Date], @endDate) = 0

	-- Получаем список счетов с остатками на начальную и конечную даты и обороты за период
	-- если запрос от помощника, то смотрим на ограничения
	select ID, [Order], Name, Rest1, Rest2, Cred, Deb from 
	(select ID, [Order], Name, Rest1, Rest2+Cred-Deb Rest2, Cred, Deb, @beginDateID OperDay_ID1, @endDateID OperDay_ID2 
	 from 
		(SELECT a.ID 
			   ,a.[Order]
			   ,a.Name
			   ,ISNULL((select top 1 ar1.Rest 
						from Acc_Rests ar1 
						where a.ID = ar1.Account_ID AND ar1.OperDay_ID = @beginDateID), 0) Rest1
			   ,ISNULL((select top 1 ar1.Rest 
						from Acc_Rests ar1 
						where a.ID = ar1.Account_ID AND ar1.OperDay_ID = @endDateID), 0) Rest2
		from Accounts a
		where a.UserID = @userID and a.IsDeleted = 0) b
		left join (
			select Account_ID, SUM(Credit) Cred, SUM(Debet) Deb
			from Acc_Rests ar
			where Account_ID in (select ID from Accounts where UserID = @userID) 
				  and OperDay_ID between @beginDateID AND @endDateID 
			group by Account_ID
		) c on b.ID = c.Account_ID
	union
	select ID, [Order], Name, Rest1, Rest2, Cred, Deb, @beginDateID OperDay_ID1, @endDateID OperDay_ID2 
	from 
		(SELECT a2.ID 
			   ,a2.[Order]
			   ,a2.Name
			   ,ISNULL((select top 1 ar1.Rest 
						from Acc_Rests ar1 
						where a2.ID = ar1.Account_ID AND ar1.OperDay_ID = @beginDateID), 0) Rest1
			   ,ISNULL((select top 1 ar1.Rest 
						from Acc_Rests ar1 
						where a2.ID = ar1.Account_ID AND ar1.OperDay_ID = @endDateID), 0) Rest2
		from Accounts a2
		where a2.ID in (select aa.Account_ID from [AccountAssistant] aa where aa.Assistant_ID = @userID)  and a2.IsDeleted = 0) b2
		left join (
			select Account_ID, SUM(Credit) Cred, SUM(Debet) Deb
			from Acc_Rests ar
			where Account_ID in (select aa.Account_ID from [AccountAssistant] aa where aa.Assistant_ID = @userID)
				  and OperDay_ID between @beginDateID AND @endDateID 
			group by Account_ID
		) c2 on b2.ID = c2.Account_ID) e
	ORDER BY [Order] DESC

END

GO
/****** Object:  StoredProcedure [dbo].[GetAccountTableAM22]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetAccountTableAM22] 
	@beginDate DATE, 
	@endDate DATE,
	@userID INT
AS
BEGIN

	DECLARE 
	@parentID INT,
	@minDate DATE, 
	@maxDate DATE,
	@beginDateID INT,
	@endDateID INT

	select @parentID = ParentID from Users where ID = @UserID 
	if @parentID is not null set @userID = @parentID

	-- Вместо дат дальше используем ID (не очень конечно)
	exec GetOperDayIdByDateAM22 @beginDate, @beginDateID out
	exec GetOperDayIdByDateAM22 @endDate,   @endDateID out

	-- ********************2022-10-19*AM***
	-- TODO: Если @beginDate > даты последней операции по счету, 
	-- то входящий не будет учитывать операции в последний день
	-- ********************
	-- Получаем список счетов с остатками на начальную и конечную даты и обороты за период
	-- если запрос от помощника, то смотрим на ограничения
	select ID, [Order], Name, Rest1, Rest2, Cred, Deb from 
	(select b.ID, b.[Order], b.Name, b.Rest1, b.Rest2, 
			IsNull(c.Cred, 0) Cred, IsNull(c.Deb, 0) Deb--, @beginDateID OperDay_ID1, @endDateID OperDay_ID2 
	 from 
		(SELECT a.ID 
			   ,a.[Order]
			   ,a.Name
			   ,ISNULL((select top 1 ar.Rest -- TODO
						from Acc_Rests ar join OperDay d on d.ID=ar.OperDay_ID
						where a.ID = ar.Account_ID AND d.Date <= @beginDate order by d.Date desc), 0) Rest1
			   ,ISNULL((select top 1 ar.Rest+ar.Credit-ar.Debet 
						from Acc_Rests ar join OperDay d on d.ID=ar.OperDay_ID
		 				where a.ID = ar.Account_ID AND d.Date <= @endDate order by d.Date desc), 0) Rest2
		 from Accounts a
		 where a.UserID = @userID and a.IsDeleted = 0
		) b
		left join (
			select Account_ID, SUM(Credit) Cred, SUM(Debet) Deb
			from Acc_Rests ar join Accounts ac on ar.Account_ID=ac.ID join OperDay d on d.ID=ar.OperDay_ID
			where ac.UserID = @userID and d.Date between @beginDate AND @endDate 
			group by Account_ID
		) c on b.ID = c.Account_ID
	union
	select ID, [Order], Name, Rest1, Rest2, IsNull(Cred, 0) Cred, IsNull(Deb, 0) Deb--, @beginDateID OperDay_ID1, @endDateID OperDay_ID2 
	from 
		(SELECT a.ID 
			   ,a.[Order]
			   ,a.Name
			   ,ISNULL((select top 1 ar.Rest from Acc_Rests ar join OperDay d on d.ID=ar.OperDay_ID
						where a.ID = ar.Account_ID AND d.Date <= @beginDate order by d.Date desc), 0) Rest1
			   ,ISNULL((select top 1 ar.Rest+ar.Credit-ar.Debet from Acc_Rests ar join OperDay d on d.ID=ar.OperDay_ID
						where a.ID = ar.Account_ID AND d.Date <= @endDate order by d.Date desc), 0) Rest2
		from Accounts a
		where a.ID in (select aa.Account_ID from [AccountAssistant] aa where aa.Assistant_ID = @userID) and a.IsDeleted = 0) b2
		left join (
			select Account_ID, SUM(Credit) Cred, SUM(Debet) Deb
			from Acc_Rests ar join OperDay d on d.ID=ar.OperDay_ID
			where Account_ID in (select aa.Account_ID from [AccountAssistant] aa where aa.Assistant_ID = @userID)
				  and d.Date between @beginDate AND @endDate 
			group by Account_ID
		) c2 on b2.ID = c2.Account_ID) e
	ORDER BY [Order] DESC

END

GO
/****** Object:  StoredProcedure [dbo].[GetAllCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAllCategories] 
	@UserID INT
AS
BEGIN

	DECLARE @u int set @u = 0	
	select @u = ParentID from Users where ID = @UserID
	if @u > 0 set @UserID = @u

	SELECT c.ID, c.Name, --CreditRating Credit, DebetRating Debet
		dbo.GetCategoryCreditFunc(c.ID) Credit, dbo.GetCategoryDebetFunc(c.ID) Debet
	FROM dbo.Categories AS c
	WHERE c.UserID = @UserID
	ORDER BY c.Name ASC
END

GO
/****** Object:  StoredProcedure [dbo].[GetAllHints]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAllHints]
AS
BEGIN
	SELECT ID, Header, Content, Alias
	FROM dbo.Hints
END
GO
/****** Object:  StoredProcedure [dbo].[GetBalanceAccountsReport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetBalanceAccountsReport] @beginDate DATE, 
										    @endDate DATE,
										    @userID INT
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	IF ((@beginDate IS NULL) OR (@beginDate < @minDate))
		SET @beginDate = @minDate
	IF ((@endDate IS NULL) OR (@endDate > @maxDate))
		SET @endDate = @maxDate
		
	DECLARE @minDateID INT
	DECLARE @maxDateID INT
	SET @minDateID = (SELECT TOP 1 ID FROM dbo.OperDay WHERE [Date] = @beginDate)
	SET @maxDateID = (SELECT TOP 1 ID FROM dbo.OperDay WHERE [Date] = @endDate)
	
	CREATE TABLE #tempTable(Number INT, IsAccount BIT, Name NVARCHAR(MAX), Credit MONEY, Debet MONEY, Rest MONEY)
	DECLARE @counter INT SET @counter = 0
	
	DECLARE @accountID INT

	DECLARE AccountsCursor CURSOR FOR
	SELECT ac.ID
	FROM dbo.Accounts AS ac
	WHERE ac.UserID = @userID AND ac.IsDeleted = 0
	
	OPEN AccountsCursor

	FETCH NEXT FROM AccountsCursor
	INTO @accountID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @counter = @counter + 1
		
		--Вставка информации по счету
		INSERT INTO #tempTable(Number, IsAccount, Name, Credit, Debet, Rest)
		SELECT @counter AS Number, 1 AS IsAccount, v.Name, SUM(v.Credit) AS Credit, SUM(v.Debet) AS Debet, SUM(v.Credit)-SUM(v.Debet) AS Rest
		FROM 
		(
		 (SELECT a.Name, opC.Amount AS Credit, 0 AS Debet
		  FROM dbo.Accounts AS a
		  JOIN dbo.Operations AS opC ON opC.Credit_ID = a.ID
		  WHERE a.ID = @accountID AND (opC.OperDay_ID BETWEEN @minDateID AND @maxDateID))
		 UNION ALL
		 (SELECT a.Name, 0 AS Credit, opD.Amount AS Debet
		  FROM dbo.Accounts AS a
		  JOIN dbo.Operations AS opD ON opD.Debet_ID = a.ID
		  WHERE a.ID = @accountID AND (opD.OperDay_ID BETWEEN @minDateID AND @maxDateID))
		) v
		GROUP BY v.Name
			
		--Вставка информации по категориям
		DECLARE @categoryID INT

		DECLARE CategoriesCursor CURSOR FOR
		SELECT DISTINCT c.ID
		FROM dbo.Categories AS c
		JOIN dbo.Operations AS o ON o.Category_ID = c.ID
		WHERE (o.Credit_ID = @accountID OR o.Debet_ID = @accountID) AND (o.OperDay_ID BETWEEN @minDateID AND @maxDateID)
		
		OPEN CategoriesCursor
		FETCH NEXT FROM CategoriesCursor
		INTO @categoryID
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @counter = @counter + 1
				
			INSERT INTO #tempTable(Number, IsAccount, Name, Credit, Debet, Rest)
			SELECT @counter AS Number, 0 AS IsAccount, v.Name, SUM(v.Credit) AS Credit, SUM(v.Debet) AS Debet, SUM(v.Credit)-SUM(v.Debet) AS Rest
			FROM
			(
			 (SELECT c.Name, opC.Amount AS Credit, 0 AS Debet
			  FROM Accounts AS a
			  JOIN dbo.Operations AS opC ON opC.Credit_ID = a.ID
			  JOIN dbo.Categories AS c ON opC.Category_ID = c.ID
			  WHERE a.ID = @accountID AND c.ID = @categoryID AND (opC.OperDay_ID BETWEEN @minDateID AND @maxDateID) AND a.IsDeleted = 0)
			  UNION ALL
			 (SELECT c.Name, 0 AS Credit, opD.Amount AS Debet
			  FROM Accounts AS a
			  JOIN dbo.Operations AS opD ON opD.Debet_ID = a.ID
			  JOIN dbo.Categories AS c ON opD.Category_ID = c.ID
			  WHERE a.ID = @accountID AND c.ID = @categoryID AND (opD.OperDay_ID BETWEEN @minDateID AND @maxDateID) AND a.IsDeleted = 0)
			) v
			GROUP BY v.Name
			
			FETCH NEXT FROM CategoriesCursor
			INTO @categoryID
		END
		CLOSE CategoriesCursor
		DEALLOCATE CategoriesCursor
		
		FETCH NEXT FROM AccountsCursor
		INTO @accountID
	END
	
	CLOSE AccountsCursor
	DEALLOCATE AccountsCursor
	
	SELECT 
	  Number, IsAccount, Name, 
	  CONVERT(decimal(9,2), Credit) AS Credit, CONVERT(decimal(9,2), Debet) AS Debet, 
	  CONVERT(decimal(9,2), Rest) AS Rest
	FROM #tempTable

	DROP TABLE #tempTable
END
GO
/****** Object:  StoredProcedure [dbo].[GetBalanceCategoriesReport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetBalanceCategoriesReport] 
/*
	Возвращает статистику движения среддств сгруппированную по категориям.
	Оригинальная версия возвращает сразу все категории, новая, доработанная 24.02.2015 АМ,
	может получать @CategoryID и позволяет вынести цикл по категориям из процедуры в код.
*/
	@beginDate DATE, 
	@endDate DATE,
	@userID INT,
	@CategoryID INT = NULL
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	IF ((@beginDate IS NULL) OR (@beginDate < @minDate))	SET @beginDate = @minDate
	IF ((@endDate IS NULL) OR (@endDate > @maxDate))		SET @endDate = @maxDate
		
	DECLARE @minDateID INT SET @minDateID = (SELECT TOP 1 ID FROM dbo.OperDay WHERE [Date] = @beginDate)
	DECLARE @maxDateID INT SET @maxDateID = (SELECT TOP 1 ID FROM dbo.OperDay WHERE [Date] = @endDate)
	
	CREATE TABLE ##tempTable(Number INT, IsCategory BIT, Name NVARCHAR(MAX), Credit MONEY, Debet MONEY, Rest MONEY)
	
	DECLARE @catID INT

	DECLARE CategoriesCursor CURSOR FOR
	SELECT DISTINCT c.ID
	FROM dbo.Categories AS c
	JOIN dbo.Operations AS o ON o.Category_ID = c.ID
	WHERE c.UserID = @userID AND (c.ID = @CategoryID OR @CategoryID is NULL) AND NOT c.Name = 'Перевод'
	
	OPEN CategoriesCursor

	FETCH NEXT FROM CategoriesCursor
	INTO @catID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		--Вставка информации по категории
		INSERT INTO ##tempTable(Number, IsCategory, Name, Credit, Debet, Rest)
		SELECT COUNT(*) AS Number, 1 AS IsCategory, v.Name, SUM(v.Credit) AS Credit, SUM(v.Debet) AS Debet, SUM(v.Credit) - SUM(v.Debet) AS Rest
		FROM 
		(
		 (SELECT c.Name, opC.Amount AS Credit, 0 AS Debet
		  FROM Accounts AS a
		  JOIN dbo.Operations AS opC ON opC.Credit_ID = a.ID
		  JOIN dbo.Categories AS c ON opC.Category_ID = c.ID
		  WHERE c.ID = @catID AND c.UserID = @userID AND (opC.OperDay_ID BETWEEN @minDateID AND @maxDateID) AND a.IsDeleted = 0)
		 UNION ALL
		 (SELECT c.Name, 0 AS Credit, opD.Amount AS Debet
		  FROM Accounts AS a
		  JOIN dbo.Operations AS opD ON opD.Debet_ID = a.ID
		  JOIN dbo.Categories AS c ON opD.Category_ID = c.ID
		  WHERE c.ID = @catID AND c.UserID = @userID AND (opD.OperDay_ID BETWEEN @minDateID AND @maxDateID) AND a.IsDeleted = 0)
		) v
		GROUP BY v.Name
		
		FETCH NEXT FROM CategoriesCursor
		INTO @catID

	END
	
	CLOSE CategoriesCursor
	DEALLOCATE CategoriesCursor

	SELECT 
		Number, IsCategory, Name, 
		CONVERT(decimal(9,2), Credit) AS Credit, CONVERT(decimal(9,2), Debet) AS Debet, CONVERT(decimal(9,2), Rest) AS Rest
	FROM ##tempTable
	
	DROP TABLE ##tempTable
END

GO
/****** Object:  StoredProcedure [dbo].[GetBalanceReport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetBalanceReport] @beginDate DATE, 
									@endDate DATE,
									@userID INT
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	IF ((@beginDate IS NULL) OR (@beginDate < @minDate))
		SET @beginDate = @minDate
	IF ((@endDate IS NULL) OR (@endDate > @maxDate))
		SET @endDate = @maxDate
		
	DECLARE @minDateID INT
	DECLARE @maxDateID INT
	SET @minDateID = (SELECT TOP 1 ID FROM dbo.OperDay WHERE [Date] = @beginDate)
	SET @maxDateID = (SELECT TOP 1 ID FROM dbo.OperDay WHERE [Date] = @endDate)
	
	SELECT Account, Category, SUM(Credit) AS Credit, SUM(Debet) AS Debet, SUM(Credit) - SUM(Debet) AS Rest
	FROM
	(
	  (SELECT acD.Name AS Account, cat.Name As Category, 0 AS Credit, SUM(op.Amount) AS Debet
	    FROM dbo.Operations AS op
	    JOIN dbo.Accounts AS acD ON acD.ID = op.Debet_ID
	    JOIN dbo.Categories AS cat ON cat.ID = op.Category_ID
	    WHERE OperDay_ID >= @minDateID AND OperDay_ID <=@maxDateID AND acD.UserID = @userID
	    GROUP BY acD.Name, cat.Name)
	  UNION
	  (SELECT acC.Name AS Account, cat.Name As Category, SUM(op.Amount) AS Credit, 0 AS Debet
	    FROM dbo.Operations AS op
	    JOIN dbo.Accounts AS acC ON acC.ID = op.Credit_ID
	    JOIN dbo.Categories AS cat ON cat.ID = op.Category_ID
	    WHERE OperDay_ID >= @minDateID AND OperDay_ID <=@maxDateID AND acC.UserID = @userID
	    GROUP BY acC.Name, cat.Name)
	) AS bal
	GROUP BY Account, Category


	
	
END
GO
/****** Object:  StoredProcedure [dbo].[GetBanners]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetBanners] 
  @userID INT = null 
AS
BEGIN
	SELECT ID, IsPicture, Content, Link, Url
	FROM dbo.Banners
END
GO
/****** Object:  StoredProcedure [dbo].[GetBannersWithAuthorization]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetBannersWithAuthorization] @userID INT
AS
BEGIN
	SELECT ID, IsPicture, Content, Link,
	Url =
		CASE
			WHEN Url = 'http://www.ultrazoom.ru/ProductsDescription/Budget' THEN Url+'?login='+(SELECT TOP 1 [Login] FROM dbo.Users WHERE ID=@userID)+'&pass='+(SELECT TOP 1 [Password] FROM dbo.Users WHERE ID=@userID)
			ELSE Url
		END
	FROM dbo.Banners

END
GO
/****** Object:  StoredProcedure [dbo].[GetBudgetAccounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetBudgetAccounts] @userID INT
AS
BEGIN
	SELECT a.ID, a.Name, dbo.GetAccountCreditFunc(a.ID) AS Credit, dbo.GetAccountDebetFunc(a.ID) AS Debet
	FROM dbo.Accounts AS a
	WHERE a.UserID = @userID AND a.IsDeleted = 0
	ORDER BY a.Name ASC
END
GO
/****** Object:  StoredProcedure [dbo].[GetCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCategories] @userID INT,    --категория для конкретного пользователя
									  @credit BIT,    --рейтинг по категориям зачисления и списания
									  @transfer BIT,  --рейтинг по переводу со счета на счет
									  @accountID INT  --рейтинг для конкретного счета
AS
BEGIN

	DECLARE @u int set @u = 0	
	select @u = ParentID from Users where ID = @UserID
	if @u > 0 set @UserID = @u

	IF (@transfer = 1)
	BEGIN
		SELECT c.ID, c.Name, dbo.GetCategoryCreditFunc(c.ID) AS Credit, dbo.GetCategoryDebetFunc(c.ID) AS Debet
		FROM dbo.Categories AS c
		LEFT JOIN dbo.AccountsCategoriesRating AS acR ON c.ID = acR.CategoryID AND acR.AccountID = @accountID
		WHERE c.UserID = @userID
		ORDER BY c.TransferRating DESC, acR.Rating DESC
	END
	ELSE IF (@credit = 1)
	BEGIN
		SELECT c.ID, c.Name, dbo.GetCategoryCreditFunc(c.ID) AS Credit, dbo.GetCategoryDebetFunc(c.ID) AS Debet
		FROM dbo.Categories AS c
		LEFT JOIN dbo.AccountsCategoriesRating AS acR ON c.ID = acR.CategoryID AND acR.AccountID = @accountID
		WHERE c.UserID = @userID
		ORDER BY c.CreditRating DESC, acR.Rating DESC
	END
	ELSE IF (@credit = 0)
	BEGIN
		SELECT c.ID, c.Name, dbo.GetCategoryCreditFunc(c.ID) AS Credit, dbo.GetCategoryDebetFunc(c.ID) AS Debet
		FROM dbo.Categories AS c
		LEFT JOIN dbo.AccountsCategoriesRating AS acR ON c.ID = acR.CategoryID AND acR.AccountID = @accountID
		WHERE c.UserID = @userID
		ORDER BY c.DebetRating DESC, acR.Rating DESC
	END
END

GO
/****** Object:  StoredProcedure [dbo].[GetCategoriesForExport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCategoriesForExport] @userID INT
AS
BEGIN
	SELECT c.ID, c.Name, c.CreditRating, c.DebetRating, c.TransferRating, c.UserID, c.Limit, c.[Plan], c.FirstDay
	FROM dbo.Categories AS c
	WHERE c.UserID = @userID
	ORDER BY c.Name ASC
END
GO
/****** Object:  StoredProcedure [dbo].[GetCategory]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCategory] @categoryID INT
AS
BEGIN
	SELECT TOP 1 c.ID, c.Name, c.Limit, c.[Plan], c.FirstDay
	FROM dbo.Categories AS c
	WHERE c.ID = @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[GetCategoryCredit]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCategoryCredit] @categoryID INT
AS
BEGIN
	DECLARE @firstDay INT SET @firstDay = (SELECT TOP 1 FirstDay FROM dbo.Categories WHERE ID = @categoryID)
	IF @firstDay IS NULL SET @firstDay = 1
	
	DECLARE @todayYear INT SET @todayYear = YEAR(GETDATE())
	DECLARE @todayMonth INT SET @todayMonth = MONTH(GETDATE())
	DECLARE @todayDay INT SET @todayDay = DAY(GETDATE())
	
	DECLARE @stDate DATETIME, @finDate DATETIME --начало и конец расчетного периода
	
	IF @todayDay >= @firstDay
	BEGIN
		SET @stDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @todayDay - 1)
	END
	ELSE
	BEGIN
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)  --текущего месяца
		SET @stDate =  DATEADD(month, -1, @finDate) --предыдущего месяца
	END
	
	DECLARE @realAmount MONEY
	SET @realAmount = (SELECT SUM(o.Amount)
					   FROM dbo.Operations AS o
					   JOIN dbo.OperDay AS od ON o.OperDay_ID = od.ID
					   WHERE (Category_ID = @categoryID) AND (Credit_ID IS NOT NULL) AND (od.[Date] BETWEEN @stDate AND @finDate))
	IF @realAmount IS NULL SET @realAmount = 0
	DECLARE @planAmount MONEY SET @planAmount = (SELECT TOP 1 [Plan] FROM dbo.Categories WHERE ID = @categoryID)
	IF @planAmount IS NULL SET @planAmount = -1
	
	DECLARE @result FLOAT SET @result = (@realAmount/@planAmount * 100)
	IF (@result < 0) SET @result = NULL
	SELECT @result
END
GO
/****** Object:  StoredProcedure [dbo].[GetCategoryDebet]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCategoryDebet] @categoryID INT
AS
BEGIN
	DECLARE @firstDay INT SET @firstDay = (SELECT TOP 1 FirstDay FROM dbo.Categories WHERE ID = @categoryID)
	IF @firstDay IS NULL SET @firstDay = 1
	
	DECLARE @todayYear INT SET @todayYear = YEAR(GETDATE())
	DECLARE @todayMonth INT SET @todayMonth = MONTH(GETDATE())
	DECLARE @todayDay INT SET @todayDay = DAY(GETDATE())
	
	DECLARE @stDate DATETIME, @finDate DATETIME --начало и конец расчетного периода
	
	IF @todayDay >= @firstDay
	BEGIN
		SET @stDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @todayDay - 1)
	END
	ELSE
	BEGIN
		SET @finDate = DATEADD(mm, (@todayYear-1900) * 12 + @todayMonth - 1 , @firstDay - 1)  --текущего месяца
		SET @stDate =  DATEADD(month, -1, @finDate) --предыдущего месяца
	END
	
	DECLARE @realAmount MONEY
	SET @realAmount = (SELECT SUM(o.Amount)
					   FROM dbo.Operations AS o
					   JOIN dbo.OperDay AS od ON o.OperDay_ID = od.ID
					   WHERE (Category_ID = @categoryID) AND (Debet_ID IS NOT NULL) AND (od.[Date] BETWEEN @stDate AND @finDate))
	IF @realAmount IS NULL SET @realAmount = 0
	DECLARE @limitAmount MONEY SET @limitAmount = (SELECT TOP 1 [Limit] FROM dbo.Categories WHERE ID = @categoryID)
	IF @limitAmount IS NULL SET @limitAmount = -1
	
	DECLARE @result FLOAT SET @result = (@realAmount/@limitAmount * 100)
	IF (@result < 0) SET @result = NULL
	SELECT @result
END
GO
/****** Object:  StoredProcedure [dbo].[GetCategoryNameByID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetCategoryNameByID] @categoryID INT
AS
BEGIN
	SELECT Name
	FROM dbo.Categories
	WHERE ID = @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[GetComments]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetComments]
AS
BEGIN

SELECT c.ID, ct.[Type], c.Comment, c.Answer, u.ID, c.dtc, c.IsVisible
FROM dbo.Comments AS c
JOIN dbo.Users AS u ON c.UserID = u.ID
JOIN dbo.CommentTypes AS ct ON c.TypeID = ct.ID
WHERE IsVisible = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetCommentTypes]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCommentTypes]
AS
BEGIN

SELECT ID, [Type]
FROM dbo.CommentTypes

END
GO
/****** Object:  StoredProcedure [dbo].[GetCreditTypeByOperationID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetCreditTypeByOperationID] @operationID INT,
										      @accountID INT
AS
BEGIN
	SELECT Credit = 
	CASE
		WHEN o.Debet_ID = @accountID THEN 'False'
		WHEN o.Credit_ID = @accountID THEN 'True'
	END
	FROM dbo.Operations AS o
	WHERE o.ID = @operationID
END
GO
/****** Object:  StoredProcedure [dbo].[GetEmail]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[GetEmail] @userID INT
AS
BEGIN

SELECT TOP 1 Email
FROM dbo.Users
WHERE ID = @userID
	
END
GO
/****** Object:  StoredProcedure [dbo].[GetHelperAccounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetHelperAccounts] 
	@AccountOwnerID int,
	@AssistantID int
AS
BEGIN

	insert LogAction(src, msg) values('[GetHelperAccounts]', ' AccountOwnerID - ' + STR(@AccountOwnerID))
	
	SELECT [ID]
		  ,[Name]
		  ,(select count(*) from [AccountAssistant] aa where aa.Account_ID = a.ID and aa.Assistant_ID = @AssistantID) Available
	  FROM [dbo].[Accounts] a
	  where a.UserID = @AccountOwnerID and a.IsDeleted = 0

END
GO
/****** Object:  StoredProcedure [dbo].[GetHelpers]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetHelpers] 
	@userId int					   
AS
BEGIN

	insert LogAction(src, msg) values('[GetHelpers]', ' User - ' + STR(@UserID))
	
	SELECT [ID]
		  ,[Name]
		  ,[Email]
	  FROM [dbo].[Users]
	  where [ParentID] = @userId

END
GO
/****** Object:  StoredProcedure [dbo].[GetHintByAlias]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetHintByAlias] @alias NVARCHAR(128)
AS
BEGIN
	SELECT TOP 1 ID, Header, Content, Alias
	FROM dbo.Hints
	WHERE Alias = @alias
END
GO
/****** Object:  StoredProcedure [dbo].[GetImage]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetImage] @userID INT,
						    @operationID INT
AS
BEGIN
	SELECT TOP 1 [Image], [FileName]
	FROM dbo.OperationsImages
	WHERE UserID = @userID AND OperationID = @operationID
END
GO
/****** Object:  StoredProcedure [dbo].[GetMaxOperDay]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetMaxOperDay] @userID INT
AS
BEGIN
	SELECT MAX(od.[Date])
	FROM dbo.OperDay AS od
	JOIN dbo.Operations AS op ON od.ID = op.OperDay_ID
	LEFT JOIN dbo.Accounts AS aC ON op.Credit_ID = aC.ID
	LEFT JOIN dbo.Accounts AS aD ON op.Debet_ID = aD.ID
	WHERE aC.UserID = @userID OR aD.UserID = @userID
END
GO
/****** Object:  StoredProcedure [dbo].[GetMinOperDay]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetMinOperDay] @userID INT
AS
BEGIN
	SELECT MIN(od.[Date])
	FROM dbo.OperDay AS od
	JOIN dbo.Operations AS op ON od.ID = op.OperDay_ID
	LEFT JOIN dbo.Accounts AS aC ON op.Credit_ID = aC.ID
	LEFT JOIN dbo.Accounts AS aD ON op.Debet_ID = aD.ID
	WHERE aC.UserID = @userID OR aD.UserID = @userID
END
GO
/****** Object:  StoredProcedure [dbo].[GetMyCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetMyCategories] @userID INT
AS
BEGIN
	SELECT ID, Name
	FROM dbo.Categories
	WHERE UserID = @userID
END
GO
/****** Object:  StoredProcedure [dbo].[GetOperationByID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetOperationByID] @operationID INT
AS
BEGIN
	SELECT TOP 1 ID, [Description], Amount, Category_ID, Debet_ID, Credit_ID, OperDay_ID, DateCreate, DateEdit, [Status]
	FROM Operations
	WHERE ID = @operationID
END
GO
/****** Object:  StoredProcedure [dbo].[GetOperationsForExport]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetOperationsForExport] @accountID INT
AS
BEGIN
	SELECT o.ID, o.[Description], o.Amount, o.Category_ID, o.Debet_ID, o.Credit_ID, o.OperDay_ID, o.DateCreate, o.DateEdit, o.[Status]
	FROM Operations AS o
	WHERE o.Credit_ID = @accountID OR o.Debet_ID = @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[GetOperationStatuses]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetOperationStatuses]
AS
BEGIN

SELECT ID, Name
FROM Operation_Statuses
	
END


GO
/****** Object:  StoredProcedure [dbo].[GetOperationTable]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetOperationTable] 
	@beginDate DATE,
	@endDate DATE,
	@accountID INT
AS
BEGIN
	--Обновить рейтинг (Order) для аккаунта
	DECLARE @curOrder INT
	SET @curOrder = (SELECT TOP 1 a.[Order] FROM dbo.Accounts AS a WHERE a.ID = @accountID)
	
	UPDATE dbo.Accounts
	SET [Order] = @curOrder + 1
	WHERE ID = @accountID
	
	DECLARE @beginDateID INT SELECT TOP 1 @beginDateID = ID FROM OperDay WHERE DATEDIFF(DD, @beginDate, Date) >= 0 ORDER BY ID
	DECLARE @endDateID INT SELECT TOP 1 @endDateID = ID FROM OperDay WHERE DATEDIFF(DD, Date, @endDate) >= 0 ORDER BY ID DESC

	SELECT	o.ID,
			SecAccount = 
				CASE
					WHEN o.Debet_ID = @accountID AND o.Credit_ID != @accountID THEN a1.Name
					WHEN o.Debet_ID != @accountID AND o.Credit_ID = @accountID THEN a2.Name
					ELSE NULL
				END,
			c.Name as Category, d.[Date] as OperDay,
			Debet =
				CASE
					WHEN o.Debet_ID = @accountID THEN -1*o.Amount
					ELSE NULL
				END,
			Credit =
				CASE
					WHEN o.Credit_ID = @accountID THEN o.Amount
					ELSE NULL
				END,
			o.Amount, o.Credit_ID, o.Debet_ID, o.OperDay_ID, o.[Status], o.[Description], 
			--CONVERT(NVARCHAR(55),o.DateCreate,13) as DateCreate
			o.DateCreate,
			o.UserName, CASE WHEN IsNull(oi.ID, 0) > 0 THEN 1 ELSE 0 END HasImage
	FROM Operations o 
	LEFT OUTER JOIN OperationsImages oi on oi.OperationID = o.ID			
	LEFT OUTER JOIN Categories c ON c.ID = o.Category_ID
	LEFT OUTER JOIN Accounts a1 ON a1.ID = o.Credit_ID
	LEFT OUTER JOIN Accounts a2 ON a2.ID = o.Debet_ID
	JOIN OperDay d ON d.ID = o.OperDay_ID
	WHERE OperDay_ID <= @endDateID AND OperDay_ID >= @beginDateID AND (Credit_ID = @accountID OR Debet_ID = @accountID) 
	
	ORDER BY d.Date desc, o.ID desc

END
GO
/****** Object:  StoredProcedure [dbo].[GetOperationTableByUserID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetOperationTableByUserID] @beginDate DATE,
											 @endDate DATE,
											 @userID INT
AS
BEGIN
	DECLARE @beginDateID INT, @endDateID INT
	SET @beginDateID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @beginDate)
	SET @endDateID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @endDate)

	SELECT	o.ID,
			FirstAccount = (SELECT TOP 1 Name FROM dbo.Accounts WHERE ID = a.ID),
			SecAccount = 
				CASE
					WHEN o.Debet_ID = a.ID AND o.Credit_ID != a.ID THEN a1.Name
					WHEN o.Debet_ID != a.ID AND o.Credit_ID = a.ID THEN a2.Name
					ELSE NULL
				END,
			c.Name as Category, d.[Date] as OperDay,
			Debet =
				CASE
					WHEN o.Debet_ID = a.ID THEN -1*o.Amount
					ELSE NULL
				END,
			Credit =
				CASE
					WHEN o.Credit_ID = a.ID THEN o.Amount
					ELSE NULL
				END,
			o.Amount, o.OperDay_ID, o.[Status], o.[Description], CONVERT(NVARCHAR(20),o.DateCreate,13) as DateCreate
			FROM Operations o 
	LEFT OUTER JOIN Categories c ON c.ID = o.Category_ID
	LEFT OUTER JOIN Accounts a1 ON a1.ID = o.Credit_ID
	LEFT OUTER JOIN Accounts a2 ON a2.ID = o.Debet_ID
	JOIN OperDay d ON d.ID = o.OperDay_ID
	JOIN dbo.Accounts AS a ON a.UserID = @userID
	WHERE OperDay_ID <= @endDateID AND OperDay_ID >= @beginDateID AND (Credit_ID = a.ID OR Debet_ID = a.ID) ORDER BY ID desc
END
GO
/****** Object:  StoredProcedure [dbo].[GetOperDayDateByID]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetOperDayDateByID] @operDayID INT
AS
BEGIN
	SELECT [Date]
	FROM OperDay
	WHERE ID = @operDayID
END
GO
/****** Object:  StoredProcedure [dbo].[GetOperDayIdByDate]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetOperDayIdByDate] @operDayDate DATE
AS
BEGIN
	SELECT ID
	FROM dbo.OperDay
	WHERE Date = @operDayDate
END
GO
/****** Object:  StoredProcedure [dbo].[GetOperDayIdByDateAM22]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetOperDayIdByDateAM22]
@date date,
@OperDateID int output
as
begin
	declare @lastDate date

	select top 1 @lastDate = d.Date from OperDay d order by d.Date desc
	while @lastDate < @date begin
		set @lastDate = DATEADD(day, 1, @lastDate)
		insert OperDay(Date) values(@lastDate)
	end
	select @OperDateID = d.ID from OperDay d WHERE datediff(dd, d.[Date], @date) = 0
end
GO
/****** Object:  StoredProcedure [dbo].[GetPictureLinks]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetPictureLinks]
AS
BEGIN

SELECT TOP 1 ID, Link, Url
FROM dbo.Banners
WHERE ID = 1

END
GO
/****** Object:  StoredProcedure [dbo].[GetPossibleAccounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[GetPossibleAccounts] @userID INT
AS
BEGIN
	(
	SELECT Name
	FROM dbo.PredefinedAccounts
	)
	EXCEPT
	(
	SELECT Name
	FROM dbo.Accounts
	WHERE UserID = @userID AND IsDeleted = 0
	)
	ORDER BY Name ASC
END
GO
/****** Object:  StoredProcedure [dbo].[GetPossibleCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[GetPossibleCategories] @userID INT
AS
BEGIN
	(
	SELECT Name
	FROM dbo.PredefinedCategories
	)
	EXCEPT
	(
	SELECT Name
	FROM dbo.Categories
	WHERE USerID = @userID
	)
	ORDER BY Name ASC
END
GO
/****** Object:  StoredProcedure [dbo].[GetRating]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetRating] @userID INT
AS
BEGIN

	SELECT Rating
	FROM dbo.Users
	WHERE ID = @userID

END
GO
/****** Object:  StoredProcedure [dbo].[GetSecondAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetSecondAccount] @operationID INT,
									      @credit BIT
AS
BEGIN
	SELECT SecondAccount =
	CASE
		WHEN @credit = 0 THEN Credit_ID
		WHEN @credit = 1 THEN Debet_ID
		ELSE NULL
	END
	FROM dbo.Operations AS op
	WHERE op.ID = @operationID
END
GO
/****** Object:  StoredProcedure [dbo].[GetSetting]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[GetSetting] @key NVARCHAR(128)
AS
BEGIN
	SELECT TOP 1 Value
	FROM dbo.Settings
	WHERE [Key] = @key
END
GO
/****** Object:  StoredProcedure [dbo].[GetUserByAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetUserByAccount] @accountID INT
AS
BEGIN
	SELECT TOP 1 UserID
	FROM dbo.Accounts
	WHERE ID = @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[GetUserIdByEmail]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetUserIdByEmail] @Email nvarchar(50)
AS
BEGIN
	DECLARE @userId INT

	SET @userId = (SELECT TOP 1 [ID] FROM [dbo].[Users] WHERE [Login] = @Email OR [Email] = @Email)
	IF (@userId IS NULL)
		SELECT -1
	ELSE
		SELECT @userId
END
GO
/****** Object:  StoredProcedure [dbo].[GetUserName]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetUserName] @userID INT
AS
BEGIN
	SELECT TOP 1 [Login], *
	FROM dbo.Users
	WHERE ID = @userID
END

GO
/****** Object:  StoredProcedure [dbo].[HasOperationImage]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[HasOperationImage] @userID INT,
									  @operationID INT
AS
BEGIN
	IF EXISTS (SELECT TOP 1 ID FROM dbo.OperationsImages WHERE UserID = @userID AND OperationID = @operationID)
		SELECT 1
	ELSE
		SELECT 0
END
GO
/****** Object:  StoredProcedure [dbo].[HintCheckNoAccounts]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[HintCheckNoAccounts] @userID INT
AS
BEGIN
	IF EXISTS (SELECT TOP 1 ID FROM dbo.Accounts WHERE UserID = @userID AND IsDeleted = 0)
		SELECT 0  --у пользователя есть аккаунты
	ELSE
		SELECT 1  --у пользователя пока нет ни одного аккаунта
END
GO
/****** Object:  StoredProcedure [dbo].[HintCheckNoCategories]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[HintCheckNoCategories] @userID INT
AS
BEGIN
	IF EXISTS (SELECT ID
			   FROM dbo.Categories
			   WHERE UserID = @userID)
		SELECT 0  --у пользователя есть заведенные категории
	ELSE
		SELECT 1  --у пользователя пока нет заведенных категорий
END
GO
/****** Object:  StoredProcedure [dbo].[HintCheckNoOperations]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[HintCheckNoOperations] @userID INT
AS
BEGIN
	IF EXISTS (SELECT o.ID
			   FROM dbo.Operations AS o
			   LEFT JOIN dbo.Accounts AS acC ON o.Credit_ID = acC.ID
			   LEFT JOIN dbo.Accounts AS acD ON o.Debet_ID = acD.ID
			   WHERE (acC.UserID = @userID AND acC.IsDeleted = 0) OR (acD.UserID = @userID AND acD.IsDeleted = 0)
			  )
		SELECT 0  --у пользователя есть операции
	ELSE
		SELECT 1  --у пользователя пока нет операций
END
GO
/****** Object:  StoredProcedure [dbo].[IncRating]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[IncRating] @userID INT,
							  @inc INT
AS
BEGIN
	UPDATE dbo.Users
	SET Rating = Rating + @inc
	WHERE ID = @userID
END
GO
/****** Object:  StoredProcedure [dbo].[IsLoginFree]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[IsLoginFree] @login nvarchar(64)
AS
BEGIN
	IF NOT EXISTS(SELECT * FROM dbo.Users WHERE [Login] = @login)
		SELECT 1
	ELSE
		SELECT 0
END
GO
/****** Object:  StoredProcedure [dbo].[OperDayClose]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[OperDayClose] 
	@accountID INT,
	@userID INT = null, -- не нужен !!!
	@beginDate DATE,
	@endDate DATE
AS
BEGIN

  exec am_CloseAccOperDay @accountID, @beginDate, @endDate
  return
  
  -- ЗАБЛОКИРОВАНО!!! процедура не правильная

	DECLARE @minDate DATETIME, @maxDate DATETIME
	SELECT @minDate = MIN(od.[Date]), @maxDate = MAX(od.[Date]) FROM OperDay od 
	JOIN dbo.Operations op ON od.ID = op.OperDay_ID 
	JOIN dbo.Accounts a ON op.Credit_ID = a.ID or op.Debet_ID = a.ID 
	WHERE a.ID = @accountID
	
	IF ((@beginDate IS NULL) OR (@beginDate < @minDate))
		SET @beginDate = @minDate
	IF (@endDate IS NULL)
		SET @endDate = @maxDate
		
	--Удаляем из таблицы Acc_Rests все записи меньше minDate и больше maxDate-----------
	/*------------------------------------------------------------------------------------
	DELETE ar
	FROM dbo.Acc_Rests ar
	JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID
	WHERE (od.[Date] < @minDate OR od.[Date] > @maxDate) AND ar.Account_ID = @accountID
	------------------------------------------------------------------------------------*/
	
	------------------------------------------
	--Обновить таблицу Acc_Rests--------------
	DECLARE @operDayID INT
	
	DECLARE ODCursor CURSOR FOR
	SELECT od.ID
	FROM dbo.OperDay AS od
	WHERE od.[Date] BETWEEN @beginDate AND @endDate	
	
	OPEN ODCursor

	FETCH NEXT FROM ODCursor
	INTO @operDayID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS(SELECT * FROM dbo.Acc_Rests WHERE Account_ID = @accountID AND OperDay_ID = @operDayID)  --если записи не существует, создаем
			INSERT INTO dbo.Acc_Rests VALUES(@accountID, @operDayID, 0, 0, 0)
			
		FETCH NEXT FROM ODCursor
		INTO @operDayID
	END
	
	CLOSE ODCursor
	DEALLOCATE ODCursor
	------------------------------------------

	DECLARE @credit MONEY    --прибыль за ОперДень
	DECLARE @debet MONEY     --расходы за ОперДень
	DECLARE @lastRest MONEY  --последний остаток за предыдущий день
	--Подсчитать остаток на начало рассматриваемого периода-------------------
	SET @lastRest = (SELECT TOP 1 Rest FROM dbo.Acc_Rests AS ar JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID WHERE od.[Date] = @beginDate AND ar.Account_ID = @accountID)
	--------------------------------------------------------------------------
		
	DECLARE operDayCursor CURSOR FOR
	SELECT od.ID
	FROM dbo.OperDay AS od
	INNER JOIN dbo.Acc_Rests AS ar ON ar.OperDay_ID = od.ID
	WHERE (ar.Account_ID = @accountID) AND (od.[Date] >= @beginDate) AND (od.[Date] <= @endDate)
	
	OPEN operDayCursor

	FETCH NEXT FROM operDayCursor
	INTO @operDayID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @credit = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Credit_ID = @accountID)
		SET @debet = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Debet_ID = @accountID)
		IF @credit IS NULL SET @credit = 0
		IF @debet IS NULL SET @debet = 0
		IF @lastRest IS NULL SET @lastRest = 0
		
		UPDATE dbo.Acc_Rests
		SET Rest = @lastRest, Credit = @credit, Debet = @debet
		WHERE Account_ID = @accountID AND OperDay_ID = @operDayID
		
		SET @lastRest = @lastRest + @credit - @debet
		
		FETCH NEXT FROM operDayCursor
		INTO @operDayID
	END
	
	CLOSE operDayCursor
	DEALLOCATE operDayCursor
	
	--Подсчитать остаток для счета
	DECLARE @sum MONEY
	SET @sum = (SELECT (SUM(ar.Credit) - SUM(ar.Debet)) FROM dbo.Acc_Rests AS ar WHERE ar.Account_ID = @accountID)
	IF (@sum IS NULL) SET @sum = 0
	
	UPDATE dbo.Accounts
	SET Rest = (@sum)
	WHERE ID = @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[RecalculateAccountRests]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RecalculateAccountRests] 
	@accountID INT,
	@beginDate DATETIME
AS
BEGIN
	insert LogAction(src, msg) values('[RecalculateAccountRests] - BEGIN', '@accountID = ' + str(@accountID))

	Declare
	@cur cursor,
	@operDayID int,
	@debetSum money = 0,
	@creditSum money = 0,
	@accountRest money = 0
	set @accountRest = (select isnull(
					   (select TOP 1 ar.Rest lastRest
					   from Acc_Rests ar 
					   left join OperDay od on ar.OperDay_ID = od.ID 
					   where ar.Account_ID = @accountID and od.[Date] < @beginDate
					   order by od.[Date] desc), 0))
	set @cur = cursor for 
	select od.ID OperDayID from
	(SELECT @beginDate + number AS [Date]
	FROM master..spt_values 
	WHERE type='P' AND number <= DATEDIFF(DAY, @beginDate, GETDATE())) d
	join OperDay od on od.Date = d.Date
	order by od.Date

	open @cur
	fetch next from @cur into @operDayID
	while @@FETCH_STATUS = 0
	begin
		set @debetSum = isnull((select sum(o.Amount) CalculatedOperDayDebetSum
						 from Operations o
						 where o.[Status] > 3 and o.Debet_ID = @accountID and o.OperDay_ID = @operDayID
						 group by o.Debet_ID, o.OperDay_ID), 0) 

		set @creditSum = isnull((select sum(o.Amount) CalculatedOperDayCreditSum
						  from Operations o
						  where o.[Status] > 3 and o.Credit_ID = @accountID and o.OperDay_ID = @operDayID
						  group by o.Credit_ID, o.OperDay_ID), 0) 
 
		IF NOT EXISTS (SELECT * FROM Acc_Rests ar WHERE ar.Account_ID = @accountID AND ar.OperDay_ID = @operDayID)
			BEGIN
				INSERT INTO Acc_Rests
				VALUES (@accountID, @operDayID, @accountRest, @creditSum, @debetSum)
			END
		ELSE
			BEGIN
				UPDATE [dbo].[Acc_Rests]
				   SET [Rest] = @accountRest
					  ,[Credit] = @creditSum
					  ,[Debet] = @debetSum
				 WHERE [Account_ID] = @accountID AND [OperDay_ID] = @operDayID
			END
		set @accountRest = @accountRest + @creditSum - @debetSum
		fetch next from @cur into @operDayID
	end
	close @cur
	deallocate @cur
	
	insert LogAction(src, msg) values('[RecalculateAccountRests] - END',  '@accountID = ' + str(@accountID))
	
	UPDATE dbo.Accounts	SET Rest = (@accountRest) WHERE ID = @accountID

END


GO
/****** Object:  StoredProcedure [dbo].[RecalculateAllWrongAccountRests]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RecalculateAllWrongAccountRests]
	
AS
BEGIN
	
	Declare
	@cur cursor,
	@accountID int,
	@beginDate date
	set @cur = cursor for 
	select a.ID, min(a.OperationalDate) OperationalDate from
		(select a.ID, t1.OperationalDate
		 from Accounts a 
		 cross apply 
			 (select TOP 1 min(c.OperDayDate) OperationalDate 
				from
				(select sum(o.Amount) CalculatedOperDayDebetSum, od.[Date] OperDayDate, od.ID OperDayID, o.Debet_ID Account_ID
				from Operations o
				join OperDay od on od.ID = o.OperDay_ID
				where o.[Status] > 3
				group by o.Debet_ID, od.[Date],od.ID) c
				left join Acc_Rests ar on c.OperDayID = ar.OperDay_ID and c.Account_ID = ar.Account_ID
				where CalculatedOperDayDebetSum <> ar.Debet and ar.Account_ID = a.ID and a.IsDeleted = 0
			 ) t1
		 where t1.OperationalDate is not null
		 union
		 select a.ID, t1.OperationalDate
		 from Accounts a 
		 cross apply 
			 (select TOP 1 min(c.OperDayDate) OperationalDate 
				from
				(select sum(o.Amount) CalculatedOperDayCreditSum, od.[Date] OperDayDate, od.ID OperDayID, o.Credit_ID Account_ID
				from Operations o
				join OperDay od on od.ID = o.OperDay_ID
				where o.[Status] > 3
				group by o.Credit_ID, od.[Date],od.ID) c
				left join Acc_Rests ar on c.OperDayID = ar.OperDay_ID and c.Account_ID = ar.Account_ID
				where CalculatedOperDayCreditSum <> ar.Credit and ar.Account_ID = a.ID and a.IsDeleted = 0
			 ) t1
		 where t1.OperationalDate is not null
		) a
	 group by a.ID
	 order by a.ID

	open @cur
	fetch next from @cur into @accountID, @beginDate
	while @@FETCH_STATUS = 0
	begin
		exec [RecalculateAccountRests] @accountID, @beginDate
		fetch next from @cur into @accountID, @beginDate
	end
	close @cur
	deallocate @cur

END
GO
/****** Object:  StoredProcedure [dbo].[RegisterUser]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RegisterUser] 
	@login NVARCHAR(64),
	@password NVARCHAR(47),
	@ip NVARCHAR(15) = '',
	@email NVARCHAR(147) = '',
	@version NVARCHAR(10) = ''
AS
BEGIN

	IF NOT EXISTS (SELECT * FROM dbo.Users WHERE [Login] = @login)
	BEGIN
		INSERT INTO dbo.Users ([Login], [Password], [Online], IP, Email, [Version], Rating)
		VALUES (@login, @password, 0, @ip, @email, @version, 100)
		
		insert dbo.Categories (UserID, Name) values(@@IDENTITY, 'Перевод')
		
		insert LogAction(src, msg) values('RegisterUser', @login + ' / '+ @password)
		
		SELECT 0  --успешно
	END
	ELSE
		SELECT -1  --такой пользователь уже присутствует в системе

END
GO
/****** Object:  StoredProcedure [dbo].[RemoveAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[RemoveAccount]
	@accName nvarchar(150), 
	@UserID int
as

	delete Accounts where Name = @accName and UserID = @UserID
GO
/****** Object:  StoredProcedure [dbo].[report_DailyBusinessExpenses]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[report_DailyBusinessExpenses]
	@User_ID int,
	@FromDate date = null,
	@ToDate date = null
as
begin

	DECLARE @t TABLE (Rest money,  [Date] Date)
	declare @cur cursor, @date Date
	set @cur = cursor for 
		select Date from OperDay where Date between @FromDate and @ToDate order by Date

	open @cur fetch next from @cur into @date
	while @@FETCH_STATUS = 0
	begin

		insert @t
		select sum(o.Amount), @Date from Operations o
		join Accounts a on a.ID = o.Debet_ID
		join Categories c on c.ID = o.Category_ID
		join OperDay d on d.ID = o.OperDay_ID
		where a.UserID = @User_ID
		and o.Credit_ID is null
		and c.Name like '%бизнес%'
		and d.Date <= @date
		and d.Date >= DATEADD(month, DATEDIFF(month, 0, @date), 0)

		fetch next from @cur into @date

	end
	close @cur
	deallocate @cur

	select Rest, [Date] from @t

end
GO
/****** Object:  StoredProcedure [dbo].[report_DailyPersonalExpenses]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[report_DailyPersonalExpenses]
	@User_ID int,
	@FromDate date = null,
	@ToDate date = null
as
begin

	DECLARE @t TABLE (Rest money,  [Date] Date)
	declare @cur cursor, @date Date
	set @cur = cursor for 
		select Date from OperDay where Date between @FromDate and @ToDate order by Date

	open @cur fetch next from @cur into @date
	while @@FETCH_STATUS = 0
	begin

		insert @t
		select sum(o.Amount), @Date from Operations o
		join Accounts a on a.ID = o.Debet_ID
		join Categories c on c.ID = o.Category_ID
		join OperDay d on d.ID = o.OperDay_ID
		where a.UserID = @User_ID
		and o.Credit_ID is null
		and c.Name like '%личные%'
		and d.Date <= @date
		and d.Date >= DATEADD(month, DATEDIFF(month, 0, @date), 0)

		fetch next from @cur into @date

	end
	close @cur
	deallocate @cur

	select Rest, [Date] from @t

end
GO
/****** Object:  StoredProcedure [dbo].[report_DailyUserRests]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[report_DailyUserRests]
	@User_ID int,
	@FromDate date = null,
	@ToDate date = null
as
begin

	exec report_DailyUserRests_Prepare @User_ID

	select sum(r.Rest + r.Credit - r.Debet) Rest, d.Date 
	from Acc_Rests r
	join Accounts a on a.ID = r.Account_ID
	join Users u on u.ID = a.UserID
	join OperDay d on d.ID = r.OperDay_ID
	where 1 = 1
	and u.ID = @User_ID
	and (d.Date between @FromDate and @ToDate or (@FromDate is null or @ToDate is null))
	group by d.Date

end

GO
/****** Object:  StoredProcedure [dbo].[report_DailyUserRests_Prepare]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[report_DailyUserRests_Prepare]
	@User_ID int
as
begin

	declare @Account_ID int, @oid int, @Rest decimal(18,2)
	declare @Date date set @Date = getdate()
	exec GetOperDayIdByDateAM22 @Date, @oid
	--------------------------------------------------

	declare @cur cursor
	set @cur = cursor for select ID from Accounts where UserID = @User_ID

	open @cur fetch next from @cur into @Account_ID
	while @@FETCH_STATUS = 0
	begin

		select top 1 
			@Rest = Rest + r.Credit - r.Debet,
			@Date = d.Date 
		from Acc_Rests r 
		join OperDay d on d.ID = r.OperDay_ID 
		where Account_ID = @Account_ID
		order by Date desc

		insert Acc_Rests select @Account_ID, ID, @Rest, 0, 0 from OperDay where Date > @Date
	
		fetch next from @cur into @Account_ID
	end
	close @cur
	deallocate @cur

end
GO
/****** Object:  StoredProcedure [dbo].[RestorePassword]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RestorePassword]
	@UserId INT,
	@NewPassword nvarchar(47)
AS
BEGIN
	IF NOT EXISTS (SELECT TOP 1 ID FROM [dbo].[Users] WHERE ID = @UserId)
		SELECT -2  --нет такого пользователя
	ELSE
	BEGIN
		UPDATE [dbo].[Users]
		SET [Password] = @NewPassword
		WHERE ID = @UserId

		SELECT 0  --все ОК
	END
END
GO
/****** Object:  StoredProcedure [dbo].[SetAccRest]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SetAccRest] 
-- AM: 26-10-2022
	@amount MONEY,
	@accountID INT,
	@userID INT,
	@operDate DATE,
	@type INT,
	@operationID INT,
	@minus BIT OUTPUT
AS
BEGIN
	SET @minus = 0

	--------------------------------------------------------------------------
	--Определить min и max ID дат--------------------------------------------
	DECLARE @beginDate DATE, @minDate DATE, @endDate DATE 
	-- AM: на входе ОперДень его делаем как @beginDate и находим погр-е даты сущ-х операций для заданного @accountID
	SELECT @beginDate = @operDate, @minDate = MIN(d.[Date]), @endDate = MAX(d.[Date])
					FROM dbo.OperDay AS d
					JOIN dbo.Operations AS o ON d.ID = o.OperDay_ID
					JOIN dbo.Accounts AS a ON o.Credit_ID = a.ID or o.Debet_ID = a.ID
					WHERE a.ID = @accountID

	-- AM: это не понятно зачем, убрал 09/09/2012 надо проверить					
	--IF ((@beginDate IS NULL) OR (@beginDate < @minDate)) SET @beginDate = @minDate
		
	DECLARE @beginDateID INT
	SELECT @beginDateID = od.ID FROM dbo.OperDay AS od WHERE datediff(dd, od.[Date], @operDate) = 0

	insert LogAction(src, msg) values('SetAccRest', '@userID='+convert(nvarchar(55), @userID)+', @accountID='+convert(nvarchar(55), @accountID)+', '+
		'@beginDate='+convert(nvarchar(55), IsNull(@beginDate, '20000101'), 11)+', @endDate = '+convert(nvarchar(55), IsNull(@endDate, '20000101'), 11))
	
	-- AM: UserID лишний там где есть AccountID это надо почистить
	IF (@beginDate > @endDate) EXEC OperDayClose @accountID, @userID, @endDate, @beginDate
	-- AM: ОперДень для новой операции часто больше чем день последней существующей операции, в этом сл. меняем их местами
	IF (@endDate IS NULL OR @beginDate > @endDate) select @beginDate = @endDate, @endDate = @operDate
	--------------------------------------------------------------------------
	--------------------------------------------------------------------------
	
	insert Log(src, msg) values('SetAccRest', '@userID='+convert(nvarchar(55), @userID)+', @accountID='+convert(nvarchar(55), @accountID)+' @beginDate='+convert(nvarchar(55), @beginDate, 11)+', @endDate = '+convert(nvarchar(55), @endDate, 11))
	
	-- НАЧАЛО ТРАНЗАКЦИИ ОБНОВЛЕНИЯ Acc_Rests -------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------------------
	BEGIN TRANSACTION AddOperationTran
	IF @@TRANCOUNT = 1
	BEGIN
		-------------------------------------------------------------------------------------------------------------------------------
		--Удаляем из таблицы Acc_Rests все записи меньше minDate и больше maxDate------------------------------------------------------
		DELETE ar
		FROM dbo.Acc_Rests ar
		JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID
		WHERE (od.[Date] < @minDate OR od.[Date] > @endDate) AND ar.Account_ID = @accountID
		-------------------------------------------------------------------------------------------------------------------------------
		
		-------------------------------------------------------------------------------------------------------------------------------
		--Обновить таблицу Acc_Rests---------------------------------------------------------------------------------------------------
		DECLARE @operDayID INT
		
		DECLARE ODCursor CURSOR FOR
		SELECT od.ID
		FROM dbo.OperDay AS od
		WHERE od.[Date] BETWEEN @beginDate AND @endDate	
		
		OPEN ODCursor

		FETCH NEXT FROM ODCursor
		INTO @operDayID
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF NOT EXISTS(SELECT * FROM dbo.Acc_Rests WHERE Account_ID = @accountID AND OperDay_ID = @operDayID)  --если записи не существует, создаем
				INSERT INTO dbo.Acc_Rests(Account_ID, OperDay_ID, Rest, Credit, Debet) VALUES(@accountID, @operDayID, 0, 0, 0)
				
			FETCH NEXT FROM ODCursor
			INTO @operDayID
		END
		
		CLOSE ODCursor
		DEALLOCATE ODCursor
		-------------------------------------------------------------------------------------------------------------------------------
		
		DECLARE @credit MONEY    --прибыль за ОперДень
		DECLARE @debet MONEY     --расходы за ОперДень
		DECLARE @lastRest MONEY  --последний остаток за предыдущий день
		--Подсчитать остаток на начало рассматриваемого периода-------------------------------------------------------------------------------------------------------------------
		SET @lastRest = (SELECT TOP 1 Rest FROM dbo.Acc_Rests AS ar JOIN dbo.OperDay AS od ON od.ID = ar.OperDay_ID WHERE od.[Date] = @beginDate AND ar.Account_ID = @accountID)
		--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			
		DECLARE operDayCursor CURSOR FOR
		SELECT od.ID
		FROM dbo.OperDay AS od
		INNER JOIN dbo.Acc_Rests AS ar ON ar.OperDay_ID = od.ID
		WHERE (ar.Account_ID = @accountID) AND (od.[Date] >= @beginDate) AND (od.[Date] <= @endDate)
		
		OPEN operDayCursor

		FETCH NEXT FROM operDayCursor
		INTO @operDayID
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF (@operationID IS NULL)
			BEGIN
				SET @credit = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Credit_ID = @accountID)
				SET @debet = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Debet_ID = @accountID)
			END
			ELSE IF (@operationID IS NOT NULL)
			BEGIN  --при обновлении не учитываем предыдущую сумму операции
				SET @credit = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Credit_ID = @accountID AND ID != @operationID)
				SET @debet = (SELECT SUM(Amount) FROM dbo.Operations WHERE OperDay_ID = @operDayID AND Debet_ID = @accountID AND ID != @operationID)
			END
			IF @credit IS NULL SET @credit = 0
			IF @debet IS NULL SET @debet = 0
			
			--Проверка: если ОперДень тот, который передан в параметре, не забираем сумму credit и debet из таблицы Operations,
			--а подставляем значение, переданное в параметре
			IF (@operDayID = @beginDateID)
			BEGIN
				IF (@type = 1)  --тип "Кредит"
					SET @credit = @credit + @amount
				ELSE IF (@type = 2)  --тип "Дебет"
					SET @debet = @debet + @amount
			END
			
			IF (@lastRest IS NOT NULL)
			BEGIN
				UPDATE dbo.Acc_Rests
				SET Rest = @lastRest, Credit = @credit, Debet = @debet
				WHERE Account_ID = @accountID AND OperDay_ID = @operDayID
			END
			
			SET @lastRest = @lastRest + @credit - @debet
			IF (@lastRest < 0)
			BEGIN
				SET @minus = 1
				BREAK
			END
			
			FETCH NEXT FROM operDayCursor
			INTO @operDayID
		END
		
		CLOSE operDayCursor
		DEALLOCATE operDayCursor
	END
	
	-- Разрешаем иметь счета с отрицательными остатками для кредитов и др долгов
	if 1 = (select IsMinusAllowed from Accounts where ID = @accountID)
		set @minus = 0

	IF (@minus = 0)
	BEGIN
		COMMIT TRANSACTION AddOperationTran
	END
	ELSE IF (@minus = 1)
	BEGIN
		ROLLBACK TRANSACTION AddOperationTran
	END
	-- КОНЕЦ ТРАНЗАКЦИИ -----------------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------------------
END
GO
/****** Object:  StoredProcedure [dbo].[SetEmail]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[SetEmail] @userID INT,
							 @email NVARCHAR(50)
AS
BEGIN
	DECLARE @oldEmail NVARCHAR(50) SET @oldEmail = (SELECT TOP 1 Email FROM dbo.Users WHERE ID = @userID)
	
	UPDATE dbo.Users
	SET Email = @email
	WHERE ID = @userID
	
	IF @oldEmail = N''
		EXEC IncRating @userID, 500  --повысить рейтинг на 500
END
GO
/****** Object:  StoredProcedure [dbo].[SetImage]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SetImage] @userID INT,
						    @operationID INT,
							@img IMAGE,
							@fileName NVARCHAR(256)
AS
BEGIN
	IF (EXISTS (SELECT TOP 1 * FROM dbo.Users WHERE ID = @userID) AND EXISTS (SELECT TOP 1 * FROM dbo.Operations WHERE ID = @operationID))
	BEGIN
		IF (EXISTS(SELECT TOP 1 * FROM dbo.OperationsImages WHERE UserID = @userID AND OperationID = @operationID))
		BEGIN
			UPDATE dbo.OperationsImages
			SET [Image] = @img, [FileName] = @fileName
			WHERE UserID = @userID AND OperationID = @operationID
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OperationsImages(UserID, OperationID, [Image], [FileName])
			VALUES(@userID, @operationID, @img, @fileName)
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[SetOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SetOperation] 
-- AM: 26-10-2022
	@userID INT,
	@type INT,
	@amount MONEY,
	@operationID INT,
	@account1 INT,  --если перевод, то в зависимости от типа счет, на который происходит зачисление, либо списание
	@account2 INT,
	@description NVARCHAR(MAX),
	@categoryID INT,
	@operDate DATE
AS
BEGIN
  -- Получить день последней операции для заданного юзера и выбранных счетов
	DECLARE @lastOperDay DATETIME SELECT  @lastOperDay = MAX(d.[Date]) FROM OperDay d 
	JOIN Operations o ON d.ID = o.OperDay_ID JOIN dbo.Accounts a ON o.Credit_ID = a.ID or o.Debet_ID = a.ID 
	WHERE a.UserID = @userID and a.ID in (@account1, @account2)
  -- *** ----
  
	declare @msg nvarchar(max)
	DECLARE @minus BIT
	
	insert LogAction(src, msg) values(
		'SetOperation', 
		'@userID='+convert(nvarchar(55), IsNull(@userID, 0))+', '+
		'@account1='+convert(nvarchar(55), IsNull(@account1, 0))+', '+
		'@account2='+convert(nvarchar(55), IsNull(@account2, 0))+', '+
		'@operDate='+convert(nvarchar(55), IsNull(@operDate, '2000-01-01'), 11)+', '+
		'@lastOperDay='+convert(nvarchar(55), IsNull(@lastOperDay, '2000-01-01'), 11))

	IF (@type = 1)  --зачисление
	BEGIN
		exec DoLogAction 'SetOperation', 'зачисление'
		--IF @account1 IS NOT NULL SET @amount = -@amount --исправление глюка
		EXEC SetAccRest @amount, @account2, @userID, @operDate, @type, @operationID, @minus OUTPUT
	END
	ELSE IF (@type = 2)  --списание
	begin
		exec DoLogAction 'SetOperation', 'списание'
		EXEC SetAccRest @amount, @account1, @userID, @operDate, @type, @operationID, @minus OUTPUT
		-- @minus это наверно когда счет после оперы уйдет в минус, но должны быть счета долга
	end
	insert LogAction(src, msg) values('SetOperation', '@amount='+convert(nvarchar(55), @amount)+', @minus='+convert(nvarchar(55), @minus))
	
	DECLARE @operDay_ID INT exec GetOperDayIdByDateAM22 @operDate, @operDay_ID out
	set @msg = '@operDay_ID='+convert(nvarchar(10), @operDay_ID)
	exec DoLogAction 'SetOperation', @msg

	IF (@minus = 0)
	BEGIN
		IF (@operationID IS NOT NULL)
		BEGIN --обновление операции
			exec DoLogAction 'SetOperation', 'обновление операции'

			DECLARE @oldAccount1 INT, @oldAccount2 INT, @oldOperDate DATETIME
			SET @oldAccount1 = (SELECT TOP 1 Debet_ID FROM dbo.Operations WHERE ID = @operationID)
			SET @oldAccount2 = (SELECT TOP 1 Credit_ID FROM dbo.Operations WHERE ID = @operationID)
			SET @oldOperDate = (SELECT TOP 1 od.[Date] FROM dbo.OperDay AS od JOIN dbo.Operations AS o ON o.OperDay_ID = od.ID WHERE o.ID = @operationID)
		
			IF (@account1 IS NOT NULL AND @account2 IS NOT NULL AND @type = 1)  --перевод
			BEGIN
				UPDATE dbo.Operations
				SET [Description] = @description, Amount = @amount, Category_ID = @categoryID, Debet_ID = @account2, Credit_ID = @account1, DateEdit = GETDATE(), OperDay_ID = @operDay_ID
				WHERE ID = @operationID
			END
			ELSE
			BEGIN
				UPDATE dbo.Operations
				SET [Description] = @description, Amount = @amount, Category_ID = @categoryID, Debet_ID = @account1, Credit_ID = @account2, DateEdit = GETDATE(), OperDay_ID = @operDay_ID
				WHERE ID = @operationID
			END
			
			--При обновлении операции обновляем все счета, остатки по которым явно модифицированы
			IF (@oldOperDate > @operDate) SET @oldOperDate = @operDate
			IF (@oldAccount1 != @account1 AND @oldAccount1 != @account2 AND @oldAccount1 IS NOT NULL) EXEC OperDayClose @oldAccount1, @userID, @oldOperDate, NULL
			IF (@oldAccount2 != @account1 AND @oldAccount2 != @account2 AND @oldAccount2 IS NOT NULL) EXEC OperDayClose @oldAccount2, @userID, @oldOperDate, NULL
			IF (@account1 IS NOT NULL) EXEC OperDayClose @account1, @userID, @operDate, NULL
			IF (@account2 IS NOT NULL) EXEC OperDayClose @account2, @userID, @operDate, NULL
		END
		ELSE IF (@operationID IS NULL)
		BEGIN --добавление новой операции
			IF (@account1 IS NOT NULL AND @account2 IS NOT NULL AND @type = 1)  --перевод
			BEGIN
				exec DoLogAction 'SetOperation', 'добавление новой операции между счетами'
				INSERT INTO Operations ([Description], Amount, Category_ID, Debet_ID, Credit_ID, DateCreate, DateEdit, OperDay_ID, [Status]) 
				VALUES (@description, @amount, @categoryID, @account2, @account1, GETDATE(), GETDATE(), @operDay_ID, 8)
			END
			ELSE BEGIN
				exec DoLogAction 'SetOperation', 'добавление новой операции без 2-ого счета'
				INSERT INTO Operations ([Description], Amount, Category_ID, Debet_ID, Credit_ID, DateCreate, DateEdit, OperDay_ID, [Status]) 
				VALUES (@description, @amount, @categoryID, @account1, @account2, GETDATE(), GETDATE(), @operDay_ID, 8)
			END
			
			EXEC IncRating @userID, 1  --повысить рейтинг на 1
		
		    SET @operationID = @@IDENTITY
		    
		    --Если операция перевода с одного счета на другой, обновляем остатки для счета, НА КОТОРЫЙ перевели деньги, так как остатки для счетов, с которых списаны средства, уже пересчитаны
		    --DECLARE @operDateMinusDay DATETIME SET @operDateMinusDay = DATEADD(dd, -3, @operDate)
		    IF (@account1 IS NOT NULL AND @account2 IS NOT NULL)  --перевод со счета на счет
		    BEGIN
				IF (@type = 1)
					EXEC am_CloseAccOperDay @account1, @lastOperDay, NULL
				ELSE IF (@type = 2)
					EXEC am_CloseAccOperDay @account2, @lastOperDay, NULL
		    END
		    ELSE  --Операция без перевода
			BEGIN
				IF (@type = 1)
					EXEC am_CloseAccOperDay @account2, @lastOperDay, NULL
				ELSE IF (@type = 2)
					EXEC am_CloseAccOperDay @account1, @lastOperDay, NULL
			END
		END
		
		--Обновляем рейтинговые таблицы
		IF (@type = 1) UPDATE dbo.Categories SET CreditRating = CreditRating + 1 WHERE ID = @categoryID
		ELSE IF (@type = 2) UPDATE dbo.Categories SET DebetRating = DebetRating + 1 WHERE ID = @categoryID
		ELSE IF (@account1 IS NOT NULL AND @account2 IS NOT NULL) UPDATE dbo.Categories SET TransferRating = TransferRating + 1 WHERE ID = @categoryID
		
		IF @account1 IS NOT NULL EXEC UpdateAccountCategoryRating @account1, @categoryID
		IF @account2 IS NOT NULL EXEC UpdateAccountCategoryRating @account2, @categoryID
			
		update Operations set UserName = (select [Login] from users where ID = @userID) where ID = @operationID
		SELECT @operationID  --УСПЕШНО
	END
	ELSE IF (@minus = 1)
		SELECT -3  --НЕ УСПЕШНО (ИЗМЕНЕНИЯ ПЕРЕВОДЯТ ОСТАТКИ В МИНУС)


END
GO
/****** Object:  StoredProcedure [dbo].[SetSetting]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SetSetting] @key NVARCHAR(128),
							   @value NVARCHAR(256)
AS
BEGIN
	UPDATE dbo.Settings
	SET Value = @value
	WHERE [Key] = @key
END
GO
/****** Object:  StoredProcedure [dbo].[SetUser]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SetUser] 
	@UserID INT = NULL,
	@LoginName NVARCHAR(64),
	@LoginEMail NVARCHAR(64),
	@Password NVARCHAR(47),
	@ParentID INT = NULL,
	@IP NVARCHAR(64) = NULL
/*
go
exec [SetUser] NULL, 'puser7', 'a7@b.c', '123', NULL
*/
AS
BEGIN
	set nocount on

	insert LogAction(src, msg) values('[SetUser]', @LoginName)

	IF @UserID IS NULL
	BEGIN  --добавление нового пользователя

		IF NOT EXISTS (SELECT * FROM dbo.Users WHERE [Login] = @LoginName or [Login] = @LoginEMail)
		BEGIN
			INSERT Users ([Name], [Login], [Password], IP, Email, ParentID)
			VALUES (@LoginName, @LoginEMail, @Password,  @IP, @LoginEMail, @ParentID)
		
			set @UserID = @@IDENTITY
			
			if @ParentId is NULL
			begin
				select @UserID
				-- Для нового юзера создать стандартные категории и счета
				exec [AddCategory] 'Перевод', @UserID
				exec [AddAccount] 'Касса', @UserID
				exec [AddAccount] 'Карта', @UserID
				
				return
			end
		END
	ELSE
		begin
			SELECT -1  --такой пользователь уже присутствует в системе
			return
		end
	END
	ELSE
	BEGIN  --редактирование пароля и не только
	
		UPDATE Users
		SET [Password] = @password, [Name] = @LoginName
		WHERE ID = @userID
	END

	SELECT @UserID
	
END

GO
/****** Object:  StoredProcedure [dbo].[SetUserIP]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SetUserIP] @userID INT,
						     @ip NVARCHAR(15)
AS
BEGIN
	UPDATE dbo.Users
	SET IP = @ip
	WHERE ID = @userID

	UPDATE dbo.[Sessions]
	SET IP = @ip
	WHERE ID = (SELECT TOP 1 LastSessionID FROM dbo.Users WHERE ID = @userID)
END
GO
/****** Object:  StoredProcedure [dbo].[SetVersion]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SetVersion] @userID INT,
							  @version NVARCHAR(10)
AS
BEGIN
	UPDATE dbo.Users
	SET [Version] = @version
	WHERE ID = @userID

	UPDATE dbo.[Sessions]
	SET [Version] = @version
	WHERE ID = (SELECT TOP 1 LastSessionID FROM dbo.Users WHERE ID = @userID)
END
GO
/****** Object:  StoredProcedure [dbo].[Transfer]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Transfer] @fromAccountID INT,
							     @toAccountID INT,
								 @amount MONEY,
								 @categoryID INT,
								 @operDate DATE
AS
BEGIN
	DECLARE @operDay_ID INT

	IF @operDate IS NULL
		SET @operDay_ID = (SELECT TOP 1 od.ID FROM dbo.OperDay AS od WHERE od.[Date] < GETDATE() ORDER BY [Date] DESC)
	ELSE
	BEGIN
		DECLARE @minDate DATETIME
		DECLARE @maxDate DATETIME
		SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
		SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

		IF EXISTS (SELECT ID FROM OperDay WHERE [Date] = @operDate)
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @operDate)
		ELSE
		BEGIN
			IF @operDate > @maxDate
				SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @maxDate)
			ELSE IF @operDate < @minDate
				SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @minDate)
		END
	END

	INSERT INTO dbo.Operations ([Description], Amount, Category_ID, Debet_ID, Credit_ID, OperDay_ID, DateCreate, DateEdit, [Status])
	VALUES ('Перевод средств', @amount, @categoryID, @fromAccountID, @toAccountID, @operDay_ID, GETDATE(), GETDATE(), 8)
	
	--Обновляем рейтинговую таблицу
	UPDATE dbo.Categories
	SET TransferRating = TransferRating + 1
	WHERE ID = @categoryID
	
	IF @categoryID IS NOT NULL
	BEGIN
		EXEC UpdateAccountCategoryRating @fromAccountID, @categoryID
		EXEC UpdateAccountCategoryRating @toAccountID, @categoryID
	END
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateAccount]
-- AM: 26-10-2022
	@accountID INT,
	@name NVARCHAR(MAX),
	@limit MONEY,
	@plan MONEY,
	@firstDay INT,
	@IsMinusAllowed bit = 0
AS
BEGIN
	UPDATE dbo.Accounts
	SET Name = @name, Limit = @limit, [Plan] = @plan, FirstDay = @firstDay,
			   IsMinusAllowed = @IsMinusAllowed
	WHERE ID = @accountID
	
	SELECT @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateAccountCategoryRating]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateAccountCategoryRating] @accountID INT,
													@categoryID INT
AS
BEGIN
	IF EXISTS (SELECT * FROM dbo.AccountsCategoriesRating WHERE AccountID = @accountID AND CategoryID = @categoryID)
	BEGIN
		UPDATE dbo.AccountsCategoriesRating
		SET Rating = Rating + 1
		WHERE AccountID = @accountID AND CategoryID = @categoryID
	END
	ELSE
	BEGIN
		INSERT INTO dbo.AccountsCategoriesRating(AccountID, CategoryID, Rating)
		VALUES (@accountID, @categoryID, 1)
	END
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateAccountName]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateAccountName] @accountID INT, @name NVARCHAR(MAX)
AS
BEGIN
	UPDATE dbo.Accounts
	SET Name = @name
	WHERE ID = @accountID
	
	SELECT @accountID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateCategoryFirstDay]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateCategoryFirstDay] @categoryID INT,
										   @firstDay INT
AS
BEGIN
	UPDATE dbo.Categories
	SET FirstDay = @firstDay
	WHERE ID = @categoryID
	
	SELECT @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateCategoryLimit]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateCategoryLimit] @categoryID INT,
									   @limit MONEY
AS
BEGIN
	UPDATE dbo.Categories
	SET Limit = @limit
	WHERE ID = @categoryID
	
	SELECT @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateCategoryName]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateCategoryName] @categoryID INT,
									  @name NVARCHAR(MAX)
AS
BEGIN
	UPDATE dbo.Categories
	SET Name = @name
	WHERE ID = @categoryID
	
	SELECT @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateCategoryPlan]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateCategoryPlan] @categoryID INT,
									   @plan MONEY
AS
BEGIN
	UPDATE dbo.Categories
	SET [Plan] = @plan
	WHERE ID = @categoryID
	
	SELECT @categoryID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateCreditOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateCreditOperation] @id INT,
										 @description NVARCHAR(MAX), 
										 @amount MONEY, 
									     @category INT, 
										 @credit INT, 
										 @operDate DATE
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	DECLARE @operDay_ID INT
	IF EXISTS (SELECT ID FROM OperDay WHERE [Date] = @operDate)
		SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @operDate)
	ELSE
	BEGIN
		IF @operDate > @maxDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @maxDate)
		ELSE IF @operDate < @minDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @minDate)
	END

	UPDATE dbo.Operations
	SET [Description] = @description, Amount = @amount, Category_ID = @category, Debet_ID = null, Credit_ID = @credit, DateEdit = GETDATE(), OperDay_ID = @operDay_ID
	WHERE ID = @id
	
	--Обновляем рейтинговую таблицу
	UPDATE dbo.Categories
	SET CreditRating = CreditRating + 1
	WHERE ID = @category
	
	EXEC UpdateAccountCategoryRating @credit, @category
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateDebetOperation]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateDebetOperation] @id INT,
										 @description NVARCHAR(MAX), 
										 @amount MONEY, 
									     @category INT, 
										 @debet INT, 
										 @operDate DATE
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	DECLARE @operDay_ID INT
	IF EXISTS (SELECT ID FROM OperDay WHERE [Date] = @operDate)
		SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @operDate)
	ELSE
	BEGIN
		IF @operDate > @maxDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @maxDate)
		ELSE IF @operDate < @minDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @minDate)
	END

	UPDATE dbo.Operations
	SET [Description] = @description, Amount = @amount, Category_ID = @category, Debet_ID = @debet, Credit_ID = null, DateEdit = GETDATE(), OperDay_ID = @operDay_ID
	WHERE ID = @id
	
	--Обновляем рейтинговую таблицу
	UPDATE dbo.Categories
	SET DebetRating = DebetRating + 1
	WHERE ID = @category
	
	EXEC UpdateAccountCategoryRating @debet, @category
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateTransfer]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateTransfer] @operationID INT,
									   @fromAccountID INT,
									   @toAccountID INT,
									   @amount MONEY,
									   @categoryID INT,
									   @operDate DATE
AS
BEGIN
	DECLARE @minDate DATETIME
	DECLARE @maxDate DATETIME
	SET @minDate = (SELECT MIN(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)
	SET @maxDate = (SELECT MAX(od.[Date]) FROM dbo.OperDay AS od JOIN dbo.Operations AS op ON op.OperDay_ID = od.ID)

	DECLARE @operDay_ID INT
	IF EXISTS (SELECT ID FROM OperDay WHERE [Date] = @operDate)
		SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @operDate)
	ELSE
	BEGIN
		IF @operDate > @maxDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @maxDate)
		ELSE IF @operDate < @minDate
			SET @operDay_ID = (SELECT TOP 1 ID FROM OperDay WHERE [Date] = @minDate)
	END

	UPDATE dbo.Operations
	SET [Description] = 'Перевод средств', Amount = @amount, Category_ID = @categoryID, Debet_ID = @fromAccountID, Credit_ID = @toAccountID, OperDay_ID = @operDay_ID, DateEdit = GETDATE()
	WHERE ID = @operationID
	
	--Обновляем рейтинговую таблицу
	UPDATE dbo.Categories
	SET TransferRating = TransferRating + 1
	WHERE ID = @categoryID
	
	IF @categoryID IS NOT NULL
	BEGIN
		EXEC UpdateAccountCategoryRating @fromAccountID, @categoryID
		EXEC UpdateAccountCategoryRating @toAccountID, @categoryID
	END
END
GO
/****** Object:  StoredProcedure [dbo].[UserLogIn]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UserLogIn] 
	@Login NVARCHAR(64),
	@Password NVARCHAR(47)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM dbo.Users WHERE [Login] = @login)
		SELECT -1 --такого пользователя нет
	ELSE
	BEGIN
		IF (@password = (SELECT TOP 1 [Password] FROM Users WHERE [Login] = @Login))
		BEGIN
			DECLARE @UserID INT
			SET @UserID = (SELECT TOP 1 ID FROM Users WHERE [Login] = @Login)
			
			INSERT INTO [Sessions]([TimeIn], [TimeOut], Duration, UserID, IP, [Version])
			VALUES(GETDATE(), NULL, NULL, @UserID, NULL, NULL)
			
			UPDATE Users
			SET [Online] = 1, LastSessionID = @@IDENTITY
			WHERE ID = @UserID
			
			EXEC IncRating @UserID, 1 --увеличить рейтинг пользователя за вход на 1
			
			SELECT @UserID
		END
		ELSE
			SELECT -2  --неверный пароль
	END
END
GO
/****** Object:  StoredProcedure [dbo].[UserLogOut]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UserLogOut] @userID INT
AS
BEGIN
	UPDATE dbo.Users
	SET [Online] = 0
	WHERE ID = @userID
	
	DECLARE @lastSessionID INT SET @lastSessionID = (SELECT TOP 1 LastSessionID FROM dbo.Users WHERE ID = @userID)
	DECLARE @timeIn DATETIME SET @timeIn = (SELECT TOP 1 TimeIn FROM dbo.[Sessions] WHERE ID = @lastSessionID)
	
	UPDATE dbo.[Sessions]
	SET [TimeOut] = GETDATE(), Duration = DATEDIFF(ms, @timeIn, GETDATE())
	WHERE ID = @lastSessionID
END
GO
/****** Object:  StoredProcedure [dbo].[UserOwnsAccount]    Script Date: 09.11.2022 14:01:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROC [dbo].[UserOwnsAccount] 
	@userID INT,
	@accountID int
AS
BEGIN
	IF EXISTS (SELECT TOP 1 ID FROM dbo.Accounts WHERE UserID = @userID AND ID = @accountID)
		SELECT 1  -- пользователь владеет счетом
	ELSE
		SELECT 0  -- пользователь не владеет счетом
END
GO
