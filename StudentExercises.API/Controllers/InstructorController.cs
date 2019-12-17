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
    public class InstructorController : ControllerBase
    {
        private readonly IConfiguration _config;
        public InstructorController(IConfiguration config)
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
        public async Task<IActionResult> GetAllInstructors()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS InstructorLastName, i.SlackHandle AS InstructorSlackHandle,
                                        i.CohortId, c.Name AS CohortName,s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, 
                                        s.SlackHandle AS StudentSlackHandle  FROM
                                        Instructor i INNER JOIN Cohort c ON i.CohortId = c.Id
                                        LEFT JOIN Student s ON c.Id = s.CohortId ";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        var instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                        var instructorAlreadyAdded = instructors.FirstOrDefault(i => i.Id == instructorId);
                        var studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                        var studentAlreadyAdded = instructors.FirstOrDefault(s => s.Id == studentId);

                        if (instructorAlreadyAdded == null)
                        {
                            Instructor instructor = new Instructor
                            {
                                Id = instructorId,
                                FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>(),
                                  
                                }
                                
                            };
                            instructors.Add(instructor);

                        }
                       
                    }
                    reader.Close();
                    return Ok(instructors);

                }

            }
        }
    }
}
