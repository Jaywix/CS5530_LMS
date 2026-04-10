using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from c in db.Courses
                        where c.Department == subject && c.Number == (uint)num
                        join cl in db.Classes on c.CatalogId equals cl.Listing
                        where cl.Season == season && cl.Year == (uint)year
                        join e in db.Enrolleds on cl.ClassId equals e.Class
                        join s in db.Students on e.Student equals s.UId

                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade
                        };


            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var query = from c in db.Courses
                        where c.Department == subject && c.Number == (uint)num
                        join cl in db.Classes on c.CatalogId equals cl.Listing
                        where cl.Season == season && cl.Year == (uint)year
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                        where category == null || ac.Name == category
                        join a in db.Assignments on ac.CategoryId equals a.Category into assign
                        from a in assign.DefaultIfEmpty()

                        select new
                        {
                            aname = a != null ? a.Name : null,
                            cname = ac.Name,
                            due = a != null ? a.Due : (DateTime?)null,
                            submissions = a != null ? a.Submissions.Count() : 0
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from c in db.Courses
                        where c.Department == subject && c.Number == (uint)num
                        join cl in db.Classes on c.CatalogId equals cl.Listing
                        where cl.Season == season && cl.Year == (uint)year
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            try
            {
                var query = from c in db.Classes
                            where c.ListingNavigation.Department == subject && c.ListingNavigation.Number == (uint)num && c.Season == season && c.Year == (uint)year
                            select c.ClassId;

                AssignmentCategory ac = new AssignmentCategory() { InClass = query.First(), Weight = (uint)catweight, Name = category };
                db.AssignmentCategories.Add(ac);

                if (db.SaveChanges() > 0)
                    return Json(new { success = true });
                else
                    return Json(new { success = false });
            }
            catch(Exception e)
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            try
            {
                var query = from c in db.Classes
                            where c.ListingNavigation.Department == subject && c.ListingNavigation.Number == (uint)num && c.Season == season && c.Year == (uint)year
                            select c.ClassId;

                var catquery = from ac in db.AssignmentCategories
                               where ac.InClass == query.First() && ac.Name == category
                               select ac.CategoryId;

                Assignment a = new Assignment() { Name = asgname, MaxPoints = (uint)asgpoints, Due = asgdue, Contents = asgcontents, Category = catquery.First() };
                db.Assignments.Add(a);
                
                if(db.SaveChanges() > 0)
                    return Json(new { success = true });
                else
                    return Json(new { success = false });
            }
            catch(Exception e)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from c in db.Courses
                        where c.Department == subject && c.Number == (uint)num
                        join cl in db.Classes on c.CatalogId equals cl.Listing
                        where cl.Season == season && cl.Year == (uint)year
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                        where ac.Name == category
                        join a in db.Assignments on ac.CategoryId equals a.Category
                        where a.Name == asgname
                        join s in db.Submissions on a.AssignmentId equals s.Assignment
                        join st in db.Students on s.Student equals st.UId

                        select new
                        {
                            fname = st.FName,
                            lname = st.LName,
                            uid = st.UId,
                            time = s.Time,
                            score = s.Score
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            try
            {
                var query = from c in db.Courses
                            where c.Department == subject && c.Number == (uint)num
                            join cl in db.Classes on c.CatalogId equals cl.Listing
                            where cl.Season == season && cl.Year == (uint)year
                            join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                            where ac.Name == category
                            join a in db.Assignments on ac.CategoryId equals a.Category
                            where a.Name == asgname
                            join s in db.Submissions on a.AssignmentId equals s.Assignment
                            where s.Student == uid
                            select s;

                query.First().Score = (uint)score;

                if (db.SaveChanges() > 0)
                    return Json(new { success = true });
                else
                    return Json(new { success = false });

            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {   
            var query = from c in db.Classes
                        where c.TaughtBy == uid
                        select new
                        {
                            subject = c.ListingNavigation.Department,
                            number = c.ListingNavigation.Number,
                            name = c.ListingNavigation.Name,
                            season = c.Season,
                            year = c.Year
                        };

            return Json(query.ToArray());
        }


        
        /*******End code to modify********/
    }
}

