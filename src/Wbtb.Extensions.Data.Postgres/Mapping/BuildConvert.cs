using Npgsql;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    public class BuildConvert : IRecordConverter<Build>
    {
        private Build ToCommonSingle(NpgsqlDataReader reader)
        {
            return new Build
            {
                Id = reader["id"].ToString(),
                Signature = reader["signature"].ToString(),
                JobId = reader["jobid"].ToString(),
                IncidentBuildId = reader["incidentbuildid"] == DBNull.Value ? null : reader["incidentbuildid"].ToString(),
                Identifier = reader["identifier"].ToString(),
                TriggeringCodeChange = reader["triggeringcodechange"].ToString(),
                TriggeringType = reader["triggeringtype"].ToString(),
                LogPath = reader["logpath"] == DBNull.Value ? null : reader["logpath"].ToString(),
                StartedUtc = DateTime.Parse(reader["startedutc"].ToString()),
                EndedUtc = reader["endedutc"] == DBNull.Value ? (DateTime?)null : DateTime.Parse(reader["endedutc"].ToString()),
                Hostname = reader["hostname"].ToString(),
                Status = (BuildStatus)reader["status"]
            };
        }

        public Build ToCommon(NpgsqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            reader.Read();
            return ToCommonSingle(reader);
        }

        public IEnumerable<Build> ToCommonList(NpgsqlDataReader reader)
        { 
            IList<Build> list = new List<Build>();
            while (reader.Read())
                list.Add(this.ToCommonSingle(reader));

            return list;
        }
    }
}
