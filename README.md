# sharp-store
F# Web App experiment with DDD

## Develop

1. Install dotnet tools
 
        dotnet tools restore
    
2. Restore nuget packages

       dotnet nuget restore

3. Build

       dotnet build

4. Run

       dotnet run --project SharpStore.Web

5. Or run with watch

       dotnet watch run --project SharpStore.Web

6. Or use configured launch settings and run from IDE

## Code formatting (Fantomas)

Project uses Fantomas for code formatting and styles.

To format all files:

    dotnet fantomas .
