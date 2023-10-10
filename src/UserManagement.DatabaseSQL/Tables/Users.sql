CREATE TABLE [dbo].[Users]
(
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Age] INT NOT NULL DEFAULT 1 ,
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Roles] nvarchar(max) NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
)
