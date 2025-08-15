using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.CategoryModule;

namespace ECommerce.Application.Maps.Resolvers.CategoryResolvers;
internal class CategoryNameTranslationResolver : IValueResolver<Category, CategoryResult, string>
{
	private readonly ICultureService _cultureService;

	public CategoryNameTranslationResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}
	public string Resolve(Category source, CategoryResult destination, string destMember, ResolutionContext context)
	{
		if (_cultureService.CanTranslate)
		{
			var catTrans = source.Translations.FirstOrDefault(t => t.LanguageCode == _cultureService.LanguageCode);
			return catTrans is not null ? catTrans.Name : source.Name;
		}

		return source.Name;
	}
}
