# Quickstart Testing Guide: Document Upload and Management

**Feature**: Document Upload and Management  
**Branch**: 001-document-management  
**Date**: 2026-01-12

## Prerequisites

- ContosoDashboard running on .NET 10
- SQL Server LocalDB database initialized with sample users
- Clean AppData/uploads directory (or create it during first upload)
- Browser (Chrome, Edge, or Firefox)

---

## Test Scenario 1: Upload Personal Document (P1 - Core Upload)

**User**: Ni Kang (Employee)  
**Story**: US1 - Upload and Store Documents  
**Success**: Document appears in "My Documents" list

### Steps

1. **Login**
   - Navigate to `http://localhost:5000/login`
   - Select "Ni Kang" (`ni.kang@contoso.com`)
   - Click "Login"

2. **Navigate to Documents**
   - Click "Documents" in the navigation menu
   - Verify "My Documents" page loads

3. **Upload a Document**
   - Click "Upload Document" button
   - Fill upload form:
     - **Title**: "Test Document 1"
     - **Description**: "This is a test upload"
     - **Category**: Select "Personal Files"
     - **Project**: Leave blank (personal document)
     - **Tags**: "test,demo"
   - Click "Choose File"
   - Select a PDF file under 25 MB
   - Click "Upload" button
   - Verify progress indicator appears

4. **Verify Upload Success**
   - Verify success message: "Document uploaded successfully"
   - Verify document appears in documents list
   - Verify columns display: Title, Category, Upload Date, File Size, File Type

### Expected Results

✅ Document uploaded successfully  
✅ Document appears in list with correct metadata  
✅ File stored in `AppData/uploads/3/personal/{guid}.pdf`  
✅ Database record created with correct UploadedByUserId=3 (Ni Kang)

### Validation Checks

- File size displayed correctly (e.g., "1.2 MB")
- Upload date is today
- Category shows "Personal Files"
- Title shows "Test Document 1"

---

## Test Scenario 2: Validation Errors (P1 - Upload Validation)

**User**: Ni Kang (Employee)  
**Story**: US1 - Upload and Store Documents  
**Success**: System rejects invalid uploads with clear error messages

### Steps

1. **Test Oversized File**
   - Click "Upload Document"
   - Select a file over 25 MB (create if needed: large video or zip)
   - Attempt upload
   - **Expected**: Error message "File size exceeds maximum limit of 25 MB"

2. **Test Unsupported File Type**
   - Click "Upload Document"
   - Select an .exe or .dll file
   - Attempt upload
   - **Expected**: Error message "File type not supported. Allowed types: PDF, Word, Excel, PowerPoint, Text, JPEG, PNG"

3. **Test Missing Required Fields**
   - Click "Upload Document"
   - Select valid PDF file
   - Leave Title empty
   - Attempt upload
   - **Expected**: Validation error "Title is required"

4. **Test Missing Category**
   - Click "Upload Document"
   - Select valid PDF file
   - Enter Title: "Missing Category"
   - Leave Category unselected
   - Attempt upload
   - **Expected**: Validation error "Category is required"

### Expected Results

✅ Oversized files rejected  
✅ Unsupported file types rejected  
✅ Missing required fields validated  
✅ Clear, actionable error messages displayed

---

## Test Scenario 3: Browse and Filter Documents (P2 - Organization)

**User**: Floris Kregel (Team Lead)  
**Story**: US2 - Browse and Organize Documents  
**Success**: Can find documents using sort, filter, search

### Setup

Upload 5-6 test documents with varying:
- Categories (mix of Personal Files, Reports, Project Documents)
- Upload dates (use system time)
- File types (PDF, Word, Excel, images)

### Steps

1. **Test Sorting**
   - View documents list
   - Click "Sort by: Upload Date" → Verify newest first
   - Click "Sort by: Title" → Verify alphabetical
   - Click "Sort by: File Size" → Verify largest first
   - Click "Sort by: Category" → Verify grouped by category

2. **Test Category Filter**
   - Click "Filter by Category"
   - Select "Reports"
   - Verify only "Reports" category documents shown
   - Clear filter
   - Verify all documents return

3. **Test Search**
   - Enter "test" in search box
   - Verify documents with "test" in title or description appear
   - Verify search completes within 2 seconds
   - Clear search
   - Verify all documents return

4. **Test Date Range Filter**
   - Select "Last 7 Days" filter
   - Verify only recent documents shown
   - Select "Last 30 Days"
   - Verify more documents appear

### Expected Results

✅ Sorting works for all columns  
✅ Category filter shows correct subset  
✅ Search returns matching documents quickly (<2 sec)  
✅ Date range filter works correctly  
✅ Filters can be cleared to show all documents

---

## Test Scenario 4: Download and Preview (P3 - Access)

**User**: Ni Kang (Employee)  
**Story**: US3 - Download and Manage Documents  
**Success**: Can download documents and preview PDFs/images

