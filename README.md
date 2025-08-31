# Scb Driver Sample - README
This README provides instructions for running the **AstraDB SCB (Secure Connect Bundle) Driver
Sample** project. The sample demonstrates how to connect to an Astra database using the C#
Cassandra driver with token-based authentication and perform basic CRUD operations.

## Prerequisites
- Install .NET 8.0 SDK or later.
- Download the Secure Connect Bundle (SCB) for your Astra database from the DataStax Astra
portal.
- Obtain a valid Astra Database Token.
- Update the `appsettings.json` file with your SCB path, token, and keyspace name.

## Project Structure
The sample project has the following structure: - **ScbSdkSample.csproj** : Project configuration file
with dependencies. - **Program.cs** : Entry point demonstrating CRUD with Astra via SCB. -
**ScbSessionHandle.cs** : Session factory wrapper for managing cluster lifecycle with token-based
authentication. - **UserDoc.cs** : Example document model for table records. - **TablePrinter.cs** :
Utility to pretty-print rows in a tabular format. - **appsettings.json** : Configuration file for SCB path,
token, and keyspace.

## Running the Sample
- Open a terminal in the project root folder.
- Run `dotnet build` to restore dependencies and build the project.
- Ensure your `appsettings.json` has the correct values for SCB path, token, and keyspace.
- Run the sample with `dotnet run`.
- The program will connect to Astra, create a table if not exists, and perform insert, read, update, and
delete operations.
- The results will be printed to the console in both JSON and table formats.

## Notes
- Token authentication is used via `.WithCredentials("token", token)`.
- The driver does not return modified or deleted counts. To verify updates/deletes, re-read the row.
- CQL `INSERT` acts as an upsert; for conditional writes, use LWT (`IF EXISTS` / `IF NOT EXISTS`).
- Dispose of the cluster/session properly by using `ScbSessionHandle`.