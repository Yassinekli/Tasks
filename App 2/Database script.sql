create database FilesUploadedDB

use FilesUploadedDB

CREATE TABLE [Uploaded Files](
	[File ID] [int] IDENTITY(1,1) primary key,
	[Title] [text] NULL,
	[Description] [text] NULL,
	[File Path] [text] NOT NULL
)


