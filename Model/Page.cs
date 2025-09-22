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
        public string Workspace { get; set; }

    }
    /*
    public class DbVersion
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;   // 永远就一行
        public int Version { get; set; }
    }

    public static class DatabaseMigrator
    {
        public static void Migrate(SQLiteConnection conn)
        {
            // 确保版本表存在
            conn.CreateTable<DbVersion>();
            var versionRow = conn.Find<DbVersion>(1);
            if (versionRow == null)
            {
                versionRow = new DbVersion { Id = 1, Version = 0 };
                conn.Insert(versionRow);
            }

            int currentVersion = versionRow.Version;
            int latestVersion = migrations.Count;

            for (int v = currentVersion + 1; v <= latestVersion; v++)
            {
                migrations[v](conn);
                versionRow.Version = v;
                conn.Update(versionRow);
            }
        }

        // 在这里登记所有迁移脚本
        private static readonly Dictionary<int, Action<SQLiteConnection>> migrations =
            new Dictionary<int, Action<SQLiteConnection>>
            {
            {
                1, conn =>
                {
                    // 比如：在 MyEntity 表里加 Age 字段
                    conn.Execute("ALTER TABLE Page ADD COLUMN Workspace VARCHAR");
                }
            },
            {
                2, conn =>
                {
                    // 再比如：加一个 Email 字段
                    conn.Execute("ALTER TABLE MyEntity ADD COLUMN Email TEXT");
                }
            }
            };
    }*/

    public class DatabaseService
    {
        public async Task<SQLiteAsyncConnection> OpenOrCreateDatabase(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory)&&!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            return new SQLiteAsyncConnection(dbPath);
        }
        //https://github.com/praeclarum/sqlite-net/wiki/Automatic-Migrations
        public async Task<CreateTableResult> CreateTables(SQLiteAsyncConnection db)
        {
            return await db.CreateTableAsync<Page>().ConfigureAwait(false);
        }
    }
}
