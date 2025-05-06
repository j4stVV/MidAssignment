using Lib.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lib.Persistence.EnityConfigurations;

public class BookBorrowingRequestDetailsConfiguration : IEntityTypeConfiguration<BookBorrowingRequestDetails>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequestDetails> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookBorrowingRequestId)
            .IsRequired();

        builder.HasOne(b => b.BookBorrowingRequest)
            .WithMany(b => b.Details)
            .HasForeignKey(b => b.BookBorrowingRequestId);

        builder.HasOne(b => b.Book)
            .WithMany(b => b.BorrowingDetails)
            .HasForeignKey(b => b.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("BookBorrowingRequestDetails");
    }
}
