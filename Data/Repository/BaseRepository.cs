using AutoMapper;

namespace UserManagement.Data.Repository
{
    public class BaseRepository<TEntity, TRead, TCreate, TUpdate> : IBaseRepository<TEntity, TRead, TCreate, TUpdate>
        where TEntity : class
        where TRead : class
        where TCreate : class
        where TUpdate : class
    {
        protected readonly AppDbContext _context;
        protected readonly IMapper _mapper;

        public BaseRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TRead>> GetAll()
        {
            var entityResult = _context.Set<TEntity>().AsQueryable();
            if (!entityResult.Any())
            {
                throw new KeyNotFoundException($"No entity has been found");
            }

            var result= new List<TRead>();
            foreach (var entity in entityResult)
            {
                var newEntity = _mapper.Map<TRead>(entity);
                result.Add(newEntity);
            }

            return result.AsQueryable();
        }

        public async Task<TRead> GetById(int id)
        {
            var entityResult = await _context.Set<TEntity>().FindAsync(id);
            if (entityResult == null)
            {
                throw new KeyNotFoundException($"There is no entity with id: {id}");
            }

            var result = _mapper.Map<TRead>(entityResult);

            return result;
        }

        public virtual async Task Create(TCreate entity)
        {
            var entityModel = _mapper.Map<TEntity>(entity);
            await _context.Set<TEntity>().AddAsync(entityModel);
            await _context.SaveChangesAsync();
        }

        public virtual async Task Update(int id, TUpdate entity)
        {
            var entityResult = await _context.Set<TEntity>().FindAsync(id);
            if (entityResult != null)
            {
                _mapper.Map(entity, entityResult);
                _context.Update(entityResult);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task Delete(int id)
        {
            var entityResult = await _context.Set<TEntity>().FindAsync(id);
            if (entityResult != null)
            {
                _context.Remove(entityResult);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new BadHttpRequestException($"Entity with id:{id} doesn't exist");
            }
        }
    }
}
