using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace api.Service
{
    public class DatabaseInitializerHostedService : IHostedService
    {

        private readonly IDbConnection _db;

        public DatabaseInitializerHostedService(IDbConnection db)
        {
            _db = db;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}