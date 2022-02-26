namespace UserManagement.Data.Repository
{
    public interface IBaseRepository<TEntity, TRead, TCreate, TUpdate>
        where TEntity : class
        where TRead : class
        where TCreate : class
        where TUpdate : class
    {
        Task<IEnumerable<TRead>> GetAll();
        Task<TRead> GetById(int id);
        Task Create(TCreate entity);
        Task Update(int id, TUpdate entity);
        Task Delete(int id);
    }
}
