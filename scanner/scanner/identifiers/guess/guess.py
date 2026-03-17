from typing import Any, cast

from guessit.api import default_api
from rebulk import Rebulk
from rebulk.match import Match

from . import rules

default_api.configure({})
rblk = cast(Rebulk, default_api.rebulk).rules(rules)


def guessit(
	name: str,
	*,
	extra_flags: dict[str, Any] = {},
) -> dict[str, list[Match]]:
	return default_api.guessit(
		name,
		{
			"episode_prefer_number": True,
			"excludes": "language",
			"enforce_list": True,
			"advanced": True,
		}
		| extra_flags,
	)
