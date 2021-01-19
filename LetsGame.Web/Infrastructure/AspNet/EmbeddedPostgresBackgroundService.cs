using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MysticMind.PostgresEmbed;

namespace LetsGame.Web.Infrastructure.AspNet
{
    public class EmbeddedPostgresHostedService : IHostedService
    {
        private static readonly PgServer Server = new("10.7.1", instanceId: new Guid("FA64AD2C-6D54-4575-A4A4-521EBC00A8FC"));

        public static int PgPort => Server.PgPort;
        public static string DatabaseUrl => $"postgres://postgres:test@localhost:{PgPort}/postgres";

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Server.StartAsync(cancellationToken);
            
            Console.WriteLine("PG Started - {0}", DatabaseUrl);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Server?.Dispose();
            
            return Task.CompletedTask;
        }
    }
}