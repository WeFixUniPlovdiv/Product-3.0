using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using Newtonsoft.Json;

namespace Warehouse.Manager.Utils
{
    public class DbJsonException
    {
        public string Procedure { set; get; }
        public string ClientMessage { set; get; }
        public string ClientMessageJsonArgs { set; get; }
        public ExceptionHelper.ExceptionCallback Callback { set; get; }
        public ExceptionHelper.ExceptionHandler Handler { set; get; }
        public string Message { set; get; }
        public int? State { set; get; }
        public int? Line { set; get; }
        public int? Number { set; get; }
        public bool? TransactionRollbackFlag { set; get; }
        public int? EvaluatedLine { set; get; }
        public object MoreDetails { get; set; }
    }

    public interface IAppException
    {
        string ClientTitle { get; }
        string ClientMessage { get; }
        ExceptionHelper.ExceptionCallback Callback { get; set; }
        ExceptionHelper.ExceptionHandler Handler { get; set; }
        Dictionary<string, object> Args { get; }
        HttpStatusCode StatusCode { get; }

        string Message { get; }
        string StackTrace { get; }
    }

    public static class ExceptionHelper
    {
        public enum ExceptionCallback { NONE, GoBack, GoHome, GoToAssets, Log, CloseToppestModal, Origin };
        public enum ExceptionHandler { NONE, ResendInvitation, ResendEmailVerification, Log };

        public static Exception GetException(Exception e, Func<Exception, IAppException> func)
        {
            if (e as IAppException != null)
                return e;
            return (Exception)func(e);
        }
    }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
  public class ClientException : Exception, IAppException
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
  {
        private static HttpStatusCode DefaultStatusCode => HttpStatusCode.BadRequest;

        public string ClientTitle => "Warning";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode { get; }

        public ClientException(string clientMessage)
          : this(clientMessage, null, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, ExceptionHelper.ExceptionHandler.NONE, null) { }
        public ClientException(string clientMessage, string error)
          : this(clientMessage, error, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, ExceptionHelper.ExceptionHandler.NONE, null) { }
        public ClientException(string clientMessage, HttpStatusCode statusCode)
          : this(clientMessage, null, statusCode, ExceptionHelper.ExceptionCallback.NONE, ExceptionHelper.ExceptionHandler.NONE, null) { }

        public ClientException(string clientMessage, ExceptionHelper.ExceptionCallback callback)
          : this(clientMessage, null, DefaultStatusCode, callback, ExceptionHelper.ExceptionHandler.NONE, null) { }
        public ClientException(string clientMessage, ExceptionHelper.ExceptionCallback callback, Dictionary<string, object> args)
          : this(clientMessage, null, DefaultStatusCode, callback, ExceptionHelper.ExceptionHandler.NONE, args) { }
        public ClientException(string clientMessage, HttpStatusCode statusCode, ExceptionHelper.ExceptionCallback callback)
          : this(clientMessage, null, statusCode, callback, ExceptionHelper.ExceptionHandler.NONE, null) { }
        public ClientException(string clientMessage, HttpStatusCode statusCode, ExceptionHelper.ExceptionCallback callback, Dictionary<string, object> args)
          : this(clientMessage, null, statusCode, callback, ExceptionHelper.ExceptionHandler.NONE, args) { }
        public ClientException(string clientMessage, string error, ExceptionHelper.ExceptionCallback callback)
          : this(clientMessage, error, DefaultStatusCode, callback, ExceptionHelper.ExceptionHandler.NONE, null) { }
        public ClientException(string clientMessage, string error, ExceptionHelper.ExceptionCallback callback, Dictionary<string, object> args)
          : this(clientMessage, error, DefaultStatusCode, callback, ExceptionHelper.ExceptionHandler.NONE, args) { }
        public ClientException(string clientMessage, string error, HttpStatusCode statusCode, ExceptionHelper.ExceptionCallback callback)
          : this(clientMessage, error, statusCode, callback, ExceptionHelper.ExceptionHandler.NONE, null) { }
        public ClientException(string clientMessage, string error, HttpStatusCode statusCode, ExceptionHelper.ExceptionCallback callback, Dictionary<string, object> args)
          : this(clientMessage, error, statusCode, callback, ExceptionHelper.ExceptionHandler.NONE, args) { }

        public ClientException(ExceptionHelper.ExceptionHandler handler)
          : this(null, null, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, handler, null) { }
        public ClientException(ExceptionHelper.ExceptionHandler handler, Dictionary<string, object> args)
          : this(null, null, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, handler, args) { }
        public ClientException(string clientMessage, ExceptionHelper.ExceptionHandler handler)
          : this(clientMessage, null, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, handler, null) { }
        public ClientException(string clientMessage, ExceptionHelper.ExceptionHandler handler, Dictionary<string, object> args)
          : this(clientMessage, null, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, handler, args) { }
        public ClientException(string clientMessage, HttpStatusCode statusCode, ExceptionHelper.ExceptionHandler handler)
          : this(clientMessage, null, statusCode, ExceptionHelper.ExceptionCallback.NONE, handler, null) { }
        public ClientException(string clientMessage, HttpStatusCode statusCode, ExceptionHelper.ExceptionHandler handler, Dictionary<string, object> args)
          : this(clientMessage, null, statusCode, ExceptionHelper.ExceptionCallback.NONE, handler, args) { }
        public ClientException(string clientMessage, string error, ExceptionHelper.ExceptionHandler handler)
          : this(clientMessage, error, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, handler, null) { }
        public ClientException(string clientMessage, string error, ExceptionHelper.ExceptionHandler handler, Dictionary<string, object> args)
          : this(clientMessage, error, DefaultStatusCode, ExceptionHelper.ExceptionCallback.NONE, handler, args) { }
        public ClientException(string clientMessage, string error, HttpStatusCode statusCode, ExceptionHelper.ExceptionHandler handler)
          : this(clientMessage, error, statusCode, ExceptionHelper.ExceptionCallback.NONE, handler, null) { }
        public ClientException(string clientMessage, string error, HttpStatusCode statusCode, ExceptionHelper.ExceptionHandler handler, Dictionary<string, object> args)
          : this(clientMessage, error, statusCode, ExceptionHelper.ExceptionCallback.NONE, handler, args) { }

        public ClientException(string clientMessage, string error, HttpStatusCode statusCode,
          ExceptionHelper.ExceptionCallback callback, ExceptionHelper.ExceptionHandler handler, Dictionary<string, object> args) : base(error)
        {
            ClientMessage = clientMessage;
            StatusCode = statusCode;
            Callback = callback;
            Handler = handler;
            Args = args;
        }
    }

