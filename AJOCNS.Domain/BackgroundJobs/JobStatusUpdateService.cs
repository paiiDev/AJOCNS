using AJOCNS.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AJOCNS.Domain.BackgroundJobs
{
    public class JobStatusUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public JobStatusUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var expiredJobs = await context.JobPosts
                        .Where(j => j.Status == "Open" && j.ClosingDate < DateTime.UtcNow && !j.IsDeleted)
                        .ToListAsync(stoppingToken);
                    if (expiredJobs.Any())
                    {
                        foreach (var job in expiredJobs)
                        {
                            job.Status = "Closed";
                        }
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                var timeUntilMidnight = nextMidnight - now;

                await Task.Delay(timeUntilMidnight, stoppingToken);
            }
        }
    }
}