using AutoMapper;
using ECommerce.Core.Dtos.OrderDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.OrderModule;

namespace ECommerce.Application.Maps.Resolvers.DeliveryMethodResolvers;
internal class DeliveryMethodShortNameResolver : IValueResolver<DeliveryMethod, DeliveryMethodResult, string>
{
	private readonly ICultureService _cultureService;

	public DeliveryMethodShortNameResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}
	public string Resolve(DeliveryMethod source, DeliveryMethodResult destination, string destMember, ResolutionContext context)
	{
		if (_cultureService.CanTranslate)
		{
			var deliveryMethodTrans = source.Translations.FirstOrDefault(t => t.LanguageCode == _cultureService.LanguageCode);
			return deliveryMethodTrans is not null ? deliveryMethodTrans.ShortName : source.ShortName;
		}
		return source.ShortName;
	}
}
