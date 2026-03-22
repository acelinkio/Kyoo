from typing import Annotated, Literal

from fastapi import APIRouter, BackgroundTasks, Depends, Security

from ..fsscan import create_scanner
from ..identifiers.identify import identify
from ..jwt import validate_bearer
from ..models.movie import SearchMovie
from ..models.request import CreateRequest, Request, RequestRet
from ..models.serie import SearchSerie
from ..models.videos import Video
from ..providers.composite import CompositeProvider
from ..requests import RequestCreator
from ..status import StatusService
from ..utils import Language
from .dependencies import get_preferred_languages, get_provider, get_request_creator

router = APIRouter()


@router.get("/scan")
async def get_scan_status(
	svc: Annotated[StatusService, Depends(StatusService.create)],
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.trigger"])],
	status: Literal["pending", "running", "failed"] | None = None,
) -> list[RequestRet]:
	"""
	Get scan status, know what tasks are running, pending or failed.
	"""

	return await svc.list_requests(status=status)


@router.put(
	"/scan",
	status_code=204,
	response_description="Scan started.",
)
async def trigger_scan(
	tasks: BackgroundTasks,
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.trigger"])],
):
	"""
	Trigger a full scan of the filesystem, trying to find new videos & deleting old ones.
	"""

	async def run():
		async with create_scanner() as scanner:
			await scanner.scan()

	tasks.add_task(run)


@router.get(
	"/guess",
	status_code=200,
	response_description="Identify a path",
)
async def get_guess(
	path: str,
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.guess"])],
) -> Video:
	"""
	Identify a video path and return a serie/movie guess.
	"""

	return await identify(path)


@router.get(
	"/movies",
	status_code=200,
	response_description="Found movies",
)
async def get_movies(
	provider: Annotated[CompositeProvider, Depends(get_provider)],
	language: Annotated[list[Language], Depends(get_preferred_languages)],
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.search"])],
	query: str,
	year: int | None = None,
) -> list[SearchMovie]:
	"""
	Search for a movie
	"""

	return await provider.search_movies(query, year=year, language=language)


@router.get(
	"/series",
	status_code=200,
	response_description="Found series",
)
async def get_series(
	provider: Annotated[CompositeProvider, Depends(get_provider)],
	language: Annotated[list[Language], Depends(get_preferred_languages)],
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.search"])],
	query: str,
	year: int | None = None,
) -> list[SearchSerie]:
	"""
	Search for a serie
	"""

	return await provider.search_series(query, year=year, language=language)


@router.post(
	"/movies",
	status_code=201,
	response_description="Movie metadata request created.",
)
async def create_movie(
	body: CreateRequest,
	requests: Annotated[RequestCreator, Depends(get_request_creator)],
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.add"])],
) -> RequestRet:
	"""
	Create a movie metadata request.
	"""

	[ret] = await requests.enqueue(
		[
			Request(
				kind="movie",
				title=body.title,
				year=body.year,
				external_id=body.external_id,
				videos=body.videos,
			)
		]
	)
	return ret


@router.post(
	"/series",
	status_code=201,
	response_description="Series metadata request created.",
)
async def create_serie(
	body: CreateRequest,
	requests: Annotated[RequestCreator, Depends(get_request_creator)],
	_: Annotated[None, Security(validate_bearer, scopes=["scanner.add"])],
) -> RequestRet:
	"""
	Create a series metadata request.
	"""

	[ret] = await requests.enqueue(
		[
			Request(
				kind="episode",
				title=body.title,
				year=body.year,
				external_id=body.external_id,
				videos=body.videos,
			)
		]
	)
	return ret
