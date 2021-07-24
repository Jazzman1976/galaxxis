USE [GalaxxisPrivat]
GO

/****** Object:  Table [dbo].[GalaxyToUniverse]    Script Date: 16.07.2020 13:43:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GalaxyToUniverse](
	[GalaxyId] [int] NOT NULL,
	[UniverseId] [int] NOT NULL,
 CONSTRAINT [PK_GalaxyToUniverse] PRIMARY KEY CLUSTERED 
(
	[GalaxyId] ASC,
	[UniverseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[GalaxyToUniverse]  WITH CHECK ADD  CONSTRAINT [FK_GalaxyToUniverse_Galaxy] FOREIGN KEY([GalaxyId])
REFERENCES [dbo].[Galaxy] ([GalaxyId])
GO

ALTER TABLE [dbo].[GalaxyToUniverse] CHECK CONSTRAINT [FK_GalaxyToUniverse_Galaxy]
GO

ALTER TABLE [dbo].[GalaxyToUniverse]  WITH CHECK ADD  CONSTRAINT [FK_GalaxyToUniverse_Universe] FOREIGN KEY([UniverseId])
REFERENCES [dbo].[Universe] ([UniverseId])
GO

ALTER TABLE [dbo].[GalaxyToUniverse] CHECK CONSTRAINT [FK_GalaxyToUniverse_Universe]
GO