### Steps

1. **Test Download**
   - View documents list
   - Click download icon next to a PDF document
   - Verify browser downloads file
   - Open downloaded file
   - Verify content matches uploaded file

2. **Test PDF Preview**
   - Find a PDF document in list
   - Click "Preview" button
   - Verify PDF opens in new browser tab/window
   - Verify PDF displays correctly without downloading
   - Verify URL is `/api/documents/{id}/preview`

3. **Test Image Preview**
   - Upload a JPEG or PNG image if not present
   - Click "Preview" button on image document
   - Verify image displays in browser
   - Verify no download occurred

4. **Test Unsupported Preview**
   - Find a Word or Excel document
   - Click "Preview" button
   - Verify message: "Preview not available for this file type. Click Download to view."
   - Verify Download button still works

### Expected Results

✅ Download saves file correctly  
✅ PDF preview displays in browser  
✅ Image preview displays in browser  
✅ Unsupported types show appropriate message  
✅ Authorization checked (403 for unauthorized documents)

---

## Test Scenario 5: Edit and Delete Documents (P3 - Management)

**User**: Ni Kang (Employee)  
**Story**: US3 - Download and Manage Documents  
**Success**: Can update metadata and delete owned documents

### Steps

1. **Test Edit Metadata**
   - Click "Edit" button on your document
   - Update Title to "Updated Test Document"
   - Update Description
   - Change Category
   - Add/remove Tags
   - Click "Save"
   - Verify changes reflected in list
   - Verify UpdatedDate changed

2. **Test Replace File**
   - Click "Replace File" on your document
   - Select different file with same extension
   - Click "Upload"
   - Verify new file replaces old
   - Verify metadata (title, category) preserved
   - Download and verify new file content

3. **Test Delete Document**
   - Click "Delete" button on your document
   - Verify confirmation dialog: "Are you sure you want to delete this document? This action cannot be undone."
   - Click "Cancel" → Verify document remains
   - Click "Delete" again → Click "Confirm"
   - Verify document removed from list
   - Verify file removed from AppData/uploads

### Expected Results

✅ Metadata edits save correctly  
✅ File replacement works without breaking metadata  
✅ Delete requires confirmation  
✅ Delete removes both database record and file  
✅ Cannot edit/delete documents you don't own

---

## Test Scenario 6: Project Documents (P2 + P1 - Integration)

**User**: Camille Nicole (Project Manager)  
**Story**: US1 + US2 - Upload to project and view project documents  
**Success**: Project documents visible to all project members

### Steps

1. **Upload to Project**
   - Login as Camille Nicole
   - Click "Upload Document"
   - Title: "Project Roadmap"
   - Category: "Project Documents"
   - **Project**: Select "ContosoDashboard Development"
   - Upload PDF file
   - Verify upload success

2. **View in Project Context**
   - Navigate to "Projects" page
   - Click on "ContosoDashboard Development"
   - Click "Documents" tab on project details
   - Verify "Project Roadmap" appears in project documents
   - Verify all project documents shown (not just yours)

3. **Verify Project Member Access**
   - Logout
   - Login as Ni Kang (project team member)
   - Navigate to project details
   - Click "Documents" tab
   - Verify you can see "Project Roadmap" uploaded by Camille
   - Click Download → Verify you can access it

4. **Verify Non-Member Cannot Access**
   - Logout
   - Login as System Administrator (not a project member)
   - Try to download document directly: `/api/documents/{id}/download`
   - **Expected**: 403 Forbidden (unless Administrator role has override)

### Expected Results

✅ Documents upload to projects successfully  
✅ Project documents appear in project view  
✅ All project members can access project documents  
✅ Non-members cannot access project documents (except admins)  
✅ File path includes projectId: `AppData/uploads/{userId}/{projectId}/{guid}.pdf`

---

## Test Scenario 7: Document Sharing (P4 - Collaboration)

**User**: Ni Kang (Employee)  
**Story**: US4 - Share Documents with Team Members  
**Success**: Shared documents appear in recipient's "Shared with Me"

### Steps

1. **Share a Document**
   - Login as Ni Kang
   - Find a personal document you uploaded
   - Click "Share" button
   - Select user: "Floris Kregel"
   - Click "Share"
   - Verify success message: "Document shared with Floris Kregel"

2. **Verify Recipient Notification**
   - Login as Floris Kregel
   - Check notifications (bell icon)
   - Verify notification: "Ni Kang shared a document with you: {DocumentTitle}"
   - Click notification
   - Verify redirected to document or "Shared with Me" page

3. **View Shared Document**
   - Navigate to "Shared with Me" section
   - Verify Ni Kang's document appears
   - Verify shows: Title, Shared By, Shared Date
   - Click Download → Verify you can access it
   - Verify you CANNOT delete it (not the owner)

