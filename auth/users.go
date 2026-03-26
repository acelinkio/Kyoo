package main

import (
	"crypto/md5"
	"encoding/hex"
	"fmt"
	"net/http"
	"strconv"
	"strings"

	"github.com/alexedwards/argon2id"
	"github.com/google/uuid"
	"github.com/jackc/pgerrcode"
	"github.com/jackc/pgx/v5"
	"github.com/labstack/echo/v5"
	"github.com/zoriya/kyoo/keibi/dbc"
	. "github.com/zoriya/kyoo/keibi/models"
)

func MapDbUser(user *dbc.User) User {
	return User{
		Pk:          user.Pk,
		Id:          user.Id,
		Username:    user.Username,
		Email:       user.Email,
		HasPassword: user.Password != nil,
		CreatedDate: user.CreatedDate,
		LastSeen:    user.LastSeen,
		Claims:      user.Claims,
		Oidc:        nil,
	}
}

// @Summary      List all users
// @Description  List all users existing in this instance.
// @Tags         users
// @Accept       json
// @Produce      json
// @Security     Jwt[users.read]
// @Param        after   query      string  false  "used for pagination."
// @Success      200  {object}  Page[User]
// @Failure      422  {object}  KError "Invalid after id"
// @Router       /users [get]
func (h *Handler) ListUsers(c *echo.Context) error {
	ctx := c.Request().Context()

	err := CheckPermissions(c, []string{"users.read"})
	if err != nil {
		return err
	}

	limit := int32(20)
	id := c.Param("after")

	if id == "" {
		users, err := h.db.GetAllUsers(ctx, limit)
		if err != nil {
			return err
		}

		ret := make([]User, 0, len(users))
		for _, user := range users {
			u := MapDbUser(&user.User)
			u.Oidc = user.Oidc
			ret = append(ret, u)
		}
		return c.JSON(200, NewPage(ret, c.Request().URL, limit))
	} else {
		pk, err := strconv.Atoi(id)
		if err != nil {
			return echo.NewHTTPError(http.StatusUnprocessableEntity, "Invalid `after` parameter")
		}
		users, err := h.db.GetAllUsersAfter(ctx, dbc.GetAllUsersAfterParams{
			Limit:   limit,
			AfterPk: int32(pk),
		})
		if err != nil {
			return err
		}

		ret := make([]User, 0, len(users))
		for _, user := range users {
			u := MapDbUser(&user.User)
			u.Oidc = user.Oidc
			ret = append(ret, u)
		}
		return c.JSON(200, NewPage(ret, c.Request().URL, limit))
	}
}

// @Summary      Get user
// @Description  Get informations about a user from it's id
// @Tags         users
// @Produce      json
// @Security     Jwt[users.read]
// @Param        id   path      string    true  "The id of the user" Format(uuid)
// @Success      200  {object}  User
// @Failure      404  {object}  KError "No user with the given id found"
// @Failure      422  {object}  KError "Invalid id (not a uuid)"
// @Router /users/{id} [get]
func (h *Handler) GetUser(c *echo.Context) error {
	ctx := c.Request().Context()
	err := CheckPermissions(c, []string{"users.read"})
	if err != nil {
		return err
	}

	id := c.Param("id")
	uid, err := uuid.Parse(id)
	dbuser, err := h.db.GetUser(ctx, dbc.GetUserParams{
		UseId:    err == nil,
		Id:       uid,
		Username: id,
	})
	if err == pgx.ErrNoRows {
		return echo.NewHTTPError(404, fmt.Sprintf("No user found with id or username: '%s'.", id))
	} else if err != nil {
		return err
	}

	ret := MapDbUser(&dbuser.User)
	ret.Oidc = dbuser.Oidc
	return c.JSON(200, ret)
}

