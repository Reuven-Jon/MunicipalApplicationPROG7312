# MunicipalApplicationPROG7312
## 📌 Project Overview

This Windows Forms application was developed as part of the PROG7312 Portfolio of Evidence (POE) — Parts 1 and 2.
It allows residents to report municipal service issues, attach media, and track the progress of submitted issues through a dynamic and interactive user interface.

The project showcases C# programming mastery through the integration of multiple advanced data structures (LinkedList, Queue, Stack, Dictionary), a modular architecture, and user-friendly UI/UX enhancements.

## 🚀 Features

### Issue Reporting

Capture location, category, and description of a municipal fault

Attach/remove files with displayed path

User Engagement Strategies

POPIA-aligned consent checkbox (mandatory before submission)

Progress bar fills as each field is completed

### 📊 Issue Tracking & Status Form (Part 2)

The Part 2 implementation introduced the Status Form, which demonstrates practical use of multiple C# collections and data management techniques.

Key Functionalities:

View all reported issues in a data grid.

Track and update issue statuses (e.g., Pending → Resolved).

Rebuild and refresh indexes dynamically from the shared IssueStore.

Search issues by Ticket ID for O(1) lookups.

View recently accessed issues (Stack) and pending ones (Queue).

Update UI counters in real-time to reflect active data structure states.

Data Structure Demonstration:
Data Structure	Purpose	Implementation
Dictionary<Guid, Issue>	O(1) lookup of issues by unique ID	_index
Queue<Guid>	FIFO tracking of pending issues	_pending
Stack<Guid>	LIFO tracking of recently viewed issues	_recentLookups
LinkedList<Issue>	Ordered in-memory storage of all issues	IssueStore.Instance.All()
LinkedList<string>	Attachment path storage	Issue.Attachments
Additional Enhancements:

Grid auto-refresh for updated data.

Counters and labels to display number of issues, pending, and viewed.

Error handling for invalid searches or missing issues.

Modular architecture with separate UI, Domain, and Persistence layers.

Red ❌ icons highlight empty required fields on startup

Success dialog with ticket ID and status

Multilingual support (English, isiXhosa/isiZulu, Afrikaans)

Font size adjustment for accessibility

Design Enhancements

Light/Dark mode toggle

Accessible font resizing

Multilingual UI options

Data Structures

LinkedList<Issue> for storing issues

Dictionary<Guid, LinkedListNode<Issue>> for fast issue lookups

Queue<Guid> for pending issues

Stack<Guid> for recent activity tracking

LinkedList<string> for attachments

## 🛠️ Installation & Setup

Clone or download this repository.

Open the solution file in Visual Studio 2022.

Build the solution with target framework .NET 8.0 (Windows).

Run the application via Ctrl + F5.

## 📖 Usage

Part 1: Report an Issue

Open the app → Main Menu loads.

Click Report Issue.

Enter:

Location

Category

Description

Attach optional media files.

Tick POPIA Consent checkbox.

Watch the progress bar fill as you complete fields.

Click Submit → success message displays with Ticket ID.

Change Font and Language functionality

Part 2: Check Issue Status

From the Main Menu, open Issue Status.

View all current issues in the data grid.

Search for an issue using its Ticket ID.

Review issue details and current status.

The Queue, Stack, and Dictionary update automatically as you interact with the form.

## 👨‍💻 Developer Information

Name: Reuven-Jon Kadalie

Student Number: ST10271460

Module: PROG7312

## ⚖️ Notes

No database is required for this submission.

All data is stored temporarily in memory using advanced data structures.

A demo video accompanies this submission to verify functionality.
