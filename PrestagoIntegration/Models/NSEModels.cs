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
        /// État de l'équipement (ex: "AVAILABLE", "INSTALLED", etc.)
        /// IMPORTANT: Modifié de "Status" à "State" pour correspondre à l'API
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = "AVAILABLE";

        /// <summary>
        /// Numéro d'intervention
        /// </summary>
        [JsonPropertyName("interventionNumber")]
        public int? InterventionNumber { get; set; }
    }
}