// @Summary      Get me
// @Description  Get informations about the currently connected user
// @Tags         users
// @Produce      json
// @Security     Jwt
// @Success      200  {object}  User
// @Failure      401  {object}  KError "Missing jwt token"
// @Failure      403  {object}  KError "Invalid jwt token (or expired)"
// @Router /users/me [get]
func (h *Handler) GetMe(c *echo.Context) error {
	ctx := c.Request().Context()
	id, err := GetCurrentUserId(c)
	if err != nil {
		return err
	}
	dbuser, err := h.db.GetUser(ctx, dbc.GetUserParams{
		UseId: true,
		Id:    id,
	})
	if err == pgx.ErrNoRows {
		return c.JSON(403, "Invalid jwt token, couldn't find user.")
	} else if err != nil {
		return err
	}

	ret := MapDbUser(&dbuser.User)
	ret.Oidc = dbuser.Oidc
	return c.JSON(200, ret)
}

func (h *Handler) streamGravatar(c *echo.Context, email string) error {
	hash := md5.Sum([]byte(strings.TrimSpace(strings.ToLower(email))))
	url := fmt.Sprintf("https://www.gravatar.com/avatar/%s?d=404", hex.EncodeToString(hash[:]))

	req, err := http.NewRequestWithContext(c.Request().Context(), http.MethodGet, url, nil)
	if err != nil {
		return err
	}

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return echo.NewHTTPError(http.StatusBadGateway, "Could not fetch gravatar image")
	}
	defer resp.Body.Close()

	if resp.StatusCode == http.StatusNotFound {
		return echo.NewHTTPError(http.StatusNotFound, "No gravatar image found for this user")
	}
	if resp.StatusCode != http.StatusOK {
		return echo.NewHTTPError(http.StatusBadGateway, "Could not fetch gravatar image")
	}

	contentType := resp.Header.Get("Content-Type")
	if contentType == "" {
		contentType = "application/octet-stream"
	}

	return c.Stream(http.StatusOK, contentType, resp.Body)
}

// @Summary      Get my logo
// @Description  Get the current user's gravatar image
// @Tags         users
// @Produce      image/*
// @Security     Jwt
// @Success      200  {file}  binary
// @Failure      401  {object}  KError "Missing jwt token"
// @Failure      403  {object}  KError "Invalid jwt token (or expired)"
// @Failure      404  {object}  KError "No gravatar image found for this user"
// @Router /users/me/logo [get]
func (h *Handler) GetMyLogo(c *echo.Context) error {
	ctx := c.Request().Context()
	id, err := GetCurrentUserId(c)
	if err != nil {
		return err
	}

	users, err := h.db.GetUser(ctx, dbc.GetUserParams{
		UseId: true,
		Id:    id,
	})
	if err != nil {
		return err
	}

	return h.streamGravatar(c, users.User.Email)
}

// @Summary      Get user logo
// @Description  Get a user's gravatar image
// @Tags         users
// @Produce      image/*
// @Security     Jwt[users.read]
// @Param        id   path      string    true  "The id or username of the user"
// @Success      200  {file}  binary
// @Failure      404  {object}  KError "No user found with id or username"
// @Failure      404  {object}  KError "No gravatar image found for this user"
// @Router /users/{id}/logo [get]
func (h *Handler) GetUserLogo(c *echo.Context) error {
	ctx := c.Request().Context()
	err := CheckPermissions(c, []string{"users.read"})
	if err != nil {
		return err
	}

	id := c.Param("id")
	uid, err := uuid.Parse(id)
	users, err := h.db.GetUser(ctx, dbc.GetUserParams{
		UseId:    err == nil,
		Id:       uid,
		Username: id,
	})
	if err == pgx.ErrNoRows {
		return echo.NewHTTPError(404, fmt.Sprintf("No user found with id or username: '%s'.", id))
	} else if err != nil {
		return err
	}

	return h.streamGravatar(c, users.User.Email)
}

