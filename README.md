# UNO

## EF

~~~bash
dotnet tool update --global dotnet-ef

dotnet ef migrations add --project DAL --startup-project ConsoleApp InitialCreate
dotnet ef migrations add --project DAL --startup-project WebApp InitialCreate
~~~

## WebPage

~~~bash
dotnet tool install --global dotnet-aspnet-codegenerator
dotnet aspnet-codegenerator razorpage -m Domain.Database.Game -dc AppDbContext -udl -outDir Pages/Games --referenceScriptLibraries 
    
dotnet aspnet-codegenerator razorpage -m Domain.Database.Player -dc AppDbContext -udl -outDir Pages/Players --referenceScriptLibraries 
~~~
