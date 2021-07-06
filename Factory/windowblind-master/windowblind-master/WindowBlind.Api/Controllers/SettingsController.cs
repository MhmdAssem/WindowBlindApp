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
    public class SettingsController : ControllerBase
    {
        public SettingsController(IRepository repository)
        {
            Repository = repository;
        }
        private IRepository Repository;
        // GET: api/<TablesController>
        [HttpGet]
        public async Task<IEnumerable<ModuleSettings>> Get()
        {
            var settings = this.Repository.Settings.Find(new BsonDocument());
            return await settings.ToListAsync();
        }

        // PUT api/<TablesController>/5
        [HttpPut("{id}")]
        public Task Put(string id, [FromBody] Station value)
        {
            return this.Repository.Stations.ReplaceOneAsync(s => s.Id == id, value);
        }

    }
}
