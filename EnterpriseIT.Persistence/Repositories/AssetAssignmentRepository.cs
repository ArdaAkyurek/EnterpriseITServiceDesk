using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories;

public class AssetAssignmentRepository
    : GenericRepository<AssetAssignment>, IAssetAssignmentRepository
{
    public AssetAssignmentRepository(EnterpriseITDbContext context)
        : base(context)
    {
    }

    public async Task<List<AssetAssignment>> GetAllWithDetailsAsync()
    {
        return await _context.AssetAssignments
            .Include(x => x.Asset)
            .Include(x => x.User)
            .ToListAsync();
    }

    public async Task<AssetAssignment?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.AssetAssignments
            .Include(x => x.Asset)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.AssetAssignmentId == id);
    }

    public async Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId)
    {
        return await _context.AssetAssignments
            .FirstOrDefaultAsync(x =>
                x.AssetId == assetId &&
                x.ReturnedDate == null);
    }
}