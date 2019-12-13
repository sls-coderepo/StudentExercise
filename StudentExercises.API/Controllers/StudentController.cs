using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using StudentExercises.API.Models;
using Microsoft.AspNetCore.Http;

namespace StudentExercises.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;
        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, c.Id AS CohortId, c.Name AS CohortName, e.Id AS ExerciseId, e.Name AS ExerciseName, e.Language FROM Student s 
                                        INNER JOIN Cohort c ON s.CohortId = c.Id
                                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId ";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while(reader.Read())
                    {
                        var studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                        var studentAlreadyAdded = students.FirstOrDefault(s => s.Id == studentId);
                        var hasExercise = !reader.IsDBNull(reader.GetOrdinal("ExerciseId"));

                        if (studentAlreadyAdded == null)
                        {
                            Student student = new Student
                            {
                                Id = studentId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>(),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };
                            students.Add(student);
                           
                            if (hasExercise)
                            {

                                student.Exercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });
                            }
                        }
                        else
                        {
                            if(hasExercise)
                            {
                                studentAlreadyAdded.Exercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });
                            }
                        }
                    }
                    reader.Close();
                    return Ok(students);
                }
               
            }
           
        }
    }
}
