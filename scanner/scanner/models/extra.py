from enum import Enum

from ..utils import Model


class ExtraKind(str, Enum):
	OTHER = "other"
	TRAILER = "trailer"
	INTERVIEW = "interview"
	BEHIND_THE_SCENE = "behind-the-scene"
	DELETED_SCENE = "deleted-scene"
	BLOOPER = "blooper"


class Extra(Model):
	kind: ExtraKind
	slug: str
	name: str
	runtime: int | None
	thumbnail: str | None
	video: str
