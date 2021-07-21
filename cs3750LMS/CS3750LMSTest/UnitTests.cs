using Microsoft.VisualStudio.TestTools.UnitTesting;
using cs3750LMS.Controllers;
using System;
using cs3750LMS.Models;
using System.Transactions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using cs3750LMS.Models.Repository;
using Microsoft.Extensions.Logging;
using cs3750LMS.Models.entites;
using System.Collections.Generic;
using cs3750LMS.Models.validation;

namespace CS3750LMSTest
{
    [TestClass]
    public class UnitTests
    {
        // variables to access db and controller
        cs3750Context _context;

       private INotificationRepository _notification;
        private IHostingEnvironment Environment;
        
        ILogger<HomeController> _logger;

        // constructor of what happens at every time this class is called
        public UnitTests()
        {
            // connect to database
            var serviceProvider = new ServiceCollection()
           .AddEntityFrameworkSqlServer()
           .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<cs3750Context>();
            builder.UseSqlServer($"Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSBinEnt;USER ID=LMSBinEnt;Password=8!N4Ry3n7")
                    .UseInternalServiceProvider(serviceProvider);

            _context = new cs3750Context(builder.Options);
            _context.Database.Migrate();

         
            //TODO: need to initialize _notification here. 
      
        }

        /// <summary>
        /// This method tests to make sure that the instructor can create a course
        /// </summary>
        [TestMethod]
        public void InstructorCanCreateACourseTest()
        {
            // in a transaction scope so it will not be run in the database
            using (new TransactionScope())
            {
                // Arrange
                // start with a known instructor
                var instructor = _context.Users.Find(1025);

                // grab the instructor course count
                var instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
                var expectedInstructorCourses = instructorCourses + 1; // grab the expected result

                // define time spans
                TimeSpan startTime = new TimeSpan(21, 40, 50);
                TimeSpan endTime = new TimeSpan(21, 50, 50);

                // call instructor controller with the context and enviroment passed in
                var controller = new InstructorController(_context, Environment, _notification);

                // create a new class object
                ClassValidationAdd newClass = new ClassValidationAdd();

                // add fields
                newClass.Instructor = instructor.UserId.ToString();
                newClass.Department = 3;
                newClass.ClassNumber = "1234";
                newClass.ClassTitle = "TestClass";
                newClass.Description = "This is a test";
                newClass.Location = "Test Location";
                newClass.Credits = 4;
                newClass.Capacity = 40;
                newClass.MeetDays = "xyxyxyx";
                newClass.StartTime = startTime;
                newClass.EndTime = endTime;
                newClass.Color = "#1ecbe1";

                // Act
                // add the class in the controller
                controller.AddClassTodb(instructor.UserId, newClass);
                // find out the count again
                instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));

