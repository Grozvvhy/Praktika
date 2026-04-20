--
-- PostgreSQL database dump
--

\restrict h53cyJC3UQaghkcCy1hsAtHwTTUSIi2HMUAjHe2r1t5spTrGK9kIBtC6TAvysc8

-- Dumped from database version 18.3
-- Dumped by pg_dump version 18.3

-- Started on 2026-04-20 14:18:26

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
-- TOC entry 235 (class 1259 OID 17175)
-- Name: viewlistrequests; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.viewlistrequests AS
SELECT
    NULL::integer AS request_id,
    NULL::character varying(20) AS type,
    NULL::date AS start_date,
    NULL::date AS end_date,
    NULL::text AS purpose,
    NULL::timestamp without time zone AS created_at,
    NULL::character varying(100) AS department_name,
    NULL::character varying(150) AS employee_full_name,
    NULL::character varying(50) AS status_name,
    NULL::character varying(255) AS user_email,
    NULL::text AS visitors_list;


ALTER VIEW public.viewlistrequests OWNER TO postgres;

--
-- TOC entry 242 (class 1255 OID 17180)
-- Name: filteringrequests(character varying, integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.filteringrequests(p_type character varying DEFAULT NULL::character varying, p_department_id integer DEFAULT NULL::integer, p_status_id integer DEFAULT NULL::integer) RETURNS SETOF public.viewlistrequests
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM public.ViewListRequests v
    WHERE (p_type IS NULL OR v.type = p_type)
      AND (p_department_id IS NULL OR v.department_name = (SELECT name FROM public.departments WHERE id = p_department_id))
      AND (p_status_id IS NULL OR v.status_name = (SELECT name FROM public.statuses WHERE id = p_status_id));
END;
$$;


ALTER FUNCTION public.filteringrequests(p_type character varying, p_department_id integer, p_status_id integer) OWNER TO postgres;

--
-- TOC entry 241 (class 1255 OID 17184)
-- Name: generate_visitor_login(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.generate_visitor_login() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    email_prefix TEXT;
    new_login TEXT;
BEGIN
    email_prefix := SPLIT_PART(NEW.email, '@', 1);
    new_login := CONCAT(email_prefix, '_', NEW.id);
    
    UPDATE public.visitors
    SET login = new_login
    WHERE id = NEW.id;
    
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.generate_visitor_login() OWNER TO postgres;

--
-- TOC entry 239 (class 1255 OID 17153)
-- Name: sp_login_user(character varying, character); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_login_user(p_email character varying, p_password_hash character) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM users WHERE email = p_email AND password_hash = p_password_hash);
END;
$$;


ALTER FUNCTION public.sp_login_user(p_email character varying, p_password_hash character) OWNER TO postgres;

--
-- TOC entry 240 (class 1255 OID 17154)
-- Name: sp_register_user(character varying, character); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_register_user(p_email character varying, p_password_hash character) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM users WHERE email = p_email) THEN
        RETURN FALSE;
    END IF;
    INSERT INTO users (email, password_hash, registered_at) VALUES (p_email, p_password_hash, NOW());
    RETURN TRUE;
END;
$$;


ALTER FUNCTION public.sp_register_user(p_email character varying, p_password_hash character) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 234 (class 1259 OID 17159)
-- Name: blacklist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.blacklist (
    id integer NOT NULL,
    visitor_id integer NOT NULL,
    reason text,
    added_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.blacklist OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 17158)
-- Name: blacklist_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.blacklist_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.blacklist_id_seq OWNER TO postgres;

--
-- TOC entry 5039 (class 0 OID 0)
-- Dependencies: 233
-- Name: blacklist_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.blacklist_id_seq OWNED BY public.blacklist.id;


--
-- TOC entry 224 (class 1259 OID 17041)
-- Name: department_employees; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.department_employees (
    id integer NOT NULL,
    full_name character varying(150) NOT NULL,
    department_id integer NOT NULL
);


ALTER TABLE public.department_employees OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 17040)
-- Name: department_employees_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.department_employees_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.department_employees_id_seq OWNER TO postgres;

--
-- TOC entry 5040 (class 0 OID 0)
-- Dependencies: 223
-- Name: department_employees_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.department_employees_id_seq OWNED BY public.department_employees.id;


--
-- TOC entry 222 (class 1259 OID 17030)
-- Name: departments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.departments (
    id integer NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE public.departments OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 17029)
