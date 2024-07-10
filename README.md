# STSL.SmartLocker.Utils

## Index

[Overview](#overview) \
[Directory Structure](#top-level-directory-structure) \
[Solution Structure](#solution-and-projects-structure) \
[Quick start guide](#quick-start-guide) \
[In-Depth configuration](#project-configuration-deep-dive)

## TODO

> * Break this document into 2 separate documents; one that describes this solution specifically
> and one that describes the general procedures, structure and conventions used across STSL solutions.
> All individual solutions should link to the general guide.

## Overview

This solution is the first iteration of STSL's SmartLoc software offering, aimed
at meeting the specific needs of UCLH whilst also aiming to lay the groundworks for
a more widely applicable multi tenanted solution.

This document will cover information relevant to all developers and should be kept
up to date with the current state of the solution.

### Conventions used

*Italics - used to denote the name of an existing directory or file.
Also used to denote a navigational headings or exact text when giving instructions*

```text
code blocks - code that should be entered exactly e.g. scripts or full command line options
```

`inline code - similar to above but used for very short commands or single options`

'~' - Refers to the top level directory which contains the solution and is the topmost directory
of the git repository. For example: *\~/Source* is where all the solution projects are kept

> __Note:__ This gives supplementary information, tips or tricks that may not be directly related
> to the current topic but can still be of some benefit.

---

## Top level directory structure

### *Source*

All source code arranged into projects - the structure of which is detailed in the next section.

### *Database*

Schema, seeding & utility scripts for individual database providers.

### *Infrastructure*

Files relating to deployment, infrastructure as code or containerization related files.
Docker compose files may be kept here as well as other infrastructure specific configuration files
or scripts.

### *Notes*

Any additional information related to the project
e.g. information about functional requirements, client data schemas and examples,
notes about deployment & permissions, hardware requirements, or any other useful information.

---

## Solution and projects structure

All projects are prefixed with 'STSL.' followed by the namespace of the solution,
in this case SmartLocker.

### .Api

C# ASP.NET Core RESTful API, this depends directly on the service contracts
(in .Data.Services.Contracts)
and indirectly on their implementations (.Data.Services) through dependency injection.
The Api depends on a 'Data.(provider)' project for setting up the database
(currently Sql Server).
This project also depends on the Common & DTO projects.

### .CLI

Command line interface tools, used commonly for accessing API endpoints through scripts and for
transforming data feeds. Useful for administrators who wish to perform bulk operations and have repeated
custom behaviour.
May depend directly on API or on service layer depending on the project. An API can be desirable when
used to maintain a single source of truth or manage authorization.
Direct service layer access often allows simpler project integration and greater flexibility.

### .Common

Holds data and helpful functions common across most projects. Enums, exceptions and simple domain
related data primitives are kept here as well as utility extension methods and global helper classes.
Does not depend on any other projects.

### .Data

Defines the shape and configuration of the EF Core context without any provider specific detail.
May also contain helper functions for querying or updating the context and any
value converters for converting simple domain primitives to C# primitives.
The only assumption made about the provider is a relational database using a compatible EF Core connector.
Depends directly on .Domain to specify the database entities.

### .Data.Services

The service implementation layer that provides high level domain functionality.
This project houses the bulk of the code that makes the solution work!
Generally .Domain data objects should not escape this service layer, interactions
with the database, third party libraries, cloud and hardware connectors should be managed
through service implementations.
Depends directly on .Common, .Data.Contracts, .DTO and .Domain.

### .Data.Service.Contracts

The service contract layer provides a declaration of abstractions that other projects can depend upon.
Dependency inversion gives us the ability to use these common interfaces without relying
on specific implementations making it simpler to extend our codebase.
Depends directly on .DTO and .Common

### .Data.(Provider) - (currently SqlServer)

The database provider project allows us to switch out database providers without having to
make changes at the DbContext level. This project contains Migrations specific to the provider
and any provider specific concerns like exception handlers or configuration information.
Depends on .Data and potentially .Common and .Data.Services.Contracts.

### .Domain

Houses the C# POD / POCO domain data objects that EF Core will use to represent the database.
Entity relationships and data structure are defined here.
Depends on .Common only.

### .DTO

The data transfer object layer, provides data structures that can be passed between the service layer
and any client facing applications. DTOs will often closely resemble domain data objects but can be useful
for hiding implementation detail and creating aggregate or wrapped entities where useful or appropriate.
Depends on .Common only.

### .Tests

Automated unit tests of the service layer live here, as can end to end tests of the API controllers or CLI methods.
Depends on projects that require testing!

### .WebClient

A graphical user interface in the form of a single page application running in the user's web browser.
This project will be built into static files that are served from a web server and used to access
API functionality in a simple and intuitive manner.
Using Angular, Typescript and various frontend technologies.
This project has no direct dependencies on the others but does depend indirectly on access to the API
through a secure (HTTPS) connection.

---

## Quick start guide

### Setting up the Database

Create a new database in MSSQLServer, the name of your database should match the connection string in
*appsettings.Development.json* (currently *"STSL.SmartLocker"*).
This can be done within Visual Studio using the "SQL Server Object Explorer" panel or by using a program
such as Sql Server Management Studio.

> TODO: Provide simple guide for creating new database?

Once your database has been created, you will need to run 2 sets of migrations - 1 for each schema (slk & slkmart).

Firstly create the slk tables by opening a terminal window in the *STSL.SmartLocker.Utils.Data.SqlServer* folder,
and running the following script:

```powershell
dotnet ef database update --connection "Server=(localdb)\mssqllocaldb;Database=STSL.SmartLocker;Trusted_Connection=True;" --context SmartLockerSqlServerDbContext
```

(For more information read the [Migrations](#migrations) section below.)

Secondly to create the slmart tables, views and stored procedures run the *STSL.SmartLocker.Utils.Reporting.Data.Deploy*
project. This will use DbUp to run the required SQL schema update scripts.

#### Starting the WebClient

Ensure you have NodeJS installed (anything above version 16 should suffice).
Navigate to the *STSL.SmartLocker.Utils.WebClient* project folder and run
`npm install` from a command line to install project dependencies.
To start the project; run `npm run start`.

#### Starting the Api

##### Alongside the UI

Read the [Auth](#authentication-and-authorization) section below in full.

##### Using Swagger

> TODO: Setup swagger simplified build using user-jwts + appsettings.Swagger.json

#### Creating a tenant (Production)

To get started using the endpoints in Release; first create a dummy tenant that you will use
by posting to the tenant endpoint. This requires the *maintain:tenants*
permission (available only to the super-user role) in Release, alternatively
this can be done by manually creating a new tenant in your database.
Another way to easily create a new tenant is by using the Swagger UI with the Development method.
If successful the endpoint should return the created tenant data including the Id.
This Id must be [added to your Auth0 user](#adding-a-tenant) under *app_metadata*.

---

## Project configuration deep dive

### Authentication and authorization

#### Auth Overview

The *STSL.SmartLocker.Utils.Api* project has some subtle differences depending on how it is built and run.
In development; Authorization rules are relaxed to making testing easier. This means anyone calling
the Api can access any tenant's data and can call any endpoint as permissions & tenant checks are disabled.

There are 2 methods of building & running the project;

* Development: Using the *Debug* build target with the *Development* startup configuration.
* Production: Using the *Release* build target with the *Production* startup configuration.

The Development method uses user-jwts as the JWT issuer when calling Api endpoints
and settings are loaded from *appsettings.Development.json*
(due to the *ASPNETCORE_ENVIRONMENT* environment variable being set to Development).

The Production method uses Auth0 as the JWT issuer and signing authority,
and settings are loaded from *appsettings.json*
(due to the *ASPNETCORE_ENVIRONMENT* environment variable being set to Production).

It is important that any code deployed in a public facing setting (production or staging) is using
the Release build target, has set *ASPNETCORE_ENVIRONMENT=Production* and that settings in *appsettings.json*
are correct.

#### Using 'user-jwts' in Debug

The command line utility '[user-jwts](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-7.0&tabs=windows)'
can be used during development to create JWT bearer tokens allowing authorized use of the API
from the Swagger UI or using Postman etc.

Navigate to the API project directory *~/Source/STSL.SmartLocker.Api* in your terminal
and run the following:

```powershell
dotnet user-jwts create  --scope "read:tenants maintain:tenants read:locations maintain:locations read:locker-banks maintain:locker-banks read:lockers maintain:lockers read:locks maintain:locks read:card-holders maintain:card-holders read:card-credentials maintain:card-credentials" --audience https://smart-locker.dev.stsl.co.uk/api/v1 --claim "https://smart-locker.dev.stsl.co.uk/roles=super-user"
```

This will print out a JWT that will allow you to call any API endpoint.

#### Using Auth0 in Release to manually setup users

Auth0 has been configured to add a role based permissions claim to the Bearer JWT and will also
add a tenants claim from the Auth0 User metadata.
To setup Auth0 you must have the appropriate privileges.

##### Adding a role

Once logged into Auth0 use the sidebar on the left to
navigate to [*Users* under the *User Management* tab](https://manage.auth0.com/dashboard/eu/stsl-dev/users), select the user,
then select *Roles*. Click *Assign Roles* and select the role from the dropdown list.
There are currently 3 roles all prefixed with <https://smart-locker.dev.stsl.co.uk/>:

*super-user* \
*installer* \
*locker-bank-admin*

##### Adding a tenant

Once logged into Auth0 use the sidebar on the left to
navigate to [*Users* under the *User Management* tab](https://manage.auth0.com/dashboard/eu/stsl-dev/users), select the user,
then scroll down to the *Metadata* heading and enter the JSON data in the *app_metadata* text box.

Tenants should be added as an array of strings of valid GUID format
using the property "*gotoSecureTenants*" e.g.

```json
{
  "gotoSecureTenants": [
    "5010a07f-a98e-4e35-b40c-08db34ec3e09",
    "e6c18c44-964d-466c-b558-4f1e9c57d678",
    "eaa77365-3a77-44a9-8ebc-887bf2503fa7"
  ]
}
```

Be sure to click save after making any changes.

[Auth0 Sidebar Navigation](Notes/Images/auth0_sidebar.png) \
[Auth0 Adding User Roles](Notes/Images/auth0_user_roles.png) \
[Auth0 Adding User Tenants](Notes/Images/auth0_user_tenants.png)

### Migrations

Migrations can be managed with the ef core tools CLI which can be installed by running

```powershell
dotnet tool install --global dotnet-ef
```

#### Creating a migration

Migrations are created specific to the database provider e.g. Sql Server, Postgres MySQL / MariaDB.

To  create a new migration; navigate to the provider project folder (e.g. Data.SqlServer)
in your command line and run:

```powershell
dotnet ef migrations add {Migration Name} --context {Provider Derived Context}
```

Migrations should be named using PascalCase e.g.

*InitialCreate*, \
*AddXToY*, \
*DescriptiveMigrationNamingPreferred*

When creating a new migration it is also useful to create a script that can be used to directly
update a database in production or on a staging server.

The top level *Database* directory contain these scripts organized by provider in the *Schema* directory
e.g. *~/Scripts/SqlServer/Schema*.

To create a script for the latest migration use the `dotnet ef migrations script {Name of previous Migration} {Name of latest Migration}` command.

Scripts should be named using the same name as their migration
and should be prefixed with 4 digits that indicate the order in
which they should run i.e. the following scripts would be run in sequence due to their prefix:

*0000-initial-create.sql* \
*0005-added-table-x.sql* \
*0010-updated-column-y-in-x.sql* \
*0012-changed-type-of-y-z-in-x.sql*

To create the idempotent script:

```powershell
cd Source\STSL.SmartLocker.Utils.Data.SqlServer
dotnet ef migrations script --idempotent --output ..\..\Database\SqlServer\Schema\IdempotentFull.sql
```

#### Applying migrations

To run migrations against a database, first ensure the database exists for the provider the project is using
(SqlServer at time of writing) and that you have the necessary user permissions to make changes to the schema.

To update a database using particular provider migrations;
navigate to the relevant provider project directory (e.g. Data.SqlServer) and run:

```powershell
dotnet ef database update --connection "{Database Connection String}" --context {Provider Derived Context}
```

> __Note:__ If you are copying a connection string from your *appsettings.json* file be sure to
> remove escape sequences e.g.
>
> In *appsettings.json*: \
> `Server=(localdb)\\mssqllocaldb;Database=STSL.SmartLocker.Utils`
>
> In dotnet ef CLI: \
> `Server=(localdb)\mssqllocaldb;Database=STSL.SmartLocker.Utils`
>
> Notice the single backslash in the CLI command versus the double backslash in the *appsettings.json*.
> *appsettings.json* strings, like regular C# strings require escaping certain characters, this is not required
> for the ef CLI commands.

This will update the database to include changes from all the latest migrations.
To see options on how to customize the behaviour of ef tools, simply append `--help`
to any command to get an overview of options e.g.

```powershell
dotnet ef migrations script --help
```

### STSL.SmartLocker.Utils.CLI

#### User Secrets

There is one user secret that must be set to authenticate against the default endpoint \
(note that appsettings.json already contains the URL and organisation)

From the project directory run

```powershell
dotnet user-secrets set EndpointOptions:Password "{password}"
```

#### CLI Usage

To see a brief overview of the available commands run the cli with ```--help```

##### Get

You can get a full view of a locker config by running

```powershell
CLIv1 get {locker-address}
```

or a simplified view of the current locker state by adding the ```-s``` option.

##### Patch

You can partially update the locker config with a JSON file. The schema for this file is found in the Examples folder.

It is possible to specify a single locker at a time or multiple lockers depending on the format you use and options passed to the CLI.

##### Put

Put will allow you to merge a partial update with the default state for a locker. \
This has the effect of running the reset command followed by patch, in a single transaction.

##### Reset

Reset will reset a locker back to it's default state.
