using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Models.ProductModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.GalleryResolvers;
internal class GalleryPictureUrlResolver : IValueResolver<ProductGallery, ProductGalleryResult, string>
{
	private readonly IConfiguration _config;

	public GalleryPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}

	public string Resolve(ProductGallery source, ProductGalleryResult destination, string destMember, ResolutionContext context)
	{
		return string.IsNullOrEmpty(source.PictureUrl)
		? string.Empty
		: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
