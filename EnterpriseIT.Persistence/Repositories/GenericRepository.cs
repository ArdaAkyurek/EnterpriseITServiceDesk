using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly EnterpriseITDbContext _context;

    public GenericRepository(EnterpriseITDbContext context) {
        _context = context;
    }
    public async Task<List<T>> GetAllAsync() {

        return await _context.Set<T>().ToListAsync();
    }
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);

    }
    public async Task AddAsync(T entity) {

        await _context.Set<T>().AddAsync(entity);

    }
    public void Update(T entity)
    { 
         _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }


}


