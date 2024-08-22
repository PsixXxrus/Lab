using System;
using System.IO;
using System.Text;

namespace ArjSharp
{
    public class ARJ
    {
        private const byte MAIN_HEADER_SIZE = 0x1E;  // Размер основного заголовка
        private const byte FILE_HEADER_SIZE = 0x2E;  // Размер заголовка файла

        // Таблица для вычисления CRC32
        private static readonly uint[] Crc32Table;

        static ARJ()
        {
            Crc32Table = new uint[256];
            const uint polynomial = 0xedb88320;
            for (uint i = 0; i < Crc32Table.Length; i++)
            {
                uint crc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ polynomial;
                    else
                        crc >>= 1;
                }
                Crc32Table[i] = crc;
            }
        }

        private static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xffffffff;
            foreach (byte b in data)
            {
                byte tableIndex = (byte)(((crc) & 0xff) ^ b);
                crc = Crc32Table[tableIndex] ^ (crc >> 8);
            }
            return ~crc;
        }

        // Преобразование DateTime в формат DOS времени
        private static ushort ToDosTime(DateTime dateTime)
        {
            return (ushort)(((dateTime.Hour & 0x1F) << 11) |
                            ((dateTime.Minute & 0x3F) << 5) |
                            ((dateTime.Second / 2) & 0x1F));
        }

        // Преобразование DateTime в формат DOS даты
        private static ushort ToDosDate(DateTime dateTime)
        {
            return (ushort)(((dateTime.Year - 1980) & 0x7F) << 9 |
                            ((dateTime.Month & 0xF) << 5) |
                            (dateTime.Day & 0x1F));
        }

        // Метод для создания ARJ архива
        public static void Create(string outputFileName, string[] inputFiles)
        {
            using (FileStream arjFile = new FileStream(outputFileName, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(arjFile))
            {
                WriteMainHeader(writer, inputFiles.Length);

                foreach (var fileName in inputFiles)
                {
                    WriteFileHeader(writer, fileName);
                    WriteFileData(writer, fileName);
                }

                WriteEndOfArchiveHeader(writer);
            }
        }

        // Метод для записи основного заголовка
        private static void WriteMainHeader(BinaryWriter writer, int fileCount)
        {
            writer.Write((byte)0xEA); // Идентификатор сигнатуры
            writer.Write((byte)MAIN_HEADER_SIZE); // Размер заголовка
            writer.Write((short)0x60); // Главный заголовок
            writer.Write((short)0); // Дополнительные данные
            writer.Write((byte)2); // Версия архива
            writer.Write((byte)0); // Минимальная версия
            writer.Write((byte)2); // Версия OS
            writer.Write((byte)0); // Тип архиватора
            writer.Write((byte)0); // Метод сжатия
            writer.Write((byte)fileCount); // Количество файлов
            writer.Write((byte)0); // Размер комментария
            writer.Write((short)0); // Резерв
            writer.Write((int)0); // CRC (заполнено позже)
        }

        // Метод для записи заголовка файла
        private static void WriteFileHeader(BinaryWriter writer, string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            byte[] fileNameBytes = Encoding.ASCII.GetBytes(fileInfo.Name);

            using (MemoryStream headerStream = new MemoryStream())
            using (BinaryWriter headerWriter = new BinaryWriter(headerStream))
            {
                headerWriter.Write((byte)0xEA); // Идентификатор сигнатуры
                headerWriter.Write((byte)(FILE_HEADER_SIZE + fileNameBytes.Length)); // Размер заголовка
                headerWriter.Write((short)0x02); // Тип заголовка файла
                headerWriter.Write((short)0); // Дополнительные данные
                headerWriter.Write((byte)0); // Атрибуты файла
                headerWriter.Write((byte)0); // Версия OS
                headerWriter.Write((int)fileInfo.Length); // Размер файла

                // Преобразование даты и времени в формат DOS
                ushort dosTime = ToDosTime(fileInfo.LastWriteTime);
                ushort dosDate = ToDosDate(fileInfo.LastWriteTime);
                uint dosDateTime = ((uint)dosDate << 16) | dosTime;

                headerWriter.Write(dosDateTime); // Время и дата в формате DOS
                headerWriter.Write((short)0); // CRC заголовка (заполнено позже)
                headerWriter.Write((byte)fileNameBytes.Length); // Длина имени файла
                headerWriter.Write(fileNameBytes); // Имя файла
                headerWriter.Write((byte)0); // Размер комментария

                // Подсчет CRC для заголовка
                byte[] headerData = headerStream.ToArray();
                uint headerCrc = CalculateCrc32(headerData);
                writer.Write(headerData, 0, headerData.Length - 2); // Запись заголовка (до CRC)
                writer.Write((short)headerCrc); // Запись CRC заголовка
            }
        }

        // Метод для записи данных файла
        private static void WriteFileData(BinaryWriter writer, string fileName)
        {
            byte[] fileData = File.ReadAllBytes(fileName);
            uint fileCrc = CalculateCrc32(fileData);
            writer.Write(fileData);
            writer.Write((short)fileCrc); // Запись CRC файла
        }

        // Метод для записи заголовка окончания архива
        private static void WriteEndOfArchiveHeader(BinaryWriter writer)
        {
            writer.Write((byte)0xEA); // Идентификатор сигнатуры
            writer.Write((byte)0x1E); // Размер заголовка
            writer.Write((short)0x60); // Конец архива
            writer.Write((short)0); // Дополнительные данные
            writer.Write((byte)0); // Версия
            writer.Write((byte)0); // Минимальная версия
            writer.Write((byte)0); // OS
            writer.Write((byte)0); // Тип архиватора
            writer.Write((byte)0); // Метод сжатия
            writer.Write((byte)0); // Количество файлов
            writer.Write((byte)0); // Размер комментария
            writer.Write((short)0); // CRC
            writer.Write((int)0); // CRC заголовка
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        string[] filesToArchive = { "file1.txt", "file2.txt", "file3.txt" };
        ARJ.Create("output.arj", filesToArchive);
    }
}