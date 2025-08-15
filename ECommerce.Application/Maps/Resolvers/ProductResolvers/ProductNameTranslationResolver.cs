using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.ProductModule;

namespace ECommerce.Application.Maps.Resolvers.ProductResolvers;
internal class ProductNameTranslationResolver : IValueResolver<Product, ProductResult, string>
{
	private readonly ICultureService _cultureService;

	public ProductNameTranslationResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}

	public string Resolve(Product source, ProductResult destination, string destMember, ResolutionContext context)
	{
		if (_cultureService.CanTranslate)
		{
			var productTrans = source.Translations.FirstOrDefault(t => t.LanguageCode == _cultureService.LanguageCode);
			return productTrans is not null ? productTrans.Name : source.Name;
		}
		return source.Name;
	}
}
