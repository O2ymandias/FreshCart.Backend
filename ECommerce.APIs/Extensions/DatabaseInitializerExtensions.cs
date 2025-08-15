using ECommerce.Infrastructure.Database;
using ECommerce.Infrastructure.Database.SeedData;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.APIs.Extensions;

public static class DatabaseInitializerExtensions
{
	public static async Task InitializeDatabaseAsync(this WebApplication app)
	{
		using IServiceScope scope = app.Services.CreateScope();
		IServiceProvider serviceProvider = scope.ServiceProvider;

		var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

		var appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
		var appDatabaseSeeder = serviceProvider.GetRequiredService<AppDatabaseSeeder>();

		try
		{
			await appDbContext.Database.MigrateAsync();
			logger.LogInformation("Database migrations were applied successfully for AppDbContext.");
			await appDatabaseSeeder.SeedAsync();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error: {Message}", ex.Message);
			throw;
		}
	}
}
