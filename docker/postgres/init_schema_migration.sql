create schema if not exists migration authorization bigmom;

set search_path to migration;

drop table if exists commerce cascade;
drop table if exists terminal cascade;
drop table if exists application cascade;
drop table if exists commerce_application cascade;
drop table if exists commerce_terminal cascade;

create table commerce (
	ID varchar(20) constraint pk_commerce primary key, -- Clé primaire = numéro EMV commerçant.
	NOM_COMMERCE varchar(255),
	CODE_EF varchar(5), -- Code de l'établissement financier du commerce, sur 5 chiffres.
	MCC varchar(4),
	id_client_sitpe integer,
	code_postal varchar(5),
	date_debut timestamp,
	date_fin timestamp
);

create table terminal (
	ID bigserial constraint pk_terminal primary key, -- Clé primaire auto-incrémentée.
	NUM_SERIE varchar(50), -- Numéro de série du matériel.
	TYPE_TERMINAL varchar(10), -- Type de matériel : TPE, ELC.
	TYPE_CONNEXION varchar(50),
	MODELE varchar(50),
	STATUT varchar(50)
);

create table application (
	ID bigserial constraint pk_application primary key, -- Clé primaire auto-incrémentée.
	NOM_APPLI varchar(20)
);

create table commerce_application (
	ID_COMMERCE varchar(20) references commerce,
	ID_APPLICATION bigint references application,
	NUM_CONTRAT varchar(50),
	date_debut timestamp,
	date_fin timestamp
);

create table commerce_terminal (
	id_commerce varchar(20) references commerce,
	id_terminal bigint references terminal,
	num_site varchar(2),
	date_debut timestamp,
	date_fin timestamp
);
	