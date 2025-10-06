using System;
using _Project.GameFeatures.Database;
using Zenject;
using Mono.Data.Sqlite;
using System.Data;
using _Project.Core.Services;
using UnityEngine;

namespace _Project.GameFeatures.UI.Librarians
{
    public class LibrariansPresenter : IInitializable, IDisposable
    {
        private readonly LibrariansPopup _librariansPopup;
        private readonly DatabaseController _databaseController;
        private readonly NotificationService _notificationService;

        private DataTable _librariansData;
        private int _currentIndex = -1;
        private bool _isNewRecordMode = false;

        public LibrariansPresenter(LibrariansPopup librariansPopup, DatabaseController databaseController, NotificationService notificationService)
        {
            _librariansPopup = librariansPopup;
            _databaseController = databaseController;
            _notificationService = notificationService;
        }

        public void Initialize()
        {
            _librariansPopup.OnPreviousClick += OnPreviousClick;
            _librariansPopup.OnNextClick += OnNextClick;
            _librariansPopup.OnDeleteClick += OnDeleteClick;

            LoadLibrariansData();
            ShowFirstRecord();
        }

        public void Dispose()
        {
            _librariansPopup.OnPreviousClick -= OnPreviousClick;
            _librariansPopup.OnNextClick -= OnNextClick;
            _librariansPopup.OnDeleteClick -= OnDeleteClick;
        }

        private void LoadLibrariansData()
        {
            try
            {
                string query = "SELECT * FROM Библиотекари ORDER BY id_библиотекаря";
                _librariansData = new DataTable();
                
                using (var reader = _databaseController.ReadData(query))
                {
                    _librariansData.Load(reader);
                }

                _notificationService.Notify($"Загружено записей библиотекарей: {_librariansData.Rows.Count}");
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке данных: {ex.Message}");
                _librariansData = new DataTable();
            }
        }

        private void ShowFirstRecord()
        {
            if (_librariansData.Rows.Count > 0)
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
            if (_currentIndex >= 0 && _currentIndex < _librariansData.Rows.Count)
            {
                DataRow row = _librariansData.Rows[_currentIndex];
                
                _librariansPopup.LastNameInput.text = row["фамилия"].ToString();
                _librariansPopup.FirstNameInput.text = row["имя"].ToString();
                _librariansPopup.SurnameInput.text = row["отчество"].ToString();
                _librariansPopup.LoginInput.text = row["логин"].ToString();
                _librariansPopup.PasswordInput.text = row["пароль"].ToString();
                
                string accessLevel = row["уровень_доступа"].ToString();
                int accessIndex = _librariansPopup.AccessInput.options.FindIndex(option => option.text == accessLevel);
                if (accessIndex >= 0)
                {
                    _librariansPopup.AccessInput.value = accessIndex;
                }
            }
        }

        private void EnterNewRecordMode()
        {
            _librariansPopup.ClearFields();
            _currentIndex = -1;
            _isNewRecordMode = true;
            _notificationService.Notify("Режим добавления новой записи");
        }

        private void OnPreviousClick()
        {
            if (_isNewRecordMode)
            {
                if (_librariansData.Rows.Count > 0)
                {
                    _currentIndex = _librariansData.Rows.Count - 1;
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
                InsertNewLibrarian();
            }
            else if (_currentIndex < _librariansData.Rows.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentRecord();
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void InsertNewLibrarian()
        {
            string lastName = _librariansPopup.LastNameInput.text;
            string firstName = _librariansPopup.FirstNameInput.text;
            string surname = _librariansPopup.SurnameInput.text;
            string login = _librariansPopup.LoginInput.text;
            string password = _librariansPopup.PasswordInput.text;
            string accessLevel = _librariansPopup.AccessInput.options[_librariansPopup.AccessInput.value].text;

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) || 
                string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            try
            {
                string query = @"INSERT INTO Библиотекари 
                                (фамилия, имя, отчество, логин, пароль, уровень_доступа) 
                                VALUES (@lastName, @firstName, @surname, @login, @password, @accessLevel)";

                IDbDataParameter[] parameters = {
                    new SqliteParameter("@lastName", lastName),
                    new SqliteParameter("@firstName", firstName),
                    new SqliteParameter("@surname", string.IsNullOrEmpty(surname) ? DBNull.Value : (object)surname),
                    new SqliteParameter("@login", login),
                    new SqliteParameter("@password", password),
                    new SqliteParameter("@accessLevel", accessLevel)
                };

                _databaseController.ExecuteQuery(query, parameters);
                
                _notificationService.Notify("Библиотекарь успешно добавлен в базу данных!");
                
                LoadLibrariansData();
                _currentIndex = _librariansData.Rows.Count - 1;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при добавлении библиотекаря: {ex.Message}");
            }
        }

        private void OnDeleteClick()
        {
            if (_isNewRecordMode || _currentIndex < 0 || _currentIndex >= _librariansData.Rows.Count)
                return;

            try
            {
                int id = Convert.ToInt32(_librariansData.Rows[_currentIndex]["id_библиотекаря"]);
                string query = "DELETE FROM Библиотекари WHERE id_библиотекаря = @id";
                
                IDbDataParameter[] parameters = {
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(query, parameters);
                
                _notificationService.Notify("Запись удалена!");
                
                LoadLibrariansData();
                
                if (_librariansData.Rows.Count > 0)
                {
                    _currentIndex = Math.Min(_currentIndex, _librariansData.Rows.Count - 1);
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