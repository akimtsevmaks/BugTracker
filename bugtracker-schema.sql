--
-- PostgreSQL database dump
--

\restrict yNBgBAfhgzUtts2liWKvrrSem8avsuaRroiJuZUJ4Cy0Mj1eT0OoaB6sD9Gu7g4

-- Dumped from database version 18.7
-- Dumped by pg_dump version 18.7

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: bugtracker; Type: SCHEMA; Schema: -; Owner: bugtracker_owner
--

CREATE SCHEMA bugtracker;


ALTER SCHEMA bugtracker OWNER TO bugtracker_owner;

--
-- Name: attempt_result; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.attempt_result AS ENUM (
    'accepted',
    'returned'
);


ALTER TYPE bugtracker.attempt_result OWNER TO bugtracker_owner;

--
-- Name: bug_priority; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.bug_priority AS ENUM (
    'low',
    'medium',
    'high'
);


ALTER TYPE bugtracker.bug_priority OWNER TO bugtracker_owner;

--
-- Name: bug_state; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.bug_state AS ENUM (
    'waiting',
    'in_work',
    'on_review',
    'closed'
);


ALTER TYPE bugtracker.bug_state OWNER TO bugtracker_owner;

--
-- Name: channel_type; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.channel_type AS ENUM (
    'vk',
    'telegram',
    'gmail',
    'max'
);


ALTER TYPE bugtracker.channel_type OWNER TO bugtracker_owner;

--
-- Name: delivery_status; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.delivery_status AS ENUM (
    'pending',
    'sending',
    'sent',
    'failed'
);


ALTER TYPE bugtracker.delivery_status OWNER TO bugtracker_owner;

--
-- Name: history_property; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.history_property AS ENUM (
    'state',
    'assignee',
    'priority',
    'deadline',
    'release_url'
);


ALTER TYPE bugtracker.history_property OWNER TO bugtracker_owner;

--
-- Name: nonempty; Type: DOMAIN; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE DOMAIN bugtracker.nonempty AS text
	CONSTRAINT nonempty_check CHECK ((VALUE ~ '[^[:space:]]'::text));


ALTER DOMAIN bugtracker.nonempty OWNER TO bugtracker_owner;

--
-- Name: report_state; Type: TYPE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TYPE bugtracker.report_state AS ENUM (
    'not_processed',
    'in_processing',
    'processed'
);


ALTER TYPE bugtracker.report_state OWNER TO bugtracker_owner;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: admin_accounts; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.admin_accounts (
    id uuid DEFAULT uuidv7() NOT NULL,
    login bugtracker.nonempty NOT NULL,
    normalized_login text GENERATED ALWAYS AS (lower(btrim((login)::text))) STORED NOT NULL,
    display_name bugtracker.nonempty NOT NULL,
    password_hash bugtracker.nonempty NOT NULL
);


ALTER TABLE bugtracker.admin_accounts OWNER TO bugtracker_owner;

--
-- Name: admin_channels; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.admin_channels (
    id uuid DEFAULT uuidv7() NOT NULL,
    admin_id uuid NOT NULL,
    channel bugtracker.channel_type NOT NULL,
    destination text NOT NULL,
    enabled boolean NOT NULL,
    CONSTRAINT admin_channels_check CHECK (((NOT enabled) OR (destination ~ '[^[:space:]]'::text)))
);


ALTER TABLE bugtracker.admin_channels OWNER TO bugtracker_owner;

--
-- Name: attachments; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.attachments (
    id uuid DEFAULT uuidv7() NOT NULL,
    bug_id uuid,
    report_id uuid,
    comment_id uuid,
    file_name bugtracker.nonempty NOT NULL,
    content_type bugtracker.nonempty NOT NULL,
    bytes bytea NOT NULL,
    CONSTRAINT attachments_bytes_check CHECK ((octet_length(bytes) <= 3145728)),
    CONSTRAINT attachments_check CHECK ((num_nonnulls(bug_id, report_id, comment_id) = 1))
);


ALTER TABLE bugtracker.attachments OWNER TO bugtracker_owner;