                // Assert
                Assert.AreEqual(instructorCourses, expectedInstructorCourses);

            } // Dispose rolls back everything.
        }

        /// <summary>
        /// This method tests to make sure that the create assignment functionality does not break.
        /// </summary>
        [TestMethod]
        public void InstructorCanCreateAnAssignmentTest()
        {
            // Set up Transaction Scope so that nothing is added to the database
            using (new TransactionScope())
            {
                // Grab user 1025 who is a test instructor in the database
                var instructor = _context.Users.Find(1025);

                // grab list of courses
                var instructorCourses = _context.Courses.Where(c => c.InstructorID.Equals(instructor.UserId)).ToList();
                // Get latest course
                var latestCourse = instructorCourses.Last();
                // Get assignment count
                int currentAssignmentCount = _context.Assignments.Count(a => a.CourseID.Equals(latestCourse.CourseID));

                // Create new assignment
                AssignmentValidationAdd newAssignment = new AssignmentValidationAdd();

                // Assign values to each field
                newAssignment.CourseID = latestCourse.CourseID;
                newAssignment.Title = "Test Assignment";
                newAssignment.Description = "This is just a test";
                newAssignment.MaxPoints = 100;
                newAssignment.DueDate = DateTime.Now.AddDays(5);
                newAssignment.DueTime = new TimeSpan(21, 40, 50);
                newAssignment.SubmitType = 1;

                // Create an instance of the Instructor controller
                InstructorController controller = new InstructorController(_context, Environment, _notification);

                // Add the Assignment in the controller
                controller.AddAssignmentTodb(newAssignment);

                // Check assignment count again
                var updatedAssignmentCount = _context.Assignments.Count(a => a.CourseID.Equals(latestCourse.CourseID));

                // Determine if everything is working.
                Assert.AreEqual(updatedAssignmentCount, currentAssignmentCount + 1);

            }
        }

        /* Start login Testing */

        [TestMethod]
        public void UserFoundExistsTest()
        {
            /////////////////Prep tests
            //Student Email should exists
            string email = "student@mail.com";
            User emailResult;

            //non-email should not-exist
            string nonEmail = "nomail";
            User nonEmailResult;

            //empty should not-exist
            string emptyMail = "";
            User emptyMailResult;

            //HomeController loginControl = new HomeController(_logger, _context);

            /////////////////////////Tests
            //test Student Email should not exist
            emailResult = HomeController.FindUserByEmail(email, _context);

            //test non-email should not-exist
            nonEmailResult = HomeController.FindUserByEmail(nonEmail, _context);

            //empty should not-exist
            emptyMailResult = HomeController.FindUserByEmail(emptyMail, _context);

            /////////////////////////Test Results
            //test student email result
            Assert.AreEqual(emailResult.Email, email);

            //test non-email result
            Assert.AreEqual(nonEmailResult, null);

            //test empty-email result
            Assert.AreEqual(emptyMailResult, null);

        }

        [TestMethod]
        public void UserFoundPasswordsAreMatchingTest() 
        {
            //////////////////Prep Tests
            ///Student info
            User student = HomeController.FindUserByEmail("student@mail.com", _context);

            //Test correct password
            string passwordStudent = "password";
            bool correctInfoResult;

            //Test wrong case
            string passwordUpperCase = "PASSWORD";
            bool upperCaseResult;

            //Test empty password
            string passwordEmpty = "";
            bool emptyResult;

            //Test wrong password same length
            string wrongPassword = "12345678";
            bool wrongPasswordResult;

            ////////////////////Tests
            //Test correct password
            correctInfoResult = HomeController.ComparePasswords(student.Password, passwordStudent);

            //Test wrong case
            upperCaseResult = HomeController.ComparePasswords(student.Password, passwordUpperCase);

            //Test empty password
            emptyResult = HomeController.ComparePasswords(student.Password, passwordEmpty);

            //Test wrong password same length
            wrongPasswordResult = HomeController.ComparePasswords(student.Password, wrongPassword);

            //////////////////Test Results
            //Test correct password
            Assert.IsTrue(correctInfoResult);

            //Test wrong case
            Assert.IsFalse(upperCaseResult);

            //Test empty password
            Assert.IsFalse(emptyResult);

            //Test wrong password same length
            Assert.IsFalse(wrongPasswordResult);

        }

        /* end Login Testing */

        /* Begin Grade Update Testing */

        /// <summary>
        /// This method tests to make sure that grades can be updated.
        /// </summary>
        [TestMethod]
        public void InstructorCanUpdateGradeTest()
        {
            // Set up Transaction Scope so that nothing is added to the database
            using (new TransactionScope())
            {
                // Grab list of submissions
                List<Submission> submissions = _context.Submissions.ToList();
                // Get last submission
                Submission lastSubmission = submissions.Last();

                GradeValidation grade = new GradeValidation();

                grade.SubmissionID = lastSubmission.SubmissionID;
                grade.AssignmentID = lastSubmission.AssignmentID;
                grade.StudentID = lastSubmission.StudentID;
                grade.SubmissionDate = lastSubmission.SubmissionDate;
                grade.SubmissionType = lastSubmission.SubmissionType;
                grade.Grade = lastSubmission.Grade;
                grade.Contents = lastSubmission.Contents;

                // Create an instance of the Instructor controller
                InstructorController controller = new InstructorController(_context, Environment, _notification);

                // Test positive grade
                int gradePos = 96;
                bool positiveGrade;

                // Test negative grade
                int gradeNeg = -3;
                bool negativeGrade;

                // Test 0 grade
                int gradeZero = 0;
                bool zeroGrade;

                ////////////////////Tests
                // Test positive grade
                grade.Grade = gradePos;
                positiveGrade = controller.UpdateGrade(grade, _context);

                // Test negative grade
                grade.Grade = gradeNeg;
                negativeGrade = controller.UpdateGrade(grade, _context);

                // Test 0 grade
                grade.Grade = gradeZero;
                zeroGrade = controller.UpdateGrade(grade, _context);

                //////////////////Test Results
                //Test correct password
                Assert.IsTrue(positiveGrade);

                //Test wrong case
                Assert.IsFalse(negativeGrade);

                //Test empty password
                Assert.IsTrue(zeroGrade);

            }
            /* End Grade Update Testing */
        }
      
        /// <summary>
        /// This method tests to make sure that the application can register users
        /// </summary>
        [TestMethod]
        public void RegisterUserTest()
        {
            // in a transaction scope so it will not be run in the database
            using (new TransactionScope())
            {
                // call home controller with the context and logger passed in
                var controller = new HomeController(_logger, _context);

                // grab the users count
                var allUsers = _context.Users.Count();
                var expectedUsers = allUsers + 1; // grab the expected result

                // create a new class object
                UserValidationSignUp newUser = new UserValidationSignUp();

                // add fields
                newUser.Email = "testing@test.com";
                newUser.FirstName = "test";
                newUser.LastName = "testing";
                newUser.Birthday = DateTime.Now.AddDays(7300);
                newUser.Password = "password";
                newUser.ConfirmPassword = "password";
                newUser.AccountType = 1;

                // Act
                // add the class in the controller
                controller.AddUserToDB(newUser);

                // find out the count again
                allUsers = _context.Users.Count();

                // Assert
                Assert.AreEqual(allUsers, expectedUsers);

            } // Dispose rolls back everything.
        }


        /// <summary>
        /// This method tests to make sure that notifications can be created
        /// </summary>
        [TestMethod]
        public void CreateNotificationTest()
        {
            // in a transaction scope so it will not be run in the database
            using (new TransactionScope())
            {
                PublicController controller = new PublicController(_context, Environment, _notification);

                var instructor = _context.Users.Find(1025);

                int PreNotiCount = _context.Notifications.Count(n => n.RecipientID == instructor.UserId);

                bool success = false;

                 success = controller.CreateNotification(instructor.UserId, 0000, "Test", "This is a Unit Test");

                int PostNotiCount = _context.Notifications.Count(n => n.RecipientID == instructor.UserId);

                Assert.IsTrue(success);
                Assert.AreEqual(PreNotiCount + 1, PostNotiCount);



            } // Dispose rolls back everything.
        }

    }
}
