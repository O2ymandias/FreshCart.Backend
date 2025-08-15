using ECommerce.Core.Common.Constants;
using ECommerce.Core.Common.Helpers;
using ECommerce.Core.Common.Options;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Core.Models.AuthModule;
using ECommerce.Core.Models.BrandModule;
using ECommerce.Core.Models.CategoryModule;
using ECommerce.Core.Models.OrderModule;
using ECommerce.Core.Models.ProductModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ECommerce.Infrastructure.Database.SeedData;

public class AppDatabaseSeeder : IDatabaseSeeder
{
	private readonly AppDbContext _dbContext;
	private readonly ILogger<AppDatabaseSeeder> _logger;
	private readonly UserManager<AppUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly AdminOptions _adminOptions;

	public AppDatabaseSeeder(
		AppDbContext dbContext,
		ILogger<AppDatabaseSeeder> logger,
		UserManager<AppUser> userManager,
		RoleManager<IdentityRole> roleManager,
		IOptions<AdminOptions> adminOptions
	)
	{
		_dbContext = dbContext;
		_logger = logger;
		_userManager = userManager;
		_roleManager = roleManager;
		_adminOptions = adminOptions.Value;
	}

	public async Task SeedAsync()
	{
		await SeedFromJsonAsync<Brand>("brands.json");
		await SeedFromJsonAsync<Category>("categories.json");
		await SeedFromJsonAsync<Product>("products.json");
		await SeedFromJsonAsync<ProductGallery>("productGalleries.json");
		await SeedFromJsonAsync<DeliveryMethod>("deliveryMethods.json");
		await SeedFromJsonAsync<ProductTranslation>("productTranslations.json");
		await SeedFromJsonAsync<BrandTranslation>("brandTranslations.json");
		await SeedFromJsonAsync<CategoryTranslation>("categoryTranslations.json");
		await SeedFromJsonAsync<DeliveryMethodTranslation>("deliveryMethodTranslations.json");

		await SeedRolesAsync();
		await SeedUsersAsync();
	}

	private async Task SeedFromJsonAsync<T>(string fileName) where T : ModelBase
	{
		var entityName = typeof(T).Name;

		if (_dbContext.Set<T>().Any())
		{
			_logger.LogInformation("{Entity} table already contains data. Skipping seeding.", entityName);
			return;
		}

		var filePath = $"./../ECommerce.Infrastructure/Database/SeedData/{fileName}";

		if (!File.Exists(filePath))
		{
			_logger.LogWarning("Seed file not found for {Entity}. Expected path: {Path}", entityName, filePath);
			return;
		}

		try
		{
			var jsonData = await File.ReadAllTextAsync(filePath);
			var items = JsonConvert.DeserializeObject<List<T>>(jsonData);

			if (items?.Count > 0)
			{
				_dbContext.Set<T>().AddRange(items);
				var affected = await _dbContext.SaveChangesAsync();

				if (affected > 0)
					_logger.LogInformation("{Count} {Entity} records successfully seeded.", affected, entityName);
				else
					_logger.LogInformation("No changes were made while seeding {Entity}.", entityName);
			}

			else
				_logger.LogWarning("No records found in the seed file for {Entity}.", entityName);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while seeding {Entity} from file {FileName}.", entityName, fileName);
		}
	}

	private async Task SeedRolesAsync()
	{
		if (_roleManager.Roles.Any()) return;

		var roles = new List<IdentityRole>()
		{
			new() { Name = RolesConstants.Admin },
			new() { Name = RolesConstants.User }
		};

		foreach (var role in roles)
		{
			var result = await _roleManager.CreateAsync(role);
			IdentityResultLogger.LogInfo(
				result,
				_logger,
				$"Role `{role.Name}` has been created successfully."
				);
		}
	}

	private async Task SeedUsersAsync()
	{
		if (_userManager.Users.Any()) return;

		var admin = new AppUser()
		{
			DisplayName = _adminOptions.DisplayName,
			UserName = _adminOptions.UserName,
			Email = _adminOptions.Email,
			EmailConfirmed = true
		};

		var created = await _userManager.CreateAsync(admin, _adminOptions.Password);
		IdentityResultLogger.LogInfo(
			created,
			_logger,
			$"User `{admin.DisplayName}` has been created successfully."
			);

		var addedToRole = await _userManager.AddToRoleAsync(admin, RolesConstants.Admin);
		IdentityResultLogger.LogInfo(
			addedToRole,
			_logger,
			$"Role `{RolesConstants.Admin}` has been assigned to User `{admin.DisplayName}`"
			);
	}

}
