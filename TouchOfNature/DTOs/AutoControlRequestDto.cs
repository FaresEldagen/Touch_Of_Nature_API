namespace TouchOfNature.DTOs
{
    public class AutoControlRequestDto
    {
        public int SoilMoisture { get; set; }
        public int LightDependentResistor { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
    }
}
