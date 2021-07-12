using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace WebDemo.Data.Internal.Extensions
{
    public static class DatabaseFacadeExtentions
    {
        /// <summary>
        /// 查询单值结果为字符串
        /// </summary>
        /// <param name="databaseFacade"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static string FromSqlString(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
        {
            var val = ExecuteScalar(databaseFacade, commandText, commandParameters);

            if (val != null && val != DBNull.Value)
            {
                return val.ToString();
            }

            return null;
        }

        /// <summary>
        /// 查询单值结果为整数
        /// </summary>
        /// <param name="databaseFacade"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static int FromSqlInt(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
        {
            var val = ExecuteScalar(databaseFacade, commandText, commandParameters);

            if (val != null && val != DBNull.Value)
            {
                int.TryParse(val.ToString(), out int intValue);
                return intValue;
            }

            return 0;
        }

        public static async Task<int> FromSqlIntAsync(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
        {
            var val = await ExecuteScalarAsync(databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);

            if (val != null && val != DBNull.Value)
            {
                int.TryParse(val.ToString(), out int intValue);
                return intValue;
            }

            return 0;
        }

        private static object ExecuteScalar(
            DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            => ExecuteScalar(
                databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);

        public static T FromSql<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => ExecuteDataReader<T>(
                databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);

        public static async Task<T> FromSqlAsync<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => await ExecuteDataReaderAsync<T>(
                databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);

        public static object FromStoreProcedure(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            => ExecuteScalar(
                databaseFacade,
                CommandType.StoredProcedure,
                commandText,
                commandParameters);

        public static async Task<object> FromStoreProcedureAsync(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            => await ExecuteScalarAsync(
                databaseFacade,
                CommandType.StoredProcedure,
                commandText,
                commandParameters);


        public static T FromStoreProcedure<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => ExecuteDataReader<T>(
                databaseFacade,
                CommandType.StoredProcedure,
                commandText,
                commandParameters);

        public static async Task<T> FromStoreProcedureAsync<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => await ExecuteDataReaderAsync<T>(
                databaseFacade,
                CommandType.StoredProcedure,
                commandText,
                commandParameters);

        public static ICollection<T> FromSqlCollection<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => ExecuteDataReaderCollection<T>(
                databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);

        public static async Task<ICollection<T>> FromSqlCollectionAsync<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => await ExecuteDataReaderCollectionAsync<T>(
                databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);

        public static ICollection<T> FromStoreProcedureCollection<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => ExecuteDataReaderCollection<T>(
                databaseFacade,
                CommandType.StoredProcedure,
                commandText,
                commandParameters);

        public static async Task<ICollection<T>> FromStoreProcedureCollectionAsync<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
            => await ExecuteDataReaderCollectionAsync<T>(
                databaseFacade,
                CommandType.StoredProcedure,
                commandText,
                commandParameters);

        static object ExecuteScalar(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            var dbCommand = CreateCommand(
                dbConnection,
                commandType,
                commandText,
                commandParameters);

            object result;

            try
            {
                dbConnection.Open();
                result = dbCommand.ExecuteScalar();
            }
            finally
            {
                dbCommand.Parameters.Clear();
                dbCommand.Dispose();
                dbConnection.Close();
            }

            return result;
        }

        static async Task<object> ExecuteScalarAsync(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            var dbCommand = CreateCommand(
                dbConnection,
                commandType,
                commandText,
                commandParameters);

            object result;

            try
            {
                CancellationToken cancellationToken = CancellationToken.None;
                await dbConnection.OpenAsync(cancellationToken);
                result = await dbCommand.ExecuteScalarAsync(cancellationToken);
            }
            catch
            {
                throw;
            }
            finally
            {
                dbCommand.Parameters.Clear();
                dbCommand.Dispose();
                dbConnection.Close();
            }

            return result;
        }

        static T ExecuteDataReader<T>(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
            where T : new()
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            IDataReader dataReader;

            try
            {
                var dbCommand = CreateCommand(
                    dbConnection,
                    commandType,
                    commandText,
                    commandParameters);

                dbConnection.Open();
                dataReader = dbCommand.ExecuteReader();
            }
            catch
            {
                // If failure happens creating the data reader, then it won't be available to
                // handle closing the connection, so do it explicitly here to preserve ref counting.
                dbConnection.Close();
                throw;
            }

            try
            {
                using (dataReader)
                {
                    return dataReader.ToObject<T>();
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static async Task<T> ExecuteDataReaderAsync<T>(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters) where T : new()
        {
            CancellationToken cancellationToken = CancellationToken.None;

            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            IDataReader dataReader;

            try
            {
                var dbCommand = CreateCommand(
                    dbConnection,
                    commandType,
                    commandText,
                    commandParameters);

                await dbConnection.OpenAsync(cancellationToken);
                dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken);
            }
            catch
            {
                // If failure happens creating the data reader, then it won't be available to
                // handle closing the connection, so do it explicitly here to preserve ref counting.
                dbConnection.Close();
                throw;
            }

            try
            {
                using (dataReader)
                {
                    return dataReader.ToObject<T>();
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static ICollection<T> ExecuteDataReaderCollection<T>(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
            where T : new()
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            IDataReader dataReader;

            try
            {
                var dbCommand = CreateCommand(
                    dbConnection,
                    commandType,
                    commandText,
                    commandParameters);

                dbConnection.Open();
                dataReader = dbCommand.ExecuteReader();
            }
            catch
            {
                // If failure happens creating the data reader, then it won't be available to
                // handle closing the connection, so do it explicitly here to preserve ref counting.
                dbConnection.Close();
                throw;
            }

            try
            {
                using (dataReader)
                {
                    return dataReader.ToObjectCollection<T>();
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static async Task<ICollection<T>> ExecuteDataReaderCollectionAsync<T>(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters) where T : new()
        {
            CancellationToken cancellationToken = CancellationToken.None;

            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            IDataReader dataReader;

            try
            {
                var dbCommand = CreateCommand(
                    dbConnection,
                    commandType,
                    commandText,
                    commandParameters);

                await dbConnection.OpenAsync(cancellationToken);
                dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken);
            }
            catch
            {
                // If failure happens creating the data reader, then it won't be available to
                // handle closing the connection, so do it explicitly here to preserve ref counting.
                dbConnection.Close();
                throw;
            }

            try
            {
                using (dataReader)
                {
                    return dataReader.ToObjectCollection<T>();
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static DbCommand CreateCommand(
            IRelationalConnection connection,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
        {
            var command = connection.DbConnection.CreateCommand();

            command.CommandType = commandType;
            command.CommandText = commandText;

            if (connection.CurrentTransaction != null)
            {
                command.Transaction = connection
                    .CurrentTransaction.GetDbTransaction();
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            if (commandParameters != null && commandParameters.Length > 0)
            {
                foreach (var parameter in commandParameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }
        public static DataTable SqlQuery(this DatabaseFacade databaseFacade,
           CommandType commandType,
           string commandText,
           params object[] commandParameters)
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }
            try
            {
                DataTable dt = new DataTable();
                using (var dbConnection = databaseFacade.GetService<IRelationalConnection>())
                {
                    dbConnection.Open();
                    using (var dbCommand = CreateCommand(dbConnection, commandType, commandText, commandParameters))
                    {
                        using (IDataReader dataReader = dbCommand.ExecuteReader())
                        {
                            dt.Load(dataReader);
                            return dt;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        public static async Task<DataTable> SqlQueryAsync(this DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }
            try
            {
                DataTable dt = new DataTable();
                using (var dbConnection = databaseFacade.GetService<IRelationalConnection>())
                {
                    await dbConnection.OpenAsync(cancellationToken);
                    using (var dbCommand = CreateCommand(dbConnection, commandType, commandText, commandParameters))
                    {
                        using (IDataReader dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken))
                        {
                            dt.Load(dataReader);
                            return dt;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
