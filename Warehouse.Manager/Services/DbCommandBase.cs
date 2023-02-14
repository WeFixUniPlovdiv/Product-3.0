using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Warehouse.Manager.Utils;

namespace Warehouse.Manager.Services
{
    public interface IDbCommandBase
    {
        void RevertUnauthorizedAccess(long? key = null);
        Task<long> AllowUnauthorizedAccess(bool state);
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        Task ExecuteNonQuery(string stmt, CommandType commandType = CommandType.Text);
        Task ExecuteNonQuery(string stmt, SqlParameter parameter, CommandType commandType = CommandType.Text);
        Task ExecuteNonQuery(string stmt, List<SqlParameter> parameters, CommandType commandType = CommandType.Text);
        Task<T> GetScalarResult<T>(string stmt);
        Task<T> GetScalarResult<T>(string stmt, SqlParameter parameter);
        Task<T> GetScalarResult<T>(string stmt, List<SqlParameter> parameters);
        Task<List<Dictionary<string, object>>> GetDictionaryListResult(string stmt);
        Task<List<Dictionary<string, object>>> GetDictionaryListResult(string stmt, SqlParameter parameter);
        Task<List<Dictionary<string, object>>> GetDictionaryListResult(string stmt, List<SqlParameter> parameters);
        Task<Dictionary<string, object>> GetDictionaryResult(string stmt);
        Task<Dictionary<string, object>> GetDictionaryResult(string stmt, SqlParameter parameter);
        Task<Dictionary<string, object>> GetDictionaryResult(string stmt, List<SqlParameter> parameters);
        Task<List<T>> GetListResult<T>(string stmt);
        Task<List<T>> GetListResult<T>(string stmt, SqlParameter parameter);
        Task<List<T>> GetListResult<T>(string stmt, List<SqlParameter> parameters);
        Task<T> GetFirstResult<T>(string stmt);
        Task<T> GetFirstResult<T>(string stmt, SqlParameter parameter);
        Task<T> GetFirstResult<T>(string stmt, List<SqlParameter> parameters);
        Task<List<List<Dictionary<string, object>>>> GetMultiDictionaryListResult(string stmt);
        Task<List<List<Dictionary<string, object>>>> GetMultiDictionaryListResult(string stmt, SqlParameter parameter);
        Task<List<List<Dictionary<string, object>>>> GetMultiDictionaryListResult(string stmt, List<SqlParameter> parameters);
    }

    public abstract class DbCommandBase : IDbCommandBase, IDisposable
    {
        public static object ValueOrNull(object value) => value ?? DBNull.Value;

        protected readonly ILoggingSingletonService loggingService;
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly SqlConnection sqlConnection;
        protected SqlCommand SqlCommand { get; set; }
        protected SqlTransaction SqlTransaction { get; set; }
        protected bool ConnectionOpened { get; set; }
        protected bool InTransaction { get; set; }
        protected bool UnauthorizedAccessAllowed { get; set; }
        protected Dictionary<long, bool> UnauthorizedAccessMemory { get; } = new Dictionary<long, bool>();

        public DbCommandBase(IOptions<AppConfig> appSettings, ILoggingSingletonService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            this.loggingService = loggingService;
            this.httpContextAccessor = httpContextAccessor;
            sqlConnection = new SqlConnection(appSettings.Value.DbConnectionsStrings[appSettings.Value.DbConnection].GetConnectionString());
        }

        private async Task ConnectionOpen()
        {
            if (ConnectionOpened)
                return;
            try
            {
                await sqlConnection.OpenAsync();
            }
            catch (Exception e)
            {
                throw ExceptionHelper.GetException(e, _e => new NoConnectionException(_e.Message));
            }
            ConnectionOpened = sqlConnection.State == ConnectionState.Open;
            if (!ConnectionOpened)
                throw new NoConnectionException($"Could not open the connection. Current connection state is {sqlConnection.State}.");
        }

        private void ConnectionClose(bool force = false)
        {
            if (!force && InTransaction || !ConnectionOpened)
                return;
            try { sqlConnection.Close(); } catch (Exception) { }
            ConnectionOpened = !(sqlConnection.State == ConnectionState.Closed);
            if (ConnectionOpened)
                throw new NoConnectionException($"Could not close the connection. Current connection state is {sqlConnection.State}.");
        }

