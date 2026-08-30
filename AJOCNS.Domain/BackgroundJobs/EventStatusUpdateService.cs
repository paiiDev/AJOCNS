using AJOCNS.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.BackgroundJobs
{
    public class EventStatusUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public EventStatusUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var pastEvents = await context.Events.Where(e => e.Status == "Upcoming" && e.EventDate < DateTime.Now).ToListAsync(stoppingToken);
                    if(pastEvents.Any())
                    {
                        foreach (var pastEvent in pastEvents)
                        {
                            pastEvent.Status = "Completed";
                        }
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                //await Task.Delay(TimeSpan.FromHours(10), stoppingToken);
            }
        }
        }
}
