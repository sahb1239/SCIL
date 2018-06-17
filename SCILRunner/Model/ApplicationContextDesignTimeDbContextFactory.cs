using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SCILRunner.Model
{
    /// <summary>
    /// Class used for setting up temp database for usage when running dotnet ef migrate commands
    /// </summary>
    public class ApplicationContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var contextBuilder = new DbContextOptionsBuilder<DataContext>();
            contextBuilder.UseSqlite("Data Source=Migrations.db");
            return new DataContext(contextBuilder.Options);
        }
    }
}
