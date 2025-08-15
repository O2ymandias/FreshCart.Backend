using AutoMapper;
using ECommerce.Core.Dtos.OrderDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.OrderModule;

namespace ECommerce.Application.Maps.Resolvers.OrderResolvers;
internal class OrderItemProductNameResolver : IValueResolver<OrderItem, OrderItemResult, string>
{
	private readonly ICultureService _cultureService;

	public OrderItemProductNameResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}
	public string Resolve(OrderItem source, OrderItemResult destination, string destMember, ResolutionContext context)
	{
		if (
			_cultureService.CanTranslate &&
			source.Product.NameTranslations.TryGetValue(_cultureService.LanguageCode.ToString(), out string? val)
			)
		{
			return val ?? source.Product.Name;
		}
		return source.Product.Name;
	}
}
