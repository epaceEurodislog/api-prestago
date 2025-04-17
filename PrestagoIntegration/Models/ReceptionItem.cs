using System.Text.Json.Serialization;

namespace PrestagoIntegration.Models
{
    public class ReceptionItem
    {
        [JsonPropertyName("equipmentCode")]
        public string EquipmentCode { get; set; }

        [JsonPropertyName("equipmentName")]
        public string EquipmentName { get; set; }

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; } = "AVAILABLE";

        [JsonPropertyName("interventionNumber")]
        public int? InterventionNumber { get; set; }
    }
}