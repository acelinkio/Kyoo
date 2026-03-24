begin;

create table keibi.oidc_login(
	pk serial primary key,
	id uuid not null default gen_random_uuid(),
	opaque varchar(128) not null unique,
	provider varchar(256) not null,
	redirect_url text not null,
	tenant varchar(256) not null,
	code text,
	created_at timestamptz not null default now()::timestamptz
);

commit;