--
-- Name: attempts; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.attempts (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    bug_id uuid NOT NULL,
    submitted_by_id uuid NOT NULL,
    links bugtracker.nonempty[] NOT NULL,
    reviewer_id uuid,
    result bugtracker.attempt_result,
    reason bugtracker.nonempty,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    reviewed_at timestamp with time zone,
    CONSTRAINT attempts_check CHECK ((((result IS NULL) AND (reviewer_id IS NULL) AND (reviewed_at IS NULL) AND (reason IS NULL)) OR ((result IS NOT NULL) AND (reviewer_id IS NOT NULL) AND (reviewed_at IS NOT NULL) AND (((result = 'accepted'::bugtracker.attempt_result) AND (reason IS NULL)) OR ((result = 'returned'::bugtracker.attempt_result) AND (reason IS NOT NULL)))))),
    CONSTRAINT attempts_check1 CHECK (((reviewed_at IS NULL) OR (reviewed_at >= created_at))),
    CONSTRAINT attempts_links_check CHECK ((((cardinality(links) >= 1) AND (cardinality(links) <= 20)) AND (array_ndims(links) = 1) AND (array_position(links, (NULL::text)::bugtracker.nonempty) IS NULL)))
);


ALTER TABLE bugtracker.attempts OWNER TO bugtracker_owner;

--
-- Name: bugs; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.bugs (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    author_id uuid NOT NULL,
    source_report_id uuid,
    assignee_id uuid,
    title bugtracker.nonempty NOT NULL,
    description bugtracker.nonempty NOT NULL,
    reproduction_steps bugtracker.nonempty NOT NULL,
    priority bugtracker.bug_priority NOT NULL,
    state bugtracker.bug_state NOT NULL,
    deadline date,
    release_url bugtracker.nonempty,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    version integer DEFAULT 1 NOT NULL,
    CONSTRAINT bugs_check CHECK ((((state = 'waiting'::bugtracker.bug_state) AND (assignee_id IS NULL) AND (release_url IS NULL)) OR ((state = ANY (ARRAY['in_work'::bugtracker.bug_state, 'on_review'::bugtracker.bug_state])) AND (assignee_id IS NOT NULL) AND (release_url IS NULL)) OR ((state = 'closed'::bugtracker.bug_state) AND (assignee_id IS NOT NULL) AND (release_url IS NOT NULL)))),
    CONSTRAINT bugs_version_check CHECK ((version > 0))
);


ALTER TABLE bugtracker.bugs OWNER TO bugtracker_owner;

--
-- Name: channels; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.channels (
    id uuid DEFAULT uuidv7() NOT NULL,
    user_id uuid NOT NULL,
    channel bugtracker.channel_type NOT NULL,
    destination text NOT NULL,
    enabled boolean NOT NULL,
    CONSTRAINT channels_check CHECK (((NOT enabled) OR (destination ~ '[^[:space:]]'::text)))
);


ALTER TABLE bugtracker.channels OWNER TO bugtracker_owner;