-- Name: departments_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.departments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.departments_id_seq OWNER TO postgres;

--
-- TOC entry 5041 (class 0 OID 0)
-- Dependencies: 221
-- Name: departments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.departments_id_seq OWNED BY public.departments.id;


--
-- TOC entry 232 (class 1259 OID 17131)
-- Name: request_visitors; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.request_visitors (
    id integer NOT NULL,
    request_id integer NOT NULL,
    visitor_id integer NOT NULL,
    order_number integer NOT NULL
);


ALTER TABLE public.request_visitors OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 17130)
-- Name: request_visitors_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.request_visitors_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.request_visitors_id_seq OWNER TO postgres;

--
-- TOC entry 5042 (class 0 OID 0)
-- Dependencies: 231
-- Name: request_visitors_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.request_visitors_id_seq OWNED BY public.request_visitors.id;


--
-- TOC entry 230 (class 1259 OID 17090)
-- Name: requests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.requests (
    id integer NOT NULL,
    user_id integer NOT NULL,
    type character varying(20) NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    purpose text NOT NULL,
    department_id integer NOT NULL,
    employee_id integer NOT NULL,
    status_id integer DEFAULT 1 NOT NULL,
    rejection_reason text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT requests_type_check CHECK (((type)::text = ANY ((ARRAY['personal'::character varying, 'group'::character varying])::text[])))
);


ALTER TABLE public.requests OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 17089)
-- Name: requests_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.requests_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.requests_id_seq OWNER TO postgres;

--
-- TOC entry 5043 (class 0 OID 0)
-- Dependencies: 229
-- Name: requests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.requests_id_seq OWNED BY public.requests.id;


--
-- TOC entry 220 (class 1259 OID 17019)
-- Name: statuses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.statuses (
    id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.statuses OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 17018)
-- Name: statuses_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.statuses_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.statuses_id_seq OWNER TO postgres;

--
-- TOC entry 5044 (class 0 OID 0)
-- Dependencies: 219
-- Name: statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.statuses_id_seq OWNED BY public.statuses.id;


--
-- TOC entry 236 (class 1259 OID 17186)
-- Name: travel_time; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.travel_time (
    department_id integer NOT NULL,
    minutes integer NOT NULL,
    CONSTRAINT travel_time_minutes_check CHECK ((minutes > 0))
);


ALTER TABLE public.travel_time OWNER TO postgres;

--
-- TOC entry 5045 (class 0 OID 0)
-- Dependencies: 236
-- Name: TABLE travel_time; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.travel_time IS 'Время перемещения от проходной до подразделения в минутах';


--
-- TOC entry 226 (class 1259 OID 17056)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    email character varying(255) NOT NULL,
    password_hash character(32) NOT NULL,
    registered_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 17055)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO postgres;

--
-- TOC entry 5046 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- TOC entry 238 (class 1259 OID 17200)
-- Name: visit_log; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visit_log (
    id integer NOT NULL,
    request_id integer NOT NULL,
    visitor_id integer NOT NULL,
    entry_time timestamp without time zone,
    department_arrival_time timestamp without time zone,
    department_departure_time timestamp without time zone,
    exit_time timestamp without time zone,
    violation_time boolean DEFAULT false,
    violation_exit boolean DEFAULT false
);


ALTER TABLE public.visit_log OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 17199)
-- Name: visit_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.visit_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.visit_log_id_seq OWNER TO postgres;

--
-- TOC entry 5047 (class 0 OID 0)
-- Dependencies: 237
-- Name: visit_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visit_log_id_seq OWNED BY public.visit_log.id;


--
-- TOC entry 228 (class 1259 OID 17069)
-- Name: visitors; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visitors (
    id integer NOT NULL,
    last_name character varying(50) NOT NULL,
    first_name character varying(50) NOT NULL,
    middle_name character varying(50),
    phone character varying(20),
    email character varying(255) NOT NULL,
    organization character varying(200),
    note text NOT NULL,
    birth_date date NOT NULL,
    passport_series character(4) NOT NULL,
    passport_number character(6) NOT NULL,
    photo_path character varying(500),
    passport_scan_path character varying(500) NOT NULL,
    login character varying(100),
    CONSTRAINT check_age CHECK ((EXTRACT(year FROM age((birth_date)::timestamp with time zone)) >= (16)::numeric)),
    CONSTRAINT check_passport_number CHECK ((passport_number ~ '^[0-9]{6}$'::text)),
    CONSTRAINT check_passport_series CHECK ((passport_series ~ '^[0-9]{4}$'::text))
);