    public class UnpermittedException : Exception, IAppException
    {
        private static string DefaultClientMessage => "You don't have enough permission to execute this action.";

        public string ClientTitle => "Warning";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.Forbidden;

        public UnpermittedException() : this(null) { }
        public UnpermittedException(string error) : this(error, DefaultClientMessage) { }
        public UnpermittedException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

    public class UnauthorizedException : Exception, IAppException
    {
        private static string DefaultClientMessage => "key.ExpiredSession";

        public string ClientTitle => "Warning";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;

        public UnauthorizedException() : this(null) { }
        public UnauthorizedException(string error) : this(error, DefaultClientMessage) { }
        public UnauthorizedException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

    public class CorruptedRequestException : Exception, IAppException
    {
        private static string DefaultClientMessage => "Client operational error.";

        public string ClientTitle => "Error";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.ExpectationFailed;

        public CorruptedRequestException() : this(null) { }
        public CorruptedRequestException(string error) : this(error, DefaultClientMessage) { }
        public CorruptedRequestException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

    public class ParseException : Exception, IAppException
    {
        private static string DefaultClientMessage => "Server operational error.";

        public string ClientTitle => "Error";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.ExpectationFailed;

        public ParseException() : this(null) { }
        public ParseException(string error) : this(error, DefaultClientMessage) { }
        public ParseException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
  public class DuplicateDatabaseObjectException : Exception, IAppException
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
  {
        public string ClientTitle => "Unique key violation";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.Conflict;

        public string IndexName { get; }
        public string TableName { get; }
        public string Value { get; }

        private readonly List<string> MsgConstants = new() {
      "Violation of UNIQUE KEY constraint ",
      "Cannot insert duplicate key in object ",
      "The duplicate key value is ",
      "The statement has been terminated.",
    };

        public DuplicateDatabaseObjectException(string error) : base(error)
        {
            int _position;
            var _message = error;
            _position = _message.IndexOf(MsgConstants[1]);
            var _indexName = _message.Substring(0, _position).Replace(MsgConstants[0], string.Empty).Replace("'", string.Empty).Trim();
            IndexName = _indexName.Substring(0, _indexName.Length - 1);
            _message = _message.Substring(_position);

            _position = _message.IndexOf(MsgConstants[2]);
            var _tableName = _message.Substring(0, _position).Replace(MsgConstants[1], string.Empty).Replace("'", string.Empty).Trim();
            TableName = _tableName.Substring(0, _tableName.Length - 1);
            _message = _message.Substring(_position);

            _position = _message.IndexOf(MsgConstants[3]);
            var _value = _message.Substring(0, _position).Replace(MsgConstants[2], string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Trim();
            Value = _value.Substring(0, _value.Length - 1);
            _ = _message.Substring(_position);

            ClientMessage = $"Duplicated values are: [{_value}].";
        }
    }

    public class DatabaseException : Exception, IAppException
    {
        private static string DefaultClientMessage => "Error processing data.";

        public string ClientTitle => "Error";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.ServiceUnavailable;

        public DatabaseException(SqlException sqlException)
        {
            DbJsonException dbJsonException;
            try
            {
                dbJsonException = JsonConvert.DeserializeObject<DbJsonException>(sqlException.Message);
            }
            catch (Exception)
            {
                if (sqlException.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                    throw new DuplicateDatabaseObjectException(sqlException.Message);
                else
                    throw new DatabaseException(JsonConvert.SerializeObject(new
                    {
                        sqlException.Message,
                        sqlException.Number,
                        sqlException.Procedure,
                        sqlException.LineNumber,
                    }), DefaultClientMessage);
            }
            if (dbJsonException.Line != null)
                dbJsonException.EvaluatedLine = dbJsonException.Line + 7;

            dbJsonException.MoreDetails = new
            {
                sqlException.ErrorCode,
                sqlException.InnerException,
                sqlException.Class,
                sqlException.LineNumber,
                sqlException.Number,
                sqlException.Procedure,
                sqlException.State
            };
            if (dbJsonException.ClientMessage == null)
                throw new DatabaseException(JsonConvert.SerializeObject(dbJsonException), DefaultClientMessage);

            string clientMessage = null;
            if (dbJsonException.ClientMessageJsonArgs != null)
                try { clientMessage = string.Format(dbJsonException.ClientMessage, JsonConvert.DeserializeObject<object[]>(dbJsonException.ClientMessageJsonArgs)); } catch (Exception) { }
            if (clientMessage == null)
                clientMessage = dbJsonException.ClientMessage;
            dbJsonException.ClientMessage = null;

            Dictionary<string, object> args = null;
            try { args = JsonConvert.DeserializeObject<Dictionary<string, object>>(dbJsonException.ClientMessageJsonArgs); } catch (Exception) { }
            throw new ClientException(
              clientMessage,
              JsonConvert.SerializeObject(dbJsonException, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
              HttpStatusCode.ExpectationFailed,
              dbJsonException.Callback,
              dbJsonException.Handler,
              args
            );
        }
        public DatabaseException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
        public DatabaseException(string error) : base(error) => ClientMessage = "Something went wrong";
    }

    public class TokenException : Exception, IAppException
    {
        public string ClientTitle => "Error";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.ExpectationFailed;

        public TokenException(string error) : this(error, null) { }
        public TokenException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

    public class NoConnectionException : Exception, IAppException
    {
        private static string DefaultClientMessage => "Could not establish connection.";

        public string ClientTitle => "Error";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.GatewayTimeout;

        public NoConnectionException() : this(null) { }
        public NoConnectionException(string error) : this(error, DefaultClientMessage) { }
        public NoConnectionException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

    public class UnknownException : Exception, IAppException
    {
        private static string DefaultClientMessage => "Something went wrong.";

        public string ClientTitle => "Error";
        public string ClientMessage { get; }
        public ExceptionHelper.ExceptionCallback Callback { get; set; }
        public ExceptionHelper.ExceptionHandler Handler { get; set; }
        public Dictionary<string, object> Args { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;

        public UnknownException() : this(null) { }
        public UnknownException(string error) : this(error, DefaultClientMessage) { }
        public UnknownException(string error, string clientMessage) : base(error) => ClientMessage = clientMessage;
    }

}