        private void EndTransaction()
        {
            SqlCommand = null;
            SqlTransaction = null;
            InTransaction = false;
            ConnectionClose(true);
        }

        private SqlCommand GetPrepareCommand(string stmt, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            SqlCommand cmd;
            if (InTransaction)
            {
                cmd = SqlCommand;
                cmd.Parameters.Clear();
            }
            else
            {
                cmd = sqlConnection.CreateCommand();
                cmd.Connection = sqlConnection;
            }
            //cmd.CommandText = "SET ANSI_WARNINGS OFF; " + stmt;
            cmd.CommandType = commandType;
            cmd.CommandText = stmt;
            if (parameters != null)
                if (parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());
            return cmd;
        }

        private async Task<SqlCommand> GetCommand(string stmt, List<SqlParameter> parameters = null)
        {
            await Task.CompletedTask;
            var cmd = GetPrepareCommand(stmt, parameters);
            if (InTransaction)
                cmd.Transaction = SqlTransaction;
            return cmd;
        }
        private async Task<SqlCommand> GetCommand(string stmt, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            await Task.CompletedTask;
            var cmd = GetPrepareCommand(stmt, parameters, commandType);
            if (InTransaction)
                cmd.Transaction = SqlTransaction;
            return cmd;
        }
        private async Task<DataTable> GetTableResult(string stmt, List<SqlParameter> parameters)
        {
            try
            {
                var dt = new DataTable();
                await ConnectionOpen();
                using (var cmd = await GetCommand(stmt, parameters))
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    dt.Load(dr);
                ConnectionClose();
                return dt;
            }
            catch (SqlException e)
            {
                ConnectionClose(true);
                var ex = ExceptionHelper.GetException(e, _e => new DatabaseException(e));
                if (!(ex is ClientException))
                    loggingService.Log(LogLevel.Error, "0x9910001", Exception: e, Parameters: new Dictionary<string, object>() {
                        {nameof(stmt), stmt },
                        {nameof(parameters), parameters.ToDictionary(x => x.ParameterName, x => new { x.Value, x.SqlDbType }) }
                    });
                throw ex;
            }
        }

        private static List<SqlParameter> CopySqlParameters(List<SqlParameter> parameters)
        {
            return parameters.Select(
              p => new SqlParameter(
                p.ParameterName,
                p.SqlDbType,
                p.Size,
                p.Direction,
                p.Precision,
                p.Scale,
                p.SourceColumn,
                p.SourceVersion,
                p.SourceColumnNullMapping,
                p.Value,
                p.XmlSchemaCollectionDatabase,
                p.XmlSchemaCollectionOwningSchema,
                p.XmlSchemaCollectionName
              )
            ).ToList();
        }

        public void RevertUnauthorizedAccess(long? key = null)
        {
            var state = false;
            if (key == null)
                if (UnauthorizedAccessMemory.Count > 0)
                    key = UnauthorizedAccessMemory.Last().Key;
            if (key is long _key)
                if (UnauthorizedAccessMemory.ContainsKey(_key))
                {
                    state = UnauthorizedAccessMemory[_key];
                    UnauthorizedAccessMemory.Remove(_key);
                }
            UnauthorizedAccessAllowed = state;
        }

        public async Task<long> AllowUnauthorizedAccess(bool state)
        {
            var key = DateTime.Now.Ticks;
            await Task.Delay(1);
            UnauthorizedAccessMemory.Add(key, UnauthorizedAccessAllowed);
            UnauthorizedAccessAllowed = state;
            return key;
        }

        public async Task BeginTransaction()
        {
            if (InTransaction)
            {
                var e = new DatabaseException("Nested local sql transactions are not implemented!");
                loggingService.Log(LogLevel.Error, "0x9910000", e);
                throw e;
            }

            await ConnectionOpen();
            SqlTransaction = sqlConnection.BeginTransaction($"{nameof(DbCommandBase)}Transaction{new Random().Next(100, 999)}");
            SqlCommand = sqlConnection.CreateCommand();
            SqlCommand.Connection = sqlConnection;
            SqlCommand.Transaction = SqlTransaction;
            InTransaction = true;
        }

        public async Task CommitTransaction()
        {
            SqlTransaction.Commit();
            EndTransaction();
            await Task.CompletedTask;
        }

        public async Task RollbackTransaction()
        {
            SqlTransaction.Rollback();
            EndTransaction();
            await Task.CompletedTask;
        }

