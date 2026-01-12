# Data Model: Document Upload and Management

**Feature**: Document Upload and Management  
**Branch**: 001-document-management  
**Date**: 2026-01-12

## Overview

Data model for document management including document metadata storage, file references, and sharing relationships. Extends existing ContosoDashboard entities (User, Project, Notification) with document-related relationships.

---

## Entities

### Document

Represents an uploaded document with metadata and file reference.

**Properties**:

| Property | Type | Constraints | Description |
|----------|------|-------------|-------------|
| DocumentId | int | PK, Identity | Unique document identifier |
| Title | string | Required, MaxLength(255) | User-provided document title |
| Description | string | Optional, MaxLength(2000) | User-provided description |
| FileName | string | Required, MaxLength(255) | Original filename from upload |
| FilePath | string | Required, MaxLength(500) | Relative storage path (local or blob name) |
| FileSize | long | Required | File size in bytes |
| FileType | string | Required, MaxLength(255) | MIME type (e.g., "application/pdf") |
| Category | string | Required, MaxLength(100) | Category from predefined list |
| Tags | string | Optional, MaxLength(500) | Comma-separated tags for search |
| UploadDate | DateTime | Required | UTC timestamp of upload |
| UploadedByUserId | int | Required, FK → User | User who uploaded the document |
| ProjectId | int | Optional, FK → Project | Associated project (null for personal documents) |
| UpdatedDate | DateTime | Required | UTC timestamp of last metadata update |

**Navigation Properties**:
- `UploadedBy` → User (many-to-one, required)
- `Project` → Project (many-to-one, optional)
- `Shares` → ICollection<DocumentShare> (one-to-many)

**Indexes**:
- `IX_Document_UploadedByUserId` - For "My Documents" queries
- `IX_Document_ProjectId` - For project documents queries
- `IX_Document_UploadDate` - For sorting and date filters
- `IX_Document_Category` - For category filtering

**Validation Rules**:
- Title must not be empty or whitespace
- FileName must have valid extension from whitelist
- FileSize must be > 0 and ≤ 26,214,400 bytes (25 MB)
- FileType must match extension (validated against whitelist)
- Category must be one of: "Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other"
- FilePath must be unique (GUID-based ensures this)

**Sample Data**:
```json
{
  "DocumentId": 1,
  "Title": "Q4 Project Plan",
  "Description": "Project planning document for Q4 initiatives",
  "FileName": "Q4_Plan.pdf",
  "FilePath": "1/5/a7f3c8e9-4d2b-4f1a-9c6d-7e8f3a2b1c5d.pdf",
  "FileSize": 2457600,
  "FileType": "application/pdf",
  "Category": "Project Documents",
  "Tags": "planning,Q4,roadmap",
  "UploadDate": "2026-01-12T10:30:00Z",
  "UploadedByUserId": 1,
  "ProjectId": 5,
  "UpdatedDate": "2026-01-12T10:30:00Z"
}
```

---

### DocumentShare

Represents a document sharing relationship between users.

**Properties**:

| Property | Type | Constraints | Description |
|----------|------|-------------|-------------|
| ShareId | int | PK, Identity | Unique share identifier |
| DocumentId | int | Required, FK → Document | Document being shared |
| SharedWithUserId | int | Required, FK → User | User receiving access |
| SharedByUserId | int | Required, FK → User | User who shared the document |
| SharedDate | DateTime | Required | UTC timestamp of sharing action |
| CanEdit | bool | Required, Default: false | Whether recipient can edit metadata (future: not used in MVP) |

**Navigation Properties**:
- `Document` → Document (many-to-one, required)
- `SharedWithUser` → User (many-to-one, required)
- `SharedByUser` → User (many-to-one, required)

**Indexes**:
- `IX_DocumentShare_SharedWithUserId` - For "Shared with Me" queries
- `IX_DocumentShare_DocumentId` - For finding shares of a document
- `IX_DocumentShare_SharedByUserId` - For finding documents I've shared

**Validation Rules**:
- SharedWithUserId must not equal SharedByUserId (cannot share with self)
- Unique constraint on (DocumentId, SharedWithUserId) - prevent duplicate shares
- SharedByUserId must be the document owner or have sharing rights

**Sample Data**:
```json
{
  "ShareId": 1,
  "DocumentId": 1,
  "SharedWithUserId": 3,
  "SharedByUserId": 1,
  "SharedDate": "2026-01-12T11:00:00Z",
  "CanEdit": false
}
```

---

## Extended Entities

### User (Existing - Extended)

**New Navigation Properties**:
- `UploadedDocuments` → ICollection<Document> (one-to-many)
- `SharedDocumentsReceived` → ICollection<DocumentShare> (one-to-many, via SharedWithUserId)
- `SharedDocumentsSent` → ICollection<DocumentShare> (one-to-many, via SharedByUserId)

**No schema changes** - relationships only.

---

### Project (Existing - Extended)

**New Navigation Properties**:
- `Documents` → ICollection<Document> (one-to-many)

**No schema changes** - relationships only.

---

### Notification (Existing - Extended)

**New NotificationType Values**:
- `DocumentShared` - When someone shares a document with you
- `ProjectDocumentAdded` - When a new document is added to your project

**No schema changes** - uses existing NotificationType enum or string column.

---

## Relationships Diagram