ALTER TABLE public.visitors OWNER TO postgres;

--
-- TOC entry 5048 (class 0 OID 0)
-- Dependencies: 228
-- Name: COLUMN visitors.login; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.visitors.login IS 'Уникальный логин, генерируемый автоматически';


--
-- TOC entry 227 (class 1259 OID 17068)
-- Name: visitors_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.visitors_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.visitors_id_seq OWNER TO postgres;

--
-- TOC entry 5049 (class 0 OID 0)
-- Dependencies: 227
-- Name: visitors_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visitors_id_seq OWNED BY public.visitors.id;


--
-- TOC entry 4817 (class 2604 OID 17162)
-- Name: blacklist id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklist ALTER COLUMN id SET DEFAULT nextval('public.blacklist_id_seq'::regclass);


--
-- TOC entry 4809 (class 2604 OID 17044)
-- Name: department_employees id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.department_employees ALTER COLUMN id SET DEFAULT nextval('public.department_employees_id_seq'::regclass);


--
-- TOC entry 4808 (class 2604 OID 17033)
-- Name: departments id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments ALTER COLUMN id SET DEFAULT nextval('public.departments_id_seq'::regclass);


--
-- TOC entry 4816 (class 2604 OID 17134)
-- Name: request_visitors id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.request_visitors ALTER COLUMN id SET DEFAULT nextval('public.request_visitors_id_seq'::regclass);


--
-- TOC entry 4813 (class 2604 OID 17093)
-- Name: requests id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests ALTER COLUMN id SET DEFAULT nextval('public.requests_id_seq'::regclass);


--
-- TOC entry 4807 (class 2604 OID 17022)
-- Name: statuses id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statuses ALTER COLUMN id SET DEFAULT nextval('public.statuses_id_seq'::regclass);


--
-- TOC entry 4810 (class 2604 OID 17059)
-- Name: users id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- TOC entry 4819 (class 2604 OID 17203)
-- Name: visit_log id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_log ALTER COLUMN id SET DEFAULT nextval('public.visit_log_id_seq'::regclass);


--
-- TOC entry 4812 (class 2604 OID 17072)
-- Name: visitors id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors ALTER COLUMN id SET DEFAULT nextval('public.visitors_id_seq'::regclass);


--
-- TOC entry 5030 (class 0 OID 17159)
-- Dependencies: 234
-- Data for Name: blacklist; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.blacklist (id, visitor_id, reason, added_at) FROM stdin;
\.


--
-- TOC entry 5020 (class 0 OID 17041)
-- Dependencies: 224
-- Data for Name: department_employees; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.department_employees (id, full_name, department_id) FROM stdin;
1	Фомичева Авдотья Трофимовна	1
2	Гаврилова Римма Ефимовна	2
3	Носкова Наталия Прохоровна	3
4	Архипов Тимофей Васильевич	4
5	Орехова Вероника Артемовна	5
6	Савельев Павел Степанович	6
7	Чернов Всеволод Наумович	7
\.


--
-- TOC entry 5018 (class 0 OID 17030)
-- Dependencies: 222
-- Data for Name: departments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.departments (id, name) FROM stdin;
1	Производство
2	Сбыт
3	Администрация
4	Служба безопасности
5	Планирование
6	Общий отдел
7	Охрана
\.


--
-- TOC entry 5028 (class 0 OID 17131)
-- Dependencies: 232
-- Data for Name: request_visitors; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.request_visitors (id, request_id, visitor_id, order_number) FROM stdin;
1	1	1	1
2	2	2	1
3	3	3	1
4	4	4	1
5	5	5	1
6	6	6	1
7	7	7	1
8	8	8	1
9	9	9	1
10	10	10	1
11	11	11	1
12	12	12	1
13	13	13	1
14	14	14	1
15	15	15	1
16	16	16	1
17	16	17	2
18	16	18	3
19	16	19	4
20	16	20	5
21	16	21	6
22	16	22	7
23	17	23	1
24	17	24	2
25	17	25	3
26	17	26	4
27	17	27	5
28	17	28	6
29	17	29	7
30	17	30	8
\.


