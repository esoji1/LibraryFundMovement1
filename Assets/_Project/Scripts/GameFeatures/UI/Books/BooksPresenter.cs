using System;
using System.Collections.Generic;
using System.Data;
using _Project.Core.Services;
using _Project.GameFeatures.Database;
using Mono.Data.Sqlite;
using UnityEngine;
using Zenject;

namespace _Project.GameFeatures.UI.Books
{
    public class BooksPresenter : IInitializable, IDisposable
    {
        private readonly BooksPopup _booksPopup;
        private readonly DatabaseController _databaseController;
        private readonly NotificationService _notificationService;

        private DataTable _booksData;
        private int _currentIndex = -1;
        private bool _isNewRecordMode = false;

        public BooksPresenter(BooksPopup booksPopup, DatabaseController databaseController,
            NotificationService notificationService)
        {
            _booksPopup = booksPopup;
            _databaseController = databaseController;
            _notificationService = notificationService;
        }

        public void Initialize()
        {
            _booksPopup.OnPreviousClick += OnPreviousClick;
            _booksPopup.OnNextClick += OnNextClick;
            _booksPopup.OnDeleteClick += OnDeleteClick;
            _booksPopup.OnSaveClick += OnSaveClick;

            LoadGenresIntoDropdown();
            LoadBooksData();
            ShowFirstRecord();
        }

        public void Dispose()
        {
            _booksPopup.OnPreviousClick -= OnPreviousClick;
            _booksPopup.OnNextClick -= OnNextClick;
            _booksPopup.OnDeleteClick -= OnDeleteClick;
            _booksPopup.OnSaveClick -= OnSaveClick;
        }

