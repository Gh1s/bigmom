using Csb.BigMom.Infrastructure.Entities;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Integration
{
    /// <summary>
    /// Models a telecollection integration request sent through Kafka by Golden Gate.
    /// </summary>
    public class TlcIntegrationRequest : IRequest
    {
        /// <summary>
        /// The table that was modified.
        /// </summary>
        [JsonPropertyName("table")]
        public string Table { get; set; }

        /// <summary>
        /// The row before it was modified.
        /// </summary>
        [JsonPropertyName("before")]
        public TlcRow Before { get; set; }

        /// <summary>
        /// The row after it has been modified.
        /// </summary>
        [JsonPropertyName("after")]
        public TlcRow After { get; set; }

        /// <summary>
        /// The operation type.
        /// </summary>
        [JsonPropertyName("op_type")]
        public string OpType { get; set; }

        /// <summary>
        /// The timestamp.
        /// </summary>
        [JsonPropertyName("current_ts")]
        public string Timestamp { get; set; }

        /// <summary>
        /// The integration trace.
        /// </summary>
        public TlcIntegrationTrace Trace { get; set; }
    }

    /// <summary>
    /// Models an row of the TERMINAL_POS_WATCH_ACTIVITY table in Powercard.
    /// </summary>
    public class TlcRow
    {
        [JsonPropertyName("TERMINAL_POS_NUMBER")]
        public string Idsa { get; set; }

        [JsonPropertyName("PROCESSING_DATE")]
        public string ProcessingDate { get; set; }

        [JsonPropertyName("CONNECTION_TYPE")]
        public string ConnectionType { get; set; }

        [JsonPropertyName("TRANSACTION_DOWNLOAD_STATUS")]
        public string TransactionDowloadStatus { get; set; }

        [JsonPropertyName("NBR_DEBIT_1")]
        public int NbrDebit { get; set; }

        [JsonPropertyName("NBR_CREDIT_1")]
        public int NbrCredit { get; set; }

        [JsonPropertyName("TOTAL_DEBIT_1")]
        public decimal TotalDebit { get; set; }

        [JsonPropertyName("TOTAL_CREDIT_1")]
        public decimal TotalCredit { get; set; }

        [JsonPropertyName("NET_RECONCILIATION_AMOUNT_1")]
        public decimal TotalReconcilie { get; set; }
        
        [JsonPropertyName("POS_ACRONYM")]
        public string PosAcronym { get; set; }
        
        [JsonPropertyName("POS_PROFILE_CODE")]
        public string PosAcronymCode { get; set; }
        
        [JsonPropertyName("POS_LOCATION")]
        public string PosLocation { get; set; }
        
        [JsonPropertyName("SERIAL_NUMBER")]
        public string SerialNumber { get; set; }
        
        [JsonPropertyName("INSTALLATION_DATE")]
        public string InstallationDate { get; set; }
        
        [JsonPropertyName("DIAL_HOUR")]
        public string DialHour { get; set; }
        
        [JsonPropertyName("EQUIPMENT_FEE_CODE")]
        public string EquipmentFeeCode { get; set; }
        
        [JsonPropertyName("ACC_POINT_NAME")]
        public string AccPointName { get; set; }
        
        [JsonPropertyName("EXTERNAL_ID_1")]
        public string CommerceId { get; set; }
        
        [JsonPropertyName("MER_ACCEPTOR_POINT_ID")]
        public string MerAcceptorId { get; set; }
        
    }
}