namespace Million.PropertiesService.Persistance.Interfaces;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}