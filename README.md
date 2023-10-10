# UserManagementAPI
Task: Create a ASP.NET Core Web API that will expose CRUD (Create, Read, Update, Delete) operations to manage the list of users.
## Architecture
This solution was developed on Onion architecture with the folowing stack:
### Main:
- ASP.NET Core 7.0
- Entity Framework as an ORM
### Database:
- MS SQL Server
### Libraries
- Dynamic.LINQ for filtration
- NSwag for API documentation
- Serilog as a log provider
## Possible Drawbacks/Concerns (What should reviewers look out for?)
	- 

## Testing Notes (How do we know this works & doesn't break other things)
* Created automated db creation for integration tests with dacpac service.

## Structure
* [Common](src/UserManagement.Common) - Contains entity classes and validation of exception class.
* [DataAccess Layer](src/UserManagement.DataAccess/) - Contains repository for User class.
* [UserManagementAPI](/src/UserManagement.UserManagementAPI/) - Constains ASP.NET Core WebAPI project.
* [Database](src/UserManagement.DatabaseSQL/) - Project database.
* [Unit Tests](test/UserManagement.UnitTests/) Unit tests for business logic's of Controllers.
* [Integration Tests](test/UserManagement.IntegrationTests/) for DataAccess Layer.

# How to build and run the whole solution
Need to create and deploy(dacpac, MSSS or any) a [Database](src/UserManagement.DatabaseSQL/) from this solution first

## Steps how to check
Deployment of the database for tests is automatic.
To specify another test database, enter its path in [testconfig.json](test/UserManagement.IntegrationTests/testconfig.json):
```
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=TestUserManagement.Database;Integrated Security=True;Encrypt=False"
  },
  "Database": {
    "DefaultDatabaseName": "TestUserManagement.Database",
    "DefaultDatabaseFileName": "TestUserManagement.Database.dacpac"
  }
```
It is possible to specify absolute and relative (from the compilation folder) paths.

If need, for projects [UserAPI](/src/UserManagement.UserManagementAPI/):
```
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=UserManagement.Database;Integrated Security=True"
  },
```
In configuration files: [UserAPI/appsettings.?.json](/src/UserManagement.UserManagementAPI/appsettings.Development.json)

# Credentials
To log in as a SuperAdmin :

Email: admin@admin.com
Password: admin


![Jokes Card](https://readme-jokes.vercel.app/api)
