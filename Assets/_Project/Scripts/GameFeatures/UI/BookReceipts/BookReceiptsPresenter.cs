using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using _Project.Core.Services;
using _Project.GameFeatures.Database;
using Mono.Data.Sqlite;
using Zenject;

namespace _Project.GameFeatures.UI.BookReceipts
{
    public class BookReceiptsPresenter : IInitializable, IDisposable
    {
        private readonly BookReceiptsPopup _bookReceiptsPopup;
        private readonly DatabaseController _databaseController;
        private readonly NotificationService _notificationService;

        private DataTable _bookReceiptsData;
        private int _currentIndex = -1;
        private bool _isNewRecordMode = false;

        public BookReceiptsPresenter(BookReceiptsPopup bookReceiptsPopup, DatabaseController databaseController,
            NotificationService notificationService)
        {
            _bookReceiptsPopup = bookReceiptsPopup;
            _databaseController = databaseController;
            _notificationService = notificationService;
        }

        public void Initialize()
        {
            _bookReceiptsPopup.OnPreviousClick += OnPreviousClick;
            _bookReceiptsPopup.OnNextClick += OnNextClick;
            _bookReceiptsPopup.OnDeleteClick += OnDeleteClick;
            _bookReceiptsPopup.OnSaveClick += OnSaveClick;

            LoadBooksIntoDropdown();
            LoadLibrariansIntoDropdown();
            LoadBookReceiptsData();
            ShowFirstRecord();
        }

        public void Dispose()
        {
            _bookReceiptsPopup.OnPreviousClick -= OnPreviousClick;
            _bookReceiptsPopup.OnNextClick -= OnNextClick;
            _bookReceiptsPopup.OnDeleteClick -= OnDeleteClick;
            _bookReceiptsPopup.OnSaveClick -= OnSaveClick;
        }

