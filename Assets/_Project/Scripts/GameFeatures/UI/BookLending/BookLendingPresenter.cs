using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using _Project.Core.Services;
using _Project.GameFeatures.Database;
using Mono.Data.Sqlite;
using Zenject;

namespace _Project.GameFeatures.UI.BookLending
{
    public class BookLendingPresenter : IInitializable, IDisposable
    {
        private readonly BookLendingPopup _bookLendingPopup;
        private readonly DatabaseController _databaseController;
        private readonly NotificationService _notificationService;

        private DataTable _bookLendingData;
        private int _currentIndex = -1;
        private bool _isNewRecordMode = false;

        public BookLendingPresenter(BookLendingPopup bookLendingPopup, DatabaseController databaseController,
            NotificationService notificationService)
        {
            _bookLendingPopup = bookLendingPopup;
            _databaseController = databaseController;
            _notificationService = notificationService;
        }

        public void Initialize()
        {
            _bookLendingPopup.OnPreviousClick += OnPreviousClick;
            _bookLendingPopup.OnNextClick += OnNextClick;
            _bookLendingPopup.OnDeleteClick += OnDeleteClick;
            _bookLendingPopup.OnSaveClick += OnSaveClick;

            LoadBooksIntoDropdown();
            LoadReadersIntoDropdown();
            LoadLibrariansIntoDropdown();
            LoadBookLendingsData();
            ShowFirstRecord();
        }

        public void Dispose()
        {
            _bookLendingPopup.OnPreviousClick -= OnPreviousClick;
            _bookLendingPopup.OnNextClick -= OnNextClick;
            _bookLendingPopup.OnDeleteClick -= OnDeleteClick;
            _bookLendingPopup.OnSaveClick -= OnSaveClick;
        }

