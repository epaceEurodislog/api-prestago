using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrestagoIntegration.Models
{
    public class ExpeditionRequest
    {
        [JsonPropertyName("stockEquipments")]
        public List<ExpeditionItem> StockEquipments { get; set; } = new List<ExpeditionItem>();

        [JsonPropertyName("targetStockCode")]
        public string TargetStockCode { get; set; }
    }
}