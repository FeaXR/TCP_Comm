using Npgsql;
using System;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;

namespace DataBase
{
    public class DataBaseConnection : IDisposable
    {
        private NpgsqlConnection conn;
        private NpgsqlCommand InsertCommand { get; set; }
        public NpgsqlDataAdapter ResultDataAdapter { get; private set; } = new NpgsqlDataAdapter();

        /// <summary>
        /// Returned data is stored in here
        /// </summary>
        public DataTable ResultDataTable { get; private set; } = new DataTable();

        /// <summary>
        /// Triggers when command execution into public property DataTable is finished
        /// </summary>
        public event EventHandler DataTableReady;

        /// <summary>
        /// Opens PostgreSQL database connection with the given parameters. Enter IP 0.0.0.0 for localhost connection
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="databasename"></param>
        /// <param name="pooling"></param>
        /// <param name="minpoolsize"></param>
        /// <param name="maxpoolsize"></param>
        /// <param name="timeout"></param>
        /// <exception cref="Exception"> Thrown when connection is already open </exception>
        /// <exception cref="FormatException"> Thrown when IP is in wrong format</exception>
        public void DataBaseOpen(string IP = "0.0.0.0", int port = 5432, string username = "postgres", string password = "admin", string databasename = "database", bool pooling = false, int minpoolsize = 1, int maxpoolsize = 999, int timeout = 15)
        {
            if (conn?.State == ConnectionState.Open)
            {
                throw new Exception("Server connection is already open, can not open it again!");
            }

            if (!new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b").IsMatch(IP) && IP != "localhost")
            {
                throw new FormatException("Invalid IP for server connection!");
            }

            if (IP == "0.0.0.0")
            {
                IP = "localhost";
            }

            var sConnectionString = "Server=" + IP + ";Port=" + port + ";Username=" + username + ";Password=" + password + ";Database=" + databasename + ";Pooling=" + pooling + ";MinPoolSize=" + minpoolsize + ";MaxPoolSize=" + maxpoolsize + ";Timeout=" + timeout + ";";
            conn = new NpgsqlConnection(sConnectionString);
            InsertCommand = conn.CreateCommand();

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Execute command with returned data stored in DataTable.
        /// Returned data will be stored in public property DataTable.
        /// Automatically closes DataBase Connection
        /// </summary>
        /// <param name="query"></param>
        public void ExecuteCommandToDataAdapter(string query)
        {
            var dataTableWorker = new BackgroundWorker();
            dataTableWorker.DoWork += DataTableWorker_DoWork;
            dataTableWorker.RunWorkerCompleted += DataTableWorker_RunWorkerCompleted;
            dataTableWorker.RunWorkerAsync(argument: query);
        }

        /// <summary>
        /// Execute command with no returned data
        /// Automatically closes DataBase Connection
        /// </summary>
        /// <exception cref="NpgsqlException">Thrown when connection is not established</exception>
        public void ExecuteInsertCommand(string query)
        {
            if (conn.State != ConnectionState.Open)
            {
                throw new NpgsqlException("Connection is not established to the server!");
            }

            var insertCommandWorker = new BackgroundWorker();
            insertCommandWorker.DoWork += InsertCommandWorker_DoWork;
            insertCommandWorker.RunWorkerAsync(argument: query);
        }

        /// <summary>
        /// Closes DataBase Connection
        /// </summary>
        public void DataBaseClose()
        {
            NpgsqlConnection.ClearAllPools();
            conn.Close();
        }

        private void InsertCommandWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            InsertCommand.CommandText = (string)e.Argument;
            InsertCommand.ExecuteNonQuery();
            NpgsqlConnection.ClearAllPools();
            conn.Close();
        }

        private void DataTableWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var command = (string)e.Argument;
            InsertCommand = new NpgsqlCommand(command, conn);
            NpgsqlDataAdapter dataAdp = new NpgsqlDataAdapter(InsertCommand);
            e.Result = dataAdp;
            NpgsqlConnection.ClearAllPools();
            conn.Close();
        }

        private void DataTableWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ResultDataAdapter = (NpgsqlDataAdapter)e.Result;
            ResultDataTable = new DataTable();
            ResultDataAdapter.Fill(ResultDataTable);
            DataTableReady(this, EventArgs.Empty);
        }

        /// <summary>
        /// Checks if datatable exists on server, default ip is localhost
        /// </summary>
        /// <param name="tableName"></param>
        public bool CheckIfTableExist(string tableName)
        {
            if (conn.State != ConnectionState.Open)
            {
                throw new NpgsqlException("Connection is not open!");
            }

            try
            {
                InsertCommand.CommandText = "SELECT * FROM public." + tableName;
                InsertCommand.ExecuteNonQuery();
                NpgsqlConnection.ClearAllPools();

                return true;
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        /// <summary>
        /// Dispose DataBase Connection
        /// </summary>
        public void Dispose()
        {
            DataBaseClose();
            conn.Dispose();
            InsertCommand.Dispose();
            ResultDataTable.Dispose();
            ResultDataAdapter.Dispose();
        }
    }
}
