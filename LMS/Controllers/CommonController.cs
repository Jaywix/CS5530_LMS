using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {            
            var query = from d in db.Departments
                        select new { name = d.Name, subject = d.Subject };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {            
            var query = from d in db.Departments
                        select new
                        {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = (from c in db.Courses
                                      where c.Department == d.Subject
                                      select new { number = c.Number, cname = c.Name })
                                      .ToList()
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {            
            var query = from o in db.Classes join c in db.Courses
                        on o.Listing equals c.CatalogId
                        where c.Department == subject && c.Number == number
                        select new
                        {
                            season = o.Season,
                            year = o.Year,
                            location = o.Location,
                            start = o.StartTime.ToString(),
                            end = o.EndTime.ToString(),
                            fname = o.TaughtByNavigation.FName,
                            lname = o.TaughtByNavigation.LName
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {   
            var query = from a in db.Assignments join ac in db.AssignmentCategories
                        on a.Category equals ac.CategoryId join cl in db.Classes
                        on ac.InClassNavigation.ClassId equals cl.ClassId join co in db.Courses
                        on cl.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                        && ac.Name == category && a.Name == asgname
                        select a.Contents;
            // if there is no such assignment, return the empty string
            if (query.Count() == 0)
            {
                return Content("");
            }
            return Content(query.First());
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {            
            var query = from s in db.Submissions join a in db.Assignments
                        on s.Assignment equals a.AssignmentId join ac in db.AssignmentCategories
                        on a.Category equals ac.CategoryId join cl in db.Classes
                        on ac.InClassNavigation.ClassId equals cl.ClassId join co in db.Courses
                        on cl.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                        && ac.Name == category && a.Name == asgname && s.Student == uid
                        select s.SubmissionContents;
            // if there is no such submission, return the empty string
            if (query.Count() == 0)
            {
                return Content("");
            }
            return Content(query.First());
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {   
            // query for students, because there are probably more students than professors or administrators, so this is more likely to find a match if the user exists.
            // if null then query for professors, if null then return {success: false} because if the user is an Administrator, this field is not present in the returned JSON
            var query = from s in db.Students
                        where s.UId == uid
                        select new { fname = s.FName, lname = s.LName, uid = s.UId, department = s.MajorNavigation.Name };
            if (query.Count() == 0)
            {
                query = from p in db.Professors
                        where p.UId == uid
                        select new { fname = p.FName, lname = p.LName, uid = p.UId, department = p.WorksInNavigation.Name };
                if (query.Count() == 0)
                {
                    return Json(new { success = false });   
                }
            }
            return Json(query.First());
        }


        /*******End code to modify********/
    }
}