        private void LoadBookReceiptsData()
        {
            try
            {
                string query = @"SELECT br.*, b.название as название_книги, 
                               l.фамилия || ' ' || l.имя || COALESCE(' ' || l.отчество, '') as фио_библиотекаря 
                               FROM Поступления_книг br
                               LEFT JOIN Книги b ON br.id_книги = b.id_книги
                               LEFT JOIN Библиотекари l ON br.id_библиотекаря = l.id_библиотекаря
                               ORDER BY br.id_поступления";
                _bookReceiptsData = new DataTable();

                using (var reader = _databaseController.ReadData(query))
                {
                    _bookReceiptsData.Load(reader);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке данных: {ex.Message}");
                _bookReceiptsData = new DataTable();
            }
        }

        private void ShowFirstRecord()
        {
            if (_bookReceiptsData.Rows.Count > 0)
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
            if (_currentIndex >= 0 && _currentIndex < _bookReceiptsData.Rows.Count)
            {
                DataRow row = _bookReceiptsData.Rows[_currentIndex];

                _bookReceiptsPopup.InvoiceNumberInput.text = row["номер_накладной"].ToString();
                _bookReceiptsPopup.DateReceiptInput.text = row["дата_поступления"].ToString();
                _bookReceiptsPopup.SupplierInput.text = row["поставщик"].ToString();
                _bookReceiptsPopup.QuantityInput.text = row["количество"].ToString();
                _bookReceiptsPopup.PricePerUnitInput.text = row["цена_за_единицу"].ToString();

                string bookName = row["название_книги"].ToString();
                if (!string.IsNullOrEmpty(bookName))
                {
                    int bookIndex = _bookReceiptsPopup.BookInput.options.FindIndex(option => option.text == bookName);
                    if (bookIndex >= 0)
                    {
                        _bookReceiptsPopup.BookInput.value = bookIndex;
                    }
                    else
                    {
                        _notificationService.Notify($"Книга '{bookName}' не найдена в dropdown");
                        _bookReceiptsPopup.BookInput.value = 0;
                    }
                }
                else
                {
                    _bookReceiptsPopup.BookInput.value = 0;
                }

                string librarianName = row["фио_библиотекаря"].ToString();
                if (!string.IsNullOrEmpty(librarianName))
                {
                    int librarianIndex =
                        _bookReceiptsPopup.LibrarianInput.options.FindIndex(option => option.text == librarianName);
                    if (librarianIndex >= 0)
                    {
                        _bookReceiptsPopup.LibrarianInput.value = librarianIndex;
                    }
                    else
                    {
                        _notificationService.Notify($"Библиотекарь '{librarianName}' не найден в dropdown");
                        _bookReceiptsPopup.LibrarianInput.value = 0;
                    }
                }
                else
                {
                    _bookReceiptsPopup.LibrarianInput.value = 0;
                }
            }
        }

        private void EnterNewRecordMode()
        {
            _bookReceiptsPopup.ClearFields();
            _currentIndex = -1;
            _isNewRecordMode = true;
            _notificationService.Notify("Режим добавления новой записи поступления");
        }

        private void OnPreviousClick()
        {
            if (_isNewRecordMode)
            {
                if (_bookReceiptsData.Rows.Count > 0)
                {
                    _currentIndex = _bookReceiptsData.Rows.Count - 1;
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
                InsertNewBookReceipt();
            }
            else if (_currentIndex < _bookReceiptsData.Rows.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentRecord();
            }
            else
            {
                EnterNewRecordMode();
            }
        }

        private void InsertNewBookReceipt()
        {
            string invoiceNumber = _bookReceiptsPopup.InvoiceNumberInput.text;
            string dateReceipt = _bookReceiptsPopup.DateReceiptInput.text;
            string supplier = _bookReceiptsPopup.SupplierInput.text;
            string quantity = _bookReceiptsPopup.QuantityInput.text;
            string pricePerUnit = _bookReceiptsPopup.PricePerUnitInput.text;
            string bookName = _bookReceiptsPopup.BookInput.options[_bookReceiptsPopup.BookInput.value].text;
            string librarianName =
                _bookReceiptsPopup.LibrarianInput.options[_bookReceiptsPopup.LibrarianInput.value].text;

            if (string.IsNullOrEmpty(invoiceNumber) || string.IsNullOrEmpty(dateReceipt) ||
                string.IsNullOrEmpty(supplier) || string.IsNullOrEmpty(quantity) ||
                string.IsNullOrEmpty(pricePerUnit))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            if (TextIsDate(dateReceipt) == false)
            {
                _notificationService.Notify("Некорректная дата");
                return;
            }

            if (!int.TryParse(quantity, out int qty) || qty <= 0)
            {
                _notificationService.Notify("Количество должно быть положительным целым числом");
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

                string librarianId = GetLibrarianIdByFullName(librarianName);
                if (string.IsNullOrEmpty(librarianId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID библиотекаря!");
                    return;
                }

                string query = @"INSERT INTO Поступления_книг 
                        (номер_накладной, дата_поступления, поставщик, количество, цена_за_единицу, id_книги, id_библиотекаря) 
                        VALUES (@invoiceNumber, @dateReceipt, @supplier, @quantity, @pricePerUnit, @bookId, @librarianId)";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@invoiceNumber", invoiceNumber),
                    new SqliteParameter("@dateReceipt", dateReceipt),
                    new SqliteParameter("@supplier", supplier),
                    new SqliteParameter("@quantity", qty),
                    new SqliteParameter("@pricePerUnit", pricePerUnit),
                    new SqliteParameter("@bookId", bookId),
                    new SqliteParameter("@librarianId", librarianId)
                };

                _databaseController.ExecuteQuery(query, parameters);

                _notificationService.Notify("Поступление книги успешно добавлено в базу данных!");

                UpdateBookCopiesCount(bookId, qty);

                LoadBookReceiptsData();
                _currentIndex = _bookReceiptsData.Rows.Count - 1;
                DisplayCurrentRecord();
                _isNewRecordMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при добавлении поступления: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавляет qty к количеству_экземпляров и к доступно_экземпляров для книги.
        /// qty должен быть > 0.
        /// </summary>
        private void UpdateBookCopiesCount(string bookId, int qty)
        {
            try
            {
                if (qty <= 0)
                    return;

                string updateQuery = @"UPDATE Книги 
                                         SET количество_экземпляров = количество_экземпляров + @qty,
                                             доступно_экземпляров = доступно_экземпляров + @qty
                                         WHERE id_книги = @bookId";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@qty", qty),
                    new SqliteParameter("@bookId", bookId)
                };

                _databaseController.ExecuteQuery(updateQuery, parameters);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при обновлении количества экземпляров: {ex.Message}");
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
            if (_isNewRecordMode || _currentIndex < 0 || _currentIndex >= _bookReceiptsData.Rows.Count)
                return;

            try
            {
                int id = Convert.ToInt32(_bookReceiptsData.Rows[_currentIndex]["id_поступления"]);
                string bookId = _bookReceiptsData.Rows[_currentIndex]["id_книги"].ToString();
                string quantity = _bookReceiptsData.Rows[_currentIndex]["количество"].ToString();

                string query = "DELETE FROM Поступления_книг WHERE id_поступления = @id";

                IDbDataParameter[] parameters =
                {
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(query, parameters);

                DecreaseBookCopiesCount(bookId, quantity);

                _notificationService.Notify("Запись поступления удалена!");

                LoadBookReceiptsData();

                if (_bookReceiptsData.Rows.Count > 0)
                {
                    _currentIndex = Math.Min(_currentIndex, _bookReceiptsData.Rows.Count - 1);
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

        private void DecreaseBookCopiesCount(string bookId, string quantity)
        {
            try
            {
                if (int.TryParse(quantity, out int qty) && qty > 0)
                {
                    string updateQuery = @"UPDATE Книги 
                                         SET количество_экземпляров = количество_экземпляров - @qty,
                                             доступно_экземпляров = доступно_экземпляров - @qty
                                         WHERE id_книги = @bookId AND количество_экземпляров >= @qty";

                    IDbDataParameter[] parameters =
                    {
                        new SqliteParameter("@qty", qty),
                        new SqliteParameter("@bookId", bookId)
                    };

                    _databaseController.ExecuteQuery(updateQuery, parameters);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при уменьшении количества экземпляров: {ex.Message}");
            }
        }

        private void LoadBooksIntoDropdown()
        {
            try
            {
                _bookReceiptsPopup.BookInput.ClearOptions();

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

                _bookReceiptsPopup.BookInput.AddOptions(bookNames);
            }
            catch (Exception ex)
            {
                _notificationService.Notify($"Ошибка при загрузке книг: {ex.Message}");
            }
        }

        private void LoadLibrariansIntoDropdown()
        {
            try
            {
                _bookReceiptsPopup.LibrarianInput.ClearOptions();

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

                _bookReceiptsPopup.LibrarianInput.AddOptions(librarianNames);
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
            SaveBookReceipt();
        }

        private void SaveBookReceipt()
        {
            string invoiceNumber = _bookReceiptsPopup.InvoiceNumberInput.text;
            string dateReceipt = _bookReceiptsPopup.DateReceiptInput.text;
            string supplier = _bookReceiptsPopup.SupplierInput.text;
            string quantity = _bookReceiptsPopup.QuantityInput.text;
            string pricePerUnit = _bookReceiptsPopup.PricePerUnitInput.text;
            string bookName = _bookReceiptsPopup.BookInput.options[_bookReceiptsPopup.BookInput.value].text;
            string librarianName =
                _bookReceiptsPopup.LibrarianInput.options[_bookReceiptsPopup.LibrarianInput.value].text;

            if (string.IsNullOrEmpty(invoiceNumber) || string.IsNullOrEmpty(dateReceipt) ||
                string.IsNullOrEmpty(supplier) || string.IsNullOrEmpty(quantity) ||
                string.IsNullOrEmpty(pricePerUnit))
            {
                _notificationService.Notify("Не все обязательные поля заполнены!");
                return;
            }

            if (TextIsDate(dateReceipt) == false)
            {
                _notificationService.Notify("Некорректная дата");
                return;
            }

            if (!int.TryParse(quantity, out int qty) || qty <= 0)
            {
                _notificationService.Notify("Количество должно быть положительным целым числом");
                return;
            }

            if (!decimal.TryParse(pricePerUnit, out decimal price) || price <= 0)
            {
                _notificationService.Notify("Цена за единицу должна быть положительным числом");
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

                string librarianId = GetLibrarianIdByFullName(librarianName);
                if (string.IsNullOrEmpty(librarianId))
                {
                    _notificationService.Notify("Ошибка: не удалось найти ID библиотекаря!");
                    return;
                }

                int id = Convert.ToInt32(_bookReceiptsData.Rows[_currentIndex]["id_поступления"]);
                string oldBookId = _bookReceiptsData.Rows[_currentIndex]["id_книги"].ToString();
                int oldQuantity = Convert.ToInt32(_bookReceiptsData.Rows[_currentIndex]["количество"]);

                string updateQuery = @"UPDATE Поступления_книг 
                            SET номер_накладной = @invoiceNumber,
                                дата_поступления = @dateReceipt,
                                поставщик = @supplier,
                                количество = @quantity,
                                цена_за_единицу = @pricePerUnit,
                                id_книги = @bookId,
                                id_библиотекаря = @librarianId
                            WHERE id_поступления = @id";

                IDbDataParameter[] updateParameters =
                {
                    new SqliteParameter("@invoiceNumber", invoiceNumber),
                    new SqliteParameter("@dateReceipt", dateReceipt),
                    new SqliteParameter("@supplier", supplier),
                    new SqliteParameter("@quantity", qty),
                    new SqliteParameter("@pricePerUnit", price),
                    new SqliteParameter("@bookId", bookId),
                    new SqliteParameter("@librarianId", librarianId),
                    new SqliteParameter("@id", id)
                };

                _databaseController.ExecuteQuery(updateQuery, updateParameters);
                _notificationService.Notify("Запись поступления успешно обновлена!");

                if (oldBookId != bookId || oldQuantity != qty)
                {
                    if (oldBookId != bookId)
                    {
                        DecreaseBookCopiesCount(oldBookId, oldQuantity.ToString());
                    }
                    else
                    {
                        int quantityDiff = qty - oldQuantity;
                        if (quantityDiff > 0)
                        {
                            UpdateBookCopiesCount(bookId, quantityDiff);
                        }
                        else if (quantityDiff < 0)
                        {
                            DecreaseBookCopiesCount(bookId,
                                Math.Abs(quantityDiff).ToString());
                        }
                    }

                    if (oldBookId != bookId)
                    {
                        UpdateBookCopiesCount(bookId, qty);
                    }
                }

                LoadBookReceiptsData();

                if (_isNewRecordMode)
                {
                    _currentIndex = _bookReceiptsData.Rows.Count - 1;
                    _isNewRecordMode = false;
                }

                DisplayCurrentRecord();
            }
            catch (Exception ex)
            {
                string operation = _isNewRecordMode ? "добавлении" : "обновлении";
                _notificationService.Notify($"Ошибка при {operation} поступления: {ex.Message}");
            }
        }
    }
}