using AutoMapper;
using Wishlist.Contracts.Admin;
using Wishlist.Contracts.Gdpr;
using Wishlist.Contracts.Invites;
using Wishlist.Contracts.Items;
using Wishlist.Contracts.Lists;
using Wishlist.Contracts.Members;
using Wishlist.Contracts.Purchases;
using Wishlist.Contracts.Reservations;
using Wishlist.Contracts.Users;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;

namespace Wishlist.Application.Mappings;

public class DomainProfile : Profile
{
    public DomainProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserSnapshot>();
        CreateMap<List, ListSummary>();
        CreateMap<List, ListSnapshot>()
            .ForCtorParam("List", opt => opt.MapFrom(src => src));
        CreateMap<ListMember, ListMemberDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.User.Name));
        CreateMap<Item, ItemDto>();
        CreateMap<Item, ItemSnapshot>()
            .ForCtorParam("Priority", o => o.MapFrom(s => (int)s.Priority))
            .ForCtorParam("LinkStatus", o => o.MapFrom(s => (int)s.LinkStatus));
        CreateMap<Reservation, ReservationSnapshot>();
        CreateMap<Purchase, PurchaseSnapshot>();
        CreateMap<Invite, InviteDto>();
        CreateMap<User, AdminUserDto>();
        CreateMap<List, AdminListDto>()
            .ForCtorParam("OwnerEmail", o => o.MapFrom(s => s.Owner.Email))
            .ForCtorParam("Visibility", o => o.MapFrom(s => s.Visibility.ToString()));
    }
}
