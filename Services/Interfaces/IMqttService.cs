using TouchOfNature.DTOs;

namespace TouchOfNature.Services.Interfaces
{
    public interface IMqttService
    {
        public Task Connect();
        public Task SendCommand(string command);
        public Task EvaluateAutoControl(AutoControlRequestDto data);
    }
}