// @Summary      Register
// @Description  Register as a new user and open a session for it
// @Tags         users
// @Accept       json
// @Produce      json
// @Param        device   query   string         false  "The device the created session will be used on"  Example(android)
// @Param        user     body    RegisterDto  false  "Registration informations"
// @Success      201  {object}  SessionWToken
// @Success      409  {object}  KError "Duplicated email or username"
// @Failure      422  {object}  KError "Invalid register body"
// @Router /users [post]
func (h *Handler) Register(c *echo.Context) error {
	ctx := c.Request().Context()
	var req RegisterDto
	err := c.Bind(&req)
	if err != nil {
		return echo.NewHTTPError(http.StatusUnprocessableEntity, err.Error())
	}
	if err = c.Validate(&req); err != nil {
		return err
	}

	pass, err := argon2id.CreateHash(req.Password, argon2id.DefaultParams)
	if err != nil {
		return err
	}

	duser, err := h.db.CreateUser(ctx, dbc.CreateUserParams{
		Username:    req.Username,
		Email:       req.Email,
		Password:    &pass,
		Claims:      h.config.DefaultClaims,
		FirstClaims: h.config.FirstUserClaims,
	})
	if ErrIs(err, pgerrcode.UniqueViolation) {
		return echo.NewHTTPError(409, "Email or username already taken")
	} else if err != nil {
		return err
	}
	user := MapDbUser(&duser)
	return h.createSession(c, &user)
}

// @Summary      Delete user
// @Description  Delete an account and all it's sessions.
// @Tags         users
// @Accept       json
// @Produce      json
// @Security     Jwt[users.delete]
// @Param        id   path      string  false  "User id of the user to delete" Format(uuid)
// @Success      200  {object}  User
// @Failure      404  {object}  KError "Invalid user id"
// @Failure      422  {object}  KError "Invalid id format"
// @Router /users/{id} [delete]
func (h *Handler) DeleteUser(c *echo.Context) error {
	ctx := c.Request().Context()
	err := CheckPermissions(c, []string{"users.delete"})
	if err != nil {
		return err
	}

	uid, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return echo.NewHTTPError(422, "Invalid id given: not an uuid")
	}

	ret, err := h.db.DeleteUser(ctx, uid)
	if err == pgx.ErrNoRows {
		return echo.NewHTTPError(404, "No user found with given id")
	} else if err != nil {
		return err
	}
	return c.JSON(200, MapDbUser(&ret))
}

// @Summary      Delete self
// @Description  Delete your account and all your sessions
// @Tags         users
// @Accept       json
// @Produce      json
// @Security     Jwt
// @Success      200  {object}  User
// @Router /users/me [delete]
func (h *Handler) DeleteSelf(c *echo.Context) error {
	ctx := c.Request().Context()
	uid, err := GetCurrentUserId(c)
	if err != nil {
		return err
	}

	ret, err := h.db.DeleteUser(ctx, uid)
	if err == pgx.ErrNoRows {
		return echo.NewHTTPError(403, "Invalid token, user already deleted.")
	} else if err != nil {
		return err
	}
	return c.JSON(200, MapDbUser(&ret))
}

// @Summary      Edit self
// @Description  Edit your account's info
// @Tags         users
// @Accept       json
// @Produce      json
// @Security     Jwt
// @Param        user     body  EditUserDto  false  "Edited user info"
// @Success      200  {object}  User
// @Success      403  {object}  KError  "You can't edit a protected claim"
// @Success      422  {object}  KError  "Invalid body"
// @Router /users/me [patch]
func (h *Handler) EditSelf(c *echo.Context) error {
	ctx := c.Request().Context()
	var req EditUserDto
	err := c.Bind(&req)
	if err != nil {
		return echo.NewHTTPError(http.StatusUnprocessableEntity, err.Error())
	}
	if err = c.Validate(&req); err != nil {
		return err
	}

	for _, key := range h.config.ProtectedClaims {
		if _, contains := req.Claims[key]; contains {
			return echo.NewHTTPError(http.StatusForbidden, fmt.Sprintf("Can't edit protected claim: '%s'.", key))
		}
	}

	uid, err := GetCurrentUserId(c)
	if err != nil {
		return err
	}

	ret, err := h.db.UpdateUser(ctx, dbc.UpdateUserParams{
		Id:       uid,
		Username: req.Username,
		Email:    req.Email,
		Claims:   req.Claims,
	})
	if err == pgx.ErrNoRows {
		return echo.NewHTTPError(http.StatusNotFound, "Invalid token, user not found.")
	} else if err != nil {
		return err
	}

	return c.JSON(200, MapDbUser(&ret))
}

