using System;
using System.Data;
using _Project.Core.Services;
using _Project.GameFeatures.Database;
using Mono.Data.Sqlite;
using UnityEngine;
using Zenject;

namespace _Project.GameFeatures.UI.Genres
{
    public class GenresPresenter : IInitializable, IDisposable
    {
        private readonly GenresPopup _genresPopup;
        private readonly DatabaseController _databaseController;
        private readonly NotificationService _notificationService;

        private DataTable _genresData;
        private int _currentIndex = -1;
        private bool _isNewRecordMode = false;

        public GenresPresenter(GenresPopup genresPopup, DatabaseController databaseController, NotificationService notificationService)
        {
            _genresPopup = genresPopup;
            _databaseController = databaseController;
            _notificationService = notificationService;
        }

        public void Initialize()
        {
            _genresPopup.OnPreviousClick += OnPreviousClick;
            _genresPopup.OnNextClick += OnNextClick;
            _genresPopup.OnDeleteClick += OnDeleteClick;

            LoadGenresData();
            ShowFirstRecord();
        }

        public void Dispose()
        {
            _genresPopup.OnPreviousClick -= OnPreviousClick;
            _genresPopup.OnNextClick -= OnNextClick;
            _genresPopup.OnDeleteClick -= OnDeleteClick;
        }

        private void LoadGenresData()
        {
            try
            {
                string query = "SELECT * FROM Жанры ORDER BY id_жанра";
                _genresData = new DataTable();

                using (var reader = _databaseController.ReadData(query))
                {
                    _genresData.Load(reader);
                }

                _notificationService.Notify($"Загружено записей жанров: {_genresData.Rows.Count}");
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке данных: {ex.Message}");
                _genresData = new DataTable();
            }
        }

        private void ShowFirstRecord()
        {
            if (_genresData.Rows.Count > 0)
            {
                _currentIndex = 0;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void DisplayCurrentRecord()
        {
            if (_currentIndex >= 0 && _currentIndex < _genresData.Rows.Count)
            {
                DataRow row = _genresData.Rows[_currentIndex];
                _genresPopup.GenreInput.text = row["название_жанра"].ToString();
            }
        }

        private void EnterNewRecordMode()
        {
            _genresPopup.ClearField();
            _currentIndex = -1;
            _isNewRecordMode = true;
            _notificationService.Notify("Режим добавления новой записи");
        }

        private void OnPreviousClick()
        {
            if (_isNewRecordMode)
            {
                if (_genresData.Rows.Count > 0)
                {
                    _currentIndex = _genresData.Rows.Count - 1;
                    DisplayCurrentRecord();
                    _isNewRecordMode = false;
                }
            }
            else if (_currentIndex > 0)
            {
                _currentIndex--;
                DisplayCurrentRecord();
            }
            else
            {
                _notificationService.Notify("Это первая запись");
            }
        }

        private void OnNextClick()
        {
            if (_isNewRecordMode)
            {
                InsertNewGenre();
            }
            else if (_currentIndex < _genresData.Rows.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentRecord();
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void InsertNewGenre()
        {
            string genreText = _genresPopup.GenreInput.text;

            if (string.IsNullOrEmpty(genreText))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            try
            {
                string query = @"INSERT INTO Жанры 
                                (название_жанра) 
                                VALUES (@genreText)";

                IDbDataParameter[] parameters = { new SqliteParameter("@genreText", genreText) };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Жанр успешно добавлен в базу данных!");

                LoadGenresData();
                _currentIndex = _genresData.Rows.Count - 1;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при добавлении жанра: {ex.Message}");
            }
        }

        private void OnDeleteClick()
        {
            if (_isNewRecordMode || _currentIndex < 0 || _currentIndex >= _genresData.Rows.Count)
                return;

            try
            {
                int id = Convert.ToInt32(_genresData.Rows[_currentIndex]["id_жанра"]);
                string query = "DELETE FROM Жанры WHERE id_жанра = @id";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Запись удалена!");

                LoadGenresData();

                if (_genresData.Rows.Count > 0)
                {
                    _currentIndex = Math.Min(_currentIndex, _genresData.Rows.Count - 1);
                    DisplayCurrentRecord();
                }
                else
                {
                    EnterNewRecordMode();
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при удалении записи: {ex.Message}");
            }
        }
    }
}