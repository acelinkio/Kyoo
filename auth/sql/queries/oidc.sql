-- name: CreateOidcLogin :one
insert into keibi.oidc_login(provider, opaque, redirect_url, tenant)
	values ($1, $2, $3, $4)
returning
	*;

-- name: GetOidcLoginByOpaque :one
select
	*
from
	keibi.oidc_login
where
	opaque = $1
	and provider = $2
limit 1;

-- name: SaveOidcLoginCode :exec
update
	keibi.oidc_login
set
	code = $2
where
	id = $1;

-- name: ConsumeOidcLogin :one
delete from keibi.oidc_login
where
	opaque = $1
	and provider = $2
	and tenant = $3
	and created_at + interval '10 min' > now()::timestamptz
returning
	*;

-- name: DeleteOidcLoginById :exec
delete from keibi.oidc_login
where
	id = $1;

-- name: CleanupOidcLogins :exec
delete from keibi.oidc_login
where created_at + interval '10 min' < now()::timestamptz;
