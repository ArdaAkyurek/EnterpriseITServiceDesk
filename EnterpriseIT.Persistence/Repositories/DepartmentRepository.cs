using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories;

public class DepartmentRepository
    : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(EnterpriseITDbContext context)
        : base(context)
    {
    }

    // Departman adı kontrolü ile yazma aynı transaction içinde sıralanır; migration gerektirmez.
    private const long DepartmentWriteLock = 748210391;

    private Task<int> LockWritesAsync() => _context.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT pg_advisory_xact_lock({DepartmentWriteLock})");

    private Task<bool> NameExistsAsync(string name, int exceptId = 0) => _context.Departments
        .FromSqlInterpolated($"SELECT * FROM \"Departments\" WHERE lower(btrim(\"DepartmentName\")) = lower(btrim({name})) AND \"DepartmentId\" <> {exceptId}")
        .AnyAsync();

    public async Task<DepartmentWriteResult> CreateDepartmentAsync(Department department)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await LockWritesAsync();
        if (await NameExistsAsync(department.DepartmentName)) return DepartmentWriteResult.DuplicateName;
        department.CreatedAt = DateTime.UtcNow;
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return DepartmentWriteResult.Success;
    }

    public async Task<DepartmentWriteResult> UpdateDepartmentAsync(Department department)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await LockWritesAsync();
        if (!await _context.Departments.AnyAsync(d => d.DepartmentId == department.DepartmentId)) return DepartmentWriteResult.NotFound;
        if (await NameExistsAsync(department.DepartmentName, department.DepartmentId)) return DepartmentWriteResult.DuplicateName;
        var changed = await _context.Departments.Where(d => d.DepartmentId == department.DepartmentId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.DepartmentName, department.DepartmentName)
                .SetProperty(d => d.DepartmentDescription, department.DepartmentDescription)
                .SetProperty(d => d.UpdatedAt, DateTime.UtcNow));
        if (changed != 1) return DepartmentWriteResult.NotFound;
        await _context.RecordAuditAsync("Update", nameof(Department), department.DepartmentId);
        await transaction.CommitAsync();
        return DepartmentWriteResult.Success;
    }

    public async Task<DepartmentWriteResult> DeleteDepartmentAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await LockWritesAsync();
        // FK üzerinden yeni kullanıcı atanması ile silme işlemi aynı departman satırında sıralanır.
        var department = await _context.Departments
            .FromSqlInterpolated($"SELECT * FROM \"Departments\" WHERE \"DepartmentId\" = {id} FOR UPDATE")
            .AsNoTracking().SingleOrDefaultAsync();
        if (department == null) return DepartmentWriteResult.NotFound;
        if (await _context.Users.AnyAsync(u => u.DepartmentId == id)) return DepartmentWriteResult.HasUsers;
        if (await _context.Departments.Where(d => d.DepartmentId == id).ExecuteDeleteAsync() != 1) return DepartmentWriteResult.NotFound;
        await _context.RecordAuditAsync("Delete", nameof(Department), id);
        await transaction.CommitAsync();
        return DepartmentWriteResult.Success;
    }
}
