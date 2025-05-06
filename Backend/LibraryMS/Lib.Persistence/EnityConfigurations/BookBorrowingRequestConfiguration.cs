using Lib.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lib.Persistence.EnityConfigurations;

public class BookBorrowingRequestConfiguration : IEntityTypeConfiguration<BookBorrowingRequest>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequest> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.RequestorId)
            .IsRequired();

        builder.Property(b => b.RequestedDate)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        builder.HasOne(b => b.Requestor)
            .WithMany(b => b.BookBorrowingRequests)
            .HasForeignKey(b => b.RequestorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Approver)
            .WithMany(b => b.ApprovedRequests)
            .HasForeignKey(b => b.ApprovedId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.ToTable("BookBorrowingRequests");
    }
}
