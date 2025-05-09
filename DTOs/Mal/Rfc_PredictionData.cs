namespace MLService.Models.Prediction;

public class Rfc_PredictionData
{
    public int soil_type { get; set; }
    public double sunlight_hours { get; set; }
    public int water_frequency { get; set; }
    public int fertilizer_type { get; set; }
    public double temperature { get; set; }
    public double humidity { get; set; }

    public Rfc_PredictionData()
    {
        
    }
    
}