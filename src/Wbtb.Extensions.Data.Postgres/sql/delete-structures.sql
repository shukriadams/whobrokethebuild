-- DROP INDEXES
DROP INDEX IF EXISTS public."buildlogparseresult_buildid_fk";
DROP INDEX IF EXISTS public."buildflag_buildid_fk";
DROP INDEX IF EXISTS public."buildprocessor_buildid_fk";
DROP INDEX IF EXISTS public."buildinvolvement_buildid_fk";
DROP INDEX IF EXISTS public."buildinvolvement_revisionid_fk";
DROP INDEX IF EXISTS public."job_buildserverid_fk";
DROP INDEX IF EXISTS public."job_sourceserverid_fk";
DROP INDEX IF EXISTS public."build_jobid_fk";
DROP INDEX IF EXISTS public."build_incidentbuildid_fk";
DROP INDEX IF EXISTS public."session_userid_fk";
DROP INDEX IF EXISTS public."jobdelta_jobid_fk";
DROP INDEX IF EXISTS public."jobdelta_buildid_fk";
DROP INDEX IF EXISTS public."r_buildlogparseresult_buildinvolvement_buildlogparseresultid_fk";
DROP INDEX IF EXISTS public."r_buildlogparseresult_buildinvolvement_buildinvolvementid_fk";

-- DROP TABLES
DROP TABLE IF EXISTS public."r_buildlogparseresult_buildinvolvement" CASCADE;
DROP TABLE IF EXISTS public."buildflag" CASCADE;
DROP TABLE IF EXISTS public."buildprocessor" CASCADE;
DROP TABLE IF EXISTS public."incident" CASCADE;
DROP TABLE IF EXISTS public."buildlogparseresult" CASCADE;
DROP TABLE IF EXISTS public."buildinvolvement" CASCADE;
DROP TABLE IF EXISTS public."revision" CASCADE;
DROP TABLE IF EXISTS public."version" CASCADE;
DROP TABLE IF EXISTS public."jobdelta" CASCADE;
DROP TABLE IF EXISTS public."build" CASCADE;
DROP TABLE IF EXISTS public."job" CASCADE;
DROP TABLE IF EXISTS public."session" CASCADE;
DROP TABLE IF EXISTS public."buildserver" CASCADE;
DROP TABLE IF EXISTS public."sourceserver" CASCADE;
DROP TABLE IF EXISTS public."usr" CASCADE;
DROP TABLE IF EXISTS public."configurationstate" CASCADE;
DROP TABLE IF EXISTS public."store" CASCADE;


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
DROP SEQUENCE IF EXISTS public."store_id_seq";
DROP SEQUENCE IF EXISTS public."r_buildLogParseResult_buildinvolvement_id_seq";