        private void LoadBooksData()
        {
            try
            {
                string query = "SELECT * FROM Книги ORDER BY id_книги";
                _booksData = new DataTable();

                using (var reader = _databaseController.ReadData(query))
                {
                    _booksData.Load(reader);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке данных: {ex.Message}");
                _booksData = new DataTable();
            }
        }

        private void ShowFirstRecord()
        {
            if (_booksData.Rows.Count > 0)
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
            if (_currentIndex >= 0 && _currentIndex < _booksData.Rows.Count)
            {
                DataRow row = _booksData.Rows[_currentIndex];

                _booksPopup.ISBNInput.text = row["isbn"].ToString();
                _booksPopup.NameInput.text = row["название"].ToString();
                _booksPopup.AuthorInput.text = row["автор"].ToString();
                _booksPopup.PublishingHouseInput.text = row["издательство"].ToString();
                _booksPopup.YearPublicationInput.text = row["год_издания"].ToString();

                string genreId = row["id_жанра"].ToString();
                string genreName = GetGenreNameById(genreId);

                if (!string.IsNullOrEmpty(genreName))
                {
                    int genreIndex = _booksPopup.GenreList.options.FindIndex(option => option.text == genreName);
                    if (genreIndex >= 0)
                    {
                        _booksPopup.GenreList.value = genreIndex;
                    }
                    else
                    {
                        _notificationService.Notify($"Жанр '{genreName}' не найден в dropdown");
                        _booksPopup.GenreList.value = 0;
                    }
                }
                else
                {
                    _booksPopup.GenreList.value = 0;
                }

                _booksPopup.NumberCopiesInput.text = row["количество_экземпляров"].ToString();
                _booksPopup.CopiesAvailableInput.text = row["доступно_экземпляров"].ToString();
                _booksPopup.StorageLocationInput.text = row["место_хранения"].ToString();
            }
        }

        private void EnterNewRecordMode()
        {
            _booksPopup.ClearFields();
            _currentIndex = -1;
            _isNewRecordMode = true;
            _notificationService.Notify("Режим добавления новой записи");
        }

        private void OnPreviousClick()
        {
            if (_isNewRecordMode)
            {
                if (_booksData.Rows.Count > 0)
                {
                    _currentIndex = _booksData.Rows.Count - 1;
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
                InsertNewBooks();
            }
            else if (_currentIndex < _booksData.Rows.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentRecord();
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void InsertNewBooks()
        {
            string isbn = _booksPopup.ISBNInput.text;
            string name = _booksPopup.NameInput.text;
            string author = _booksPopup.AuthorInput.text;
            string publishingHouse = _booksPopup.PublishingHouseInput.text;
            string yearPublication = _booksPopup.YearPublicationInput.text;
            string genreName = _booksPopup.GenreList.options[_booksPopup.GenreList.value].text;
            string numberCopies = _booksPopup.NumberCopiesInput.text;
            string copiesAvailable = _booksPopup.CopiesAvailableInput.text;
            string storageLocation = _booksPopup.StorageLocationInput.text;

            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(author) || string.IsNullOrEmpty(publishingHouse) ||
                string.IsNullOrEmpty(yearPublication) || string.IsNullOrEmpty(numberCopies) ||
                string.IsNullOrEmpty(copiesAvailable) || string.IsNullOrEmpty(storageLocation))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            try
            {
                // Получаем ID жанра по названию
                string genreId = GetGenreIdByName(genreName);
                if (string.IsNullOrEmpty(genreId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID жанра!");
                    return;
                }

                string query = @"INSERT INTO Книги 
                        (isbn, название, автор, издательство, год_издания, id_жанра, количество_экземпляров, доступно_экземпляров, место_хранения) 
                        VALUES (@isbn, @name, @author, @publishingHouse, @yearPublication, @genreId, @numberCopies, 
                                @copiesAvailable, @storageLocation)";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@isbn", isbn),
                    new SqliteParameter("@name", name),
                    new SqliteParameter("@author", author),
                    new SqliteParameter("@publishingHouse", publishingHouse),
                    new SqliteParameter("@yearPublication", yearPublication),
                    new SqliteParameter("@genreId", genreId),
                    new SqliteParameter("@numberCopies", numberCopies),
                    new SqliteParameter("@copiesAvailable", copiesAvailable),
                    new SqliteParameter("@storageLocation", storageLocation)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Книга успешно добавлена в базу данных!");

                LoadBooksData();
                _currentIndex = _booksData.Rows.Count - 1;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при добавлении книги: {ex.Message}");
            }
        }

        private string GetGenreIdByName(string genreName)
        {
            if (string.IsNullOrEmpty(genreName))
                return string.Empty;

            try
            {
                string safeGenreName = genreName.Replace("'", "''");
                string query = $"SELECT id_жанра FROM Жанры WHERE название_жанра = '{safeGenreName}'";

                using (var reader = _databaseController.ReadData(query))
                {
                    if (reader.Read())
                    {
                        return reader["id_жанра"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при получении ID жанра: {ex.Message}");
            }

            return string.Empty;
        }

        private void OnDeleteClick()
        {
            if (_isNewRecordMode || _currentIndex < 0 || _currentIndex >= _booksData.Rows.Count)
                return;

            try
            {
                int id = Convert.ToInt32(_booksData.Rows[_currentIndex]["id_книги"]);
                string query = "DELETE FROM Книги WHERE id_книги = @id";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Запись удалена!");

                LoadBooksData();

                if (_booksData.Rows.Count > 0)
                {
                    _currentIndex = Math.Min(_currentIndex, _booksData.Rows.Count - 1);
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

        private string GetGenreNameById(string genreId)
        {
            if (string.IsNullOrEmpty(genreId))
                return string.Empty;

            try
            {
                string query = $"SELECT название_жанра FROM Жанры WHERE id_жанра = '{genreId}'";

                using (var reader = _databaseController.ReadData(query))
                {
                    if (reader.Read())
                    {
                        string genreName = reader["название_жанра"].ToString();
                        return genreName;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting genre name: {ex.Message}");
                _notificationService.Notify($"Ошибка при получении названия жанра: {ex.Message}");
            }

            return string.Empty;
        }

        private void LoadGenresIntoDropdown()
        {
            try
            {
                _booksPopup.GenreList.ClearOptions();

                string query = "SELECT название_жанра FROM Жанры ORDER BY название_жанра";
                List<string> genreNames = new List<string>();

                using (var reader = _databaseController.ReadData(query))
                {
                    while (reader.Read())
                    {
                        string genreName = reader["название_жанра"].ToString();
                        genreNames.Add(genreName);
                    }
                }

                _booksPopup.GenreList.AddOptions(genreNames);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке жанров: {ex.Message}");
            }
        }

        private void OnSaveClick()
        {
            SaveBook();
        }

        private void SaveBook()
        {
            string isbn = _booksPopup.ISBNInput.text;
            string name = _booksPopup.NameInput.text;
            string author = _booksPopup.AuthorInput.text;
            string publishingHouse = _booksPopup.PublishingHouseInput.text;
            string yearPublication = _booksPopup.YearPublicationInput.text;
            string genreName = _booksPopup.GenreList.options[_booksPopup.GenreList.value].text;
            string numberCopies = _booksPopup.NumberCopiesInput.text;
            string copiesAvailable = _booksPopup.CopiesAvailableInput.text;
            string storageLocation = _booksPopup.StorageLocationInput.text;

            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(author) || string.IsNullOrEmpty(publishingHouse) ||
                string.IsNullOrEmpty(yearPublication) || string.IsNullOrEmpty(numberCopies) ||
                string.IsNullOrEmpty(copiesAvailable) || string.IsNullOrEmpty(storageLocation))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            try
            {
                string genreId = GetGenreIdByName(genreName);
                if (string.IsNullOrEmpty(genreId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID жанра!");
                    return;
                }

                int id = Convert.ToInt32(_booksData.Rows[_currentIndex]["id_книги"]);
                string updateQuery = @"UPDATE Книги 
                                    SET isbn = @isbn,
                                        название = @name,
                                        автор = @author,
                                        издательство = @publishingHouse,
                                        год_издания = @yearPublication,
                                        id_жанра = @genreId,
                                        количество_экземпляров = @numberCopies,
                                        доступно_экземпляров = @copiesAvailable,
                                        место_хранения = @storageLocation
                                    WHERE id_книги = @id";

                IDbDataParameter[] updateParameters =
                {
                    new SqliteParameter("@isbn", isbn),
                    new SqliteParameter("@name", name),
                    new SqliteParameter("@author", author),
                    new SqliteParameter("@publishingHouse", publishingHouse),
                    new SqliteParameter("@yearPublication", yearPublication),
                    new SqliteParameter("@genreId", genreId),
                    new SqliteParameter("@numberCopies", numberCopies),
                    new SqliteParameter("@copiesAvailable", copiesAvailable),
                    new SqliteParameter("@storageLocation", storageLocation),
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(updateQuery, updateParameters);
                _notificationService.Notify("Запись книги успешно обновлена!");

                LoadBooksData();

                if (_isNewRecordMode)
                {
                    _currentIndex = _booksData.Rows.Count - 1;
                    _isNewRecordMode = false;
                }

                DisplayCurrentRecord();
            }
            catch (Exception ex)
            {
                string operation = _isNewRecordMode ? "добавлении" : "обновлении";
                _notificationService.Notify($"Ошибка при {operation} книги: {ex.Message}");
            }
        }
    }
}