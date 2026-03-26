# Setup first user
POST {{host}}/users
{
    "username": "sessions-user-1",
    "password": "password-sessions-user-1",
    "email": "sessions-user-1@zoriya.dev"
}
HTTP 201
[Captures]
token1: jsonpath "$.token"

GET {{host}}/jwt
Authorization: Bearer {{token1}}
HTTP 200
[Captures]
jwt1: jsonpath "$.token"

GET {{host}}/users/me
Authorization: Bearer {{jwt1}}
HTTP 200
[Captures]
user1Id: jsonpath "$.id"

# Can list my own sessions
GET {{host}}/sessions
Authorization: Bearer {{jwt1}}
HTTP 200
[Captures]
session1Id: jsonpath "$[0].id"

# Setup second user
POST {{host}}/users
{
    "username": "sessions-user-2",
    "password": "password-sessions-user-2",
    "email": "sessions-user-2@zoriya.dev"
}
HTTP 201
[Captures]
token2: jsonpath "$.token"

GET {{host}}/jwt
Authorization: Bearer {{token2}}
HTTP 200
[Captures]
jwt2: jsonpath "$.token"

# Cannot list another user's sessions without users.read
GET {{host}}/users/{{user1Id}}/sessions
Authorization: Bearer {{jwt2}}
HTTP 403

# Cleanup second user
DELETE {{host}}/users/me
Authorization: Bearer {{jwt2}}
HTTP 200

# Cleanup first user
DELETE {{host}}/users/me
Authorization: Bearer {{jwt1}}
HTTP 200
