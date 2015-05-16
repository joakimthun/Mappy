CREATE TABLE [dbo].[Post] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Text]        NVARCHAR (2500) NULL,
    [CreatedDate] DATETIME        NULL,
    [Published]   BIT             NULL,
	[UserId]      INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
	FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);

