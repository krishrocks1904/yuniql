﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ArdiLabs.Yuniql.SqlServer;

namespace ArdiLabs.Yuniql
{
    public class CommandLineService : ICommandLineService
    {

        public CommandLineService()
        {
        }

        public object RunInitOption(InitOption opts)
        {
            try
            {
                var versionService = new LocalVersionService();

                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    versionService.Init(workingPath);
                    TraceService.Info($"Initialized {workingPath}.");
                }
                else
                {
                    versionService.Init(opts.Path);
                    TraceService.Info($"Initialized {opts.Path}.");
                }
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute init function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object IncrementVersion(NextVersionOption opts)
        {
            try
            {
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    opts.Path = workingPath;
                }

                var versionService = new LocalVersionService();
                if (opts.IncrementMajorVersion)
                {
                    var nextVersion = versionService.IncrementMajorVersion(opts.Path, opts.File);
                    TraceService.Info($"New major version created {nextVersion} on {opts.Path}.");
                }
                else if (opts.IncrementMinorVersion || (!opts.IncrementMajorVersion && !opts.IncrementMinorVersion))
                {
                    var nextVersion = versionService.IncrementMinorVersion(opts.Path, opts.File);
                    TraceService.Info($"New minor version created {nextVersion} on {opts.Path}.");
                }
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute vnext function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunMigration(RunOption opts)
        {
            try
            {
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    opts.Path = workingPath;
                }

                TraceService.Info($"Started migration from {opts.Path}.");

                //if no target version specified, capture the latest from local folder structure
                if (string.IsNullOrEmpty(opts.TargetVersion))
                {
                    var localVersionService = new LocalVersionService();
                    opts.TargetVersion = localVersionService.GetLatestVersion(opts.Path);
                    TraceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //if no connection string passed, use environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
                }

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run the migration
                var csvImportService = new SqlServerCsvImportService();
                var dataService = new SqlServerDataService(opts.ConnectionString);

                var migrationService = new MigrationService(opts.ConnectionString, dataService, csvImportService);
                migrationService.Run(opts.Path, opts.TargetVersion, opts.AutoCreateDatabase, tokens);
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute run function. Target database will be rolled back to its previous state. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunVerify(VerifyOption opts)
        {
            try
            {
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    opts.Path = workingPath;
                }

                TraceService.Info($"Started verifcation from {opts.Path}.");

                //if no target version specified, capture the latest from local folder structure
                if (string.IsNullOrEmpty(opts.TargetVersion))
                {
                    var localVersionService = new LocalVersionService();
                    opts.TargetVersion = localVersionService.GetLatestVersion(opts.Path);
                    TraceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //if no connection string passed, use environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
                }

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run the migration
                var csvImportService = new SqlServerCsvImportService();
                var dataService = new SqlServerDataService(opts.ConnectionString);

                var migrationService = new MigrationService(opts.ConnectionString, dataService, csvImportService);
                migrationService.Run(opts.Path, opts.TargetVersion, autoCreateDatabase: false, tokens, verifyOnly: true);

                TraceService.Info("Verification run successful.");
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute verification function. Target database will be rolled back to its previous state. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunInfoOption(InfoOption opts)
        {
            try
            {
                //if no connection string passed, use environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
                }

                var csvImportService = new SqlServerCsvImportService();
                var dataService = new SqlServerDataService(opts.ConnectionString);

                var migrationService = new MigrationService(opts.ConnectionString, dataService, csvImportService);
                var versions = migrationService.GetAllVersions();

                var results = new StringBuilder();
                results.AppendLine($"Version\t\tCreated\t\t\t\tCreatedBy");
                versions.ForEach(v =>
                {
                    results.AppendLine($"{v.Version}\t\t{v.DateInsertedUtc.ToString("o")}\t{v.LastUserId}");
                });

                Console.WriteLine(results.ToString());
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute info function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunEraseOption(EraseOption opts)
        {
            try
            {
                //if no connection string passed, use environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
                }

                var csvImportService = new SqlServerCsvImportService();
                var dataService = new SqlServerDataService(opts.ConnectionString);

                var migrationService = new MigrationService(opts.ConnectionString, dataService, csvImportService);
                migrationService.Erase(opts.ConnectionString);
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute info function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunBaselineOption(BaselineOption opts)
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }

        public object RunRebaseOption(RebaseOption opts)
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }
    }
}
