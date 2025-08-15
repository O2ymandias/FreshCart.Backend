using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.ProductModule;

namespace ECommerce.Application.Maps.Resolvers.ProductResolvers;
internal class ProductCategoryNameTranslationResolver : IValueResolver<Product, ProductResult, string>
{
	private readonly ICultureService _cultureService;

	public ProductCategoryNameTranslationResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}
	public string Resolve(Product source, ProductResult destination, string destMember, ResolutionContext context)
	{
		if (_cultureService.CanTranslate)
		{
			var catTrans = source.Category.Translations.FirstOrDefault(t => t.LanguageCode == _cultureService.LanguageCode);
			return catTrans is not null ? catTrans.Name : source.Category.Name;
		}

		return source.Category.Name;
	}
}
