using GeneratorService.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGeneratorDbContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<GeneratorDbContext>(options =>
                options.UseSqlite(config.GetConnectionString("GeneratorDb") ?? "Data Source=generator.db"));
            return services;
        }
    }
}
