using AutoMapper;
using ECommerce.Core.Dtos.ProfileDtos;
using ECommerce.Core.Models.AuthModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.AuthResolvers;

internal class UserProfilePictureUrlResolver : IValueResolver<AppUser, UserInfoResult, string?>
{
	private readonly IConfiguration _config;

	public UserProfilePictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}

	public string? Resolve(AppUser source, UserInfoResult destination, string? destMember, ResolutionContext context)
	{
		if (source.PictureUrl is null) return null;

		return string.IsNullOrEmpty(source.PictureUrl)
			? string.Empty
			: $"{_config["BaseUrl"]}/{source.PictureUrl}";
	}
}