```
┌──────────────┐
│     User     │
└──────┬───────┘
       │
       │ 1:N (UploadedBy)
       │
       ↓
┌──────────────────┐         ┌─────────────────┐
│    Document      │─────────│  DocumentShare  │
│                  │ 1:N     │                 │
│ • DocumentId     │         │ • ShareId       │
│ • Title          │         │ • DocumentId    │
│ • FileName       │         │ • SharedWithId  │
│ • FilePath       │         │ • SharedById    │
│ • Category       │         │ • SharedDate    │
│ • UploadedById   │         └─────────────────┘
│ • ProjectId      │                 │
└──────────────────┘                 │
       │                             │
       │ N:1 (Project)               │ N:1 (Users)
       ↓                             ↓
┌──────────────┐             ┌──────────────┐
│   Project    │             │     User     │
└──────────────┘             └──────────────┘
```

---

## Entity Framework Core Configuration

**ApplicationDbContext.cs**:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentShare> DocumentShares { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.DocumentId);
            
            entity.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(d => d.FilePath)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(d => d.FileType)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(d => d.Category)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(d => d.UploadDate)
                .IsRequired();
            
            entity.Property(d => d.UpdatedDate)
                .IsRequired();
            
            // Relationships
            entity.HasOne(d => d.UploadedBy)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(d => d.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
            
            entity.HasOne(d => d.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull); // Keep document if project deleted
            
            // Indexes
            entity.HasIndex(d => d.UploadedByUserId)
                .HasDatabaseName("IX_Document_UploadedByUserId");
            
            entity.HasIndex(d => d.ProjectId)
                .HasDatabaseName("IX_Document_ProjectId");
            
            entity.HasIndex(d => d.UploadDate)
                .HasDatabaseName("IX_Document_UploadDate");
            
            entity.HasIndex(d => d.Category)
                .HasDatabaseName("IX_Document_Category");
        });
        
        // DocumentShare configuration
        modelBuilder.Entity<DocumentShare>(entity =>
        {
            entity.HasKey(ds => ds.ShareId);
            
            entity.Property(ds => ds.SharedDate)
                .IsRequired();
            
            entity.Property(ds => ds.CanEdit)
                .IsRequired()
                .HasDefaultValue(false);
            
            // Relationships
            entity.HasOne(ds => ds.Document)
                .WithMany(d => d.Shares)
                .HasForeignKey(ds => ds.DocumentId)
                .OnDelete(DeleteBehavior.Cascade); // Delete shares when document deleted
            
            entity.HasOne(ds => ds.SharedWithUser)
                .WithMany(u => u.SharedDocumentsReceived)
                .HasForeignKey(ds => ds.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ds => ds.SharedByUser)
                .WithMany(u => u.SharedDocumentsSent)
                .HasForeignKey(ds => ds.SharedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(ds => ds.SharedWithUserId)
                .HasDatabaseName("IX_DocumentShare_SharedWithUserId");
            
            entity.HasIndex(ds => ds.DocumentId)
                .HasDatabaseName("IX_DocumentShare_DocumentId");
            
            entity.HasIndex(ds => ds.SharedByUserId)
                .HasDatabaseName("IX_DocumentShare_SharedByUserId");
            
            // Unique constraint: Cannot share same document with same user twice
            entity.HasIndex(ds => new { ds.DocumentId, ds.SharedWithUserId })
                .IsUnique()
                .HasDatabaseName("IX_DocumentShare_Unique");
        });
    }
}
```

---

## Database Migration

**Migration Name**: `AddDocumentManagement`

**Up Operations**:
1. Create Documents table with all columns and indexes
2. Create DocumentShares table with all columns and indexes
3. Add foreign key constraints
4. Add unique constraint on DocumentShare (DocumentId, SharedWithUserId)

**Down Operations**:
1. Drop DocumentShares table
2. Drop Documents table

**No data seed required** - training users will upload documents manually.

---

## Query Patterns

**Get user's documents**:
```csharp
var documents = await context.Documents
    .Include(d => d.UploadedBy)
    .Include(d => d.Project)
    .Where(d => d.UploadedByUserId == userId)
    .OrderByDescending(d => d.UploadDate)
    .ToListAsync();
```

**Get project documents**:
```csharp
var documents = await context.Documents
    .Include(d => d.UploadedBy)
    .Where(d => d.ProjectId == projectId)
    .OrderByDescending(d => d.UploadDate)
    .ToListAsync();
```

**Get shared documents**:
```csharp
var sharedDocs = await context.DocumentShares
    .Include(ds => ds.Document)
        .ThenInclude(d => d.UploadedBy)
    .Include(ds => ds.SharedByUser)
    .Where(ds => ds.SharedWithUserId == userId)
    .OrderByDescending(ds => ds.SharedDate)
    .Select(ds => ds.Document)
    .ToListAsync();
```

**Search documents**:
```csharp
var results = await context.Documents
    .Include(d => d.UploadedBy)
    .Where(d => d.UploadedByUserId == userId)
    .Where(d => 
        d.Title.Contains(searchTerm) ||
        d.Description.Contains(searchTerm) ||
        d.Tags.Contains(searchTerm) ||
        d.FileName.Contains(searchTerm))
    .ToListAsync();
```

All queries use indexes for optimal performance (<2 second response time requirement).
