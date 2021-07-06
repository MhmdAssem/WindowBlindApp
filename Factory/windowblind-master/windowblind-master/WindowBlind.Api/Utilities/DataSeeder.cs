using MongoDB.Driver;
using System;
using WindowBlind.Api.Models;

namespace WindowBlind.Api.Utilities
{
    public class DataSeeder
    {
        private readonly IRepository _repository;

        public DataSeeder(IRepository repository)
        {
            _repository = repository;
        }

        
    }
}
