namespace ECommerce.APIs.Utilities;

public static class AppServices
{
	public static IServiceProvider Services { get; private set; }

	public static void Configure(IServiceProvider serviceProvider)
	{
		Services = serviceProvider;
	}
}