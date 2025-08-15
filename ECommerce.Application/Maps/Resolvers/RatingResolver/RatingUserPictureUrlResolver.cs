using AutoMapper;
using ECommerce.Core.Dtos.RatingDtos;
using ECommerce.Core.Models.AuthModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.RatingResolver;
internal class RatingUserPictureUrlResolver : IValueResolver<AppUser, RatingUser, string?>
{
	private readonly IConfiguration _config;

	public RatingUserPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}

	public string? Resolve(AppUser source, RatingUser destination, string? destMember, ResolutionContext context)
	{
		if (source.PictureUrl is null) return null;

		return string.IsNullOrEmpty(source.PictureUrl)
			? string.Empty
			: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
