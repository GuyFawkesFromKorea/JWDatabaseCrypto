#define __SQL__

namespace JWDatabseCrypto.Repository
{
    using System;
    using Dapper;
    using System.Data.SqlClient;
    using System.Configuration;

    public class BaseRepo : IDisposable
    {
        private bool disposed;
#if __SQL__
		public SqlConnection DbConnection { get; private set; }
#endif
#if __MYSQL__
		public MySqlConnection DbConnection { get; private set; }
#endif
#if __ORACLE__
		public OracleConnection DbConnection { get; private set; }
#endif
#if __SQLITE__
		public SqliteConnection DbConnection { get; private set; }
#endif


        public BaseRepo()
        {
#if __SQL__
			DbConnection = new SqlConnection(
				ConfigurationManager.ConnectionStrings["default"].ConnectionString);
#endif

#if __MYSQL__
			DbConnection = new MySqlConnection(
				ConfigurationManager.ConnectionStrings["default"].ConnectionString);
#endif

#if __ORACLE__
			DbConnection = new OracleConnection(
				ConfigurationManager.ConnectionStrings["default"].ConnectionString);
#endif

#if __SQLITE__
			DbConnection = new SqliteConnection(
				ConfigurationManager.ConnectionStrings["default"].ConnectionString);
#endif
        }

        public BaseRepo(string configName)
        {
#if __SQL__
			DbConnection = new SqlConnection(
				ConfigurationManager.ConnectionStrings[configName].ConnectionString);
#endif

#if __MYSQL__
			DbConnection = new MySqlConnection(
				ConfigurationManager.ConnectionStrings[configName].ConnectionString);
#endif

#if __ORACLE__
			DbConnection = new OracleConnection(
				ConfigurationManager.ConnectionStrings[configName].ConnectionString);
#endif

#if __SQLITE__
			DbConnection = new SqliteConnection(
				ConfigurationManager.ConnectionStrings[configName].ConnectionString);
#endif
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (DbConnection != null)
                {
                    DbConnection.Close();
                    DbConnection.Dispose();
                    DbConnection = null;
                }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        protected virtual void AddErrorMsg(string userid, string pgmId, DateTime errDtm, string comment, string dberrInfo, string etc)
        {
            this.DbConnection.Execute(
            @"
INSERT INTO [NEWSIA].[dbo].[ERRMSG_TB]
           ([USERID]
           ,[PGM_ID]
           ,[ERR_DTM]
           ,[COMMENT]
           ,[DBERR_INFO]
           ,[ETC])
     VALUES
           (@USERID
           ,@PGM_ID
           ,@ERR_DTM
           ,@COMMENT
           ,@DBERR_INFO
           ,@ETC)
			",
            new
            {
                USERID = userid,
                PGM_ID = pgmId,
                ERR_DTM = errDtm,
                COMMENT = comment,
                DBERR_INFO = dberrInfo,
                ETC = etc
            }
            );
        }
    }
}
