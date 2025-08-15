using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Models.ProductModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.ProductResolvers;

public class ProductPictureUrlResolver : IValueResolver<Product, ProductResult, string>
{
	private readonly IConfiguration _config;

	public ProductPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}

	public string Resolve(Product source, ProductResult destination, string destMember, ResolutionContext context)
	{
		return string.IsNullOrEmpty(source.PictureUrl)
			? string.Empty
			: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
