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
    public class ExercisesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ExercisesController(IConfiguration config)
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

        //Get All Exercises
        [HttpGet]
        public List<Exercise> GetAllExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Language FROM Exercise";

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        Exercise exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };

                        exercises.Add(exercise);
                    }

                    reader.Close();
                    return exercises;
                }
            }
        }

        //Get Exercise by Id
        [HttpGet("{id}", Name = "GetExercise")]
        public Exercise GetExerciseById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Name, Language FROM Exercise
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Exercise exercise = null;
                    if (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = id,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language")),

                        };

                    }
                    reader.Close();
                    return exercise;
                }
            }
        }

        //Get Exercise with query parameter
        [HttpGet]
        [Route("ExercisesWithStudents")]
        public async Task<IActionResult> GetAllExercisesWithStudents(string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.id AS ExerciseId, e.Name AS ExerciseName, e.Language, s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, c.Id AS CohortId, c.Name AS CohortName
                                        FROM Exercise e 
                                        LEFT JOIN  StudentExercise se ON e.Id = se.ExerciseId
                                        LEFT JOIN Student s ON se.StudentId = s.Id
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        WHERE 1=1";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Exercise> exercises = new List<Exercise>();


                    while (reader.Read())
                    {
                        var exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                        var exerciseAlreadyAdded = exercises.FirstOrDefault(e => e.Id == exerciseId);
                        var hasStudent = !reader.IsDBNull(reader.GetOrdinal("StudentId"));

                        if (exerciseAlreadyAdded == null)
                        {
                            Exercise exercise = new Exercise
                            {
                                Id = exerciseId,
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("Language")),
                                Students = new List<Student>()
                               
                            };
                            exercises.Add(exercise);
                            if (include != null && include == "students")
                            {
                                if (hasStudent)
                                {
                                    Student student = new Student()
                                    //exercise.Students.Add(new Student()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
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
                                    exercise.Students.Add(student);
                                }
                            }

                        }
                        else
                        {
                            if (include != null && include == "students")
                            {
                                if (hasStudent)
                                {
                                    Student student = new Student()
                                    //exerciseAlreadyAdded.Students.Add(new Student()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
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
                                    exerciseAlreadyAdded.Students.Add(student);
                                }
                            }

                        }
                    }
                    reader.Close();
                    return Ok(exercises);
                }

            }

        }

        //Add Exercise
        [HttpPost]
        public void AddExercise(Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Exercise(Name, Language) OUTPUT INSERTED.Id Values(@Name, @Language)";
                    cmd.Parameters.Add(new SqlParameter("@Name", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@Language", exercise.Language));

                    int id = (int)cmd.ExecuteScalar();
                    exercise.Id = id;

                }


            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExercise(int id, Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Exercise
                                            SET Name = @name, Language = @language
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", exercise.Id));
                        cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }
}