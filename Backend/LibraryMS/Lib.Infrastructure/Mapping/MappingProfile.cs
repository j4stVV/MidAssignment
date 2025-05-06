using AutoMapper;
using Lib.Application.Dtos.BookDTOs;
using Lib.Application.Dtos.Borrowing;
using Lib.Application.Dtos.CategoryDto;
using Lib.Domain.Entities;

namespace Lib.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Book Mappings
        CreateMap<CreateBookDto, Book>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowingDetails, opt => opt.Ignore())
            .ForMember(dest => dest.Available, opt => opt.MapFrom(src => src.Quantity));

        CreateMap<UpdateBookDto, Book>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowingDetails, opt => opt.Ignore());

        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        // Category Mappings
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Books, opt => opt.Ignore());

        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Books, opt => opt.Ignore());

        CreateMap<Category, CategoryDto>();

        // Borrowing Request Mappings
        CreateMap<CreateCategoryDto, BookBorrowingRequest>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestorId, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedId, opt => opt.Ignore())
            .ForMember(dest => dest.Requestor, opt => opt.Ignore())
            .ForMember(dest => dest.Approver, opt => opt.Ignore())
            .ForMember(dest => dest.Details, opt => opt.Ignore());

        CreateMap<BookBorrowingRequest, BorrowingRequestDto>()
            .ForMember(dest => dest.RequestorName, opt => opt.MapFrom(src => src.Requestor.UserName))
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.UserName : null))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

        CreateMap<BookBorrowingRequestDetails, BookBorrowingRequestDetailsDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title));
    }
}
