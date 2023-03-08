using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public UsersController(IRepository repository)
        {
            _repository = repository;
        }
        private IRepository _repository;


        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] User value)
        {
            User existingUser = null;
            if (value.Role == "Admin")
                existingUser = await _repository.Users.FindAsync(u => u.Name == value.Name && u.Password == value.Password).Result.FirstOrDefaultAsync();
            if (value.Role == "Customer service")
                existingUser = await _repository.Users.FindAsync(u => u.Name == value.Name && u.Role == value.Role).Result.FirstOrDefaultAsync();
            if (value.Role == "Factory staff")
                existingUser = await _repository.Users.FindAsync(u => u.Name == value.Name && u.Role == value.Role).Result.FirstOrDefaultAsync();

            if (existingUser != null)
                return new JsonResult(existingUser);
            return Ok(null);
        }


        // GET: api/<UsersController>
        [HttpGet]
        public async Task<IEnumerable<User>> Get(string searchString)
        {
            var users = String.IsNullOrEmpty(searchString) ? _repository.Users.Find(new BsonDocument()) : _repository.Users.Find(u => u.Name == searchString).Limit(1);
            return await users.ToListAsync();
        }


        [HttpGet("GetAll")]
        public async Task<IEnumerable<User>> GetAll()
        {
            var users = _repository.Users.Find(e => true);
            return await users.ToListAsync();
        }


        // POST api/<UsersController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User value)
        {
            if (String.IsNullOrEmpty(value.Name))
                return BadRequest("User name cannot be empty");
            var existingUser = _repository.Users.Find(u => u.Name == value.Name);
            if (await existingUser.AnyAsync())
                return BadRequest("User already exists");
            value.Id = Guid.NewGuid().ToString();
            await _repository.Users.InsertOneAsync(value);
            return Ok();
        }

        // PUT api/<UsersController>/5
        [HttpPut]
        public Task Put([FromBody] User value)
        {
            return _repository.Users.ReplaceOneAsync(s => s.Id == value.Id, value);
        }

        // DELETE api/<UsersController>/5
        [HttpGet("DeleteUser")]
        public Task DeleteUser(string userId)
        {
            return _repository.Users.DeleteOneAsync(u => u.Id == userId);
        }
    }
}