// @Summary      Edit user
// @Description  Edit an account info or permissions
// @Tags         users
// @Accept       json
// @Produce      json
// @Security     Jwt[users.write]
// @Param        id       path  string  false  "User id of the user to edit" Format(uuid)
// @Param        user     body  EditUserDto  false  "Edited user info"
// @Success      200  {object}  User
// @Success      403  {object}  KError  "You don't have permissions to edit another account"
// @Success      422  {object}  KError  "Invalid body"
// @Router /users/{id} [patch]
func (h *Handler) EditUser(c *echo.Context) error {
	ctx := c.Request().Context()
	err := CheckPermissions(c, []string{"users.write"})
	if err != nil {
		return err
	}

	uid, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return echo.NewHTTPError(400, "Invalid id given: not an uuid")
	}

	var req EditUserDto
	err = c.Bind(&req)
	if err != nil {
		return echo.NewHTTPError(http.StatusUnprocessableEntity, err.Error())
	}
	if err = c.Validate(&req); err != nil {
		return err
	}

	ret, err := h.db.UpdateUser(ctx, dbc.UpdateUserParams{
		Id:       uid,
		Username: req.Username,
		Email:    req.Email,
		Claims:   req.Claims,
	})
	if err == pgx.ErrNoRows {
		return echo.NewHTTPError(http.StatusNotFound, "Invalid user id, user not found")
	} else if err != nil {
		return err
	}

	return c.JSON(200, MapDbUser(&ret))
}

// @Summary      Edit password
// @Description  Edit your password
// @Tags         users
// @Accept       json
// @Produce      json
// @Security     Jwt
// @Param        invalidate  query  bool  false  "Invalidate other sessions" default(true)
// @Param        user     body  EditPasswordDto  false  "New password"
// @Success      204
// @Success      422  {object}  KError  "Invalid body"
// @Router /users/me/password [patch]
func (h *Handler) ChangePassword(c *echo.Context) error {
	ctx := c.Request().Context()
	uid, err := GetCurrentUserId(c)
	if err != nil {
		return err
	}
	user, err := h.db.GetUser(ctx, dbc.GetUserParams{
		UseId: true,
		Id:    uid,
	})

	sid, err := GetCurrentSessionId(c)
	if err != nil {
		return err
	}

	var req EditPasswordDto
	err = c.Bind(&req)
	if err != nil {
		return echo.NewHTTPError(http.StatusUnprocessableEntity, err.Error())
	}
	if err = c.Validate(&req); err != nil {
		return err
	}

	if user.User.Password != nil {
		if req.OldPassword == nil {
			return echo.NewHTTPError(http.StatusUnprocessableEntity, "Missing old password")
		}
		match, err := argon2id.ComparePasswordAndHash(
			*req.OldPassword,
			*user.User.Password,
		)
		if err != nil {
			return err
		}
		if !match {
			return echo.NewHTTPError(http.StatusForbidden, "Invalid password")
		}
	}

	pass, err := argon2id.CreateHash(req.NewPassword, argon2id.DefaultParams)
	if err != nil {
		return err
	}
	_, err = h.db.UpdateUser(ctx, dbc.UpdateUserParams{
		Id:       uid,
		Password: &pass,
	})
	if err != nil {
		return err
	}

	err = h.db.ClearOtherSessions(ctx, dbc.ClearOtherSessionsParams{
		SessionId: sid,
		UserId:    uid,
	})
	if err != nil {
		return err
	}

	return c.NoContent(http.StatusNoContent)
}
