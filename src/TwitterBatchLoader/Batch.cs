﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using TwitterLib;
using TwitterLib.Load;

namespace TwitterBatchLoader
{
    class Batch
    {
        private int batchID;
        private short runID;
        private string userName;
        private SqlConnectionStringBuilder targetDB;
        private SqlConnectionStringBuilder loaderDB;
        private string sourcePath;
        private string bulkPath;
        private bool binary;

        List<Chunk> chunks;

        public int BatchID
        {
            get { return batchID; }
            set { batchID = value; }
        }

        public short RunID
        {
            get { return runID; }
            set { runID = value; }
        }

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public SqlConnectionStringBuilder TargetDB
        {
            get { return targetDB; }
        }

        public SqlConnectionStringBuilder LoaderDB
        {
            get { return loaderDB; }
        }

        public string SourcePath
        {
            get { return sourcePath; }
            set { sourcePath = value; }
        }

        public string BulkPath
        {
            get { return bulkPath; }
            set { bulkPath = value; }
        }

        public bool Binary
        {
            get { return binary; }
            set { binary = value; }
        }

        public List<Chunk> Chunks
        {
            get { return chunks; }
        }

        public Batch()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.batchID = 0;
            this.runID = 0;
            this.userName = Environment.UserName;
            this.targetDB = new SqlConnectionStringBuilder("Data Source=localhost;Initial Catalog=Twitter;Integrated Security=true");
            this.loaderDB = new SqlConnectionStringBuilder("Data Source=localhost;Initial Catalog=TwitterLoader;Integrated Security=true");
            this.sourcePath = null;
            this.bulkPath = null;

            this.chunks = new List<Chunk>();
        }

        public void Save(DatabaseContext context)
        {
            if (batchID == 0)
            {
                Create(context);
            }
            else
            {
                Modify(context);
            }
        }

