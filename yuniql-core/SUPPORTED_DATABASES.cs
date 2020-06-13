﻿namespace Yuniql.Core
{
    /// <summary>
    /// Global constants for identifying supported database platforms
    /// </summary>
    public static class SUPPORTED_DATABASES
    {
        /// <summary>
        /// SqlServer
        /// </summary>
        public const string SQLSERVER = "sqlserver";
        
        /// <summary>
        /// PostgreSql
        /// </summary>
        public const string POSTGRESQL = "postgresql";

        /// <summary>
        /// MySql
        /// </summary>
        public const string MYSQL = "mysql";

        /// <summary>
        /// MariaDB
        /// </summary>
        public const string MARIADB = "mariadb";

        /// <summary>
        /// CockroachDB
        /// </summary>
        public const string COCKROACHDB = "cockroachdb";
    }
}
