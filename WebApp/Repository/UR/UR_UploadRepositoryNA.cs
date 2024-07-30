using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.FileService;

namespace WebApp.Repository.UR
{
    public class UR_UploadRepositoryNA : JCLib.Mvc.BaseRepository
    {
        public UR_UploadRepositoryNA(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(IEnumerable<UR_UPLOAD> ur_upload)
        {
            string sql = @"INSERT INTO UR_UPLOAD_NA (FG, UK, FP, FN, FT, FS, ST, IP) VALUES 
                        (@FG, @UK, @FP, @FN, @FT, @FS, @ST, @IP)";
            return DBWork.Connection.Execute(sql, ur_upload, DBWork.Transaction);
        }

        public IEnumerable<UR_UPLOAD> GetFilesByUploadKey(string upload_key)
        {
            string sql = @"SELECT FG, UK, FP, FN, FT, FS, FC, ST, IP, FD 
                           FROM UR_UPLOAD_NA A WHERE UK = @UK";
            return DBWork.Connection.Query<UR_UPLOAD>(sql, new { UK = upload_key }, DBWork.Transaction);
        }

        public UR_UPLOAD GetFileByGuid(string file_guid)
        {
            string sql = @"SELECT FG, UK, FP, FN, FT, FS, FC, ST, IP, FD 
                           FROM UR_UPLOAD_NA A WHERE FG = @FG";
            return DBWork.Connection.QueryFirst<UR_UPLOAD>(sql, new { FG = file_guid }, DBWork.Transaction);
        }

        public int DeleteByFG(string file_guid)
        {
            IFileService fs_client = new FileServiceClient();
            int total_physical_deleted = fs_client.DeleteFile(file_guid);

            string sql = @"DELETE FROM UR_UPLOAD_NA WHERE FG = @FG";
            return DBWork.Connection.Execute(sql, new { FG = file_guid }, DBWork.Transaction);
        }

        public int DeleteByUK(string upload_key)
        {
            string sql1 = @"SELECT FN FROM UR_UPLOAD_NA WHERE UK = @UK";
            IEnumerable<UR_UPLOAD> uploadFiles = DBWork.Connection.Query<UR_UPLOAD>(sql1, new { UK = upload_key }, DBWork.Transaction);

            int total_physical_deleted = 0;
            foreach (UR_UPLOAD uu in uploadFiles)
            {
                IFileService fs_client = new FileServiceClient();
                total_physical_deleted += fs_client.DeleteFile(uu.FN);
            }

            string sql = @"DELETE FROM UR_UPLOAD_NA WHERE UK = @UK";
            return DBWork.Connection.Execute(sql, new { UK = upload_key }, DBWork.Transaction);
        }

        public int UpdateFD(string file_guid, string file_desc)
        {
            string sql = @"UPDATE UR_UPLOAD_NA SET FD = @FD WHERE FG = @FG";
            return DBWork.Connection.Execute(sql, new { FD = file_desc, FG = file_guid }, DBWork.Transaction);
        }
    }
}