        private void Create(DatabaseContext context)
        {
            var sql = @"
INSERT batch
    (run_id, user_name, target_db, loader_db, source_path, bulk_path, binary)
VALUES (@run_id, @user_name, @target_db, @loader_db, @source_path, @bulk_path, @binary);

SELECT CAST(@@IDENTITY AS int)";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                AppendCreateModifyParameters(cmd);

                var res = cmd.ExecuteScalar();
                batchID = (int)res;
            }
        }

        private void Modify(DatabaseContext context)
        {
            var sql = @"
UPDATE batch
SET run_id = @run_id,
    user_name = @user_name,
    target_db = @target_db,
    loader_db = @loader_db,
    source_path = @source_path,
    bulk_path = @bulk_path,
    binary = @binary
WHERE batch_id = @batch_id";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                cmd.Parameters.Add("@batch_id", SqlDbType.Int).Value = batchID;
                AppendCreateModifyParameters(cmd);

                cmd.ExecuteNonQuery();
            }
        }

        private void AppendCreateModifyParameters(SqlCommand cmd)
        {
            cmd.Parameters.Add("@run_id", SqlDbType.SmallInt).Value = runID;
            cmd.Parameters.Add("@user_name", SqlDbType.NVarChar).Value = userName;
            cmd.Parameters.Add("@target_db", SqlDbType.NVarChar).Value = targetDB.ConnectionString;
            cmd.Parameters.Add("@loader_db", SqlDbType.NVarChar).Value = loaderDB.ConnectionString;
            cmd.Parameters.Add("@source_path", SqlDbType.NVarChar).Value = sourcePath;
            cmd.Parameters.Add("@bulk_path", SqlDbType.NVarChar).Value = bulkPath;
            cmd.Parameters.Add("@binary", SqlDbType.Bit).Value = binary;
        }

        public void Load(DatabaseContext context)
        {
            var sql = @"SELECT * FROM batch WHERE batch_id = @batch_id";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                cmd.Parameters.Add("@batch_id", SqlDbType.Int).Value = batchID;

                using (var dr = cmd.ExecuteReader())
                {
                    dr.Read();
                    LoadFromDataReader(dr);
                }
            }
        }

        private void LoadFromDataReader(SqlDataReader dr)
        {
            int o = -1;
            batchID = dr.GetInt32(++o);
            runID = dr.GetInt16(++o);
            userName = dr.GetString(++o);
            targetDB.ConnectionString = dr.GetString(++o);
            loaderDB.ConnectionString = dr.GetString(++o);
            sourcePath = dr.GetString(++o);
            bulkPath = dr.GetString(++o);
            binary = dr.GetBoolean(++o);
        }

        private void DeleteChunks(DatabaseContext context)
        {
            var sql = "DELETE chunk WHERE batch_id = @batch_id";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                cmd.Parameters.Add("@batch_id", SqlDbType.Int).Value = batchID;
                cmd.ExecuteNonQuery();
            }
        }

        public void LoadChunks(DatabaseContext context)
        {
            chunks.Clear();

            var sql = "SELECT * FROM chunk WHERE batch_id = @batch_id";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                cmd.Parameters.Add("@batch_id", SqlDbType.Int).Value = batchID;

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        chunks.Add(LoadChunkFromDataReader(dr));
                    }
                }
            }
        }

        private Chunk LoadChunkFromDataReader(SqlDataReader dr)
        {
            var c = new Chunk();

            int o = -1;

            c.BatchID = dr.GetInt32(++o);
            c.ChunkId = dr.GetString(++o);
            c.PrepareStart = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.PrepareEnd = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.LoadStart = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.LoadEnd = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.MergeStart = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.MergeEnd = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.CleanupStart = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;
            c.CleanupEnd = !dr.IsDBNull(++o) ? dr.GetDateTime(o) : DateTime.MinValue;

            c.Binary = binary;
            c.RunId = runID;
            c.BulkPath = bulkPath;
            c.Filename = Path.Combine(sourcePath, c.ChunkId);
            c.LoaderDB.ConnectionString = loaderDB.ConnectionString;
            c.TargetDB.ConnectionString = targetDB.ConnectionString;

            return c;
        }

        public void CreateChunks(DatabaseContext context)
        {
            foreach (var c in chunks)
            {
                CreateChunk(c, context);
            }
        }

        public void CreateChunk(Chunk chunk, DatabaseContext context)
        {
            var sql = @"
INSERT chunk
    (batch_id, chunk_id,
     prepare_start, prepare_end, load_start, load_end,
     merge_start, merge_end, cleanup_start, cleanup_end)
VALUES
    (@batch_id, @chunk_id,
     @prepare_start, @prepare_end, @load_start, @load_end,
     @merge_start, @merge_end, @cleanup_start, @cleanup_end);
";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                AppendChunkCreateModifyParameters(chunk, cmd);

                cmd.ExecuteNonQuery();
            }
        }

        public void ModifyChunk(Chunk chunk, DatabaseContext context)
        {
            var sql = @"
UPDATE chunk
SET prepare_start = @prepare_start,
    prepare_end = @prepare_end,
    load_start = @load_start,
    load_end = @load_end,
    merge_start = @merge_start,
    merge_end = @merge_end,
    cleanup_start = @cleanup_end
WHERE batch_id = @batch_id AND chunk_id = @chunk_id";

            using (var cmd = new SqlCommand(sql, context.Connection, context.Transaction))
            {
                AppendChunkCreateModifyParameters(chunk, cmd);

                cmd.ExecuteNonQuery();
            }
        }

        private void AppendChunkCreateModifyParameters(Chunk chunk, SqlCommand cmd)
        {
            cmd.Parameters.Add("@batch_id", SqlDbType.Int).Value = batchID;
            cmd.Parameters.Add("@chunk_id", SqlDbType.NVarChar).Value = chunk.ChunkId;
            cmd.Parameters.Add("@prepare_start", SqlDbType.DateTime).Value = chunk.PrepareStart == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.PrepareStart;
            cmd.Parameters.Add("@prepare_end", SqlDbType.DateTime).Value = chunk.PrepareEnd == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.PrepareEnd;
            cmd.Parameters.Add("@load_start", SqlDbType.DateTime).Value = chunk.LoadStart == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.LoadStart;
            cmd.Parameters.Add("@load_end", SqlDbType.DateTime).Value = chunk.LoadEnd == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.LoadEnd;
            cmd.Parameters.Add("@merge_start", SqlDbType.DateTime).Value = chunk.MergeStart == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.MergeStart;
            cmd.Parameters.Add("@merge_end", SqlDbType.DateTime).Value = chunk.MergeEnd == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.MergeEnd;
            cmd.Parameters.Add("@cleanup_start", SqlDbType.DateTime).Value = chunk.CleanupStart == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.CleanupStart;
            cmd.Parameters.Add("@cleanup_end", SqlDbType.DateTime).Value = chunk.CleanupEnd == DateTime.MinValue ? (object)DBNull.Value : (object)chunk.CleanupEnd;
        }

        public Chunk[] GetChunksInOrder()
        {
            var q = from c in chunks
                    orderby c.ChunkId
                    select c;

            return q.ToArray();
        }
    }
}
