using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Tutorial3.Models;
using Tutorial3.Models.Enrollment;
using Tutorial3.Models.Promotion;
using Tutorial3.Services;

namespace Tutorial3.Controllers
{
    
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentController : ControllerBase
    {
        private IStudentDbService _service;
        public EnrollmentController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost(Name = nameof(EnrollStudent))]
        [Route("enroll")]
        [SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
        public IActionResult EnrollStudent(AddEnrollment student)
        {
            var result = _service.EnrollStudent(student);
            if (result.Studies != null) return CreatedAtAction(nameof(EnrollStudent), result);
            return BadRequest(result.IdStudent);
        }
        
        [HttpPost(Name = nameof(Promote))]
        [Route("promote")]
        public IActionResult Promote(PromoteStudents study)
        {
            var result = _service.Promote(study);
            if (result.Semester != null) return CreatedAtAction(nameof(Promote), result);
            return BadRequest(result.Studies);
        }
        
        [HttpGet("{idStudent}", Name = "StudentGetter")]
        public IActionResult GetStudent(string idStudent)
        {
            using var sqlConnection =
                new SqlConnection(@"Server=db-mssql.pjwstk.edu.pl;Database=s18881;User Id=apbds18881;Password=admin;");
            using var command = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = "SELECT  Semester " +
                              "FROM Student, Enrollment " +
                              "WHERE IdStudent=@idStudent " +
                              "AND Student.IdEnrollment = Enrollment.IdEnrollment"
            };
            SqlParameter parameter = new SqlParameter();
            command.Parameters.AddWithValue("idStudent", idStudent);
            sqlConnection.Open();
            SqlDataReader dataReader = command.ExecuteReader();
            while(dataReader.Read()) 
                return Ok("Student(" + idStudent + ") started his/her studies in " +
                          Int32.Parse(dataReader["Semester"].ToString()) + ".");
            return NotFound("Invalid Input Provided");
        }
    }
}
