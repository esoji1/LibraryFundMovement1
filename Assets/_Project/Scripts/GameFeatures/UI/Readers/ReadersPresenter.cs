using System;
using System.Data;
using System.Globalization;
using _Project.Core.Services;
using _Project.GameFeatures.Database;
using Mono.Data.Sqlite;
using UnityEngine;
using Zenject;

namespace _Project.GameFeatures.UI.Readers
{
    public class ReadersPresenter : IInitializable, IDisposable
    {
        private readonly ReadersPopup _readersPopup;
        private readonly DatabaseController _databaseController;
        private readonly NotificationService _notificationService;

        private DataTable _readersData;
        private int _currentIndex = -1;
        private bool _isNewRecordMode = false;

        public ReadersPresenter(ReadersPopup readersPopup, DatabaseController databaseController,
            NotificationService notificationService)
        {
            _readersPopup = readersPopup;
            _databaseController = databaseController;
            _notificationService = notificationService;
        }

        public void Initialize()
        {
            _readersPopup.OnPreviousClick += OnPreviousClick;
            _readersPopup.OnNextClick += OnNextClick;
            _readersPopup.OnDeleteClick += OnDeleteClick;
            _readersPopup.OnSaveClick += OnSaveClick;

            LoadReadersData();
            ShowFirstRecord();
        }

        public void Dispose()
        {
            _readersPopup.OnPreviousClick -= OnPreviousClick;
            _readersPopup.OnNextClick -= OnNextClick;
            _readersPopup.OnDeleteClick -= OnDeleteClick;
            _readersPopup.OnSaveClick -= OnSaveClick;
        }

