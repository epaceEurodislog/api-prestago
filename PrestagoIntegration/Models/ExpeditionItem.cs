using System.Text.Json.Serialization;

namespace PrestagoIntegration.Models
{
    public class ExpeditionItem
    {
        [JsonPropertyName("equipmentCode")]
        public string EquipmentCode { get; set; }

        [JsonPropertyName("equipmentName")]
        public string EquipmentName { get; set; }

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; }
    }
}