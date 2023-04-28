﻿-- DROP INDEXES
DROP INDEX IF EXISTS public."buildlogparseresult_buildid_fk";
DROP INDEX IF EXISTS public."buildflag_buildid_fk";
DROP INDEX IF EXISTS public."buildprocessor_buildid_fk";
DROP INDEX IF EXISTS public."buildinvolvement_buildid_fk";
DROP INDEX IF EXISTS public."buildinvolvement_revisionid_fk";
DROP INDEX IF EXISTS public."transmissionlog_buildid_fk";
DROP INDEX IF EXISTS public."job_buildserverid_fk";
DROP INDEX IF EXISTS public."job_sourceserverid_fk";
DROP INDEX IF EXISTS public."build_jobid_fk";
DROP INDEX IF EXISTS public."build_incidentbuildid_fk";
DROP INDEX IF EXISTS public."session_userid_fk";
DROP INDEX IF EXISTS public."jobdelta_jobid_fk";
DROP INDEX IF EXISTS public."jobdelta_buildid_fk";

-- DROP TABLES
DROP TABLE IF EXISTS public."buildflag";
DROP TABLE IF EXISTS public."buildprocessor";
DROP TABLE IF EXISTS public."incident";
DROP TABLE IF EXISTS public."transmissionlog";
DROP TABLE IF EXISTS public."buildlogparseresult";
DROP TABLE IF EXISTS public."buildinvolvement";
DROP TABLE IF EXISTS public."revision";
DROP TABLE IF EXISTS public."version";
DROP TABLE IF EXISTS public."jobdelta";
DROP TABLE IF EXISTS public."build";
DROP TABLE IF EXISTS public."job";
DROP TABLE IF EXISTS public."session";
DROP TABLE IF EXISTS public."buildserver";
DROP TABLE IF EXISTS public."sourceserver";
DROP TABLE IF EXISTS public."usr";
DROP TABLE IF EXISTS public."configurationstate";


-- DROP SEQUENCES
DROP SEQUENCE IF EXISTS public."incident_id_seq";
DROP SEQUENCE IF EXISTS public."buildlogparseresult_buildid_seq";
DROP SEQUENCE IF EXISTS public."buildinvolvement_buildid_seq";
DROP SEQUENCE IF EXISTS public."buildinvolvement_revisionid_seq";
DROP SEQUENCE IF EXISTS public."buildinvolvement_mappeduserid_seq";
DROP SEQUENCE IF EXISTS public."revision_sourceserverid_seq";
DROP SEQUENCE IF EXISTS public."revision_id_seq";
DROP SEQUENCE IF EXISTS public."build_id_seq";
DROP SEQUENCE IF EXISTS public."build_jobid_seq";
DROP SEQUENCE IF EXISTS public."build_incidentbuildid_seq";
DROP SEQUENCE IF EXISTS public."buildlogparseresult_id_seq";
DROP SEQUENCE IF EXISTS public."buildflag_id_seq";
DROP SEQUENCE IF EXISTS public."buildprocessor_id_seq";
DROP SEQUENCE IF EXISTS public."buildinvolvement_id_seq";
DROP SEQUENCE IF EXISTS public."transmissionlog_id_seq";
DROP SEQUENCE IF EXISTS public."transmissionlog_buildid_seq";
DROP SEQUENCE IF EXISTS public."session_id_seq";
DROP SEQUENCE IF EXISTS public."buildserver_id_seq";
DROP SEQUENCE IF EXISTS public."job_id_seq";
DROP SEQUENCE IF EXISTS public."job_sourceserverid_seq";
DROP SEQUENCE IF EXISTS public."job_buildserverid_seq";
DROP SEQUENCE IF EXISTS public."sourceserver_id_seq";
DROP SEQUENCE IF EXISTS public."usr_id_seq";
DROP SEQUENCE IF EXISTS public."session_userid_seq";
DROP SEQUENCE IF EXISTS public."configurationstate_id_seq";
DROP SEQUENCE IF EXISTS public."jobdelta_id_seq";