--
-- TOC entry 5026 (class 0 OID 17090)
-- Dependencies: 230
-- Data for Name: requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.requests (id, user_id, type, start_date, end_date, purpose, department_id, employee_id, status_id, rejection_reason, created_at) FROM stdin;
1	1	personal	2023-04-24	2023-04-25	Личное посещение	1	1	2	\N	2026-04-15 13:08:56.571396
2	2	personal	2023-04-24	2023-04-25	Личное посещение	2	2	2	\N	2026-04-15 13:08:56.571396
3	3	personal	2023-04-24	2023-04-25	Личное посещение	3	3	2	\N	2026-04-15 13:08:56.571396
4	4	personal	2023-04-25	2023-04-26	Личное посещение	1	1	2	\N	2026-04-15 13:08:56.571396
5	5	personal	2023-04-25	2023-04-26	Личное посещение	2	2	2	\N	2026-04-15 13:08:56.571396
6	6	personal	2023-04-25	2023-04-26	Личное посещение	3	3	2	\N	2026-04-15 13:08:56.571396
7	7	personal	2023-04-26	2023-04-27	Личное посещение	1	1	2	\N	2026-04-15 13:08:56.571396
8	8	personal	2023-04-26	2023-04-27	Личное посещение	2	2	2	\N	2026-04-15 13:08:56.571396
9	9	personal	2023-04-26	2023-04-27	Личное посещение	3	3	2	\N	2026-04-15 13:08:56.571396
10	10	personal	2023-04-27	2023-04-28	Личное посещение	1	1	2	\N	2026-04-15 13:08:56.571396
11	11	personal	2023-04-27	2023-04-28	Личное посещение	2	2	2	\N	2026-04-15 13:08:56.571396
12	12	personal	2023-04-27	2023-04-28	Личное посещение	3	3	2	\N	2026-04-15 13:08:56.571396
13	13	personal	2023-04-28	2023-04-29	Личное посещение	1	1	2	\N	2026-04-15 13:08:56.571396
14	14	personal	2023-04-28	2023-04-29	Личное посещение	2	2	2	\N	2026-04-15 13:08:56.571396
15	15	personal	2023-04-28	2023-04-29	Личное посещение	3	3	2	\N	2026-04-15 13:08:56.571396
16	16	group	2023-04-24	2023-04-25	Экскурсия на производство	1	1	2	\N	2026-04-15 13:08:56.571396
17	23	group	2023-04-24	2023-04-25	Экскурсия на производство (вторая группа)	1	1	2	\N	2026-04-15 13:08:56.571396
\.


--
-- TOC entry 5016 (class 0 OID 17019)
-- Dependencies: 220
-- Data for Name: statuses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.statuses (id, name) FROM stdin;
1	проверка
2	одобрена
3	не одобрена
\.


--
-- TOC entry 5031 (class 0 OID 17186)
-- Dependencies: 236
-- Data for Name: travel_time; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.travel_time (department_id, minutes) FROM stdin;
1	10
2	5
3	3
4	2
5	7
6	4
7	1
\.