        private void LoadBookLendingsData()
        {
            try
            {
                string query = @"SELECT l.*, 
                                        b.название as название_книги, 
                                        r.фамилия || ' ' || r.имя || COALESCE(' ' || r.отчество, '') as фио_читателя,
                                        lib.фамилия || ' ' || lib.имя || COALESCE(' ' || lib.отчество, '') as фио_библиотекаря
                                 FROM Выдачи_книг l
                                 LEFT JOIN Книги b ON l.id_книги = b.id_книги
                                 LEFT JOIN Читатели r ON l.id_читателя = r.id_читателя
                                 LEFT JOIN Библиотекари lib ON l.id_библиотекаря = lib.id_библиотекаря
                                 ORDER BY l.id_выдачи";
                _bookLendingData = new DataTable();

                using (var reader = _databaseController.ReadData(query))
                {
                    _bookLendingData.Load(reader);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке данных выдач: {ex.Message}");
                _bookLendingData = new DataTable();
            }
        }

        private void ShowFirstRecord()
        {
            if (_bookLendingData.Rows.Count > 0)
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
            if (_currentIndex >= 0 && _currentIndex < _bookLendingData.Rows.Count)
            {
                DataRow row = _bookLendingData.Rows[_currentIndex];

                _bookLendingPopup.DataIssueInput.text = row["дата_выдачи"].ToString();
                _bookLendingPopup.ReturnPeriodInput.text = row.Table.Columns.Contains("срок_возврата")
                    ? row["срок_возврата"].ToString()
                    : "";
                _bookLendingPopup.ReturnDateInput.text = row.Table.Columns.Contains("дата_возврата")
                    ? row["дата_возврата"].ToString()
                    : "";
                _bookLendingPopup.StatusInput.text =
                    row.Table.Columns.Contains("статус") ? row["статус"].ToString() : "";

                string bookName = row["название_книги"].ToString();
                if (!string.IsNullOrEmpty(bookName))
                {
                    int bookIndex = _bookLendingPopup.BookInput.options.FindIndex(option => option.text == bookName);
                    if (bookIndex >= 0)
                    {
                        _bookLendingPopup.BookInput.value = bookIndex;
                    }
                    else
                    {
                        _notificationService.Notify($"Книга '{bookName}' не найдена в dropdown");
                        _bookLendingPopup.BookInput.value = 0;
                    }
                }
                else
                {
                    _bookLendingPopup.BookInput.value = 0;
                }

                string readerName = row["фио_читателя"].ToString();
                if (!string.IsNullOrEmpty(readerName))
                {
                    int readerIndex =
                        _bookLendingPopup.ReaderInput.options.FindIndex(option => option.text == readerName);
                    if (readerIndex >= 0)
                    {
                        _bookLendingPopup.ReaderInput.value = readerIndex;
                    }
                    else
                    {
                        _notificationService.Notify($"Читатель '{readerName}' не найден в dropdown");
                        _bookLendingPopup.ReaderInput.value = 0;
                    }
                }
                else
                {
                    _bookLendingPopup.ReaderInput.value = 0;
                }

                string librarianName = row["фио_библиотекаря"].ToString();
                if (!string.IsNullOrEmpty(librarianName))
                {
                    int librarianIndex =
                        _bookLendingPopup.LibrarianInput.options.FindIndex(option => option.text == librarianName);
                    if (librarianIndex >= 0)
                    {
                        _bookLendingPopup.LibrarianInput.value = librarianIndex;
                    }
                    else
                    {
                        _notificationService.Notify($"Библиотекарь '{librarianName}' не найден в dropdown");
                        _bookLendingPopup.LibrarianInput.value = 0;
                    }
                }
                else
                {
                    _bookLendingPopup.LibrarianInput.value = 0;
                }
            }
        }

        private void EnterNewRecordMode()
        {
            _bookLendingPopup.ClearFields();
            _currentIndex = -1;
            _isNewRecordMode = true;
            _notificationService.Notify("Режим добавления новой выдачи");
        }

        private void OnPreviousClick()
        {
            if (_isNewRecordMode)
            {
                if (_bookLendingData.Rows.Count > 0)
                {
                    _currentIndex = _bookLendingData.Rows.Count - 1;
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
                InsertNewLending();
            }
            else if (_currentIndex < _bookLendingData.Rows.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentRecord();
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void InsertNewLending()
        {
            string dateIssue = _bookLendingPopup.DataIssueInput.text;
            string returnPeriod = _bookLendingPopup.ReturnPeriodInput.text;
            string returnDate = _bookLendingPopup.ReturnDateInput.text;
            string status = _bookLendingPopup.StatusInput.text;
            string bookName = _bookLendingPopup.BookInput.options[_bookLendingPopup.BookInput.value].text;
            string readerName = _bookLendingPopup.ReaderInput.options[_bookLendingPopup.ReaderInput.value].text;
            string librarianName =
                _bookLendingPopup.LibrarianInput.options[_bookLendingPopup.LibrarianInput.value].text;

            if (string.IsNullOrEmpty(dateIssue) || string.IsNullOrEmpty(bookName) || string.IsNullOrEmpty(readerName) ||
                string.IsNullOrEmpty(librarianName))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            if (!TextIsDate(dateIssue))
            {
                _notificationService.Notify("Некорректная дата выдачи (ожидается формат yyyy-MM-dd)");
                return;
            }

            if (!string.IsNullOrEmpty(returnDate) && !TextIsDate(returnDate))
            {
                _notificationService.Notify("Некорректная дата возврата (ожидается формат yyyy-MM-dd)");
                return;
            }

            try
            {
                string bookId = GetBookIdByName(bookName);
                if (string.IsNullOrEmpty(bookId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID книги!");
                    return;
                }

                string readerId = GetReaderIdByFullName(readerName);
                if (string.IsNullOrEmpty(readerId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID читателя!");
                    return;
                }

                string librarianId = GetLibrarianIdByFullName(librarianName);
                if (string.IsNullOrEmpty(librarianId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID библиотекаря!");
                    return;
                }

                string query = @"INSERT INTO Выдачи_книг 
                                 (дата_выдачи, срок_возврата, дата_возврата, статус, id_книги, id_читателя, id_библиотекаря)
                                 VALUES (@dateIssue, @returnPeriod, @returnDate, @status, @bookId, @readerId, @librarianId)";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@dateIssue", dateIssue),
                    new SqliteParameter("@returnPeriod",
                        string.IsNullOrEmpty(returnPeriod) ? (object)DBNull.Value : returnPeriod),
                    new SqliteParameter("@returnDate",
                        string.IsNullOrEmpty(returnDate) ? (object)DBNull.Value : returnDate),
                    new SqliteParameter("@status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status),
                    new SqliteParameter("@bookId", bookId),
                    new SqliteParameter("@readerId", readerId),
                    new SqliteParameter("@librarianId", librarianId)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Выдача книги успешно добавлена в базу данных!");

                DecreaseBookAvailableCount(bookId);

                LoadBookLendingsData();
                _currentIndex = _bookLendingData.Rows.Count - 1;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при добавлении выдачи: {ex.Message}");
            }
        }

        private void DecreaseBookAvailableCount(string bookId)
        {
            try
            {
                string updateQuery = @"UPDATE Книги 
                                       SET доступно_экземпляров = доступно_экземпляров - 1
                                       WHERE id_книги = @bookId AND доступно_экземпляров > 0";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@bookId", bookId)
                };

                _databaseController.ExecuteQuery(updateQuery, parameters);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при обновлении доступных экземпляров: {ex.Message}");
            }
        }

        private void IncreaseBookAvailableCount(string bookId)
        {
            try
            {
                string updateQuery = @"UPDATE Книги 
                                       SET доступно_экземпляров = доступно_экземпляров + 1
                                       WHERE id_книги = @bookId";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@bookId", bookId)
                };

                _databaseController.ExecuteQuery(updateQuery, parameters);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при восстановлении доступных экземпляров: {ex.Message}");
            }
        }

        private string GetBookIdByName(string bookName)
        {
            if (string.IsNullOrEmpty(bookName))
                return string.Empty;

            try
            {
                string safeBookName = bookName.Replace("'", "''");
                string query = $"SELECT id_книги FROM Книги WHERE название = '{safeBookName}'";

                using (var reader = _databaseController.ReadData(query))
                {
                    if (reader.Read())
                    {
                        return reader["id_книги"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при получении ID книги: {ex.Message}");
            }

            return string.Empty;
        }

        private string GetReaderIdByFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            try
            {
                string[] nameParts = fullName.Split(' ');
                string lastName = nameParts.Length > 0 ? nameParts[0] : "";
                string firstName = nameParts.Length > 1 ? nameParts[1] : "";
                string middleName = nameParts.Length > 2 ? nameParts[2] : "";

                string safeLastName = lastName.Replace("'", "''");
                string safeFirstName = firstName.Replace("'", "''");

                string query =
                    $"SELECT id_читателя FROM Читатели WHERE фамилия = '{safeLastName}' AND имя = '{safeFirstName}'";

                if (!string.IsNullOrEmpty(middleName))
                {
                    string safeMiddleName = middleName.Replace("'", "''");
                    query += $" AND отчество = '{safeMiddleName}'";
                }

                using (var reader = _databaseController.ReadData(query))
                {
                    if (reader.Read())
                    {
                        return reader["id_читателя"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при получении ID читателя: {ex.Message}");
            }

            return string.Empty;
        }

        private string GetLibrarianIdByFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            try
            {
                string[] nameParts = fullName.Split(' ');
                string lastName = nameParts.Length > 0 ? nameParts[0] : "";
                string firstName = nameParts.Length > 1 ? nameParts[1] : "";
                string middleName = nameParts.Length > 2 ? nameParts[2] : "";

                string safeLastName = lastName.Replace("'", "''");
                string safeFirstName = firstName.Replace("'", "''");

                string query =
                    $"SELECT id_библиотекаря FROM Библиотекари WHERE фамилия = '{safeLastName}' AND имя = '{safeFirstName}'";

                if (!string.IsNullOrEmpty(middleName))
                {
                    string safeMiddleName = middleName.Replace("'", "''");
                    query += $" AND отчество = '{safeMiddleName}'";
                }

                using (var reader = _databaseController.ReadData(query))
                {
                    if (reader.Read())
                    {
                        return reader["id_библиотекаря"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при получении ID библиотекаря: {ex.Message}");
            }

            return string.Empty;
        }

        private void OnDeleteClick()
        {
            if (_isNewRecordMode || _currentIndex < 0 || _currentIndex >= _bookLendingData.Rows.Count)
                return;

            try
            {
                int id = Convert.ToInt32(_bookLendingData.Rows[_currentIndex]["id_выдачи"]);
                string bookId = _bookLendingData.Rows[_currentIndex]["id_книги"].ToString();

                string query = "DELETE FROM Выдачи_книг WHERE id_выдачи = @id";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(query, parameters);

                IncreaseBookAvailableCount(bookId);

                _notificationService.Notify("Запись выдачи удалена!");

                LoadBookLendingsData();

                if (_bookLendingData.Rows.Count > 0)
                {
                    _currentIndex = Math.Min(_currentIndex, _bookLendingData.Rows.Count - 1);
                    DisplayCurrentRecord();
                }
                else
                {
                    EnterNewRecordMode();
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при удалении записи выдачи: {ex.Message}");
            }
        }

        private void LoadBooksIntoDropdown()
        {
            try
            {
                _bookLendingPopup.BookInput.ClearOptions();

                string query = "SELECT название FROM Книги ORDER BY название";
                List<string> bookNames = new List<string>();

                using (var reader = _databaseController.ReadData(query))
                {
                    while (reader.Read())
                    {
                        string bookName = reader["название"].ToString();
                        bookNames.Add(bookName);
                    }
                }

                _bookLendingPopup.BookInput.AddOptions(bookNames);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке книг: {ex.Message}");
            }
        }

        private void LoadReadersIntoDropdown()
        {
            try
            {
                _bookLendingPopup.ReaderInput.ClearOptions();

                string query = "SELECT фамилия, имя, отчество FROM Читатели ORDER BY фамилия, имя";
                List<string> readerNames = new List<string>();

                using (var reader = _databaseController.ReadData(query))
                {
                    while (reader.Read())
                    {
                        string lastName = reader["фамилия"].ToString();
                        string firstName = reader["имя"].ToString();
                        string middleName = reader["отчество"].ToString();

                        string fullName = $"{lastName} {firstName}";
                        if (!string.IsNullOrEmpty(middleName))
                        {
                            fullName += $" {middleName}";
                        }

                        readerNames.Add(fullName);
                    }
                }

                _bookLendingPopup.ReaderInput.AddOptions(readerNames);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке читателей: {ex.Message}");
            }
        }

        private void LoadLibrariansIntoDropdown()
        {
            try
            {
                _bookLendingPopup.LibrarianInput.ClearOptions();

                string query = "SELECT фамилия, имя, отчество FROM Библиотекари ORDER BY фамилия, имя";
                List<string> librarianNames = new List<string>();

                using (var reader = _databaseController.ReadData(query))
                {
                    while (reader.Read())
                    {
                        string lastName = reader["фамилия"].ToString();
                        string firstName = reader["имя"].ToString();
                        string middleName = reader["отчество"].ToString();

                        string fullName = $"{lastName} {firstName}";
                        if (!string.IsNullOrEmpty(middleName))
                        {
                            fullName += $" {middleName}";
                        }

                        librarianNames.Add(fullName);
                    }
                }

                _bookLendingPopup.LibrarianInput.AddOptions(librarianNames);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке библиотекарей: {ex.Message}");
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
            SaveLending();
        }

        private void SaveLending()
        {
            string dateIssue = _bookLendingPopup.DataIssueInput.text;
            string returnPeriod = _bookLendingPopup.ReturnPeriodInput.text;
            string returnDate = _bookLendingPopup.ReturnDateInput.text;
            string status = _bookLendingPopup.StatusInput.text;
            string bookName = _bookLendingPopup.BookInput.options[_bookLendingPopup.BookInput.value].text;
            string readerName = _bookLendingPopup.ReaderInput.options[_bookLendingPopup.ReaderInput.value].text;
            string librarianName =
                _bookLendingPopup.LibrarianInput.options[_bookLendingPopup.LibrarianInput.value].text;

            if (string.IsNullOrEmpty(dateIssue) || string.IsNullOrEmpty(bookName) || string.IsNullOrEmpty(readerName) ||
                string.IsNullOrEmpty(librarianName))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            if (!TextIsDate(dateIssue))
            {
                _notificationService.Notify("Некорректная дата выдачи (ожидается формат yyyy-MM-dd)");
                return;
            }

            if (!string.IsNullOrEmpty(returnDate) && !TextIsDate(returnDate))
            {
                _notificationService.Notify("Некорректная дата возврата (ожидается формат yyyy-MM-dd)");
                return;
            }

            try
            {
                string bookId = GetBookIdByName(bookName);
                if (string.IsNullOrEmpty(bookId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID книги!");
                    return;
                }

                string readerId = GetReaderIdByFullName(readerName);
                if (string.IsNullOrEmpty(readerId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID читателя!");
                    return;
                }

                string librarianId = GetLibrarianIdByFullName(librarianName);
                if (string.IsNullOrEmpty(librarianId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID библиотекаря!");
                    return;
                }

                int id = Convert.ToInt32(_bookLendingData.Rows[_currentIndex]["id_выдачи"]);
                string oldBookId = _bookLendingData.Rows[_currentIndex]["id_книги"].ToString();
                string oldReturnDate = _bookLendingData.Rows[_currentIndex]["дата_возврата"].ToString();

                string updateQuery = @"UPDATE Выдачи_книг 
                                    SET дата_выдачи = @dateIssue,
                                        срок_возврата = @returnPeriod,
                                        дата_возврата = @returnDate,
                                        статус = @status,
                                        id_книги = @bookId,
                                        id_читателя = @readerId,
                                        id_библиотекаря = @librarianId
                                    WHERE id_выдачи = @id";

                IDbDataParameter[] updateParameters =
                {
                    new SqliteParameter("@dateIssue", dateIssue),
                    new SqliteParameter("@returnPeriod",
                        string.IsNullOrEmpty(returnPeriod) ? (object)DBNull.Value : returnPeriod),
                    new SqliteParameter("@returnDate",
                        string.IsNullOrEmpty(returnDate) ? (object)DBNull.Value : returnDate),
                    new SqliteParameter("@status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status),
                    new SqliteParameter("@bookId", bookId),
                    new SqliteParameter("@readerId", readerId),
                    new SqliteParameter("@librarianId", librarianId),
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(updateQuery, updateParameters);
                _notificationService.Notify("Запись выдачи успешно обновлена!");

                if (oldBookId != bookId)
                {
                    IncreaseBookAvailableCount(oldBookId);
                    DecreaseBookAvailableCount(bookId);
                }

                bool wasReturned = !string.IsNullOrEmpty(oldReturnDate);
                bool isReturned = !string.IsNullOrEmpty(returnDate);

                if (!wasReturned && isReturned)
                {
                    IncreaseBookAvailableCount(bookId);
                }
                else if (wasReturned && !isReturned)
                {
                    DecreaseBookAvailableCount(bookId);
                }

                LoadBookLendingsData();

                if (_isNewRecordMode)
                {
                    _currentIndex = _bookLendingData.Rows.Count - 1;
                    _isNewRecordMode = false;
                }

                DisplayCurrentRecord();
            }
            catch (Exception ex)
            {
                string operation = _isNewRecordMode ? "добавлении" : "обновлении";
                _notificationService.Notify($"Ошибка при {operation} выдачи: {ex.Message}");
            }
        }
    }
}