-- CREATE SEQUENCES
CREATE SEQUENCE public."store_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."store_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."configurationstate_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."configurationstate_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."buildlogparseresult_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."buildlogparseresult_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."session_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."session_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."revision_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."revision_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."build_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."build_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."buildinvolvement_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."buildinvolvement_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."buildserver_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."buildserver_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."job_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."job_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."sourceserver_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."sourceserver_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."usr_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."usr_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."r_buildLogParseResult_buildinvolvement_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."r_buildLogParseResult_buildinvolvement_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."daemontask_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."daemontask_id_seq" OWNER TO postgres;

CREATE SEQUENCE public."mutationreport_id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;
ALTER SEQUENCE public."mutationreport_id_seq" OWNER TO postgres;


-- CREATE TABLES
-- TABLE : BuildServer
CREATE TABLE public."buildserver"
(
    id integer NOT NULL DEFAULT nextval('"buildserver_id_seq"'::regclass),
    "key" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "buildserver_pkey" PRIMARY KEY (id),
    CONSTRAINT "buildserver_id_unique" UNIQUE ("key")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."buildserver" OWNER TO postgres;


-- TABLE : SourceServer
CREATE TABLE public."sourceserver"
(
    id integer NOT NULL DEFAULT nextval('"sourceserver_id_seq"'::regclass),
    "key" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "sourceserver_pkey" PRIMARY KEY (id),
    CONSTRAINT "sourceserver_id_unique" UNIQUE ("key")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."sourceserver" OWNER TO postgres;


-- TABLE : configstate
CREATE TABLE public."configurationstate"
(
    id integer NOT NULL DEFAULT nextval('"configurationstate_id_seq"'::regclass),
    createdutc timestamp(4) without time zone NOT NULL,
    "hash" text COLLATE pg_catalog."default" NOT NULL,
    content text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "configurationstate_pkey" PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."configurationstate" OWNER TO postgres;


-- TABLE : usr
CREATE TABLE public."usr"
(
    id integer NOT NULL DEFAULT nextval('"usr_id_seq"'::regclass),
    "key" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "usr_pkey" PRIMARY KEY (id),
    CONSTRAINT "usr_id_unique" UNIQUE ("key")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."usr" OWNER TO postgres;


-- TABLE : Job
CREATE TABLE public."job"
(
    id integer NOT NULL DEFAULT nextval('"job_id_seq"'::regclass),
    "key" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    buildserverid integer NOT NULL,
    sourceserverid integer,
    CONSTRAINT "job_pkey" PRIMARY KEY (id),
    CONSTRAINT "job_id_unique" UNIQUE ("key", buildserverid),
    CONSTRAINT "job_buildserverid_fk" FOREIGN KEY (buildserverid)
        REFERENCES public."buildserver" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "job_sourceaerverid_fk" FOREIGN KEY (sourceserverid)
        REFERENCES public."sourceserver" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."job" OWNER TO postgres;


-- TABLE : Revision
CREATE TABLE public."revision"
(
    id integer NOT NULL DEFAULT nextval('"revision_id_seq"'::regclass),
    signature character varying(38) COLLATE pg_catalog."default" NOT NULL,
    code character varying(64) COLLATE pg_catalog."default" NOT NULL,
    sourceserverid integer NOT NULL,
    created timestamp(4) without time zone,
    usr character varying(64) COLLATE pg_catalog."default" NOT NULL,
    files text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    CONSTRAINT "revision_primary_key" PRIMARY KEY (id),
    CONSTRAINT "revision_sourceserver_fk" FOREIGN KEY (sourceserverid)
        REFERENCES public."sourceserver" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."revision" OWNER TO postgres;


-- TABLE : Build
CREATE TABLE public."build"
(
    id integer NOT NULL DEFAULT nextval('"build_id_seq"'::regclass),
    signature character varying(38) COLLATE pg_catalog."default" NOT NULL,
    jobid integer NOT NULL,
    incidentbuildid integer,
    "key" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    uniquepublickey character varying(64) COLLATE pg_catalog."default" NOT NULL,
    triggeringcodechange character varying(64) COLLATE pg_catalog."default",
    triggeringtype character varying(64) COLLATE pg_catalog."default",
    startedutc timestamp(4) without time zone NOT NULL,
    endedutc timestamp(4) without time zone,
    logfetched boolean NOT NULL,
    hostname character varying(64) COLLATE pg_catalog."default",
    status integer NOT NULL,
    CONSTRAINT "build_pkey" PRIMARY KEY (id),
    CONSTRAINT "build_key_unique" UNIQUE ("key", jobid),
    CONSTRAINT "build_uniquepublickey_unique" UNIQUE (uniquepublickey),
    CONSTRAINT "build_jobid_fk" FOREIGN KEY (jobid)
        REFERENCES public."job" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "build_incidentbuildid_fk" FOREIGN KEY (incidentbuildid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."build" OWNER TO postgres;


-- TABLE : Store
CREATE TABLE public."store"
(
    id integer NOT NULL DEFAULT nextval('"store_id_seq"'::regclass),
    "key" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "plugin" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    content text COLLATE pg_catalog."default",
    CONSTRAINT "store_pkey" PRIMARY KEY (id),
    CONSTRAINT "store_key_unique" UNIQUE ("key")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."store" OWNER TO postgres;


-- TABLE : BuildInvolvement
CREATE TABLE public."buildinvolvement"
(
    id integer NOT NULL DEFAULT nextval('"buildinvolvement_id_seq"'::regclass),
    signature character varying(38) COLLATE pg_catalog."default" NOT NULL,
    buildid integer NOT NULL,
    revisioncode character varying(64) COLLATE pg_catalog."default" NOT NULL,
    revisionid integer,
    mappeduserid integer,
    isignoredfrombreakhistory boolean NOT NULL,
    blamescore integer,
    inferredrevisionlink boolean NOT NULL,
    comment character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "buildInvolvement_pkey" PRIMARY KEY (id),
    CONSTRAINT "buildInvolvement_unique_key" UNIQUE (buildid, revisioncode),
    CONSTRAINT "buildinvolvement_buildid_fk" FOREIGN KEY (buildid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "buildinvolvement_revisionid_fk" FOREIGN KEY (revisionid)
        REFERENCES public."revision" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "buildinvolvement_mappeduserid_fk" FOREIGN KEY (mappeduserid)
        REFERENCES public."usr" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."buildinvolvement" OWNER TO postgres;


-- TABLE : BuildLogParseResult
CREATE TABLE public."buildlogparseresult"
(
    id integer NOT NULL DEFAULT nextval('"buildlogparseresult_id_seq"'::regclass),
    buildid integer NOT NULL,
    logparserplugin character varying(64) COLLATE pg_catalog."default" NOT NULL,
    parsedcontent text COLLATE pg_catalog."default",
    signature character varying(38) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "buildLogparseresult_pkey" PRIMARY KEY (id),
    CONSTRAINT "buildLogparseresult_compoundkey" UNIQUE (buildid, logparserplugin),
    CONSTRAINT "buildlogparseresult_buildid_fk" FOREIGN KEY (buildid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."buildlogparseresult" OWNER TO postgres;


-- TABLE : VERSION
CREATE TABLE public."version"
(
    id character varying(64) NOT NULL COLLATE pg_catalog."default",
    created timestamp(4) without time zone,
    CONSTRAINT "version_pkey" PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."version" OWNER TO postgres;


-- TABLE : SESSION
CREATE TABLE public."session"
(
    id integer NOT NULL DEFAULT nextval('"session_id_seq"'::regclass),
    userid integer NOT NULL,
    useragent character varying(64) COLLATE pg_catalog."default",
    ip character varying(40) COLLATE pg_catalog."default",
    createdutc timestamp(4) without time zone,
    CONSTRAINT "session_pkey" PRIMARY KEY (id),
    CONSTRAINT "session_userid_fk" FOREIGN KEY (userid)
        REFERENCES public."usr" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."session" OWNER TO postgres;


-- TABLE : r_buildLogParseResult_buildinvolvement
CREATE TABLE public."r_buildlogparseresult_buildinvolvement"
(
    id integer NOT NULL DEFAULT nextval('"r_buildLogParseResult_buildinvolvement_id_seq"'::regclass),
    buildlogparseresultid integer NOT NULL,
    buildinvolvementid integer NOT NULL,
    CONSTRAINT "r_buildlogparseresult_buildinvolvement_pkey" PRIMARY KEY (id),
    CONSTRAINT "r_buildlogparseresult_buildinvolvement_compoundkey" UNIQUE (buildlogparseresultid, buildinvolvementid),
    CONSTRAINT "r_buildlogparseresult_buildinvolvement_buildlogparseresultid_fk" FOREIGN KEY (buildlogparseresultid)
        REFERENCES public."buildlogparseresult" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "r_buildlogparseresult_buildinvolvement_buildinvolvementid_fk" FOREIGN KEY (buildinvolvementid)
        REFERENCES public."buildinvolvement" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
ALTER TABLE public."r_buildlogparseresult_buildinvolvement" OWNER TO postgres;


-- CREATE DAEMONTASK
CREATE TABLE public."daemontask"
(
    id integer NOT NULL DEFAULT nextval('"daemontask_id_seq"'::regclass),
    signature character varying(38) COLLATE pg_catalog."default" NOT NULL,
    buildid integer NOT NULL,
    buildinvolvementid integer,
    stage integer NOT NULL,
    faildaemontaskid integer,
    src character varying(256) COLLATE pg_catalog."default",
    createdutc timestamp(4) without time zone NOT NULL,
    processedutc timestamp(4) without time zone,
    passed boolean,
    args text COLLATE pg_catalog."default",
    result text COLLATE pg_catalog."default",
    CONSTRAINT "daemontask_pkey" PRIMARY KEY (id),
    CONSTRAINT "daemontask_compoundkey" UNIQUE (buildid, buildinvolvementid, stage),
    CONSTRAINT "daemontask_faildaemontaskid_fk" FOREIGN KEY (faildaemontaskid)
        REFERENCES public."daemontask" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "daemontask_buildid_fk" FOREIGN KEY (buildid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "daemontask_buildinvolvementid_fk" FOREIGN KEY (buildinvolvementid)
        REFERENCES public."buildinvolvement" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (OIDS = FALSE)
TABLESPACE pg_default;
ALTER TABLE public."daemontask" OWNER TO postgres;


-- CREATE MUTATIONREPORT
CREATE TABLE public."mutationreport"
(
    id integer NOT NULL DEFAULT nextval('"mutationreport_id_seq"'::regclass),
    signature character varying(38) COLLATE pg_catalog."default" NOT NULL,
    incidentid integer NOT NULL,
    buildid integer NOT NULL,
    mutationid integer NOT NULL,
    status character varying(64) COLLATE pg_catalog."default" NOT NULL,
    summary character varying(256) COLLATE pg_catalog."default" NOT NULL,
    mutationhash text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    implicatedrevisions text COLLATE pg_catalog."default",
    processor character varying(256) COLLATE pg_catalog."default"  NOT NULL,
    createdutc timestamp(4) without time zone NOT NULL,
    CONSTRAINT "mutationreport_buildid_unique" UNIQUE (buildid),
    CONSTRAINT "mutationreport_buildid_fk" FOREIGN KEY (buildid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "mutationreport_incidentid_fk" FOREIGN KEY (incidentid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "mutationreport_mutationid_fk" FOREIGN KEY (mutationid)
        REFERENCES public."build" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (OIDS = FALSE)
TABLESPACE pg_default;
ALTER TABLE public."mutationreport" OWNER TO postgres;


-- CREATE INDEXES
CREATE INDEX "session_userid_fk"
    ON public."session" USING btree
    (userid)
    TABLESPACE pg_default;

CREATE INDEX "build_jobid_fk"
    ON public."build" USING btree
    (jobid)
    TABLESPACE pg_default;

CREATE INDEX "build_incidentbuildid_fk"
    ON public."build" USING btree
    (incidentbuildid)
    TABLESPACE pg_default;

CREATE INDEX "job_sourceserverid_fk"
    ON public."job" USING btree
    (sourceserverid)
    TABLESPACE pg_default;

CREATE INDEX "job_buildserverid_fk"
    ON public."job" USING btree
    (buildserverid)
    TABLESPACE pg_default;

CREATE INDEX "buildinvolvement_buildid_fk"
    ON public."buildinvolvement" USING btree
    (buildid)
    TABLESPACE pg_default;

CREATE INDEX "buildinvolvement_revisionid_fk"
    ON public."buildinvolvement" USING btree
    (revisionid)
    TABLESPACE pg_default;

CREATE INDEX "buildlogparseresult_buildid_fk"
    ON public."buildlogparseresult" USING btree
    (buildid)
    TABLESPACE pg_default;

CREATE INDEX "r_buildlogparseresult_buildinvolvement_buildlogparseresultid_fk"
    ON public."r_buildlogparseresult_buildinvolvement" USING btree
    (buildlogparseresultid)
    TABLESPACE pg_default;

CREATE INDEX "r_buildlogparseresult_buildinvolvement_buildinvolvementid_fk"
    ON public."r_buildlogparseresult_buildinvolvement" USING btree
    (buildinvolvementid)
    TABLESPACE pg_default;

CREATE INDEX "daemontask_buildid_fk"
    ON public."daemontask" USING btree
    (buildid)
    TABLESPACE pg_default;

CREATE INDEX "daemontask_faildaemontaskid_fk"
    ON public."daemontask" USING btree
    (faildaemontaskid)
    TABLESPACE pg_default;

CREATE INDEX "daemontask_buildinvolvementid_fk"
    ON public."daemontask" USING btree
    (buildinvolvementid)
    TABLESPACE pg_default;

CREATE INDEX "mutationreport_buildid_fk"
    ON public."mutationreport" USING btree
    (buildid)
    TABLESPACE pg_default;

CREATE INDEX "mutationreport_incidentid_fk"
    ON public."mutationreport" USING btree
    (incidentid)
    TABLESPACE pg_default;

CREATE INDEX "mutationreport_mutationid_fk"
    ON public."mutationreport" USING btree
    (mutationid)
    TABLESPACE pg_default;
