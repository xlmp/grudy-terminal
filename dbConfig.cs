using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml.Linq;

namespace Grudy
{
    public class dbConfig
    {

        private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "grudy.cfg");
        private static readonly string ConnectionString = $"Data Source={DbPath};Cache=Shared";

        public async Task Init()
        {
            await EnsureDatabaseAsync();
            await CreateTablesAsync();
        }
        public async Task EnsureDatabaseAsync()
        {
            try
            {
                // Apenas garantir que o arquivo exista (o SQLite cria ao conectar se não existir)
                if (!File.Exists(DbPath))
                {
                    // Abre e fecha uma conexão para forçar a criação
                    await using var conn = new SqliteConnection(ConnectionString);
                    await conn.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task CreateTablesAsync()
        {
            string[] sql = new string[]
            {
                 @"CREATE TABLE IF NOT EXISTS SysConfig (
                    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    WindowPosX DECIMAL,
                    WindowPosY DECIMAL,
                    WindowWidth DECIMAL,
                    WindowHeight DECIMAL
                  );",

                 @"CREATE TABLE IF NOT EXISTS Terminal (
                    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    CurrentDir TEXT NOT NULL
                  );",

                 @"CREATE TABLE IF NOT EXISTS Macros (
                    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Value TEXT NOT NULL
                  );",
            };

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            foreach (string sqlItem in sql)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sqlItem;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<TConfigs?> SysConfig()
        {
            TConfigs? configs = null;
            const string sql = @"SELECT * FROM SysConfig WHERE id = 1";

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var result = await cmd.ExecuteReaderAsync();

            if (result.Read())
            {
                configs = new TConfigs();
                configs.WindowPosX = Convert.ToDouble(result["WindowPosX"] ?? 0);
                configs.WindowPosY = Convert.ToDouble(result["WindowPosY"] ?? 0);
                configs.WindowWidth = Convert.ToDouble(result["WindowWidth"] ?? 0);
                configs.WindowHeight = Convert.ToDouble(result["WindowHeight"] ?? 0);
            }
            return configs;
        }
        public async Task<int> SysConfigSave(TConfigs e)
        {
            try
            {
                const string sql = @"
            DELETE FROM SysConfig WHERE id = 1;
            INSERT INTO SysConfig (Id, WindowPosX, WindowPosY, WindowWidth, WindowHeight)
            VALUES (1, $WindowPosX, $WindowPosY, $WindowWidth, $WindowHeight);
            SELECT last_insert_rowid(); -- retorna o último Id
            ";

                await using var conn = new SqliteConnection(ConnectionString);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("$WindowPosX", e.WindowPosX);
                cmd.Parameters.AddWithValue("$WindowPosY", e.WindowPosY);
                cmd.Parameters.AddWithValue("$WindowWidth", e.WindowWidth);
                cmd.Parameters.AddWithValue("$WindowHeight", e.WindowHeight);


                // ExecuteScalar retorna o resultado do SELECT last_insert_rowid()
                var result = await cmd.ExecuteScalarAsync();
                return (Convert.ToInt32((long)(result ?? 0L)));
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<int> NewTerminal(string name, string CurrentDir)
        {

            try
            {
                const string sql = @"
            INSERT INTO Terminal (Name, CurrentDir)
            VALUES ($Name, $CurrentDir);
            SELECT last_insert_rowid(); -- retorna o último Id
            ";

                await using var conn = new SqliteConnection(ConnectionString);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("Name", name);
                cmd.Parameters.AddWithValue("$CurrentDir", CurrentDir);

                // ExecuteScalar retorna o resultado do SELECT last_insert_rowid()
                var result = await cmd.ExecuteScalarAsync();
                return (Convert.ToInt32((long)(result ?? 0L)));
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<TTerminal> NewTerminalC(string name, string CurrentDir)
        {
            var nId = await NewTerminal(name, CurrentDir);
            return new TTerminal()
            {
                Id = nId,
                Name = name,
                CurrentDir = CurrentDir,
            };
        }
        public async Task<int> RemoveTerminal(int id)
        {
            const string sql = @"
            DELETE FROM Terminal WHERE id = $idt
            ";

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("$idt", id);

            // ExecuteScalar retorna o resultado do SELECT last_insert_rowid()
            var result = await cmd.ExecuteNonQueryAsync();
            return result;
        }
        public async Task<int> RemoveTerminal(TTerminal t)
        {
            return await this.RemoveTerminal(t.Id);
        }

        public async Task<int> UpdateTerminal(TTerminal t)
        {
            const string sql = @"
            UPDATE Terminal SET CurrentDir = $CurrentDir, Name = $Name WHERE id = $idt";

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("$idt", t.Id);
            cmd.Parameters.AddWithValue("$CurrentDir", t.CurrentDir);
            cmd.Parameters.AddWithValue("$Name", t.Name);

            // ExecuteScalar retorna o resultado do SELECT last_insert_rowid()
            var result = await cmd.ExecuteNonQueryAsync();
            return result;
        }

        public async Task<List<TTerminal>> Terminals()
        {
            List<TTerminal> lst = new List<TTerminal>();
            const string sql = @"
            SELECT * FROM Terminal
            ";

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var result = await cmd.ExecuteReaderAsync();
            while (result.Read())
            {
                var terminal = new TTerminal()
                {
                    Id = Convert.ToInt32(result["id"] ?? 0),
                    CurrentDir = (string)result["CurrentDir"],
                    Name = (string)result["Name"],
                };
                lst.Add(terminal);
            }

            return lst;
        }

        public async Task<Dictionary<string, string>> Macros()
        {
            Dictionary<string, string> Macros = new Dictionary<string, string>();
            const string sql = @"
            SELECT * FROM Macros
            ";

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var result = await cmd.ExecuteReaderAsync();
            while (result.Read())
            {
                Macros.Add((string)result["Name"], (string)result["Value"]);
            }

            return Macros;
        }
        public async Task<int> MacroAdd(string key, string value)
        {
            try
            {
                const string sql = @"INSERT INTO Macros (Name, Value)VALUES ($name, $value);
                SELECT last_insert_rowid(); -- retorna o último Id";

                await using var conn = new SqliteConnection(ConnectionString);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("$name", key);
                cmd.Parameters.AddWithValue("$value", value);

                // ExecuteScalar retorna o resultado do SELECT last_insert_rowid()
                var result = await cmd.ExecuteScalarAsync();
                return (Convert.ToInt32((long)(result ?? 0L)));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> MacroRemove(string key)
        {
            const string sql = @"
            DELETE FROM Macro WHERE $name = $namek
            ";

            await using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("$namek", key);

            // ExecuteScalar retorna o resultado do SELECT last_insert_rowid()
            var result = await cmd.ExecuteNonQueryAsync();
            return result;
        }
    }
    public class TConfigs
    {
        public double? WindowPosX { get; set; } = null;
        public double? WindowPosY { get; set; } = null;
        public double? WindowWidth { get; set; } = null;
        public double? WindowHeight { get; set; } = null;

    }
    public class TTerminal
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CurrentDir { get; set; }

    }

    

    public static class BrushExtensions
    {
        public static Brush ToFrozenBrush(this Color color)
        {
            var b = new SolidColorBrush(color);
            b.Freeze();
            return b;
        }
        public static bool TryGetColor(this Brush brush, out Color color)
        {
            if (brush is SolidColorBrush scb)
            {
                color = scb.Color;
                return true;
            }

            color = default;
            return false;
        }
    }
}
