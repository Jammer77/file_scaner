# ScanApp

## Description

ScanApp is a .NET Core console application that recursively scans all files in a specified folder, calculates MD5, SHA1, and SHA256 hashes for each file, and outputs the total number of files scanned along with the elapsed time.

Optional features include storing file information in an SQLite database to avoid duplicate scans, updating scan metadata, and logging errors to a separate log file.

## Features

- Accepts folder path as a command-line argument.
- Recursively enumerates all files in the folder and subfolders.
- Calculates MD5, SHA1, and SHA256 hashes for each file.
- Displays the count of scanned files and total scanning time.
- Saves file scan info into an SQLite database (`hashes` table).
- Caches scan results to skip previously scanned files.
- Updates scan count and last seen timestamp for existing files in DB.
- Logs errors into `errors.log`.
