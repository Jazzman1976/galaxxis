
using System;

/// <summary>
/// To update from database open Nuget Manager Console and execute:
/// Scaffold-DbContext "Server=HAWK\SQLEXPRESS;Database=GalaxxisPrivat;User Id=GalaxxisPrivatUser;Password=***REMOVED***;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context GalaxxisContext -force
/// Scaffold-DbContext "Server=Wall-E;Database=Galaxxis;User Id=GalaxxisUser;Password=***REMOVED***;" Microsoft.EntityFrameworkCore.SqlServer -NoPluralize -OutputDir Models -Context GalaxxisContext -force
/// </summary>
namespace Galaxxis.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Currently no actions available. DB Actions will be added here.");
        }
    }
}
