using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {           
            var query = from e in db.Enrolleds
                        where e.Student == uid
                        select new
                        {
                            subject = e.ClassNavigation.ListingNavigation.Department,
                            number = e.ClassNavigation.ListingNavigation.Number,
                            name = e.ClassNavigation.ListingNavigation.Name,
                            season = e.ClassNavigation.Season,
                            year = e.ClassNavigation.Year,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {   
            var query = from c in db.Courses
                        where c.Department == subject && c.Number == num
                        join cl in db.Classes on c.CatalogId equals cl.Listing
                        where cl.Season == season && cl.Year == year
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                        join a in db.Assignments on ac.CategoryId equals a.Category
                        join s in db.Submissions.Where(s => s.Student == uid) on a.AssignmentId equals s.Assignment into sa
                        from sub in sa.DefaultIfEmpty()
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            score = sub != null ? sub.Score : (uint?)null
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            try
            {
                var query = from c in db.Courses
                            where c.Department == subject && c.Number == num
                            join cl in db.Classes on c.CatalogId equals cl.Listing
                            where cl.Season == season && cl.Year == year
                            join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                            where ac.Name == category
                            join a in db.Assignments on ac.CategoryId equals a.Category
                            where a.Name == asgname
                            join s in db.Submissions.Where(s => s.Student == uid) on a.AssignmentId equals s.Assignment into sa
                            from sub in sa.DefaultIfEmpty()
                            select new { sub, a.AssignmentId };

                Submission submission;

                // Already exists, so we need to update
                if (query.First().sub != null)
                {
                    submission = query.First().sub;
                    submission.SubmissionContents = contents;
                    submission.Time = DateTime.Now;

                    db.Update(submission);
                }
                // Just add a new submission
                else
                {
                    submission = new Submission
                    {

                        Assignment = query.First().AssignmentId,
                        Student = uid,
                        Score = 0,
                        SubmissionContents = contents,
                        Time = DateTime.Now

                    };

                    db.Add(submission);
                }
                

                if (db.SaveChanges() > 0)
                    return Json(new { success = true });

                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {   
            // Find the existing class
            var classQuery = from c in db.Classes
                             where c.ListingNavigation.Department == subject
                             && c.ListingNavigation.Number == num
                             && c.Season == season
                             && c.Year == year
                             select c.ClassId;

            uint classId = classQuery.FirstOrDefault();
            if (classId == 0)
            {
                return Json(new { success = false });
            }

            Enrolled enrollment = new Enrolled { Student = uid, Class = classId, Grade = "--" };    

            db.Enrolleds.Add(enrollment);

            // Save changes and return true if successful, false otherwise (already enrolled)
            if (db.SaveChanges() > 0)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }   
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {            
            return Json(null);
        }
                
        /*******End code to modify********/

    }
}