--
-- TOC entry 5022 (class 0 OID 17056)
-- Dependencies: 226
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, email, password_hash, registered_at) FROM stdin;
1	Radinka100@yandex.ru	f2eb7eac111e507e1f4310fbca48c1b4	2026-04-15 13:08:56.571396
2	Prohor156@list.ru	6e23aa05abe3646195df7f6c06a59702	2026-04-15 13:08:56.571396
3	YUrin155@gmail.com	f9259b3e7f7e3dddae64f20eb0e6b971	2026-04-15 13:08:56.571396
4	Aljbina33@lenta.ru	b570dc22d2f243d1ff7da92cef593f91	2026-04-15 13:08:56.571396
5	Klavdiya113@live.com	9936c8f149921d756c3c3d2ed78b30f1	2026-04-15 13:08:56.571396
6	Tamara179@live.com	0b1341da3c137913d05e69094afbe755	2026-04-15 13:08:56.571396
7	Taras24@rambler.ru	3fd21450485e41d8b37718ea2ff5dab8	2026-04-15 13:08:56.571396
8	Arkadij123@inbox.ru	56f5a74d5b7013d1cd20d43d67b98c69	2026-04-15 13:08:56.571396
9	Glafira73@outlook.com	409348a3cff8264cbcb4407e55a2eff0	2026-04-15 13:08:56.571396
10	Gavriila68@msn.com	b2df6c2886befac08929c56905b183c3	2026-04-15 13:08:56.571396
11	Kuzjma124@yandex.ru	ec5402ef81b24120e36630b2c4c4ffc7	2026-04-15 13:08:56.571396
12	Roman89@gmail.com	dc07a3f9e45ba90e18a2e3ecef3dcd04	2026-04-15 13:08:56.571396
13	Aleksej43@gmail.com	06e3108cbe003a2992c866117262ee72	2026-04-15 13:08:56.571396
14	Nadezhda137@outlook.com	019239d5a96d7d7eef8fa5f0138f4af1	2026-04-15 13:08:56.571396
15	Bronislava56@yahoo.com	2ecef85244ac1671806d8c1936e27f0d	2026-04-15 13:08:56.571396
16	Taisiya177@lenta.ru	fa3507cad2500ca8f551932b44a1d0ff	2026-04-15 13:08:56.571396
17	Adelaida20@hotmail.com	bc5195aa7e7e9089065b57e17639b86f	2026-04-15 13:08:56.571396
18	Lev131@rambler.ru	8d2434100b4ef155c75e2693842b21a3	2026-04-15 13:08:56.571396
19	lzaihtvkdn@bk.ru	af0dc9fe72a0add56dacc0124a184946	2026-04-15 13:08:56.571396
20	Lyudmila123@hotmail.com	28af582469ee65604f082f9297b0a227	2026-04-15 13:08:56.571396
21	Taisiya176@hotmail.com	be630902a553ac8cf2859dd95d5b6606	2026-04-15 13:08:56.571396
22	Vera195@list.ru	3f5b98dde208683b0815c5405a9e8dad	2026-04-15 13:08:56.571396
23	YAkov196@rambler.ru	4ced337387b6e838a5a46e63d19ea292	2026-04-15 13:08:56.571396
24	Nina145@msn.com	446a7f47b0850cd4f7fe00509f5d1f52	2026-04-15 13:08:56.571396
25	Leontij161@mail.ru	ba31a68b36b48f1d65b2cb992655ddde	2026-04-15 13:08:56.571396
26	Serafima169@yahoo.com	3576d70bd143ae6b3285e231abc12833	2026-04-15 13:08:56.571396
27	Sergej35@inbox.ru	e12f4d7805d9a07087594dcd80161f4f	2026-04-15 13:08:56.571396
28	Georgij121@inbox.ru	f8a925501f7949bfc652f90cfe5df903	2026-04-15 13:08:56.571396
29	Elizar30@yandex.ru	85bb40863aea1f729f74f38d9cebfa78	2026-04-15 13:08:56.571396
30	Lana117@outlook.com	a5d100e6c4b1a7f695bcec7ca079b71b	2026-04-15 13:08:56.571396
\.


--
-- TOC entry 5033 (class 0 OID 17200)
-- Dependencies: 238
-- Data for Name: visit_log; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visit_log (id, request_id, visitor_id, entry_time, department_arrival_time, department_departure_time, exit_time, violation_time, violation_exit) FROM stdin;
\.


