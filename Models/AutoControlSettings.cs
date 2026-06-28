namespace TouchOfNature.Models
{
    public class AutoControlSettings
    {
        public bool Enabled { get; set; } = false;
        public int LightThreshold { get; set; } = 300;
        public float TempThreshold { get; set; } = 35.0f;
        public float HumidityThreshold { get; set; } = 70.0f;
        public int SoilMoistureThreshold { get; set; } = 400;
    }
}