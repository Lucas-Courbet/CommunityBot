using CommunityBot.Application.Items;
using CommunityBot.Core.Items;
using CommunityBot.Infrastructure.Persistence.Repositories;

namespace CommunityBot.Infrastructure.Persistence.Items;

public class ItemRepository(AppDbContext context)
    : ABaseRepository<Item, string>(context), IItemRepository
{
    
}