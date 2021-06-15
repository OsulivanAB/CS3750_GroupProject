﻿using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stripe;

namespace cs3750LMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly cs3750Context _context;
        public StudentController(cs3750Context context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
                UserSession session = new UserSession
                {
                    Email = userFound.Email,
                    FirstName = userFound.FirstName,
                    LastName = userFound.LastName,
                    Birthday = userFound.Birthday,
                    AccountType = userFound.AccountType
                };
                if (userFound.AccountType == 0)
                {
                    //set enrollment object for next pass
                    Enrollments enrollment = new Enrollments
                    {
                        EnrollmentList = _context.Enrollments.Where(x => x.studentID == userFound.UserId).ToList()
                    };

                    Instructors instructors = new Instructors
                    {
                        InstructorUsers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                        InstructorList = new List<Instructor>()
                    };

                    for (int i = 0; i < instructors.InstructorUsers.Count; i++)
                    {
                        Instructor newInstructor = new Instructor();
                        newInstructor.UserId = instructors.InstructorUsers[i].UserId;
                        newInstructor.FirstName = instructors.InstructorUsers[i].FirstName;
                        newInstructor.LastName = instructors.InstructorUsers[i].LastName;
                        instructors.InstructorList.Add(newInstructor);
                    }

                    Courses courses = new Courses
                    {
                        CourseList = _context.Courses.ToList(),
                        CourseInstructors = instructors.InstructorList
                    };

                    Courses studentCourses = new Courses
                    {
                        CourseList = new List<Course>()
                    };

                    for (int i = 0; i < courses.CourseList.Count; i++)
                    { 
                        for (int j = 0; j < enrollment.EnrollmentList.Count; j++)
                        {
                            if(courses.CourseList[i].CourseID == enrollment.EnrollmentList[j].courseID)
                            {
                                studentCourses.CourseList.Add(courses.CourseList[i]);
                            }
                        }
                    }

                    ViewData["Message"] = session;
                    ViewData["Courses"] = courses;
                    ViewData["StudentCourses"] = studentCourses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Register([Bind("studentID,courseID")] Enrollment passedEnrollment)
        {
            //get the session object for next pass
            User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
            UserSession session = new UserSession
            {
                Email = userFound.Email,
                FirstName = userFound.FirstName,
                LastName = userFound.LastName,
                Birthday = userFound.Birthday,
                AccountType = userFound.AccountType
            };

            //if model valid add new course
            bool success = false;

            Enrollment newEnrollment = new Enrollment
            {
                studentID = userFound.UserId,
                courseID = passedEnrollment.courseID,

            };
            
            _context.Enrollments.Add(newEnrollment);
            _context.SaveChanges();
            success = true;

            //set courses object, and success for next pass
            if (success)
            {
                session.ClassState = 0;
            }
            else
            {
                session.ClassState = 1;
            }
            //set enrollment object for next pass
            Enrollments enrollment = new Enrollments
            {
                EnrollmentList = _context.Enrollments.Where(x => x.studentID == userFound.UserId).ToList()
            };
            Instructors instructors = new Instructors
            {
                InstructorUsers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                InstructorList = new List<Instructor>()
            };

            for (int i = 0; i < instructors.InstructorUsers.Count; i++)
            {
                Instructor newInstructor = new Instructor();
                newInstructor.UserId = instructors.InstructorUsers[i].UserId;
                newInstructor.FirstName = instructors.InstructorUsers[i].FirstName;
                newInstructor.LastName = instructors.InstructorUsers[i].LastName;
                instructors.InstructorList.Add(newInstructor);
            }

            Courses courses = new Courses
            {
                CourseList = _context.Courses.ToList(),
                CourseInstructors = instructors.InstructorList
            };

            Courses studentCourses = new Courses
            {
                CourseList = new List<Course>()
            };

            for (int i = 0; i < courses.CourseList.Count; i++)
            {
                for (int j = 0; j < enrollment.EnrollmentList.Count; j++)
                {
                    if (courses.CourseList[i].CourseID == enrollment.EnrollmentList[j].courseID)
                    {
                        studentCourses.CourseList.Add(courses.CourseList[i]);
                    }
                }
            }

            ViewData["Message"] = session;
            ViewData["Courses"] = courses;
            ViewData["StudentCourses"] = studentCourses;
            return View("~/Views/Student/Register.cshtml");
        }

        public IActionResult Payment()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
                UserSession session = new UserSession
                {
                    Email = userFound.Email,
                    FirstName = userFound.FirstName,
                    LastName = userFound.LastName,
                    Birthday = userFound.Birthday,
                    AccountType = userFound.AccountType
                };
                if (userFound.AccountType == 0)
                {
                    //set enrollment object for next pass
                    Enrollments enrollment = new Enrollments
                    {
                        EnrollmentList = _context.Enrollments.Where(x => x.studentID == userFound.UserId).ToList()
                    };

                    Instructors instructors = new Instructors
                    {
                        InstructorUsers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                        InstructorList = new List<Instructor>()
                    };

                    for (int i = 0; i < instructors.InstructorUsers.Count; i++)
                    {
                        Instructor newInstructor = new Instructor();
                        newInstructor.UserId = instructors.InstructorUsers[i].UserId;
                        newInstructor.FirstName = instructors.InstructorUsers[i].FirstName;
                        newInstructor.LastName = instructors.InstructorUsers[i].LastName;
                        instructors.InstructorList.Add(newInstructor);
                    }

                    Courses courses = new Courses
                    {
                        CourseList = _context.Courses.ToList(),
                        CourseInstructors = instructors.InstructorList
                    };

                    Courses studentCourses = new Courses
                    {
                        CourseList = new List<Course>()
                    };

                    for (int i = 0; i < courses.CourseList.Count; i++)
                    {
                        for (int j = 0; j < enrollment.EnrollmentList.Count; j++)
                        {
                            if (courses.CourseList[i].CourseID == enrollment.EnrollmentList[j].courseID)
                            {
                                studentCourses.CourseList.Add(courses.CourseList[i]);
                            }
                        }
                    }

                    ViewData["Message"] = session;
                    ViewData["Courses"] = courses;
                    ViewData["StudentCourses"] = studentCourses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        public ActionResult Create(PaymentIntentCreateRequest request)
        {
            var paymentIntents = new PaymentIntentService();
            var paymentIntent = paymentIntents.Create(new PaymentIntentCreateOptions
            {
                Amount = CalculateOrderAmount(request.Items),
                Currency = "usd",
            });
            return Json(new { clientSecret = paymentIntent.ClientSecret });
        }

        private int CalculateOrderAmount(Item[] items)
        {
            // Replace this constant with a calculation of the order's amount
            // Calculate the order total on the server to prevent
            // people from directly manipulating the amount on the client
            return 1400;
        }
        public class Item
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class PaymentIntentCreateRequest
        {
            [JsonProperty("items")]
            public Item[] Items { get; set; }
        }
    }
}
