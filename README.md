# GitHubVS2013
This is the LibrarySite web site created with C# ASP.NET MVC5 and EntityFrames 6

This web site was created with Visual Studio Express 2013 for Web.
To run this on your PC, need to download the master branch and then open solution file, LibrarySite.sln, with Visual Studio Express 2013 
for Web.
Only the projects LibrarySite and LibarySite.Test were utilized.

The repository, LibrarySiteDatabaseFiles, contains the SQL Server Database files, library.ldf and library.mdf.
SQL Server 2016 Express was utilized.  
You will need to download these files to the subdirectory, MSSQL/DATA, under the Microsoft SQL Server\... main folder.
And then open Microsoft SQL Server Management Studio and right click on Databases and select Attach in the context menu.
Then click the Add button and select the library.mdf on the right hand side of the Attach Databases dialog window.

Also you will need to change connectonstrings in the main web.config (one located at the bottom) in the LibrarySite project. 
The Data Source in both connection strings will need to point to your SQL Server database name.
