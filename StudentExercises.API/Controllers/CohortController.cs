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
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;
        public CohortController(IConfiguration config)
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
        public async Task<IActionResult> GetAllCohortDetail()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS CohortId, c.Name AS CohortName, s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, 
                                        s.SlackHandle AS StudentSlackHandle, s.CohortId, e.Id AS ExerciseId, e.Name AS ExerciseName, e.Language, 
                                        i.Id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS InstructorLastName, i.SlackHandle AS InstructorSlackHandle, i.CohortId FROM Cohort c 
                                        INNER JOIN Student s ON c.Id = s.CohortId
                                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId
                                        LEFT JOIN Instructor i ON c.Id = i.CohortId ";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        var cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        var cohortAlreadyAdded = cohorts.FirstOrDefault(c => c.Id == cohortId);
                        var hasStudent = !reader.IsDBNull(reader.GetOrdinal("StudentId"));
                        var hasInstructor = !reader.IsDBNull(reader.GetOrdinal("InstructorId"));

                        if (cohortAlreadyAdded == null)
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(cohort);

                            if (hasStudent)
                            {

                                cohort.Students.Add(new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Exercises = new List<Exercise>(),
                                    Cohort = new Cohort()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                    }
                                });
                            }
                            if (hasInstructor)
                            {

                                cohort.Instructors.Add(new Instructor()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = new Cohort()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                    }
                                });
                            }
                        }
                        else
                        {
                            if (hasStudent)
                            {
                                cohortAlreadyAdded.Students.Add(new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Exercises = new List<Exercise>(),
                                    Cohort = new Cohort()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                    }
                                });
                            }
                            if (hasInstructor)
                            {

                                cohortAlreadyAdded.Instructors.Add(new Instructor()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = new Cohort()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                    }
                                });
                            }
                        }
                    }
                    reader.Close();
                    return Ok(cohorts);
                }

            }

        }


    }
}
