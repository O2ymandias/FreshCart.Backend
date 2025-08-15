using ECommerce.Core.Interfaces;
using ECommerce.Core.Interfaces.Repositories;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Database;
using ECommerce.Infrastructure.Repositories.GenericRepo;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure;
public class UnitOfWork : IUnitOfWork
{
	private IDbContextTransaction? _transaction;
	private readonly Dictionary<Type, object> _repos;
	private readonly AppDbContext _dbContext;

	public UnitOfWork(AppDbContext dbContext)
	{
		_dbContext = dbContext;
		_repos = [];
	}

	public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : ModelBase
	{
		var key = typeof(TEntity);

		if (_repos.TryGetValue(key, out var retrieved))
			return (IGenericRepository<TEntity>)retrieved;

		var repo = new GenericRepository<TEntity>(_dbContext);
		_repos.Add(key, repo);
		return repo;
	}

	public Task<int> SaveChangesAsync() =>
		_dbContext.SaveChangesAsync();

	public async ValueTask DisposeAsync()
	{
		await _dbContext.DisposeAsync();
		GC.SuppressFinalize(this);
	}

	public async Task BeginTransactionAsync()
	{
		if (_transaction is not null) return;

		_transaction = await _dbContext.Database.BeginTransactionAsync();
	}

	public async Task CommitTransactionAsync()
	{
		try
		{
			await SaveChangesAsync();
			if (_transaction is not null) await _transaction.CommitAsync();
		}
		catch (Exception)
		{
			await RollbackTransactionAsync();
			throw;
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	public async Task RollbackTransactionAsync()
	{
		if (_transaction is not null)
		{
			await _transaction.RollbackAsync();
			await DisposeTransactionAsync();
		}
	}

	private async Task DisposeTransactionAsync()
	{
		if (_transaction is not null)
		{
			await _transaction.DisposeAsync();
			_transaction = null;
		}
	}
}