--
-- TOC entry 5024 (class 0 OID 17069)
-- Dependencies: 228
-- Data for Name: visitors; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visitors (id, last_name, first_name, middle_name, phone, email, organization, note, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) FROM stdin;
1	Степанова	Радинка	Власовна	+7 (613) 272-60-62	Radinka100@yandex.ru	\N	Личное посещение	1986-10-18	0208	530509	\N	/scans/stepanova_passport.pdf	\N
2	Шилов	Прохор	Герасимович	+7 (615) 594-77-66	Prohor156@list.ru	\N	Личное посещение	1977-10-09	3036	796488	\N	/scans/shilov_passport.pdf	\N
3	Кононов	Юрин	Романович	+7 (784) 673-51-91	YUrin155@gmail.com	\N	Личное посещение	1971-10-08	2747	790512	\N	/scans/kononov_passport.pdf	\N
4	Елисеева	Альбина	Николаевна	+7 (654) 864-77-46	Aljbina33@lenta.ru	\N	Личное посещение	1983-02-15	5241	213304	\N	/scans/eliseeva_passport.pdf	\N
5	Шарова	Клавдия	Макаровна	+7 (822) 525-82-40	Klavdiya113@live.com	\N	Личное посещение	1980-07-22	8143	593309	\N	/scans/sharova_passport.pdf	\N
6	Сидорова	Тамара	Григорьевна	+7 (334) 692-79-77	Tamara179@live.com	\N	Личное посещение	1995-11-22	8143	905520	\N	/scans/sidorova_passport.pdf	\N
7	Петухов	Тарас	Фадеевич	+7 (376) 220-62-51	Taras24@rambler.ru	\N	Личное посещение	1991-01-05	1609	171096	\N	/scans/petuhov_passport.pdf	\N
8	Родионов	Аркадий	Власович	+7 (491) 696-17-11	Arkadij123@inbox.ru	\N	Личное посещение	1993-08-11	3841	642594	\N	/scans/rodionov_passport.pdf	\N
9	Горшкова	Глафира	Валентиновна	+7 (553) 343-38-82	Glafira73@outlook.com	\N	Личное посещение	1978-05-25	9170	402601	\N	/scans/gorshkova_passport.pdf	\N
10	Кириллова	Гавриила	Яковна	+7 (648) 700-43-34	Gavriila68@msn.com	\N	Личное посещение	1992-04-26	9438	379667	\N	/scans/kirillova_passport.pdf	\N
11	Овчинников	Кузьма	Ефимович	+7 (562) 866-15-27	Kuzjma124@yandex.ru	\N	Личное посещение	1993-08-02	0766	647226	\N	/scans/ovchinnikov_passport.pdf	\N
12	Беляков	Роман	Викторович	+7 (595) 196-56-28	Roman89@gmail.com	\N	Личное посещение	1991-06-07	2411	478305	\N	/scans/belyakov_passport.pdf	\N
13	Лыткин	Алексей	Максимович	+7 (994) 353-29-52	Aleksej43@gmail.com	\N	Личное посещение	1996-03-07	2383	259825	\N	/scans/lytkin_passport.pdf	\N
14	Шубина	Надежда	Викторовна	+7 (736) 488-66-95	Nadezhda137@outlook.com	\N	Личное посещение	1981-09-24	8844	708476	\N	/scans/shubina_passport.pdf	\N
15	Зиновьева	Бронислава	Викторовна	+7 (778) 565-12-18	Bronislava56@yahoo.com	\N	Личное посещение	1981-03-19	6736	319423	\N	/scans/zinovieva_passport.pdf	\N
16	Самойлова	Таисия	Гермоновна	+7 (891) 555-81-44	Taisiya177@lenta.ru	\N	Групповое посещение	1979-11-14	5193	897719	\N	/scans/samoylova_passport.pdf	\N
17	Ситникова	Аделаида	Гермоновна	+7 (793) 736-70-31	Adelaida20@hotmail.com	\N	Групповое посещение	1979-01-21	7561	148016	\N	/scans/sitnikova_passport.pdf	\N
18	Исаев	Лев	Юлианович	+7 (675) 593-89-30	Lev131@rambler.ru	\N	Групповое посещение	1994-08-05	1860	680004	\N	/scans/isaev_passport.pdf	\N
19	Никифоров	Даниил	Степанович	+7 (384) 358-77-82	Daniil198@bk.ru	\N	Групповое посещение	1970-12-13	4557	999958	\N	/scans/nikiforov_passport.pdf	\N
20	Титова	Людмила	Якововна	+7 (221) 729-16-84	Lyudmila123@hotmail.com	\N	Групповое посещение	1976-08-21	7715	639425	\N	/scans/titova_passport.pdf	\N
21	Абрамова	Таисия	Дмитриевна	+7 (528) 312-18-20	Taisiya176@hotmail.com	\N	Групповое посещение	1982-11-20	7310	893510	\N	/scans/abramova_passport.pdf	\N
22	Кузьмина	Вера	Максимовна	+7 (598) 583-53-45	Vera195@list.ru	\N	Групповое посещение	1989-12-10	3537	982933	\N	/scans/kuzmina_passport.pdf	\N
23	Мартынов	Яков	Ростиславович	+7 (546) 159-67-33	YAkov196@rambler.ru	\N	Групповое посещение	1976-12-05	1793	986063	\N	/scans/martynov_passport.pdf	\N
24	Евсеева	Нина	Павловна	+7 (833) 521-31-50	Nina145@msn.com	\N	Групповое посещение	1994-09-26	9323	745717	\N	/scans/evseeva_passport.pdf	\N
25	Голубев	Леонтий	Вячеславович	+7 (160) 527-57-41	Leontij161@mail.ru	\N	Групповое посещение	1990-10-03	1059	822077	\N	/scans/golubev_passport.pdf	\N
26	Карпова	Серафима	Михаиловна	+7 (459) 930-91-70	Serafima169@yahoo.com	\N	Групповое посещение	1989-11-19	7034	858987	\N	/scans/karpova_passport.pdf	\N
27	Орехов	Сергей	Емельянович	+7 (669) 603-29-87	Sergej35@inbox.ru	\N	Групповое посещение	1972-02-11	3844	223682	\N	/scans/orehov_passport.pdf	\N
28	Исаев	Георгий	Павлович	+7 (678) 516-36-86	Georgij121@inbox.ru	\N	Групповое посещение	1987-08-11	4076	629809	\N	/scans/isaev_georgiy_passport.pdf	\N
29	Богданов	Елизар	Артемович	+7 (165) 768-30-97	Elizar30@yandex.ru	\N	Групповое посещение	1978-02-02	0573	198559	\N	/scans/bogdanov_passport.pdf	\N
30	Тихонова	Лана	Семеновна	+7 (478) 467-75-15	Lana117@outlook.com	\N	Групповое посещение	1990-07-24	8761	609740	\N	/scans/tihonova_passport.pdf	\N
31	Иванов	Иван	Иванович	+7 (999) 123-45-67	ivanov@example.com	ООО Ромашка	Тестовый посетитель	1990-01-01	1234	567890	\N	/scans/ivanov.pdf	ivanov_31
\.


