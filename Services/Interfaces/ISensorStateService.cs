using TouchOfNature.DTOs;

namespace TouchOfNature.Services.Interfaces
{
    public interface ISensorStateService
    {
        AutoControlRequestDto GetCurrent();
    }
}
