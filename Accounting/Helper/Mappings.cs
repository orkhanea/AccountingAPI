using AutoMapper;
using Accounting.Model;
using Accounting.Model.DTOs;

namespace WebSuperApi.Helper
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<Transaction, TransactionDTO>()
                .ForMember(
                    opt => opt.SenderCompany,
                    des => des.MapFrom(src => src.SenderCompany.CompanyName)
                    )
                .ForMember(
                    opt => opt.RecieverCompany,
                    des => des.MapFrom(src => src.RecieverCompany.CompanyName)
                    );
        }
    }
}