--
-- TOC entry 5050 (class 0 OID 0)
-- Dependencies: 233
-- Name: blacklist_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.blacklist_id_seq', 1, false);


--
-- TOC entry 5051 (class 0 OID 0)
-- Dependencies: 223
-- Name: department_employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.department_employees_id_seq', 7, true);


--
-- TOC entry 5052 (class 0 OID 0)
-- Dependencies: 221
-- Name: departments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.departments_id_seq', 7, true);


--
-- TOC entry 5053 (class 0 OID 0)
-- Dependencies: 231
-- Name: request_visitors_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.request_visitors_id_seq', 30, true);


--
-- TOC entry 5054 (class 0 OID 0)
-- Dependencies: 229
-- Name: requests_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.requests_id_seq', 17, true);


--
-- TOC entry 5055 (class 0 OID 0)
-- Dependencies: 219
-- Name: statuses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.statuses_id_seq', 3, true);


--
-- TOC entry 5056 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 30, true);


--
-- TOC entry 5057 (class 0 OID 0)
-- Dependencies: 237
-- Name: visit_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visit_log_id_seq', 1, false);


--
-- TOC entry 5058 (class 0 OID 0)
-- Dependencies: 227
-- Name: visitors_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visitors_id_seq', 31, true);


--
-- TOC entry 4850 (class 2606 OID 17169)
-- Name: blacklist blacklist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklist
    ADD CONSTRAINT blacklist_pkey PRIMARY KEY (id);


--
-- TOC entry 4836 (class 2606 OID 17049)
-- Name: department_employees department_employees_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.department_employees
    ADD CONSTRAINT department_employees_pkey PRIMARY KEY (id);


--
-- TOC entry 4832 (class 2606 OID 17039)
-- Name: departments departments_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_name_key UNIQUE (name);


--
-- TOC entry 4834 (class 2606 OID 17037)
-- Name: departments departments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_pkey PRIMARY KEY (id);


--
-- TOC entry 4846 (class 2606 OID 17140)
-- Name: request_visitors request_visitors_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.request_visitors
    ADD CONSTRAINT request_visitors_pkey PRIMARY KEY (id);


--
-- TOC entry 4848 (class 2606 OID 17142)
-- Name: request_visitors request_visitors_request_id_order_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.request_visitors
    ADD CONSTRAINT request_visitors_request_id_order_number_key UNIQUE (request_id, order_number);


--
-- TOC entry 4844 (class 2606 OID 17109)
-- Name: requests requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_pkey PRIMARY KEY (id);


--
-- TOC entry 4828 (class 2606 OID 17028)
-- Name: statuses statuses_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statuses
    ADD CONSTRAINT statuses_name_key UNIQUE (name);


