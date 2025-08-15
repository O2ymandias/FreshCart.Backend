using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Models.BrandModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.BrandResolvers;

public class BrandPictureUrlResolver : IValueResolver<Brand, BrandResult, string>
{
	private readonly IConfiguration _config;

	public BrandPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}

	public string Resolve(Brand source, BrandResult destination, string destMember, ResolutionContext context)
	{
		return string.IsNullOrEmpty(source.PictureUrl)
		? string.Empty
		: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
