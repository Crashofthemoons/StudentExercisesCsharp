using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Controllers
{
        [Route("api/[controller]")]
        [ApiController]
        public class InstructorsController : ControllerBase
        {
            private readonly IConfiguration _config;

            public InstructorsController(IConfiguration config)
            {   
                _config = config;
            }

            public IDbConnection Connection
            {
                get
                {
                    return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                }
            }
            // GET api/exercise?FirstName=JavaScript
            [HttpGet]
            public async Task<IActionResult> Get(string FirstName)
            {
                using (IDbConnection conn = Connection)
                {
                    string sql = "SELECT * FROM Instructors";
                    if (FirstName != null)
                    {
                        sql += $" WHERE FirstName = '{FirstName}'";
                    }

                    var fullExercises = await conn.QueryAsync<Instructor>(
                        sql);
                    return Ok(fullExercises);
                }
            }

            // GET api/values/5
            [HttpGet("{id}")]
            public async Task<ActionResult> Get([FromRoute] int id)
            {
                using (IDbConnection conn = Connection)
                {
                    string sql = $"SELECT * FROM Instructor WHERE Id = {id}";

                    var singleInstructor = (await conn.QueryAsync<Instructor>(
                        sql)).Single();
                    return Ok(singleInstructor);
                }
            }

            // POST api/values
            [HttpPost]
            public async Task<IActionResult> Post([FromBody] Instructor instructor)
            {
                string sql = $@"INSERT INTO Instructor
            (FirstName, LastName, CohortId, SlackHandle, Specialty)
            VALUES
            ('{instructor.FirstName}', '{instructor.LastName}', '{instructor.CohortId}', '{instructor.SlackHandle}', '{instructor.Specialty}');
            select MAX(Id) from Instructor";

                using (IDbConnection conn = Connection)
                {
                    var newInstructorId = (await conn.QueryAsync<int>(sql)).Single();
                    instructor.Id = newInstructorId;
                    return CreatedAtRoute("GetInstructor", new { id = newInstructorId }, instructor);
                }
            }

            // PUT api/values/5
            [HttpPut("{id}")]
            public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Instructor instructor)
            {
                string sql = $@"
            UPDATE Instructor
            SET FirstName = '{instructor.FirstName}',
                LastName = '{instructor.LastName}',
                CohortId = '{instructor.CohortId}',
                SlackHandle = '{instructor.SlackHandle}',
                Specialty = '{instructor.Specialty}'
            WHERE Id = {id}";

                try
                {
                    using (IDbConnection conn = Connection)
                    {
                        int rowsAffected = await conn.ExecuteAsync(sql);
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
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

            // DELETE api/values/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete([FromRoute] int id)
            {
                string sql = $@"DELETE FROM Instructor WHERE Id = {id}";

                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }

            }

            private bool ExerciseExists(int id)
            {
                string sql = $"SELECT Id, Name, Language FROM Instructor WHERE Id = {id}";
                using (IDbConnection conn = Connection)
                {
                    return conn.Query<Exercise>(sql).Count() > 0;
                }
            }
        }
}
