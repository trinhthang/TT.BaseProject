using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Constant
{
    /// <summary>
    /// Loại file
    /// </summary>
    public static class ContentType
    {
        public const string ContentDiposition_Format = "attachment;filename=\"{0}\";";
        public const string ContentType_Json = "application/json; charset=utf-8";
        public const string ContentType_Png = "image/png";
        public const string ContentType_Bmp = "image/bmp";
        public const string ContentType_Jpg = "image/jpg";
        public const string ContentType_Gif = "image/gif";
        public const string ContentType_Pdf = "application/pdf";
        public const string ContentType_File = "application/octet-stream";
        public const string ContentType_ZIP = "application/x-zip-compressed";

        public const string ContentType_Excel = "application/vnd.ms-excel"; //xls
        public const string ContentType_ExcelOpenXML = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; //xlsx
        public const string ContentType_OpenDoc = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; //xlsx
        public const string ContentType_OpenDocument = "application/vnd.oasis.opendocument.spreadsheet"; //ods
        public const string ContentType_Word = "application/msword"; //doc
        public const string ContentType_WordOpenXML = "application/msword"; //docx
        public const string ContentType_Text = "text/plain"; //txt
    }
}
