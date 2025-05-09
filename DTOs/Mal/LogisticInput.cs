namespace MLService.Models;

public class LogisticInput
{
    public string Soil_Type { get; set; }
    public string Water_Frequency { get; set; }
    public string Fertilizer_Type { get; set; }
    public int Sunlight_Hours { get; set; }
    public int Temperature { get; set; }
    public int Humidity { get; set; }
}