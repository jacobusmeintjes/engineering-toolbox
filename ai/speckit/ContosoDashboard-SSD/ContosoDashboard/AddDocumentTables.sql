-- Add Documents table
CREATE TABLE [Documents] (
    [DocumentId] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [FileName] nvarchar(255) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [FileSize] bigint NOT NULL,
    [FileType] nvarchar(255) NOT NULL,
    [Category] nvarchar(100) NOT NULL,
    [Tags] nvarchar(500) NULL,
    [UploadDate] datetime2 NOT NULL,
    [UploadedByUserId] int NOT NULL,
    [ProjectId] int NULL,
    [UpdatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [FK_Documents_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([ProjectId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Documents_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

-- Add DocumentShares table
CREATE TABLE [DocumentShares] (
    [ShareId] int NOT NULL IDENTITY,
    [DocumentId] int NOT NULL,
    [SharedWithUserId] int NOT NULL,
    [SharedByUserId] int NOT NULL,
    [SharedDate] datetime2 NOT NULL,
    [CanEdit] bit NOT NULL,
    CONSTRAINT [PK_DocumentShares] PRIMARY KEY ([ShareId]),
    CONSTRAINT [FK_DocumentShares_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DocumentShares_Users_SharedByUserId] FOREIGN KEY ([SharedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DocumentShares_Users_SharedWithUserId] FOREIGN KEY ([SharedWithUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

-- Create indexes for Documents table
CREATE INDEX [IX_Documents_Category] ON [Documents] ([Category]);
CREATE INDEX [IX_Documents_ProjectId] ON [Documents] ([ProjectId]);
CREATE INDEX [IX_Documents_UploadDate] ON [Documents] ([UploadDate]);
CREATE INDEX [IX_Documents_UploadedByUserId] ON [Documents] ([UploadedByUserId]);

-- Create indexes for DocumentShares table
CREATE INDEX [IX_DocumentShares_DocumentId] ON [DocumentShares] ([DocumentId]);
CREATE INDEX [IX_DocumentShares_SharedByUserId] ON [DocumentShares] ([SharedByUserId]);
CREATE INDEX [IX_DocumentShares_SharedWithUserId] ON [DocumentShares] ([SharedWithUserId]);

-- Mark migration as applied
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260112181505_AddDocumentManagement', '10.0.0');