        public async Task ExecuteNonQuery(string stmt, CommandType commandType = CommandType.Text)
        {
            await ExecuteNonQuery(stmt, new List<SqlParameter>());
        }

        public async Task ExecuteNonQuery(string stmt, SqlParameter parameter, CommandType commandType = CommandType.Text)
        {
            await ExecuteNonQuery(stmt, new List<SqlParameter>() { parameter }, commandType);
        }

        public async Task ExecuteNonQuery(string stmt, List<SqlParameter> parameters, CommandType commandType = CommandType.Text)
        {
            try
            {
                await ConnectionOpen();
                using (var cmd = await GetCommand(stmt, parameters, commandType))
                    await cmd.ExecuteNonQueryAsync();
                ConnectionClose();
            }
            catch (SqlException e)
            {
                ConnectionClose(true);
                var ex = ExceptionHelper.GetException(e, _e => new DatabaseException(e));
                if (!(ex is ClientException))
                    loggingService.Log(LogLevel.Error, "0x9910002", Exception: e, Parameters: new Dictionary<string, object>() {
                        {nameof(stmt), stmt },
                        {nameof(parameters), parameters.ToDictionary(x => x.ParameterName, x => new { x.Value, x.SqlDbType }) }
                    });
                throw ex;
            }
        }

        public async Task<T> GetScalarResult<T>(string stmt)
        {
            return await GetScalarResult<T>(stmt, new List<SqlParameter>());
        }

        public async Task<T> GetScalarResult<T>(string stmt, SqlParameter parameter)
        {
            return await GetScalarResult<T>(stmt, new List<SqlParameter>() { parameter });
        }

