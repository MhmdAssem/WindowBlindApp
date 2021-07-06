using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        public OrdersController(IRepository repository)
        {
            Repository = repository;
        }
        private IRepository Repository;
        // GET: api/<OrdersController>
        [HttpGet]
        public async Task<IEnumerable<Order>> Get([FromQuery] string searchString)
        {
            var orders = (String.IsNullOrEmpty(searchString) ?
                this.Repository.Orders.Find(new BsonDocument()) :
                this.Repository.Orders.Find(o => o.CBNumber.EndsWith(searchString))).Limit(10);
            return await orders.ToListAsync();
        }

        [HttpPost("importOrders")]
        public async Task ImportOrders([FromBody]Station station)
        {
            await Repository.ImportOrders(station);
        }
        // GET api/<OrdersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<OrdersController>
        [HttpPost("createFiles")]
        public async Task CreateFiles([FromBody] List<Piece> pieces)
        {
            var tableNumber = pieces.FirstOrDefault().TableNumber;
            var stations = await this.Repository.Stations.Find(new BsonDocument()).ToListAsync();
            var table = stations.SelectMany(s => s.Tables).First(t => t.Name == tableNumber);
            Func<Piece, string> pieceBlock = (Piece piece) =>
            $"START##\t{piece.Department}\r\n" +
            $"ORDER NUMBER\t{piece.CBNumber}\r\n" +
            $"LINE NUMBER\t{piece.BlindNumber}\r\n" +
            $"QUANTITY\t1\r\n" +
            $"MEASURED WIDTH\t{piece.MeasuredWidth}\r\n" +
            $"MEASURED DROP\t{piece.MeasuredDrop}\r\n" +
            $"\r\n" +
            $"CONTROL TYPE\t{piece.ControlType}\r\n" +
            $"\r\n" +
            $"HEM\t\r\n" +
            $"FABRIC TYPE\t{piece.RollWidth}\r\n" +
            $"\r\n" +
            $"FABRIC COLOUR\t\r\n" +
            $"TRIM TYPE\t{piece.TrimType}\r\n" ;
            
            var filename = $"{table.NetworkPath}{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.txt";
            using StreamWriter sw = new StreamWriter(filename);
            sw.Write(String.Join("", pieces.Select(pieceBlock)));
        }

        // PUT api/<OrdersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
