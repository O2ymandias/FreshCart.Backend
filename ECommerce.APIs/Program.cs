using ECommerce.APIs.ErrorModels;
using ECommerce.APIs.Extensions;
using ECommerce.APIs.Middlewares;
using ECommerce.APIs.Utilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ECommerce.APIs
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			#region Add services to the container.

			builder.Services
				.AddControllers()
				.AddNewtonsoftJson(options =>
				{
					options.SerializerSettings.Converters.Add(new StringEnumConverter());
					options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
					options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
					options.SerializerSettings.DateFormatString = "o";
				})
				.AddDataAnnotationsLocalization(opts =>
				{
					opts.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(type);
				});

			builder.Services.AddOpenApi();

			builder.Services.RegisterAppServices(builder.Configuration);


			#endregion

			var app = builder.Build();

			AppServices.Configure(app.Services);

			await app.InitializeDatabaseAsync();

			#region Configure the HTTP request pipeline.

			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "FreshCart API v1");
					options.RoutePrefix = "swagger";
				});
			}

			app.UseMiddleware<ExceptionMiddleware>();

			//app.UseHttpsRedirection();

			app.MapStaticAssets();

			app.UseCors("ECommerce");

			var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
			app.UseRequestLocalization(localizationOptions);

			app.UseAuthorization();

			app.MapControllers().WithStaticAssets();

			app.MapFallback(async context =>
			{
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				context.Response.ContentType = "application/json";

				var response = new ApiErrorResponse(
					StatusCodes.Status404NotFound,
					$"The requested endpoint '{context.Request.Path}' was not found."
				);

				await context.Response.WriteAsJsonAsync(response);
			});

			#endregion

			app.Run();
		}
	}
}
