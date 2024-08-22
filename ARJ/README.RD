using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ArjFile
{
    private const uint ARJ_HEADER_SIZE = 0x1E;
    private const uint ARJ_FILE_HEADER_SIZE = 0x40;
    private const uint ARJ_END_HEADER_SIZE = 0x1E;
    private const uint ARJ_MAGIC_NUMBER = 0x1A;
    
    private List<string> files = new List<string>();
    private string archiveName;

    public ArjFile(string archiveName)
    {
        this.archiveName = archiveName;
    }

    public void AddFile(string filePath)
    {
        files.Add(filePath);
    }

    public void CreateArchive()
    {
        using (FileStream fs = new FileStream(archiveName, FileMode.Create))
        {
            WriteMainHeader(fs);
            WriteFileHeaders(fs);
            WriteFiles(fs);
            WriteEndOfArchive(fs);
        }
    }

    private void WriteMainHeader(FileStream fs)
    {
        byte[] header = new byte[ARJ_HEADER_SIZE];
        Array.Clear(header, 0, header.Length);
        
        // ARJ signature
        Encoding.ASCII.GetBytes("ARJ", 0, 3, header, 0);
        
        // Write header
        fs.Write(header, 0, header.Length);
    }

    private void WriteFileHeaders(FileStream fs)
    {
        foreach (string filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            byte[] header = new byte[ARJ_FILE_HEADER_SIZE];
            Array.Clear(header, 0, header.Length);

            // Fill in the header
            Encoding.ASCII.GetBytes(fileInfo.Name, 0, fileInfo.Name.Length, header, 0);
            // Size of the file
            BitConverter.GetBytes((uint)fileInfo.Length).CopyTo(header, 0x20);
            // CRC32
            BitConverter.GetBytes(Crc32.Compute(filePath)).CopyTo(header, 0x24);

            // Write file header
            fs.Write(header, 0, header.Length);
        }
    }

    private void WriteFiles(FileStream fs)
    {
        foreach (string filePath in files)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(fs);
            }
        }
    }

    private void WriteEndOfArchive(FileStream fs)
    {
        byte[] endHeader = new byte[ARJ_END_HEADER_SIZE];
        Array.Clear(endHeader, 0, endHeader.Length);

        // ARJ end signature
        endHeader[0] = 0x60;
        endHeader[1] = 0xEA;

        // Write end of archive header
        fs.Write(endHeader, 0, endHeader.Length);
    }
}

public static class Crc32
{
    private static readonly uint[] CrcTable;

    static Crc32()
    {
        const uint polynomial = 0xedb88320;
        CrcTable = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (uint j = 8; j > 0; j--)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ polynomial;
                else
                    crc >>= 1;
            }
            CrcTable[i] = crc;
        }
    }

    public static uint Compute(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            uint crc = 0xffffffff;
            int byteRead;
            while ((byteRead = fs.ReadByte()) != -1)
            {
                byte b = (byte)byteRead;
                crc = (crc >> 8) ^ CrcTable[(crc & 0xff) ^ b];
            }
            return ~crc;
        }
    }
}