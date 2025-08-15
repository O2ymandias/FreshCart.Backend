using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Repositories;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.GenericRepo;
public class GenericRepository<TEntity>(AppDbContext dbContext) : IGenericRepository<TEntity> where TEntity : ModelBase
{
	public async Task<IReadOnlyList<TEntity>> GetAllAsync(ISpecification<TEntity>? specs)
	{
		var query = specs is null
			? dbContext.Set<TEntity>()
			: SpecificationsEvaluator<TEntity>.BuildQuery(dbContext.Set<TEntity>(), specs);

		return await query.ToListAsync();
	}

	public IQueryable<TEntity> GetAllAsQueryable(ISpecification<TEntity>? specs)
	{
		return specs is null
			? dbContext.Set<TEntity>()
			: SpecificationsEvaluator<TEntity>.BuildQuery(dbContext.Set<TEntity>(), specs);
	}

	public async Task<TEntity?> GetAsync(ISpecification<TEntity> specs, bool checkLocalCache = true)
	{
		// Local Search
		if (checkLocalCache)
		{
			var entity = SpecificationsEvaluator<TEntity>.BuildQuery(dbContext.Set<TEntity>().Local.AsQueryable(), specs).FirstOrDefault();
			if (entity is not null) return entity;
		}

		// Database Trip
		return await SpecificationsEvaluator<TEntity>.BuildQuery(dbContext.Set<TEntity>(), specs).FirstOrDefaultAsync();
	}

	public async Task<int> CountAsync(ISpecification<TEntity> specs) =>
		await SpecificationsEvaluator<TEntity>.BuildQuery(dbContext.Set<TEntity>(), specs).CountAsync();

	public void Add(TEntity entity) =>
		dbContext.Add(entity);

	public void Update(TEntity entity) =>
		dbContext.Update(entity);

	public void Delete(TEntity entity) =>
		dbContext.Remove(entity);


}
