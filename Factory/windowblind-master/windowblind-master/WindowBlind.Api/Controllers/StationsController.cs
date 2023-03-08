using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StationsController : ControllerBase
    {
        public StationsController(IRepository repository)
        {
            Repository = repository;
        }
        private IRepository Repository;
        // GET: api/<TablesController>
        [HttpGet]
        public async Task<IEnumerable<Station>> Get()
        {
            var stations = this.Repository.Stations.Find(new BsonDocument());
            return await stations.ToListAsync();
        }

        // POST api/<TablesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Station value)
        {
            if (String.IsNullOrEmpty(value.Name))
                return BadRequest("Station name cannot be empty");
            var existingUser = this.Repository.Stations.Find(u => u.Name == value.Name);
            if (await existingUser.AnyAsync())
                return BadRequest("Station already exists");
            await this.Repository.Stations.InsertOneAsync(value);
            return Ok();
        }

        // PUT api/<TablesController>/5
        [HttpPut("{id}")]
        public Task Put(string id, [FromBody] Station value)
        {
            return this.Repository.Stations.ReplaceOneAsync(s => s.Id == id, value);
        }

        // DELETE api/<TablesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
