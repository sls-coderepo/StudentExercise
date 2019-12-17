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
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, c.Id AS CohortId, c.Name AS CohortName, e.Id AS ExerciseId, e.Name AS ExerciseName, e.Language FROM Student s 
                                        INNER JOIN Cohort c ON s.CohortId = c.Id
                                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId ";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
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
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>()

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
                            if (hasExercise)
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

        [HttpGet]
        [Route("filteredStudent")]
        public async Task<IActionResult> GetFilteredStudent([FromQuery]int? cohortId, [FromQuery]string lastName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                                        Id, 
                                        FirstName, 
                                        LastName, 
                                        SlackHandle, 
                                        CohortId 
                                        FROM Student
                                        WHERE 1=1";
                    if (cohortId != null)
                    {
                        cmd.CommandText += " AND CohortId = @cohortId";
                        cmd.Parameters.Add(new SqlParameter("@cohortId", cohortId));
                    }
                    if (lastName != null)
                    {
                        cmd.CommandText += " AND LastName LIKE @lastName";
                        cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));
                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> allStudents = new List<Student>();
                    while (reader.Read())
                    {
                        Student stu = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                        allStudents.Add(stu);
                    }
                    reader.Close();
                    return Ok(allStudents);
                }
            }
        }

        [Route("studentWithExercise")]
        public async Task<IActionResult> GetAllStudentsWithExercises(string? include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, c.Id AS CohortId, c.Name AS CohortName, e.Id AS ExerciseId, e.Name AS ExerciseName, e.Language FROM Student s 
                                        INNER JOIN Cohort c ON s.CohortId = c.Id
                                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId 
                                        WHERE 1=1";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
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
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>()

                                }

                            };
                            students.Add(student);
                            if (include != null && include == "exercise")
                            {
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

                        }
                        else
                        {
                            if (include != null && include == "exercise")
                            {
                                if (hasExercise)
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
                    }
                    reader.Close();
                    return Ok(students);
                }

            }

        }
    }
}
