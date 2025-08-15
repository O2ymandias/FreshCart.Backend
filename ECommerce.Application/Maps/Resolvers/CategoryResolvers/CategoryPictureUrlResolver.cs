using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Models.CategoryModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.CategoryResolvers;
internal class CategoryPictureUrlResolver : IValueResolver<Category, CategoryResult, string>
{
	private readonly IConfiguration _config;

	public CategoryPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}
	public string Resolve(Category source, CategoryResult destination, string destMember, ResolutionContext context)
	{
		return string.IsNullOrEmpty(source.PictureUrl)
			? string.Empty
			: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
