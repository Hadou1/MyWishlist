using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Mappings;
using Wishlist.Infrastructure.Persistence;

namespace Wishlist.Tests.Helpers;

public class TestFixture : IDisposable
{
    public AppDbContext Context { get; }
    public IMapper Mapper { get; }

    public TestFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new AppDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DomainProfile>();
        });
        Mapper = config.CreateMapper();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
