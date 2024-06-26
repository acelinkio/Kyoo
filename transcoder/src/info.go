package src

import (
	"cmp"
	"context"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"mime"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"sync"
	"time"

	"golang.org/x/text/language"
	"gopkg.in/vansante/go-ffprobe.v2"
)

type MediaInfo struct {
	// The sha1 of the video file.
	Sha string `json:"sha"`
	/// The internal path of the video file.
	Path string `json:"path"`
	/// The extension currently used to store this video file
	Extension string `json:"extension"`
	/// The whole mimetype (defined as the RFC 6381). ex: `video/mp4; codecs="avc1.640028, mp4a.40.2"`
	MimeCodec *string `json:"mimeCodec"`
	/// The file size of the video file.
	Size uint64 `json:"size"`
	/// The length of the media in seconds.
	Duration float32 `json:"duration"`
	/// The container of the video file of this episode.
	Container *string `json:"container"`
	/// The video codec and informations.
	Video *Video `json:"video"`
	/// The list of videos if there are multiples.
	Videos []Video `json:"videos"`
	/// The list of audio tracks.
	Audios []Audio `json:"audios"`
	/// The list of subtitles tracks.
	Subtitles []Subtitle `json:"subtitles"`
	/// The list of fonts that can be used to display subtitles.
	Fonts []string `json:"fonts"`
	/// The list of chapters. See Chapter for more information.
	Chapters []Chapter `json:"chapters"`
}

type Video struct {
	/// The human readable codec name.
	Codec string `json:"codec"`
	/// The codec of this stream (defined as the RFC 6381).
	MimeCodec *string `json:"mimeCodec"`
	/// The language of this stream (as a ISO-639-2 language code)
	Language *string `json:"language"`
	/// The max quality of this video track.
	Quality Quality `json:"quality"`
	/// The width of the video stream
	Width uint32 `json:"width"`
	/// The height of the video stream
	Height uint32 `json:"height"`
	/// The average bitrate of the video in bytes/s
	Bitrate uint32 `json:"bitrate"`
}

type Audio struct {
	/// The index of this track on the media.
	Index uint32 `json:"index"`
	/// The title of the stream.
	Title *string `json:"title"`
	/// The language of this stream (as a IETF-BCP-47 language code)
	Language *string `json:"language"`
	/// The human readable codec name.
	Codec string `json:"codec"`
	/// The codec of this stream (defined as the RFC 6381).
	MimeCodec *string `json:"mimeCodec"`
	/// Is this stream the default one of it's type?
	IsDefault bool `json:"isDefault"`
	/// Is this stream tagged as forced? (useful only for subtitles)
	IsForced bool `json:"isForced"`
}

type Subtitle struct {
	/// The index of this track on the media.
	Index uint32 `json:"index"`
	/// The title of the stream.
	Title *string `json:"title"`
	/// The language of this stream (as a IETF-BCP-47 language code)
	Language *string `json:"language"`
	/// The codec of this stream.
	Codec string `json:"codec"`
	/// The extension for the codec.
	Extension *string `json:"extension"`
	/// Is this stream the default one of it's type?
	IsDefault bool `json:"isDefault"`
	/// Is this stream tagged as forced? (useful only for subtitles)
	IsForced bool `json:"isForced"`
	/// The link to access this subtitle.
	Link *string `json:"link"`
}

type Chapter struct {
	/// The start time of the chapter (in second from the start of the episode).
	StartTime float32 `json:"startTime"`
	/// The end time of the chapter (in second from the start of the episode).
	EndTime float32 `json:"endTime"`
	/// The name of this chapter. This should be a human-readable name that could be presented to the user.
	Name string `json:"name"`
	// TODO: add a type field for Opening, Credits...
}

func ParseFloat(str string) float32 {
	f, err := strconv.ParseFloat(str, 32)
	if err != nil {
		return 0
	}
	return float32(f)
}

func ParseUint(str string) uint32 {
	i, err := strconv.ParseUint(str, 10, 32)
	if err != nil {
		println(str)
		return 0
	}
	return uint32(i)
}

func ParseUint64(str string) uint64 {
	i, err := strconv.ParseUint(str, 10, 64)
	if err != nil {
		println(str)
		return 0
	}
	return i
}

func Map[T, U any](ts []T, f func(T, int) U) []U {
	us := make([]U, len(ts))
	for i := range ts {
		us[i] = f(ts[i], i)
	}
	return us
}

func MapStream[T any](streams []*ffprobe.Stream, kind ffprobe.StreamType, mapper func(*ffprobe.Stream, uint32) T) []T {
	count := 0
	for _, stream := range streams {
		if stream.CodecType == string(kind) {
			count++
		}
	}
	ret := make([]T, count)

	i := uint32(0)
	for _, stream := range streams {
		if stream.CodecType == string(kind) {
			ret[i] = mapper(stream, i)
			i++
		}
	}
	return ret
}

func OrNull(str string) *string {
	if str == "" {
		return nil
	}
	return &str
}

func NullIfUnd(str string) *string {
	if str == "und" {
		return nil
	}
	return &str
}

var SubtitleExtensions = map[string]string{
	"subrip": "srt",
	"ass":    "ass",
	"vtt":    "vtt",
}

type MICache struct {
	info  *MediaInfo
	ready sync.WaitGroup
}

var infos = NewCMap[string, *MICache]()

