using AutoMapper;
using ECommerce.Core.Dtos.CartDtos;
using ECommerce.Core.Models.CartModule;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Maps.Resolvers.CartResolvers;
internal class CartItemProductPictureUrlResolver : IValueResolver<CartItem, CartItemResult, string>
{
	private readonly IConfiguration _config;

	public CartItemProductPictureUrlResolver(IConfiguration config)
	{
		_config = config;
	}

	public string Resolve(CartItem source, CartItemResult destination, string destMember, ResolutionContext context)
	{
		return string.IsNullOrEmpty(source.ProductPictureUrl)
			? string.Empty
			: $"{_config["BaseUrl"]}/{source.ProductPictureUrl}";
	}
}
