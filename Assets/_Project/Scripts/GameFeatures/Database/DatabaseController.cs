using System;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using Zenject;

namespace _Project.GameFeatures.Database
{
    public class DatabaseController : IInitializable
    {
        private const string DatabaseName = "LibraryFundMovement.db";

        private string _databasePath;
        private IDbConnection _dbConnection;

        public void Initialize() => ConnectToDatabase();
        
        private void ConnectToDatabase()
        {
            _databasePath = Path.Combine(Application.persistentDataPath, DatabaseName);

            if (File.Exists(_databasePath) == false)
            {
                Debug.LogWarning($"Database file not found at: {_databasePath}");
                Debug.Log("Creating new database file...");
            }
            else
            {
                Debug.Log($"Found existing database at: {_databasePath}");
            }

            string connectionString = $"URI=file:{_databasePath}";

            try
            {
                _dbConnection = new SqliteConnection(connectionString);
                _dbConnection.Open();
                Debug.Log("Successfully connected to database!");
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Failed to connect to database: {exception.Message}");
            }
        }
        
        public void ExecuteQuery(string query, params IDbDataParameter[] parameters)
        {
            if (_dbConnection == null || _dbConnection.State != ConnectionState.Open)
                ConnectToDatabase();

            try
            {
                using (IDbCommand command = _dbConnection.CreateCommand())
                {
                    command.CommandText = query;
            
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                            command.Parameters.Add(param);
                    }
            
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Query execution failed: {exception.Message}");
            }
        }

        public IDataReader ReadData(string query)
        {
            if (_dbConnection == null || _dbConnection.State != ConnectionState.Open)
                ConnectToDatabase();

            try
            {
                IDbCommand command = _dbConnection.CreateCommand();
                command.CommandText = query;
                return command.ExecuteReader();
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Data reading failed: {exception.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
                Debug.Log("Database connection closed on application quit");
            }
        }
    }
}