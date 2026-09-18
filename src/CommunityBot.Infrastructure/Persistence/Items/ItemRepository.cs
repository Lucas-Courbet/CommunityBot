using CommunityBot.Application.Items;
using CommunityBot.Core.Items;

namespace CommunityBot.Infrastructure.Persistence.Items;

public class ItemRepository(AppDbContext context)
    : ABaseRepository<Item, string>(context), IItemRepository
{

}