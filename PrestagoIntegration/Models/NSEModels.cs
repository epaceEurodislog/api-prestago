using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrestagoIntegration.Models
{
    /// <summary>
    /// Modèle pour un numéro de série (NSE)
    /// </summary>
    public class NSEItem
    {
        /// <summary>
        /// Code du dépôt Prestago
        /// </summary>
        [JsonPropertyName("stockOutletCode")]
        public string StockOutletCode { get; set; }

        /// <summary>
        /// Code de l'équipement (correspond au code article)
        /// </summary>
        [JsonPropertyName("equipmentCode")]
        public string EquipmentCode { get; set; }

        /// <summary>
        /// Désignation ou nom de l'équipement
        /// </summary>
        [JsonPropertyName("equipmentName")]
        public string EquipmentName { get; set; }

        /// <summary>
        /// Numéro de série
        /// </summary>
        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// État de l'équipement (ex: "INSTALLER", "AVAILABLE", etc.)
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "AVAILABLE";

        /// <summary>
        /// Numéro d'intervention
        /// </summary>
        [JsonPropertyName("interventionNumber")]
        public string InterventionNumber { get; set; } = "";
    }
}