--
-- TOC entry 4830 (class 2606 OID 17026)
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- TOC entry 4852 (class 2606 OID 17193)
-- Name: travel_time travel_time_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.travel_time
    ADD CONSTRAINT travel_time_pkey PRIMARY KEY (department_id);


--
-- TOC entry 4838 (class 2606 OID 17067)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4840 (class 2606 OID 17065)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 4854 (class 2606 OID 17210)
-- Name: visit_log visit_log_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_log
    ADD CONSTRAINT visit_log_pkey PRIMARY KEY (id);


--
-- TOC entry 4842 (class 2606 OID 17088)
-- Name: visitors visitors_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors
    ADD CONSTRAINT visitors_pkey PRIMARY KEY (id);


--
-- TOC entry 5014 (class 2618 OID 17178)
-- Name: viewlistrequests _RETURN; Type: RULE; Schema: public; Owner: postgres
--

CREATE OR REPLACE VIEW public.viewlistrequests AS
 SELECT r.id AS request_id,
    r.type,
    r.start_date,
    r.end_date,
    r.purpose,
    r.created_at,
    d.name AS department_name,
    de.full_name AS employee_full_name,
    s.name AS status_name,
    u.email AS user_email,
    string_agg((((((v.last_name)::text || ' '::text) || (v.first_name)::text) || ' '::text) || (COALESCE(v.middle_name, ''::character varying))::text), ', '::text ORDER BY rv.order_number) AS visitors_list
   FROM ((((((public.requests r
     JOIN public.departments d ON ((r.department_id = d.id)))
     JOIN public.department_employees de ON ((r.employee_id = de.id)))
     JOIN public.statuses s ON ((r.status_id = s.id)))
     JOIN public.users u ON ((r.user_id = u.id)))
     LEFT JOIN public.request_visitors rv ON ((r.id = rv.request_id)))
     LEFT JOIN public.visitors v ON ((rv.visitor_id = v.id)))
  GROUP BY r.id, d.name, de.full_name, s.name, u.email;


--
-- TOC entry 4866 (class 2620 OID 17185)
-- Name: visitors visitor_generate_login_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER visitor_generate_login_trigger AFTER INSERT ON public.visitors FOR EACH ROW EXECUTE FUNCTION public.generate_visitor_login();


--
-- TOC entry 4862 (class 2606 OID 17170)
-- Name: blacklist blacklist_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklist
    ADD CONSTRAINT blacklist_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(id) ON DELETE CASCADE;


--
-- TOC entry 4855 (class 2606 OID 17050)
-- Name: department_employees department_employees_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.department_employees
    ADD CONSTRAINT department_employees_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(id) ON DELETE RESTRICT;


--
-- TOC entry 4860 (class 2606 OID 17143)
-- Name: request_visitors request_visitors_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.request_visitors
    ADD CONSTRAINT request_visitors_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(id) ON DELETE CASCADE;


--
-- TOC entry 4861 (class 2606 OID 17148)
-- Name: request_visitors request_visitors_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.request_visitors
    ADD CONSTRAINT request_visitors_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(id) ON DELETE CASCADE;


--
-- TOC entry 4856 (class 2606 OID 17115)
-- Name: requests requests_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(id);


--
-- TOC entry 4857 (class 2606 OID 17120)
-- Name: requests requests_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.department_employees(id);


--
-- TOC entry 4858 (class 2606 OID 17125)
-- Name: requests requests_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.statuses(id);


--
-- TOC entry 4859 (class 2606 OID 17110)
-- Name: requests requests_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- TOC entry 4863 (class 2606 OID 17194)
-- Name: travel_time travel_time_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.travel_time
    ADD CONSTRAINT travel_time_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(id);


--
-- TOC entry 4864 (class 2606 OID 17211)
-- Name: visit_log visit_log_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_log
    ADD CONSTRAINT visit_log_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(id) ON DELETE CASCADE;


--
-- TOC entry 4865 (class 2606 OID 17216)
-- Name: visit_log visit_log_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_log
    ADD CONSTRAINT visit_log_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(id) ON DELETE CASCADE;


-- Completed on 2026-04-20 14:18:26

--
-- PostgreSQL database dump complete
--

\unrestrict h53cyJC3UQaghkcCy1hsAtHwTTUSIi2HMUAjHe2r1t5spTrGK9kIBtC6TAvysc8

