using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Items;

namespace CommunityBot.Application.Items;

public interface IItemRepository : IBaseRepository<Item, string>
{
}