using Serilog.Sinks.SystemConsole.Themes;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides constants for the infrastructure layer.
    /// </summary>
    public static class InfrastructureConstants
    {
        /// <summary>
        /// Logging constants.
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// The console theme.
            /// </summary>
            public static readonly AnsiConsoleTheme Theme = AnsiConsoleTheme.Literate;

            /// <summary>
            /// The console output template.
            /// </summary>
            public const string OutputTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";
        }

        /// <summary>
        /// Integration constants.
        /// </summary>
        public static class Integration
        {
            /// <summary>
            /// Integrations statuses.
            /// </summary>
            public static class Status
            {
                /// <summary>
                /// The <see cref="InvalidPayload"/> status models an integration that doesn't have a valid payload.
                /// </summary>
                public const string InvalidPayload = "Invalid payload";

                /// <summary>
                /// The <see cref="InvalidMcc"/> status models an integration that doesn't have a valid MCC.
                /// </summary>
                public const string InvalidMcc = "Invalid MCC";

                /// <summary>
                /// The <see cref="InvalidTable"/> status models an integration that doesn't have a table name.
                /// </summary>
                public const string InvalidTable = "Invalid table";

                /// <summary>
                /// The <see cref="InvalidConnectionType"/> status models an integration that connection type is invalid.
                /// </summary>
                public const string InvalidConnectionType = "Invalid connection type";

                /// <summary>
                /// The <see cref="InvalidIdsa"/> status models an integration that connection type is invalid.
                /// </summary>
                public const string InvalidIdsa = "No valid or null IDSA";
                
                /// <summary>
                /// The <see cref="InvalidRequest"/> status models an integration that connection type is invalid.
                /// </summary>
                public const string InvalidRequest = "Invalid request";
                
                /// <summary>
                /// The <see cref="InvalidTelecollection"/> status models an integration that connection type is invalid.
                /// </summary>
                public const string InvalidTelecollection = "Invalid telecollection";

                /// <summary>
                /// The <see cref="Excluded"/> status models an integration that is excluded.
                /// </summary>
                public const string Excluded = "Excluded";

                /// <summary>
                /// The <see cref="Added"/> status models an integration that triggered an add in the database.
                /// </summary>
                public const string Added = "Added";

                /// <summary>
                /// The <see cref="Modified"/> status models an integration that triggered an update in the database.
                /// </summary>
                public const string Modified = "Modified";

                /// <summary>
                /// The <see cref="Unchanged"/> status models an integration that did nothing.
                /// </summary>
                public const string Unchanged = "Unchanged";

                /// <summary>
                /// The <see cref="Failed"/> status models an integration that has failed.
                /// </summary>
                public const string Failed = "Failed";
            }
        }

        /// <summary>
        /// Spreading constants.
        /// </summary>
        public static class Spreading
        {
            /// <summary>
            /// Spreading statuses.
            /// </summary>
            public static class Statuses
            {
                public const string Updated = "UPDATED";
                public const string Error = "ERROR";
                public const string Ignored = "IGNORED";
            }
        }

        /// <summary>
        /// Data constants.
        /// </summary>
        public static class Data
        {
            /// <summary>
            /// Data configurations.
            /// </summary>
            public static class Config
            {
                /// <summary>
                /// IDSA data request configuration.
                /// </summary>
                public static class Idsa
                {
                    /// <summary>
                    /// The configuration key.
                    /// </summary>
                    public const string Key = "Idsa";

                    /// <summary>
                    /// The configuration parameters.
                    /// </summary>
                    public static class Params
                    {
                        public const string NoContrat = "no_contrat";

                        public const string NoSerieTpe = "no_serie_tpe";

                        public const string NoSiteTpe = "no_site_tpe";

                        public const string ApplicationCode = "application_code";
                    }
                }
            }

            /// <summary>
            /// Response statuses.
            /// </summary>
            public static class Statuses
            {
                public const string Handled = "Handled";

                public const string Failed = "Failed";

                public const string NotFound = "NotFound";
            }
        }

        /// <summary>
        /// Tpe constants.
        /// </summary>
        public static class Tpe
        {
            /// <summary>
            /// The <see cref="RTC"/> TypeConnexion models the type of TPE connection.
            /// </summary>
            public const string RTC = "RTC";
        }
    }
}
