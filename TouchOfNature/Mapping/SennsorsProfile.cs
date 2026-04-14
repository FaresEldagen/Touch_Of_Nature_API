using AutoMapper;
using TouchOfNature.DTOs;
using TouchOfNature.Models;

namespace TouchOfNature.Mapping
{
    public class SennsorsProfile : Profile
    {
        public SennsorsProfile()
        {
            CreateMap<SenssorsOutput, SenssorsOutputUiDto>();
        }
    }
}
