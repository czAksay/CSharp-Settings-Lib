using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SettingsLib
{
    /// <summary>
    /// Класс для работы с файлом настроек какой-либо системы.
    /// </summary>
    public class Settings
    {
        #region Fields
        private bool isReady = false;
        private string fileName;
        private Dictionary<string, string> KeyValuePairs = new Dictionary<string, string>();
        #endregion

        #region Properties
        /// <summary>
        /// Автоматически сохранять значения при использовании методов <see cref="SetDefaults"/> и <see cref="SetValue"/>. 
        /// В противном случае, следует сохранять значения методом <see cref="Save"/>
        /// </summary>
        public bool Autosave { get; set; } = true;
        
        /// <summary>
        /// Задает, будет ли вызываться необрабатываемое исключение, если искомый ключ не будет найден в файле настроек. 
        /// В противном случае будет возвращать false.
        /// Метод GetNumericValue будет возвращать -1 при исключении.
        /// По умолчанию: false.
        /// </summary>
        public bool ThrowNotFoundException { get; set; } = false;

        /// <summary>
        /// Возвращает имя текущего файла настроек.
        /// </summary>
        public string FileName { get => fileName; }

        /// <summary>
        /// Информация о библиотеке
        /// </summary>
        public string About { get => "Settings Library v2.0.\nAuthor: Yuriy Aksaev."; }

        /// <summary>
        /// Задает, будет ли пара [ключ, значение] добавляться в файл настроек при использовании метода SetValue,
        /// если такого ключа нет в файле.
        /// По умолчанию: true.
        /// </summary>
        public bool AppendNewSettings { get; set; } = true;

        /// <summary>
        /// Задает разделитель между ключом и значением.
        /// По умолчанию: ":".
        /// </summary>
        public string KeyValueSeparator { get; set; } = ":";

        /// <summary>
        /// Задает разделитель между парами ключей и значений.
        /// По умолчанию: перенос строки ("\n").
        /// </summary>
        public string KeyValuePairSeparator { get; set; } = "\n";

        /// <summary>
        /// Отображает готовность к работе. 
        /// При появлении критических ошибок в библиотеке, вернет false и библиотека перестанет работать.
        /// В таком случае рекомендуется заново создать экземпляр класса.
        /// </summary>
        public bool IsReady { get => isReady; }
        #endregion

        #region Constructors
        /// <summary>
        /// Конструктор по умолчанию.
        /// Имя файла по умолчанию: Settings.ini.
        /// Файл создается при отсутствии.
        /// Разделитель ключа и значения по умолчанию: двоеточие (":").
        /// Разделитель пар по умолчанию: перенос строки ("\n").
        /// </summary>
        public Settings() : this("Settings.ini", true, ":", "\n") { }

        /// <summary>
        /// Конструктор с параметром.
        /// Файл создается при отсутствии.
        /// Разделитель ключа и значения по умолчанию: двоеточие (":").
        /// Разделитель пар по умолчанию: перенос строки ("\n").
        /// </summary>
        /// <param name="FileName">Имя файла настроек.</param>
        public Settings(string FileName) : this(FileName, true, ":", "\n") { }

        /// <summary>
        /// Конструктор с параметрами.
        /// Разделитель ключа и значения по умолчанию: двоеточие (":").
        /// Разделитель пар по умолчанию: перенос строки ("\n").
        /// </summary>
        /// <param name="FileName">Имя файла настроек.</param>
        /// <param name="CreateIfNotExists">Создавать ли файл настроек при его отсутствии.</param>
        public Settings(string FileName, bool CreateIfNotExists) : this(FileName, CreateIfNotExists, ":", "\n") { }

        /// <summary>
        /// Конструктор с параметрами.
        /// </summary>
        /// <param name="FileName">Имя файла настроек.</param>
        /// <param name="CreateIfNotExists">Создавать ли файл настроек при его отсутствии.</param>
        /// <param name="KeyValueSeparator">Разделитель ключа и значения.</param>
        /// <param name="KeyValuePairSeparator">Разделитель пар.</param>
        public Settings(string FileName, bool CreateIfNotExists, string KeyValueSeparator, string KeyValuePairSeparator)
        {
            this.fileName = FileName;
            this.KeyValueSeparator = KeyValueSeparator;
            this.KeyValuePairSeparator = KeyValuePairSeparator;

            if (!File.Exists(fileName))
            {
                if (CreateIfNotExists)
                {
                    using (File.Create(fileName)) { }
                }
                else
                {
                    throw new ConstructException("Файл настроек не обнаружен и не был создан.");
                }
            }

            try
            {
                StreamReader sr = new StreamReader(fileName);
                SetKeyValuePairsFromText(sr.ReadToEnd());
                sr.Close();
            }
            catch (Exception ex)
            {
                throw new ConstructException("Невозможно считать данные с файла настроек.\n" + ex.Message);
            }
            isReady = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Записывает в файл ключи настроек и их значения. 
        /// Если такой ключ (с любым значением) уже находится в файле, он не будет изменен.
        /// Пример: SetDefaults("Name:Bob\nAge:18");
        /// </summary>
        /// <param name="DefaultValues">Строка с парами [ключ[разделитель]значение].
        /// Разделителем между ключом и значением выступает свойство KeyValueSeparator.
        /// Разделителем между парами выступает свойство KeyValuePairSeparator.</param>
        public void SetDefaults(string DefaultValues)
        {
            SetDefaults(GetKeyValuePairsFromString(DefaultValues));
        }

        /// <summary>
        /// Записывает в файл ключи настроек и их значения. 
        /// Если такой ключ (с любым значением) уже находится в файле, он не будет изменен.
        /// Пример: SetDefaults(new string[] {"Name:Bob", "Age:18"});
        /// </summary>
        /// <param name="DefaultPairs">Массив пар в виде [ключ[разделитель]значение].
        /// Разделителем между ключом и значением выступает свойство KeyValueSeparator.</param>
        public void SetDefaults(string[] DefaultPairs)
        {
            foreach(string Pair in DefaultPairs)
            {
                SetDefaults(GetKeyValuePairsFromString(Pair));
            }
        }

        /// <summary>
        /// Записывает в файл ключи настроек и их значения.
        /// Если такой ключ (с любым значением) уже находится в файле, он не будет изменен.
        /// </summary>
        /// <param name="DefaultPairs">Массив пар ключей и значений.</param>
        public void SetDefaults(KeyValuePair<string, string>[] DefaultPairs)
        {
            if (!isReady)
                throw new NotReadyToWorkException();

            foreach(var Pair in DefaultPairs)
            {
                string value;
                if (!KeyValuePairs.TryGetValue(Pair.Key, out value))
                {
                    KeyValuePairs.Add(Pair.Key, Pair.Value);
                }
            }

            if (Autosave)
                Save();
        }

        /// <summary>
        /// Получить значение по имени ключа.
        /// </summary>
        /// <param name="Key">Имя искомого ключа.</param>
        /// <returns></returns>
        public object GetValueObject(string Key)
        {
            object Value = (object)GetValue(Key);
            return Value;
        }

        /// <summary>
        /// Получить значение по имени ключа в виде строки.
        /// </summary>
        /// <param name="Key">Имя искомого ключа.</param>
        /// <returns></returns>
        public string GetValue(string Key)
        {
            if (!isReady)
                throw new NotReadyToWorkException();

            string Value;
            if (!KeyValuePairs.TryGetValue(Key, out Value))
            {
                if (ThrowNotFoundException)
                {
                    throw new NotFoundException($"Ключ с именем {Key} не найден.");
                }
                else
                    return null;
            }
            return Value;
        }

        /// <summary>
        /// Получить значение по имени ключа в виде целочисленного значения.
        /// В случае неудачи преобразования строки в int, будет вызвано исключение.
        /// </summary>
        /// <param name="Key">Имя искомого ключа.</param>
        /// <returns></returns>
        public int GetNumericValue(string Key)
        {
            return GetNumericValue(Key, -1);
        }

        /// <summary>
        /// Получить значение по имени ключа в виде целочисленного значения.
        /// В случае неудачи преобразования строки в int, будет вызвано исключение.
        /// </summary>
        /// <param name="Key">Имя искомого ключа.</param>
        /// <param name="IfNotFoundValue">Значение, возвращаемое при отсутствии ключа. Значение true свойства ThrowNotFoundException игнорирует этот параметр.</param>
        /// <returns></returns>
        public int GetNumericValue(string Key, int IfNotFoundValue)
        {
            int Value;
            string strValue = GetValue(Key);
            if (strValue == null)
            {
                return IfNotFoundValue;
            }
            if (!int.TryParse(strValue, out Value))
            {
                throw new NotFoundException($"Не удается преобразовать строковое значение ключа {Key} к целочисленному.");
            }
            return Value;
        }

        /// <summary>
        /// Установить значение ключа.
        /// </summary>
        /// <param name="Key">Имя ключа.</param>
        /// <param name="Value">Значение ключа.</param>
        /// <returns>Возвращает, успешно ли установлено значение.</returns>
        public bool SetValue(string Key, object Value)
        {
            if (!isReady)
                throw new NotReadyToWorkException();

            try
            {
                KeyValuePairs[Key] = Value.ToString();
            }
            catch
            {
                if (ThrowNotFoundException)
                    throw new NotFoundException($"Ключ с именем {Key} не найден.");
                return false;
            }

            if (Autosave)
                Save();
            return true;
        }

        /// <summary>
        /// Сохранить изменения в файле настроек. 
        /// Срабатывает автоматически при значении true свойства Autosave.
        /// </summary>
        public void Save()
        {
            if (!isReady)
                throw new NotReadyToWorkException();

            StreamWriter sw = new StreamWriter(fileName, false);
            bool dontNeedSeparator = true;
            foreach(var Pair in KeyValuePairs)
            {
                if (!string.IsNullOrEmpty(Pair.Key) && !string.IsNullOrEmpty(Pair.Value))
                {
                    if (!dontNeedSeparator)
                    {
                        sw.Write(KeyValuePairSeparator);
                    }
                    sw.Write(Pair.Key + KeyValueSeparator + Pair.Value);
                    dontNeedSeparator = false;
                }
            }
            sw.Close();
        }
        #endregion

        #region PrivateMethods
        /// <summary>
        /// Заполняем словарь KeyValuePairs парами из заланного текста.
        /// </summary>
        /// <param name="Text"></param>
        private void SetKeyValuePairsFromText(string Text)
        {
            KeyValuePairs = new Dictionary<string, string>();
            var Pairs = GetKeyValuePairsFromString(Text);
            foreach(var Pair in Pairs)
            {
                KeyValuePairs.Add(Pair.Key, Pair.Value);
            }
        }

        private KeyValuePair<string, string>[] GetKeyValuePairsFromString(string String)
        {
            List<KeyValuePair<string, string>> KVPairs = new List<KeyValuePair<string, string>>();
            string[] pairs = String.Split(new string[] { KeyValuePairSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                string[] keyvalue = pair.Split(new string[] { KeyValueSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (keyvalue.Length == 2)
                {
                    KVPairs.Add(new KeyValuePair<string, string>(keyvalue[0], keyvalue[1]));
                }
            }
            return KVPairs.ToArray();
        }
        #endregion
    }

    #region Exceptions
    public class SettingsException : Exception
    {
        public SettingsException(string Message) : base(Message) { }
    }
    public class ConstructException : SettingsException
    {
        public ConstructException(string Message) : base(Message) { }
    }
    public class NotFoundException : SettingsException
    {
        public NotFoundException(string Message) : base(Message) { }
    }
    public class NotReadyToWorkException : SettingsException
    {
        public NotReadyToWorkException() : base("Данный экземпляр класса Settings не готов к работе.") { }
    }
    #endregion
}
