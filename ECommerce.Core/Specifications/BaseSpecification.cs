using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using System.Linq.Expressions;

namespace ECommerce.Core.Specifications;
public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : ModelBase
{
	public BaseSpecification()
	{
	}
	public BaseSpecification(Expression<Func<TEntity, bool>> criteria)
	{
		Criteria = criteria;
	}

	public Expression<Func<TEntity, bool>>? Criteria { get; set; }
	public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];
	//public List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> IncludesWithThen { get; set; } = [];
	public Expression<Func<TEntity, object>>? SortAsc { get; set; }
	public Expression<Func<TEntity, object>>? SortDesc { get; set; }
	public int Take { get; set; }
	public int Skip { get; set; }
	public bool IsPaginationEnabled { get; set; }
	public bool IsSplitQueryEnabled { get; set; }
	public bool IsTrackingEnabled { get; set; }

	public void IncludeRelatedData(params Expression<Func<TEntity, object>>[] includeExpressions)
	{
		foreach (var includeExpr in includeExpressions)
			Includes.Add(includeExpr);
	}

	//public void IncludeWithThen<TProperty>(
	//Expression<Func<TEntity, TProperty>> include,
	//Expression<Func<TProperty, object>> thenInclude)
	//{
	//	IncludesWithThen.Add(query => query.Include(include).ThenInclude(thenInclude));
	//}

	protected void ApplyPagination(int pageNumber, int pageSize)
	{
		IsPaginationEnabled = true;
		Take = pageSize;
		Skip = (pageNumber - 1) * pageSize;
	}
}
