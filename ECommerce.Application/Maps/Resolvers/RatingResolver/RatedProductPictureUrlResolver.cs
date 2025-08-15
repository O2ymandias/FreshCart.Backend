using AutoMapper;
using ECommerce.Core.Dtos.RatingDtos;
using ECommerce.Core.Models.ProductModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.RatingResolver;
internal class RatedProductPictureUrlResolver : IValueResolver<Product, RatedProduct, string>
{
	private readonly IConfiguration _config;

	public RatedProductPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}
	public string Resolve(Product source, RatedProduct destination, string destMember, ResolutionContext context)
	{
		return string.IsNullOrEmpty(source.PictureUrl)
			? string.Empty
			: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
