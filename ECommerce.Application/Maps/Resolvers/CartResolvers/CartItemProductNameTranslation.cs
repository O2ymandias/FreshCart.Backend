using AutoMapper;
using ECommerce.Core.Dtos.CartDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.CartModule;

namespace ECommerce.Application.Maps.Resolvers.CartResolvers;
internal class CartItemProductNameTranslation : IValueResolver<CartItem, CartItemResult, string>
{
	private readonly ICultureService _cultureService;

	public CartItemProductNameTranslation(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}

	public string Resolve(CartItem source, CartItemResult destination, string destMember, ResolutionContext context)
	{
		if (
			_cultureService.CanTranslate &&
			source.NameTranslations.TryGetValue(_cultureService.LanguageCode.ToString().ToLower(), out string? value)
			)
		{
			return value ?? source.ProductName;
		}
		return source.ProductName;
	}
}
