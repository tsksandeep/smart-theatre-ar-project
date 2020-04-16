package server

import (
	"io/ioutil"
	"log"
	"strings"
	"time"

	"github.com/martinlindhe/subtitles"

	"github.com/Subtitles/pb"
)

const (
	defaultTime = "0000-01-01 00:00:00.0"
	timeFormat  = "2006-01-02 15:04:05.0"
)

func Initiate(b Broker) {

	data, err := ioutil.ReadFile("/Users/sandeepkumar/go/src/github.com/Subtitles/sample/sample.srt")
	if err != nil {
		log.Fatal(err)
	}

	t, err := time.Parse(timeFormat, defaultTime)
	if err != nil {
		log.Fatal(err)
	}

	res, err := subtitles.NewFromSRT(string(data))
	if err != nil {
		log.Fatal(err)
	}

	for i, caption := range res.Captions {
		if i == 0 {
			time.Sleep(caption.Start.Sub(t))

			b.Publish(pb.SubManyTimesResponse{
				Result: strings.Join(caption.Text, " "),
			})

			time.Sleep(caption.End.Sub(caption.Start))
		} else {
			b.Publish(pb.SubManyTimesResponse{
				Result: "",
			})

			time.Sleep(caption.Start.Sub(res.Captions[i-1].End))

			b.Publish(pb.SubManyTimesResponse{
				Result: strings.Join(caption.Text, " "),
			})

			time.Sleep(caption.End.Sub(caption.Start))
		}
	}
}

//SRT parses the srt.file and sends the individual sentences
func SRT(stream pb.SubsService_SubManyTimesServer, subCh chan pb.SubManyTimesResponse) error {
	for {
		subResponse, ok := <-subCh
		if !ok {
			log.Fatal(ok)
		}
		err := stream.Send(&subResponse)
		if err != nil {
			return err
		}
	}
}
