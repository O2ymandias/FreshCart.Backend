using AutoMapper;
using ECommerce.Core.Dtos.ProductDtos;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.BrandModule;

namespace ECommerce.Application.Maps.Resolvers.BrandResolvers;
internal class BrandNameTranslationResolver : IValueResolver<Brand, BrandResult, string>
{
	private readonly ICultureService _cultureService;

	public BrandNameTranslationResolver(ICultureService cultureService)
	{
		_cultureService = cultureService;
	}
	public string Resolve(Brand source, BrandResult destination, string destMember, ResolutionContext context)
	{
		if (_cultureService.CanTranslate)
		{
			var brandTrans = source.Translations.FirstOrDefault(t => t.LanguageCode == _cultureService.LanguageCode);
			return brandTrans is not null ? brandTrans.Name : source.Name;
		}

		return source.Name;
	}
}