        public async Task<T> GetScalarResult<T>(string stmt, List<SqlParameter> parameters)
        {
            try
            {
                object result;
                await ConnectionOpen();
                using (var cmd = await GetCommand(stmt, parameters))
                    result = await cmd.ExecuteScalarAsync();
                ConnectionClose();
                var type = typeof(T);

                if (type == typeof(string))
                {
                    try
                    {
                        result = result?.ToString();
                    }
                    catch (Exception e)
                    {
                        throw ExceptionHelper.GetException(e, _e => new ParseException(_e.Message));
                    }
                }
                else if (type == typeof(int))
                {
                    try
                    {
                        result = int.Parse(result?.ToString());
                    }
                    catch (Exception e)
                    {
                        throw ExceptionHelper.GetException(e, _e => new ParseException(_e.Message));
                    }
                }
                else if (type == typeof(int?))
                {
                    try
                    {
                        result = int.Parse(result?.ToString());
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                }
                else if (type == typeof(bool))
                {
                    try
                    {
                        result = bool.Parse(result?.ToString());
                    }
                    catch (Exception e)
                    {
                        throw ExceptionHelper.GetException(e, _e => new ParseException(_e.Message));
                    }
                }
                else if (type == typeof(bool?))
                {
                    try
                    {
                        result = bool.Parse(result?.ToString());
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                }
                else
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<T>(result?.ToString());
                    }
                    catch (Exception e)
                    {
                        throw ExceptionHelper.GetException(e, _e => new ParseException(_e.Message));
                    }
                }
                return (T)result;
            }
            catch (SqlException e)
            {
                ConnectionClose(true);
                var ex = ExceptionHelper.GetException(e, _e => new DatabaseException(e));
                if (!(ex is ClientException))
                    loggingService.Log(LogLevel.Error, "0x9910003", Exception: e, Parameters: new Dictionary<string, object>() {
                        {nameof(stmt), stmt },
                        {nameof(parameters), parameters.ToDictionary(x => x.ParameterName, x => new { x.Value, x.SqlDbType }) }
                    });
                throw ex;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetDictionaryListResult(string stmt)
        {
            return await GetDictionaryListResult(stmt, new List<SqlParameter>());
        }

        public async Task<List<Dictionary<string, object>>> GetDictionaryListResult(string stmt, SqlParameter parameter)
        {
            return await GetDictionaryListResult(stmt, new List<SqlParameter>() { parameter });
        }

        public async Task<List<Dictionary<string, object>>> GetDictionaryListResult(string stmt, List<SqlParameter> parameters)
        {
            return JsonHelper.DataTableToDictionaryList<object>(await GetTableResult(stmt, parameters));
        }

        public async Task<Dictionary<string, object>> GetDictionaryResult(string stmt)
        {
            return await GetDictionaryResult(stmt, new List<SqlParameter>());
        }

        public async Task<Dictionary<string, object>> GetDictionaryResult(string stmt, SqlParameter parameter)
        {
            return await GetDictionaryResult(stmt, new List<SqlParameter>() { parameter });
        }

        public async Task<Dictionary<string, object>> GetDictionaryResult(string stmt, List<SqlParameter> parameters)
        {
            return JsonHelper.DataTableToDictionaryList<object>(await GetTableResult(stmt, parameters)).FirstOrDefault();
        }

        public async Task<List<T>> GetListResult<T>(string stmt)
        {
            return await GetListResult<T>(stmt, new List<SqlParameter>());
        }

        public async Task<List<T>> GetListResult<T>(string stmt, SqlParameter parameter)
        {
            return await GetListResult<T>(stmt, new List<SqlParameter>() { parameter });
        }

        public async Task<List<T>> GetListResult<T>(string stmt, List<SqlParameter> parameters)
        {
            return JsonHelper.DataTableToList<T>(await GetTableResult(stmt, parameters));
        }

        public async Task<T> GetFirstResult<T>(string stmt)
        {
            return await GetFirstResult<T>(stmt, new List<SqlParameter>());
        }

        public async Task<T> GetFirstResult<T>(string stmt, SqlParameter parameter)
        {
            return await GetFirstResult<T>(stmt, new List<SqlParameter>() { parameter });
        }

        public async Task<T> GetFirstResult<T>(string stmt, List<SqlParameter> parameters)
        {
            return JsonHelper.DataTableToList<T>(await GetTableResult(stmt, parameters)).FirstOrDefault();
        }

        public async Task<List<List<Dictionary<string, object>>>> GetMultiDictionaryListResult(string stmt)
        {
            return await GetMultiDictionaryListResult(stmt, new List<SqlParameter>());
        }

        public async Task<List<List<Dictionary<string, object>>>> GetMultiDictionaryListResult(string stmt, SqlParameter parameter)
        {
            return await GetMultiDictionaryListResult(stmt, new List<SqlParameter> { parameter });
        }

        public async Task<List<List<Dictionary<string, object>>>> GetMultiDictionaryListResult(string stmt, List<SqlParameter> parameters)
        {
            try
            {
                var res = new List<List<Dictionary<string, object>>>();
                await ConnectionOpen();

                using (var cmd = await GetCommand(stmt, parameters))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                using (DataSet ds = new DataSet())
                {
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    foreach (DataTable dt in ds.Tables)
                        res.Add(JsonHelper.DataTableToDictionaryList<object>(dt));
                }
                ConnectionClose();
                return res;
            }
            catch (SqlException e)
            {
                ConnectionClose(true);
                var ex = ExceptionHelper.GetException(e, _e => new DatabaseException(e));
                if (!(ex is ClientException))
                    loggingService.Log(LogLevel.Error, "0x9910004", Exception: e, Parameters: new Dictionary<string, object>() {
                        {nameof(stmt), stmt },
                        {nameof(parameters), parameters.ToDictionary(x => x.ParameterName, x => new { x.Value, x.SqlDbType }) }
                    });
                throw ex;
            }
        }

        public void Dispose()
        {
            if (!InTransaction && !ConnectionOpened)
                return;

            var command = string.Empty;
            var parameters = new Dictionary<string, object>();
            if (SqlCommand != null)
            {
                if (SqlCommand.CommandText != null)
                    command = SqlCommand.CommandText;
                if (SqlCommand.Parameters != null)
                    foreach (SqlParameter param in SqlCommand.Parameters)
                        parameters.Add(param.ParameterName, new { param.Value, param.SqlDbType });
            }

            loggingService.Log(LogLevel.Error, "0x9000101", new Exception($"{nameof(Dispose)} of ${nameof(DbCommandBase)}"), new Dictionary<string, object>()
              {
                { nameof(command), command},
                { nameof(parameters), parameters},
                { nameof(InTransaction), InTransaction},
                { nameof(ConnectionOpened), ConnectionOpened},
                { nameof(UnauthorizedAccessAllowed), UnauthorizedAccessAllowed}
              });

            GC.SuppressFinalize(this);
        }
    }
}


