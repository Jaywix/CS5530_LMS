using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Core.Types;
using System;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
           // An example of a simple unit test on the CommonController
           CommonController ctrl = new CommonController(MakeTinyDB());

           var allDepts = ctrl.GetDepartments() as JsonResult;

           dynamic x = allDepts.Value;

           Assert.Equal( 1, x.Length );
           Assert.Equal( "CS", x[0].subject );
        }

        // [Fact]
        // public void addUserTest1()
        // {
        //    // An example of a simple unit test on the CommonController
        //    CommonController ctrl = new CommonController(MakeTinyDB());

        //    var allUsers = ctrl.createNewUser(new User { firstName = "John", lastName = "Doe", DOB = DateTime.Now }) as JsonResult;

        //    dynamic x = allUsers.Value;

        //    Assert.Equal( 1, x.Length );
        //    Assert.Equal( "CS", x[0].subject );
        // }


        /// <summary>
        /// Make a very tiny in-memory database, containing just one department
        /// and nothing else.
        /// </summary>
        /// <returns></returns>
        LMSContext MakeTinyDB()
        {
           var contextOptions = new DbContextOptionsBuilder<LMSContext>()
           .UseInMemoryDatabase( "LMSControllerTest" )
           .ConfigureWarnings( b => b.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
           .UseApplicationServiceProvider( NewServiceProvider() )
           .Options;

           var db = new LMSContext(contextOptions);

           db.Database.EnsureDeleted();
           db.Database.EnsureCreated();

           db.Departments.Add( new Department { Name = "KSoC", Subject = "CS" } );

           // TODO: add more objects to the test database

           db.SaveChanges();

           return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}