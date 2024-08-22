using System;
using System.IO;
using System.Text;

namespace ArjSharp
{
    public class ARJ
    {
        private const byte MAIN_HEADER_SIZE = 0x1E;  // Размер основного заголовка
        private const byte FILE_HEADER_SIZE = 0x2E;  // Размер заголовка файла

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
            writer.Write((int)0); // CRC
        }

        // Метод для записи заголовка файла
        private static void WriteFileHeader(BinaryWriter writer, string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            byte[] fileNameBytes = Encoding.ASCII.GetBytes(fileInfo.Name);

            writer.Write((byte)0xEA); // Идентификатор сигнатуры
            writer.Write((byte)(FILE_HEADER_SIZE + fileNameBytes.Length)); // Размер заголовка
            writer.Write((short)0x02); // Тип заголовка файла
            writer.Write((short)0); // Дополнительные данные
            writer.Write((byte)0); // Атрибуты файла
            writer.Write((byte)0); // Версия OS
            writer.Write((int)fileInfo.Length); // Размер файла
            writer.Write((int)fileInfo.LastWriteTime.ToFileTime()); // Время модификации файла
            writer.Write((short)0); // CRC заголовка
            writer.Write((byte)fileNameBytes.Length); // Длина имени файла
            writer.Write(fileNameBytes); // Имя файла
            writer.Write((byte)0); // Размер комментария
        }

        // Метод для записи данных файла
        private static void WriteFileData(BinaryWriter writer, string fileName)
        {
            byte[] fileData = File.ReadAllBytes(fileName);
            writer.Write(fileData);
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
