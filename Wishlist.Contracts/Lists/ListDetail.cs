using Wishlist.Contracts.Items;
using Wishlist.Contracts.Members;
using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Lists;

public record ListDetail(ListSummary List, IReadOnlyCollection<ListMemberDto> Members, IReadOnlyCollection<ItemDto> Items);
