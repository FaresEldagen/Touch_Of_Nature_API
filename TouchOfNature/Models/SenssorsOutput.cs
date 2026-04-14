namespace TouchOfNature.Models
{
    public class SenssorsOutput
    {
        public int Id { get; set; }
        public int SoilMoisture { get; set; }
        public int LightDependentResistor { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
