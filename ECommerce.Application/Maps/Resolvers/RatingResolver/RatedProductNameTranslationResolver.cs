using AutoMapper;
using ECommerce.Core.Dtos.RatingDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.ProductModule;

namespace ECommerce.Application.Maps.Resolvers.RatingResolver;
internal class RatedProductNameTranslationResolver : IValueResolver<Product, RatedProduct, string>
{
	private readonly ICultureService _cultureService;

	public RatedProductNameTranslationResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}

	public string Resolve(Product source, RatedProduct destination, string destMember, ResolutionContext context)
	{
		if (_cultureService.CanTranslate)
		{
			var ProductTrans = source.Translations.FirstOrDefault(t => t.LanguageCode == _cultureService.LanguageCode);
			return ProductTrans is not null ? ProductTrans.Name : source.Name;
		}
		return source.Name;
	}
}