func GetInfo(path string, sha string) (*MediaInfo, error) {
	var err error

	ret, _ := infos.GetOrCreate(sha, func() *MICache {
		mi := &MICache{info: &MediaInfo{Sha: sha}}
		mi.ready.Add(1)
		go func() {
			save_path := fmt.Sprintf("%s/%s/info.json", Settings.Metadata, sha)
			if err := getSavedInfo(save_path, mi.info); err == nil {
				log.Printf("Using mediainfo cache on filesystem for %s", path)
				mi.ready.Done()
				return
			}

			var val *MediaInfo
			val, err = getInfo(path)
			if err == nil {
				*mi.info = *val
				mi.info.Sha = sha
			}
			mi.ready.Done()
			saveInfo(save_path, mi.info)
		}()
		return mi
	})
	ret.ready.Wait()
	return ret.info, err
}

func getSavedInfo[T any](save_path string, mi *T) error {
	saved_file, err := os.Open(save_path)
	if err != nil {
		return err
	}
	saved, err := io.ReadAll(saved_file)
	if err != nil {
		return err
	}
	err = json.Unmarshal([]byte(saved), mi)
	if err != nil {
		return err
	}
	return nil
}

func saveInfo[T any](save_path string, mi *T) error {
	content, err := json.Marshal(*mi)
	if err != nil {
		return err
	}
	return os.WriteFile(save_path, content, 0o644)
}

func getInfo(path string) (*MediaInfo, error) {
	defer printExecTime("mediainfo for %s", path)()

	ctx, cancelFn := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancelFn()

	mi, err := ffprobe.ProbeURL(ctx, path)
	if err != nil {
		return nil, err
	}

	ret := MediaInfo{
		Path: path,
		// Remove leading .
		Extension: filepath.Ext(path)[1:],
		Size:      ParseUint64(mi.Format.Size),
		Duration:  float32(mi.Format.DurationSeconds),
		Container: OrNull(mi.Format.FormatName),
		Videos: MapStream(mi.Streams, ffprobe.StreamVideo, func(stream *ffprobe.Stream, i uint32) Video {
			lang, _ := language.Parse(stream.Tags.Language)
			return Video{
				Codec:     stream.CodecName,
				MimeCodec: GetMimeCodec(stream),
				Language:  NullIfUnd(lang.String()),
				Quality:   QualityFromHeight(uint32(stream.Height)),
				Width:     uint32(stream.Width),
				Height:    uint32(stream.Height),
				// ffmpeg does not report bitrate in mkv files, fallback to bitrate of the whole container
				// (bigger than the result since it contains audio and other videos but better than nothing).
				Bitrate: ParseUint(cmp.Or(stream.BitRate, mi.Format.BitRate)),
			}
		}),
		Audios: MapStream(mi.Streams, ffprobe.StreamAudio, func(stream *ffprobe.Stream, i uint32) Audio {
			lang, _ := language.Parse(stream.Tags.Language)
			return Audio{
				Index:     i,
				Title:     OrNull(stream.Tags.Title),
				Language:  NullIfUnd(lang.String()),
				Codec:     stream.CodecName,
				MimeCodec: GetMimeCodec(stream),
				IsDefault: stream.Disposition.Default != 0,
				IsForced:  stream.Disposition.Forced != 0,
			}
		}),
		Subtitles: MapStream(mi.Streams, ffprobe.StreamSubtitle, func(stream *ffprobe.Stream, i uint32) Subtitle {
			extension := OrNull(SubtitleExtensions[stream.CodecName])
			var link *string
			if extension != nil {
				x := fmt.Sprintf("%s/%s/subtitle/%d.%s", Settings.RoutePrefix, base64.StdEncoding.EncodeToString([]byte(path)), i, *extension)
				link = &x
			}
			lang, _ := language.Parse(stream.Tags.Language)
			return Subtitle{
				Index:     uint32(i),
				Title:     OrNull(stream.Tags.Title),
				Language:  NullIfUnd(lang.String()),
				Codec:     stream.CodecName,
				Extension: extension,
				IsDefault: stream.Disposition.Default != 0,
				IsForced:  stream.Disposition.Forced != 0,
				Link:      link,
			}
		}),
		Chapters: Map(mi.Chapters, func(c *ffprobe.Chapter, _ int) Chapter {
			return Chapter{
				Name:      c.Title(),
				StartTime: float32(c.StartTimeSeconds),
				EndTime:   float32(c.EndTimeSeconds),
			}
		}),
		Fonts: MapStream(mi.Streams, ffprobe.StreamAttachment, func(stream *ffprobe.Stream, i uint32) string {
			font, _ := stream.TagList.GetString("filename")
			return fmt.Sprintf("%s/%s/attachment/%s", Settings.RoutePrefix, base64.StdEncoding.EncodeToString([]byte(path)), font)
		}),
	}
	var codecs []string
	if len(ret.Videos) > 0 && ret.Videos[0].MimeCodec != nil {
		codecs = append(codecs, *ret.Videos[0].MimeCodec)
	}
	if len(ret.Audios) > 0 && ret.Audios[0].MimeCodec != nil {
		codecs = append(codecs, *ret.Audios[0].MimeCodec)
	}
	container := mime.TypeByExtension(fmt.Sprintf(".%s", ret.Extension))
	if container != "" {
		if len(codecs) > 0 {
			codecs_str := strings.Join(codecs, ", ")
			mime := fmt.Sprintf("%s; codecs=\"%s\"", container, codecs_str)
			ret.MimeCodec = &mime
		} else {
			ret.MimeCodec = &container
		}
	}

	if len(ret.Videos) > 0 {
		ret.Video = &ret.Videos[0]
	}
	return &ret, nil
}