--
-- Name: comments; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.comments (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    bug_id uuid NOT NULL,
    author_id uuid NOT NULL,
    text bugtracker.nonempty NOT NULL,
    is_pinned boolean NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE bugtracker.comments OWNER TO bugtracker_owner;

--
-- Name: deliveries; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.deliveries (
    id uuid DEFAULT uuidv7() NOT NULL,
    notification_id uuid NOT NULL,
    channel bugtracker.channel_type NOT NULL,
    destination bugtracker.nonempty NOT NULL,
    status bugtracker.delivery_status NOT NULL,
    attempts integer NOT NULL,
    next_try_at timestamp with time zone NOT NULL,
    last_error text,
    CONSTRAINT deliveries_attempts_check CHECK ((attempts >= 0))
);


ALTER TABLE bugtracker.deliveries OWNER TO bugtracker_owner;

--
-- Name: history; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.history (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    bug_id uuid NOT NULL,
    actor_id uuid NOT NULL,
    operation_id uuid NOT NULL,
    property bugtracker.history_property NOT NULL,
    old_value text,
    new_value text,
    reason text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT history_check CHECK ((old_value IS DISTINCT FROM new_value)),
    CONSTRAINT history_check1 CHECK (((property <> ALL (ARRAY['priority'::bugtracker.history_property, 'deadline'::bugtracker.history_property])) OR ((reason IS NOT NULL) AND (reason ~ '[^[:space:]]'::text))))
);


ALTER TABLE bugtracker.history OWNER TO bugtracker_owner;

--
-- Name: members; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.members (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    user_id uuid NOT NULL,
    roles integer NOT NULL,
    is_active boolean NOT NULL,
    CONSTRAINT members_check CHECK (((NOT is_active) OR (roles > 0))),
    CONSTRAINT members_roles_check CHECK (((roles >= 0) AND (roles <= 7)))
);


ALTER TABLE bugtracker.members OWNER TO bugtracker_owner;

--
-- Name: notifications; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.notifications (
    id uuid DEFAULT uuidv7() NOT NULL,
    user_id uuid NOT NULL,
    bug_id uuid NOT NULL,
    text bugtracker.nonempty NOT NULL,
    is_read boolean NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE bugtracker.notifications OWNER TO bugtracker_owner;

--
-- Name: projects; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.projects (
    id uuid DEFAULT uuidv7() NOT NULL,
    name bugtracker.nonempty NOT NULL,
    description text NOT NULL,
    repository_url text NOT NULL,
    is_archived boolean NOT NULL
);


ALTER TABLE bugtracker.projects OWNER TO bugtracker_owner;

--
-- Name: report_sources; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.report_sources (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    name bugtracker.nonempty NOT NULL,
    key_hash text,
    normalized_name text GENERATED ALWAYS AS (lower(btrim((name)::text))) STORED
);


ALTER TABLE bugtracker.report_sources OWNER TO bugtracker_owner;

--
-- Name: reports; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.reports (
    id uuid DEFAULT uuidv7() NOT NULL,
    project_id uuid NOT NULL,
    sender_id uuid,
    external_source_id uuid,
    external_id bugtracker.nonempty,
    content text NOT NULL,
    state bugtracker.report_state NOT NULL,
    handler_id uuid,
    explanation bugtracker.nonempty,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    version integer DEFAULT 1 NOT NULL,
    CONSTRAINT reports_check CHECK ((((sender_id IS NOT NULL) AND (external_source_id IS NULL) AND (external_id IS NULL)) OR ((sender_id IS NULL) AND (external_source_id IS NOT NULL) AND (external_id IS NOT NULL)))),
    CONSTRAINT reports_check1 CHECK ((((state = 'not_processed'::bugtracker.report_state) AND (handler_id IS NULL) AND (explanation IS NULL)) OR ((state = 'in_processing'::bugtracker.report_state) AND (handler_id IS NOT NULL) AND (explanation IS NULL)) OR ((state = 'processed'::bugtracker.report_state) AND (handler_id IS NOT NULL)))),
    CONSTRAINT reports_version_check CHECK ((version > 0))
);


ALTER TABLE bugtracker.reports OWNER TO bugtracker_owner;

--
-- Name: users; Type: TABLE; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE TABLE bugtracker.users (
    id uuid DEFAULT uuidv7() NOT NULL,
    login bugtracker.nonempty NOT NULL,
    normalized_login text GENERATED ALWAYS AS (lower(btrim((login)::text))) STORED NOT NULL,
    display_name bugtracker.nonempty NOT NULL,
    password_hash bugtracker.nonempty NOT NULL
);


ALTER TABLE bugtracker.users OWNER TO bugtracker_owner;

--
-- Name: admin_accounts admin_accounts_normalized_login_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.admin_accounts
    ADD CONSTRAINT admin_accounts_normalized_login_key UNIQUE (normalized_login);


--
-- Name: admin_accounts admin_accounts_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.admin_accounts
    ADD CONSTRAINT admin_accounts_pkey PRIMARY KEY (id);


--
-- Name: admin_channels admin_channels_admin_id_channel_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.admin_channels
    ADD CONSTRAINT admin_channels_admin_id_channel_key UNIQUE (admin_id, channel);


--
-- Name: admin_channels admin_channels_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.admin_channels
    ADD CONSTRAINT admin_channels_pkey PRIMARY KEY (id);


--
-- Name: attachments attachments_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attachments
    ADD CONSTRAINT attachments_pkey PRIMARY KEY (id);


--
-- Name: attempts attempts_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attempts
    ADD CONSTRAINT attempts_pkey PRIMARY KEY (id);


--
-- Name: bugs bugs_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_pkey PRIMARY KEY (id);


--
-- Name: bugs bugs_project_id_id_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_project_id_id_key UNIQUE (project_id, id);


--
-- Name: bugs bugs_source_report_id_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_source_report_id_key UNIQUE (source_report_id);


--
-- Name: channels channels_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.channels
    ADD CONSTRAINT channels_pkey PRIMARY KEY (id);


--
-- Name: channels channels_user_id_channel_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.channels
    ADD CONSTRAINT channels_user_id_channel_key UNIQUE (user_id, channel);


--
-- Name: comments comments_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.comments
    ADD CONSTRAINT comments_pkey PRIMARY KEY (id);


--
-- Name: deliveries deliveries_notification_id_channel_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.deliveries
    ADD CONSTRAINT deliveries_notification_id_channel_key UNIQUE (notification_id, channel);


--
-- Name: deliveries deliveries_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.deliveries
    ADD CONSTRAINT deliveries_pkey PRIMARY KEY (id);


--
-- Name: history history_bug_id_operation_id_property_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.history
    ADD CONSTRAINT history_bug_id_operation_id_property_key UNIQUE (bug_id, operation_id, property);


--
-- Name: history history_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.history
    ADD CONSTRAINT history_pkey PRIMARY KEY (id);


--
-- Name: members members_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.members
    ADD CONSTRAINT members_pkey PRIMARY KEY (id);


--
-- Name: members members_project_id_id_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.members
    ADD CONSTRAINT members_project_id_id_key UNIQUE (project_id, id);


--
-- Name: members members_project_id_user_id_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.members
    ADD CONSTRAINT members_project_id_user_id_key UNIQUE (project_id, user_id);


--
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (id);


--
-- Name: projects projects_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.projects
    ADD CONSTRAINT projects_pkey PRIMARY KEY (id);


--
-- Name: report_sources report_sources_key_hash_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.report_sources
    ADD CONSTRAINT report_sources_key_hash_key UNIQUE (key_hash);


--
-- Name: report_sources report_sources_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.report_sources
    ADD CONSTRAINT report_sources_pkey PRIMARY KEY (id);


--
-- Name: report_sources report_sources_project_id_id_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.report_sources
    ADD CONSTRAINT report_sources_project_id_id_key UNIQUE (project_id, id);


--
-- Name: report_sources report_sources_project_id_normalized_name_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.report_sources
    ADD CONSTRAINT report_sources_project_id_normalized_name_key UNIQUE (project_id, normalized_name);


--
-- Name: reports reports_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.reports
    ADD CONSTRAINT reports_pkey PRIMARY KEY (id);


--
-- Name: reports reports_project_id_id_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.reports
    ADD CONSTRAINT reports_project_id_id_key UNIQUE (project_id, id);


--
-- Name: users users_normalized_login_key; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.users
    ADD CONSTRAINT users_normalized_login_key UNIQUE (normalized_login);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: admin_accounts_singleton_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE UNIQUE INDEX admin_accounts_singleton_idx ON bugtracker.admin_accounts USING btree ((1));


--
-- Name: attachments_bug_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attachments_bug_id_idx ON bugtracker.attachments USING btree (bug_id);


--
-- Name: attachments_comment_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attachments_comment_id_idx ON bugtracker.attachments USING btree (comment_id);


--
-- Name: attachments_report_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attachments_report_id_idx ON bugtracker.attachments USING btree (report_id);


--
-- Name: attempts_bug_id_created_at_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attempts_bug_id_created_at_idx ON bugtracker.attempts USING btree (bug_id, created_at);


--
-- Name: attempts_bug_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE UNIQUE INDEX attempts_bug_id_idx ON bugtracker.attempts USING btree (bug_id) WHERE (result IS NULL);


--
-- Name: attempts_project_id_bug_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attempts_project_id_bug_id_idx ON bugtracker.attempts USING btree (project_id, bug_id);


--
-- Name: attempts_project_id_reviewer_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attempts_project_id_reviewer_id_idx ON bugtracker.attempts USING btree (project_id, reviewer_id);


--
-- Name: attempts_project_id_submitted_by_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX attempts_project_id_submitted_by_id_idx ON bugtracker.attempts USING btree (project_id, submitted_by_id);


--
-- Name: bugs_project_id_assignee_id_state_deadline_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX bugs_project_id_assignee_id_state_deadline_idx ON bugtracker.bugs USING btree (project_id, assignee_id, state, deadline);


--
-- Name: bugs_project_id_author_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX bugs_project_id_author_id_idx ON bugtracker.bugs USING btree (project_id, author_id);


--
-- Name: bugs_project_id_source_report_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX bugs_project_id_source_report_id_idx ON bugtracker.bugs USING btree (project_id, source_report_id);


--
-- Name: bugs_project_id_state_created_at_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX bugs_project_id_state_created_at_idx ON bugtracker.bugs USING btree (project_id, state, created_at);


--
-- Name: comments_bug_id_created_at_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX comments_bug_id_created_at_idx ON bugtracker.comments USING btree (bug_id, created_at);


--
-- Name: comments_project_id_author_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX comments_project_id_author_id_idx ON bugtracker.comments USING btree (project_id, author_id);


--
-- Name: comments_project_id_bug_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX comments_project_id_bug_id_idx ON bugtracker.comments USING btree (project_id, bug_id);


--
-- Name: deliveries_next_try_at_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX deliveries_next_try_at_id_idx ON bugtracker.deliveries USING btree (next_try_at, id) WHERE (status = ANY (ARRAY['pending'::bugtracker.delivery_status, 'sending'::bugtracker.delivery_status]));


--
-- Name: history_bug_id_created_at_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX history_bug_id_created_at_id_idx ON bugtracker.history USING btree (bug_id, created_at, id);


--
-- Name: history_project_id_actor_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX history_project_id_actor_id_idx ON bugtracker.history USING btree (project_id, actor_id);


--
-- Name: history_project_id_bug_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX history_project_id_bug_id_idx ON bugtracker.history USING btree (project_id, bug_id);


--
-- Name: members_user_id_is_active_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX members_user_id_is_active_idx ON bugtracker.members USING btree (user_id, is_active);


--
-- Name: notifications_bug_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX notifications_bug_id_idx ON bugtracker.notifications USING btree (bug_id);


--
-- Name: notifications_user_id_created_at_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX notifications_user_id_created_at_id_idx ON bugtracker.notifications USING btree (user_id, created_at, id);


--
-- Name: reports_external_source_id_external_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE UNIQUE INDEX reports_external_source_id_external_id_idx ON bugtracker.reports USING btree (external_source_id, external_id) WHERE (external_source_id IS NOT NULL);


--
-- Name: reports_project_id_external_source_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX reports_project_id_external_source_id_idx ON bugtracker.reports USING btree (project_id, external_source_id);


--
-- Name: reports_project_id_handler_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX reports_project_id_handler_id_idx ON bugtracker.reports USING btree (project_id, handler_id);


--
-- Name: reports_project_id_sender_id_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX reports_project_id_sender_id_idx ON bugtracker.reports USING btree (project_id, sender_id);


--
-- Name: reports_project_id_state_created_at_idx; Type: INDEX; Schema: bugtracker; Owner: bugtracker_owner
--

CREATE INDEX reports_project_id_state_created_at_idx ON bugtracker.reports USING btree (project_id, state, created_at);


--
-- Name: admin_channels admin_channels_admin_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.admin_channels
    ADD CONSTRAINT admin_channels_admin_id_fkey FOREIGN KEY (admin_id) REFERENCES bugtracker.admin_accounts(id) ON DELETE RESTRICT;


--
-- Name: attachments attachments_bug_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attachments
    ADD CONSTRAINT attachments_bug_id_fkey FOREIGN KEY (bug_id) REFERENCES bugtracker.bugs(id) ON DELETE RESTRICT;


--
-- Name: attachments attachments_comment_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attachments
    ADD CONSTRAINT attachments_comment_id_fkey FOREIGN KEY (comment_id) REFERENCES bugtracker.comments(id) ON DELETE RESTRICT;


--
-- Name: attachments attachments_report_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attachments
    ADD CONSTRAINT attachments_report_id_fkey FOREIGN KEY (report_id) REFERENCES bugtracker.reports(id) ON DELETE RESTRICT;


--
-- Name: attempts attempts_project_id_bug_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attempts
    ADD CONSTRAINT attempts_project_id_bug_id_fkey FOREIGN KEY (project_id, bug_id) REFERENCES bugtracker.bugs(project_id, id) ON DELETE RESTRICT;


--
-- Name: attempts attempts_project_id_reviewer_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attempts
    ADD CONSTRAINT attempts_project_id_reviewer_id_fkey FOREIGN KEY (project_id, reviewer_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: attempts attempts_project_id_submitted_by_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.attempts
    ADD CONSTRAINT attempts_project_id_submitted_by_id_fkey FOREIGN KEY (project_id, submitted_by_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: bugs bugs_project_id_assignee_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_project_id_assignee_id_fkey FOREIGN KEY (project_id, assignee_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: bugs bugs_project_id_author_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_project_id_author_id_fkey FOREIGN KEY (project_id, author_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: bugs bugs_project_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_project_id_fkey FOREIGN KEY (project_id) REFERENCES bugtracker.projects(id) ON DELETE RESTRICT;


--
-- Name: bugs bugs_project_id_source_report_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.bugs
    ADD CONSTRAINT bugs_project_id_source_report_id_fkey FOREIGN KEY (project_id, source_report_id) REFERENCES bugtracker.reports(project_id, id) ON DELETE RESTRICT;


--
-- Name: channels channels_user_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.channels
    ADD CONSTRAINT channels_user_id_fkey FOREIGN KEY (user_id) REFERENCES bugtracker.users(id) ON DELETE RESTRICT;


--
-- Name: comments comments_project_id_author_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.comments
    ADD CONSTRAINT comments_project_id_author_id_fkey FOREIGN KEY (project_id, author_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: comments comments_project_id_bug_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.comments
    ADD CONSTRAINT comments_project_id_bug_id_fkey FOREIGN KEY (project_id, bug_id) REFERENCES bugtracker.bugs(project_id, id) ON DELETE RESTRICT;


--
-- Name: deliveries deliveries_notification_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.deliveries
    ADD CONSTRAINT deliveries_notification_id_fkey FOREIGN KEY (notification_id) REFERENCES bugtracker.notifications(id) ON DELETE RESTRICT;


--
-- Name: history history_project_id_actor_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.history
    ADD CONSTRAINT history_project_id_actor_id_fkey FOREIGN KEY (project_id, actor_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: history history_project_id_bug_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.history
    ADD CONSTRAINT history_project_id_bug_id_fkey FOREIGN KEY (project_id, bug_id) REFERENCES bugtracker.bugs(project_id, id) ON DELETE RESTRICT;


--
-- Name: members members_project_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.members
    ADD CONSTRAINT members_project_id_fkey FOREIGN KEY (project_id) REFERENCES bugtracker.projects(id) ON DELETE RESTRICT;


--
-- Name: members members_user_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.members
    ADD CONSTRAINT members_user_id_fkey FOREIGN KEY (user_id) REFERENCES bugtracker.users(id) ON DELETE RESTRICT;


--
-- Name: notifications notifications_bug_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.notifications
    ADD CONSTRAINT notifications_bug_id_fkey FOREIGN KEY (bug_id) REFERENCES bugtracker.bugs(id) ON DELETE RESTRICT;


--
-- Name: notifications notifications_user_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.notifications
    ADD CONSTRAINT notifications_user_id_fkey FOREIGN KEY (user_id) REFERENCES bugtracker.users(id) ON DELETE RESTRICT;


--
-- Name: report_sources report_sources_project_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.report_sources
    ADD CONSTRAINT report_sources_project_id_fkey FOREIGN KEY (project_id) REFERENCES bugtracker.projects(id) ON DELETE RESTRICT;


--
-- Name: reports reports_project_id_external_source_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.reports
    ADD CONSTRAINT reports_project_id_external_source_id_fkey FOREIGN KEY (project_id, external_source_id) REFERENCES bugtracker.report_sources(project_id, id) ON DELETE RESTRICT;


--
-- Name: reports reports_project_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.reports
    ADD CONSTRAINT reports_project_id_fkey FOREIGN KEY (project_id) REFERENCES bugtracker.projects(id) ON DELETE RESTRICT;


--
-- Name: reports reports_project_id_handler_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.reports
    ADD CONSTRAINT reports_project_id_handler_id_fkey FOREIGN KEY (project_id, handler_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: reports reports_project_id_sender_id_fkey; Type: FK CONSTRAINT; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER TABLE ONLY bugtracker.reports
    ADD CONSTRAINT reports_project_id_sender_id_fkey FOREIGN KEY (project_id, sender_id) REFERENCES bugtracker.members(project_id, id) ON DELETE RESTRICT;


--
-- Name: SCHEMA bugtracker; Type: ACL; Schema: -; Owner: bugtracker_owner
--

GRANT USAGE ON SCHEMA bugtracker TO bugtracker_service;


--
-- Name: TABLE admin_accounts; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.admin_accounts TO bugtracker_service;


--
-- Name: TABLE admin_channels; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.admin_channels TO bugtracker_service;


--
-- Name: COLUMN admin_channels.admin_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(admin_id) ON TABLE bugtracker.admin_channels TO bugtracker_service;


--
-- Name: COLUMN admin_channels.channel; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(channel) ON TABLE bugtracker.admin_channels TO bugtracker_service;


--
-- Name: COLUMN admin_channels.destination; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(destination),UPDATE(destination) ON TABLE bugtracker.admin_channels TO bugtracker_service;


--
-- Name: COLUMN admin_channels.enabled; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(enabled),UPDATE(enabled) ON TABLE bugtracker.admin_channels TO bugtracker_service;


--
-- Name: TABLE attachments; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: COLUMN attachments.bug_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(bug_id) ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: COLUMN attachments.report_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(report_id) ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: COLUMN attachments.comment_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(comment_id) ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: COLUMN attachments.file_name; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(file_name) ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: COLUMN attachments.content_type; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(content_type) ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: COLUMN attachments.bytes; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(bytes) ON TABLE bugtracker.attachments TO bugtracker_service;


--
-- Name: TABLE attempts; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.bug_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(bug_id) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.submitted_by_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(submitted_by_id) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.links; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(links) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.reviewer_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(reviewer_id),UPDATE(reviewer_id) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.result; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(result),UPDATE(result) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.reason; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(reason),UPDATE(reason) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.created_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(created_at) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: COLUMN attempts.reviewed_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(reviewed_at),UPDATE(reviewed_at) ON TABLE bugtracker.attempts TO bugtracker_service;


--
-- Name: TABLE bugs; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.author_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(author_id) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.source_report_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(source_report_id) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.assignee_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(assignee_id),UPDATE(assignee_id) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.title; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(title) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.description; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(description) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.reproduction_steps; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(reproduction_steps) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.priority; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(priority),UPDATE(priority) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.state; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(state),UPDATE(state) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.deadline; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(deadline),UPDATE(deadline) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.release_url; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(release_url),UPDATE(release_url) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.created_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(created_at) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: COLUMN bugs.version; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(version),UPDATE(version) ON TABLE bugtracker.bugs TO bugtracker_service;


--
-- Name: TABLE channels; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.channels TO bugtracker_service;


--
-- Name: COLUMN channels.user_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(user_id) ON TABLE bugtracker.channels TO bugtracker_service;


--
-- Name: COLUMN channels.channel; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(channel) ON TABLE bugtracker.channels TO bugtracker_service;


--
-- Name: COLUMN channels.destination; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(destination),UPDATE(destination) ON TABLE bugtracker.channels TO bugtracker_service;


--
-- Name: COLUMN channels.enabled; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(enabled),UPDATE(enabled) ON TABLE bugtracker.channels TO bugtracker_service;


--
-- Name: TABLE comments; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: COLUMN comments.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: COLUMN comments.bug_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(bug_id) ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: COLUMN comments.author_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(author_id) ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: COLUMN comments.text; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(text) ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: COLUMN comments.is_pinned; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(is_pinned),UPDATE(is_pinned) ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: COLUMN comments.created_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(created_at) ON TABLE bugtracker.comments TO bugtracker_service;


--
-- Name: TABLE deliveries; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.notification_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(notification_id) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.channel; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(channel) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.destination; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(destination) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.status; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(status),UPDATE(status) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.attempts; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(attempts),UPDATE(attempts) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.next_try_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(next_try_at),UPDATE(next_try_at) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: COLUMN deliveries.last_error; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(last_error),UPDATE(last_error) ON TABLE bugtracker.deliveries TO bugtracker_service;


--
-- Name: TABLE history; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.bug_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(bug_id) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.actor_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(actor_id) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.operation_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(operation_id) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.property; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(property) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.old_value; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(old_value) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.new_value; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(new_value) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.reason; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(reason) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: COLUMN history.created_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(created_at) ON TABLE bugtracker.history TO bugtracker_service;


--
-- Name: TABLE members; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.members TO bugtracker_service;


--
-- Name: COLUMN members.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.members TO bugtracker_service;


--
-- Name: COLUMN members.user_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(user_id) ON TABLE bugtracker.members TO bugtracker_service;


--
-- Name: COLUMN members.roles; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(roles),UPDATE(roles) ON TABLE bugtracker.members TO bugtracker_service;


--
-- Name: COLUMN members.is_active; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(is_active),UPDATE(is_active) ON TABLE bugtracker.members TO bugtracker_service;


--
-- Name: TABLE notifications; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.notifications TO bugtracker_service;


--
-- Name: COLUMN notifications.user_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(user_id) ON TABLE bugtracker.notifications TO bugtracker_service;


--
-- Name: COLUMN notifications.bug_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(bug_id) ON TABLE bugtracker.notifications TO bugtracker_service;


--
-- Name: COLUMN notifications.text; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(text) ON TABLE bugtracker.notifications TO bugtracker_service;


--
-- Name: COLUMN notifications.is_read; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(is_read),UPDATE(is_read) ON TABLE bugtracker.notifications TO bugtracker_service;


--
-- Name: COLUMN notifications.created_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(created_at) ON TABLE bugtracker.notifications TO bugtracker_service;


--
-- Name: TABLE projects; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.projects TO bugtracker_service;


--
-- Name: COLUMN projects.name; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(name),UPDATE(name) ON TABLE bugtracker.projects TO bugtracker_service;


--
-- Name: COLUMN projects.description; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(description),UPDATE(description) ON TABLE bugtracker.projects TO bugtracker_service;


--
-- Name: COLUMN projects.repository_url; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(repository_url),UPDATE(repository_url) ON TABLE bugtracker.projects TO bugtracker_service;


--
-- Name: COLUMN projects.is_archived; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(is_archived),UPDATE(is_archived) ON TABLE bugtracker.projects TO bugtracker_service;


--
-- Name: TABLE report_sources; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.report_sources TO bugtracker_service;


--
-- Name: COLUMN report_sources.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.report_sources TO bugtracker_service;


--
-- Name: COLUMN report_sources.name; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(name) ON TABLE bugtracker.report_sources TO bugtracker_service;


--
-- Name: COLUMN report_sources.key_hash; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(key_hash),UPDATE(key_hash) ON TABLE bugtracker.report_sources TO bugtracker_service;


--
-- Name: TABLE reports; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.project_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(project_id) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.sender_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(sender_id) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.external_source_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(external_source_id) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.external_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(external_id) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.content; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(content) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.state; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(state),UPDATE(state) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.handler_id; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(handler_id),UPDATE(handler_id) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.explanation; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(explanation),UPDATE(explanation) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.created_at; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(created_at) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: COLUMN reports.version; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(version),UPDATE(version) ON TABLE bugtracker.reports TO bugtracker_service;


--
-- Name: TABLE users; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT SELECT ON TABLE bugtracker.users TO bugtracker_service;


--
-- Name: COLUMN users.login; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(login) ON TABLE bugtracker.users TO bugtracker_service;


--
-- Name: COLUMN users.display_name; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(display_name) ON TABLE bugtracker.users TO bugtracker_service;


--
-- Name: COLUMN users.password_hash; Type: ACL; Schema: bugtracker; Owner: bugtracker_owner
--

GRANT INSERT(password_hash),UPDATE(password_hash) ON TABLE bugtracker.users TO bugtracker_service;


--
-- Name: DEFAULT PRIVILEGES FOR TABLES; Type: DEFAULT ACL; Schema: bugtracker; Owner: bugtracker_owner
--

ALTER DEFAULT PRIVILEGES FOR ROLE bugtracker_owner IN SCHEMA bugtracker GRANT SELECT ON TABLES TO bugtracker_service;


--
-- PostgreSQL database dump complete
--

\unrestrict yNBgBAfhgzUtts2liWKvrrSem8avsuaRroiJuZUJ4Cy0Mj1eT0OoaB6sD9Gu7g4

