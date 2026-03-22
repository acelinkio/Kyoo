from typing import Generic, TypeVar

from ..utils import Model

T = TypeVar("T")


class Page(Model, Generic[T]):
	items: list[T]
	this_: str
	next: str | None = None
