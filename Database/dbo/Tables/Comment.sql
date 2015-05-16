CREATE TABLE [dbo].[Comment] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Text]        NVARCHAR (2500) NULL,
    [CreatedDate] DATETIME        NULL,
    [Published]   BIT             NULL,
    [PostId]      INT             NOT NULL,
	[UserId]      INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([PostId]) REFERENCES [dbo].[Post] ([Id]),
	FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);