        private void LoadReadersData()
        {
            try
            {
                string query = "SELECT * FROM Читатели ORDER BY id_читателя";
                _readersData = new DataTable();

                using (var reader = _databaseController.ReadData(query))
                {
                    _readersData.Load(reader);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке данных: {ex.Message}");
                _readersData = new DataTable();
            }
        }

        private void ShowFirstRecord()
        {
            if (_readersData.Rows.Count > 0)
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
            if (_currentIndex >= 0 && _currentIndex < _readersData.Rows.Count)
            {
                DataRow row = _readersData.Rows[_currentIndex];

                _readersPopup.LastNameInput.text = row["фамилия"].ToString();
                _readersPopup.FirstNameInput.text = row["имя"].ToString();
                _readersPopup.SurnameInput.text = row["отчество"].ToString();
                _readersPopup.PassportDetailsInput.text = row["паспортные_данные"].ToString();
                _readersPopup.TelephoneInput.text = row["телефон"].ToString();
                _readersPopup.EmailInput.text = row["email"].ToString();
                _readersPopup.DateRegistrationInput.text = row["дата_регистрации"].ToString();
            }
        }

        private void EnterNewRecordMode()
        {
            _readersPopup.ClearFields();
            _currentIndex = -1;
            _isNewRecordMode = true;
            _notificationService.Notify("Режим добавления новой записи");
        }

        private void OnPreviousClick()
        {
            if (_isNewRecordMode)
            {
                if (_readersData.Rows.Count > 0)
                {
                    _currentIndex = _readersData.Rows.Count - 1;
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
                InsertNewReader();
            }
            else if (_currentIndex < _readersData.Rows.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentRecord();
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void InsertNewReader()
        {
            string lastName = _readersPopup.LastNameInput.text;
            string firstName = _readersPopup.FirstNameInput.text;
            string surname = _readersPopup.SurnameInput.text;
            string passportDetails = _readersPopup.PassportDetailsInput.text;
            string telephone = _readersPopup.TelephoneInput.text;
            string email = _readersPopup.EmailInput.text;
            string dateRegistration = _readersPopup.DateRegistrationInput.text;

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(passportDetails) || string.IsNullOrEmpty(telephone) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(dateRegistration))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            if (TextIsDate(dateRegistration) == false)
            {
                _notificationService.Notify("Некоректная дата");
                return;
            }

            try
            {
                string query = @"INSERT INTO Читатели 
                                (фамилия, имя, отчество, паспортные_данные, телефон, email, дата_регистрации) 
                                VALUES (@lastName, @firstName, @surname, @passportDetails, @telephone, @email, @dateRegistration)";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@lastName", lastName),
                    new SqliteParameter("@firstName", firstName),
                    new SqliteParameter("@surname", string.IsNullOrEmpty(surname) ? DBNull.Value : (object)surname),
                    new SqliteParameter("@passportDetails", passportDetails),
                    new SqliteParameter("@telephone", telephone),
                    new SqliteParameter("@email", email),
                    new SqliteParameter("@dateRegistration", dateRegistration)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Читатель успешно добавлен в базу данных!");

                LoadReadersData();
                _currentIndex = _readersData.Rows.Count - 1;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при добавлении читателя: {ex.Message}");
            }
        }

        private void OnDeleteClick()
        {
            if (_isNewRecordMode || _currentIndex < 0 || _currentIndex >= _readersData.Rows.Count)
                return;

            try
            {
                int id = Convert.ToInt32(_readersData.Rows[_currentIndex]["id_читателя"]);
                string query = "DELETE FROM Читатели WHERE id_читателя = @id";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Запись удалена!");

                LoadReadersData();

                if (_readersData.Rows.Count > 0)
                {
                    _currentIndex = Math.Min(_currentIndex, _readersData.Rows.Count - 1);
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

        private bool TextIsDate(string text)
        {
            string dateFormat = "yyyy-MM-dd";

            if (DateTime.TryParseExact(text, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _))
                return true;

            return false;
        }

        private void OnSaveClick()
        {
            SaveReader();
        }

        private void SaveReader()
        {
            string lastName = _readersPopup.LastNameInput.text;
            string firstName = _readersPopup.FirstNameInput.text;
            string surname = _readersPopup.SurnameInput.text;
            string passportDetails = _readersPopup.PassportDetailsInput.text;
            string telephone = _readersPopup.TelephoneInput.text;
            string email = _readersPopup.EmailInput.text;
            string dateRegistration = _readersPopup.DateRegistrationInput.text;

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(passportDetails) || string.IsNullOrEmpty(telephone) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(dateRegistration))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            if (TextIsDate(dateRegistration) == false)
            {
                _notificationService.Notify("Некоректная дата");
                return;
            }

            try
            {
                int id = Convert.ToInt32(_readersData.Rows[_currentIndex]["id_читателя"]);
                string updateQuery = @"UPDATE Читатели 
                                        SET фамилия = @lastName, 
                                            имя = @firstName, 
                                            отчество = @surname, 
                                            паспортные_данные = @passportDetails, 
                                            телефон = @telephone, 
                                            email = @email,
                                            дата_регистрации = @dateRegistration 
                                        WHERE id_читателя = @id";

                IDbDataParameter[] updateParameters =
                {
                    new SqliteParameter("@lastName", lastName),
                    new SqliteParameter("@firstName", firstName),
                    new SqliteParameter("@surname", string.IsNullOrEmpty(surname) ? DBNull.Value : (object)surname),
                    new SqliteParameter("@passportDetails", passportDetails),
                    new SqliteParameter("@telephone", telephone),
                    new SqliteParameter("@email", email),
                    new SqliteParameter("@dateRegistration", dateRegistration),
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(updateQuery, updateParameters);
                _notificationService.Notify("Запись читателя успешно обновлена!");
                
                LoadReadersData();

                if (_isNewRecordMode)
                {
                    _currentIndex = _readersData.Rows.Count - 1;
                    _isNewRecordMode = false;
                }

                DisplayCurrentRecord();
            }
            catch (Exception ex)
            {
                string operation = _isNewRecordMode ? "добавлении" : "обновлении";
                _notificationService.Notify($"Ошибка при {operation} читателя: {ex.Message}");
            }
        }
    }
}