4. **Test Share Restrictions**
   - Try to edit the shared document metadata
   - **Expected**: Only download/preview available, no edit/delete buttons
   - Verify owner (Ni Kang) can still edit/delete their original

### Expected Results

✅ Document sharing creates DocumentShare record  
✅ Recipient receives in-app notification  
✅ Shared documents appear in "Shared with Me"  
✅ Recipients can download but not delete shared documents  
✅ Original owner retains full control

---

## Test Scenario 8: Task Integration (P5 - Integration)

**User**: Ni Kang (Employee)  
**Story**: US5 - Integrate with Tasks and Dashboard  
**Success**: Documents attach to tasks and appear on dashboard

### Steps

1. **Attach Document from Task Page**
   - Navigate to "Tasks" page
   - Click on an assigned task
   - Click "Attach Document" button
   - Either:
     a) Select existing document from list, OR
     b) Upload new document (auto-links to task)
   - Verify document appears in "Related Documents" section of task

2. **Verify Auto-Project Association**
   - Upload document from task page
   - If task has ProjectId, verify document automatically assigned to that project
   - Navigate to project documents
   - Verify document appears in project documents list

3. **Test Dashboard Widgets**
   - Navigate to Dashboard (home page)
   - Locate "Recent Documents" widget
   - Verify shows last 5 uploaded documents
   - Verify shows: Title, Upload Date
   - Click document title → Navigate to documents page

4. **Test Document Count**
   - Locate dashboard summary cards
   - Verify "My Documents" card shows total count
   - Upload a document
   - Refresh dashboard
   - Verify count incremented

### Expected Results

✅ Documents attach to tasks successfully  
✅ Task-uploaded documents auto-link to task's project  
✅ Dashboard shows recent documents widget  
✅ Dashboard shows accurate document count  
✅ Task detail page shows related documents

---

## Test Scenario 9: Role-Based Access (Security)

**Users**: All roles  
**Story**: Authorization and Security  
**Success**: Role-based permissions enforced

### Steps

1. **Employee Access**
   - Login as Ni Kang (Employee)
   - Upload personal document
   - Upload document to assigned project
   - **Cannot**: Upload to projects not assigned to
   - **Cannot**: Delete other users' documents
   - **Cannot**: Access documents not shared with you

2. **Team Lead Access**
   - Login as Floris Kregel (Team Lead)
   - View team members' documents (if feature enabled)
   - Upload to own projects
   - **Cannot**: Delete documents not owned

3. **Project Manager Access**
   - Login as Camille Nicole (Project Manager)
   - Upload to managed projects
   - Delete documents in managed projects
   - View all documents in managed projects

4. **Administrator Access**
   - Login as System Administrator
   - View all documents (audit capability)
   - Access document usage reports
   - Full access for compliance/audit purposes

### Expected Results

✅ Employees can only access own/shared/project documents  
✅ Team Leads have visibility into team documents  
✅ Project Managers manage all documents in their projects  
✅ Administrators have full audit access  
✅ 403 Forbidden returned for unauthorized access attempts

---

## Performance Testing

### Upload Performance

- Upload 25 MB PDF → Should complete within 30 seconds
- Upload 10 MB Word document → Should complete within 15 seconds
- Upload 2 MB image → Should complete within 5 seconds

### List Performance

- Load page with 100 documents → Should load within 2 seconds
- Load page with 500 documents → Should load within 2 seconds
- Apply filter with 500 documents → Should respond within 1 second

### Search Performance

- Search 500 documents → Results within 2 seconds
- Search with multiple filters → Results within 2 seconds

### Preview Performance

- Preview 10 MB PDF → Display within 3 seconds
- Preview 2 MB image → Display within 1 second

---

## Edge Cases Testing

1. **Duplicate Filenames**: Upload two files with same name → Verify both saved (GUID-based paths)
2. **Special Characters**: Upload file with name "Test (2) [Final].pdf" → Verify handled correctly
3. **Concurrent Edits**: Two users edit same document metadata simultaneously → Verify last write wins
4. **Project Deletion**: Delete project with documents → Verify documents kept but ProjectId set to null
5. **Large Tags**: Add 500 characters of tags → Verify saved and searchable
6. **Empty Search**: Search with no results → Verify "No documents found" message
7. **Storage Path Traversal**: Attempt to access `../../../` in path → Verify blocked

---

## Cleanup

After testing:
- Delete test documents via UI or database
- Clear AppData/uploads/test files
- Reset database if needed: `sqllocaldb stop/delete mssqllocaldb`

---

## Success Criteria

Feature is ready when:
- ✅ All 9 test scenarios pass
- ✅ No console errors during normal operations
- ✅ Performance targets met (<2 sec lists, <30 sec uploads)
- ✅ Authorization enforced (403 for unauthorized access)
- ✅ No orphaned files in AppData/uploads
- ✅ No database constraint violations
- ✅ User satisfaction: "Upload is simple and intuitive"
