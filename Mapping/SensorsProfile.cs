using AutoMapper;
using TouchOfNature.DTOs;
using TouchOfNature.Models;

namespace TouchOfNature.Mapping
{
    public class SensorsProfile : Profile
    {
        public SensorsProfile()
        {
            CreateMap<SensorsOutput, SensorsOutputUiDto>();
        }
    }
}
