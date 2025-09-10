# MunicipalApplicationPROG7312
## 📌 Project Overview

This Windows Forms application was developed as part of the PROG7312 POE Part 1 submission.
It allows residents to:

Report municipal service issues

Attach and remove media files (images/documents)

Track issue submission status

Interact with user engagement features such as consent, progress tracking, and multilingual support

The project demonstrates the use of advanced C# data structures (LinkedList, Queue, Stack, Dictionary) and a professionally structured UI.

## 🚀 Features

Issue Reporting

Capture location, category, and description of a municipal fault

Attach/remove files with displayed path

User Engagement Strategies

POPIA-aligned consent checkbox (mandatory before submission)

Progress bar fills as each field is completed

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

Launch the app → the Main Menu will load.

Select Report Issue.

Enter:

Location

Category

Description

Attach media files (optional)

Tick the POPIA Consent Checkbox.

Watch the progress bar fill as you complete each field.

Submit → Success dialog appears with ticket ID & status.

Optional:

Change language in Settings

Switch between light/dark mode

Adjust font size

## 👨‍💻 Developer Information

Name: Reuven-Jon Kadalie

Student Number: ST10271460

Module: PROG7312

## ⚖️ Notes

No database is required for this submission.

All data is stored temporarily in memory using advanced data structures.

A demo video accompanies this submission to verify functionality.
