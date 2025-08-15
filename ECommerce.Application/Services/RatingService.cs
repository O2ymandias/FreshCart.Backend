using AutoMapper;
using ECommerce.Core.Common.Pagination;
using ECommerce.Core.Common.SpecsParams;
using ECommerce.Core.Dtos.RatingDtos;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Services;
using ECommerce.Core.Models.ProductModule;
using ECommerce.Core.Specifications.RatingSpecifications;

namespace ECommerce.Application.Services;

public class RatingService : IRatingService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly ICultureService _cultureService;

	public RatingService(IUnitOfWork unitOfWork, IMapper mapper, ICultureService cultureService)
	{
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_cultureService = cultureService;
	}

	public async Task<RatingPaginationResult> GetRatingsAsync(RatingSpecsParams specsParams)
	{
		var canTranslate = _cultureService.CanTranslate;

		var ratingItemsSpecs = new RatingSpecs(
			specsParams: specsParams,
			enablePagination: true,
			enableSorting: true,
			enableTracking: false,
			enableSplittingQuery: canTranslate
			);
		ratingItemsSpecs.IncludeRelatedData(r => r.User, r => r.Product);

		if (canTranslate)
			ratingItemsSpecs.IncludeRelatedData(r => r.Product.Translations);

		var ratings = await _unitOfWork
			.Repository<Rating>()
			.GetAllAsync(ratingItemsSpecs);

		var (totalRatings, average) = await GetTotalAndAverageAsync(specsParams.ProductId);

		return new RatingPaginationResult()
		{
			PageNumber = specsParams.PageNumber,
			PageSize = specsParams.PageSize,
			Total = totalRatings,
			Average = average,
			Results = _mapper.Map<IReadOnlyList<RatingResult>>(ratings)
		};
	}

	public async Task<bool> AddRatingAsync(RatingInput input, string userId)
	{

		var specs = new RatingSpecs(
			specsParams: new() { ProductId = input.ProductId, UserId = userId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: true,
			enableSplittingQuery: false
			);

		var existingRating = await _unitOfWork
			.Repository<Rating>()
			.GetAsync(specs);

		if (existingRating is not null)
		{
			existingRating.Stars = input.Stars;
			existingRating.Title = input.Title;
			existingRating.Comment = input.Comment;
			existingRating.CreatedAt = DateTime.UtcNow;
		}
		else
		{
			var newRating = new Rating()
			{
				UserId = userId,
				ProductId = input.ProductId,
				Stars = input.Stars,
				Title = input.Title,
				Comment = input.Comment,
			};
			_unitOfWork.Repository<Rating>().Add(newRating);
		}

		return await _unitOfWork.SaveChangesAsync() > 0;
	}

	public async Task<bool> DeleteRatingAsync(int ratingId)
	{
		var specs = new RatingSpecs(
			specsParams: new() { RatingId = ratingId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: true,
			enableSplittingQuery: false
			);

		var rating = await _unitOfWork
			.Repository<Rating>()
			.GetAsync(specs);

		if (rating is null) return false;

		_unitOfWork.Repository<Rating>().Delete(rating);
		return await _unitOfWork.SaveChangesAsync() > 0;
	}

	private async Task<(int TotalRatings, double Average)> GetTotalAndAverageAsync(int? productId)
	{
		if (productId is null) return (0, 0);

		var specs = new RatingSpecs(
			specsParams: new() { ProductId = productId },
			enablePagination: false,
			enableSorting: false,
			enableTracking: false,
			enableSplittingQuery: false
			);

		var ratings = await _unitOfWork
			.Repository<Rating>()
			.GetAllAsync(specs);

		var average = ratings.Count > 0 ? Math.Round(ratings.Average(r => r.Stars), 1) : 0.0;
		var totalRatings = ratings.Count;

		return (totalRatings, average);
	}

	public async Task<LatestRatingsResult> GetLatestRatingsAsync(string userId, int limit)
	{
		var canTranslate = _cultureService.CanTranslate;

		var ratingsSpecsParams = new RatingSpecsParams()
		{
			PageNumber = 1,
			PageSize = limit,
			UserId = userId
		};

		var ratingSpecs = new RatingSpecs(
			specsParams: ratingsSpecsParams,
			enablePagination: true,
			enableSorting: true,
			enableTracking: false,
			enableSplittingQuery: canTranslate
			);

		ratingSpecs.IncludeRelatedData(r => r.User, r => r.Product);

		if (canTranslate)
			ratingSpecs.IncludeRelatedData(r => r.Product.Translations);


		var ratings = await _unitOfWork
			.Repository<Rating>()
			.GetAllAsync(ratingSpecs);

		var countSpecsParams = new RatingSpecsParams() { UserId = userId };
		var countSpecs = new RatingSpecs(
			specsParams: countSpecsParams,
			enablePagination: false,
			enableSorting: false,
			enableTracking: false,
			enableSplittingQuery: false
			);

		var count = await _unitOfWork
			.Repository<Rating>()
			.CountAsync(countSpecs);

		return new()
		{
			Count = count,
			LatestRatings = _mapper.Map<IReadOnlyList<RatingResult>>(ratings)
		};

	}
}
