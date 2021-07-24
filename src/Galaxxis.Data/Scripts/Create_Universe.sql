USE [GalaxxisPrivat]
GO

/****** Object:  Table [dbo].[Universe]    Script Date: 16.07.2020 13:42:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Universe](
	[UniverseId] [int] IDENTITY(1,1) NOT NULL,
	[UniverseName] [nvarchar](50) NULL,
	[UniverseTimePerTick] [bigint] NOT NULL,
	[UniverseTicks] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UniverseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Universe] ADD  DEFAULT ((10000000)) FOR [UniverseTimePerTick]
GO

ALTER TABLE [dbo].[Universe] ADD  DEFAULT ((0)) FOR [UniverseTicks]
GO

