using TouchOfNature.DTOs;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature.Services.Implementations;

public class SensorStateService : ISensorStateService
{
    private AutoControlRequestDto _current = new();
    public AutoControlRequestDto GetCurrent()
    {
        return _current;
    }
}

