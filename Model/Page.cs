using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctxmgr.Model
{
    public class Page
    {
        [PrimaryKey,AutoIncrement]
        public long Id { get; set; }
        [Indexed]
        public string Uuid { get; set; }
        public long Index { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

    }
    public class DatabaseService
    {
        public async Task<SQLiteAsyncConnection> OpenOrCreateDatabase(string dbPath)
        {
            return new SQLiteAsyncConnection(dbPath);
        }
        public async Task<CreateTableResult> CreateTables(SQLiteAsyncConnection db)
        {
            return await db.CreateTableAsync<Page>().ConfigureAwait(false);
        }
    }
}
