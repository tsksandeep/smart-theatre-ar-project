package server

import (
	"github.com/Subtitles/pb"
)

type Broker struct {
	stopCh    chan struct{}
	publishCh chan pb.SubManyTimesResponse
	subCh     chan chan pb.SubManyTimesResponse
	unsubCh   chan chan pb.SubManyTimesResponse
}

func NewBroker() *Broker {
	return &Broker{
		stopCh:    make(chan struct{}),
		publishCh: make(chan pb.SubManyTimesResponse, 1),
		subCh:     make(chan chan pb.SubManyTimesResponse, 1),
		unsubCh:   make(chan chan pb.SubManyTimesResponse, 1),
	}
}

func (b *Broker) Start() {
	subs := map[chan pb.SubManyTimesResponse]struct{}{}
	for {
		select {
		case <-b.stopCh:
			return
		case msgCh := <-b.subCh:
			subs[msgCh] = struct{}{}
		case msgCh := <-b.unsubCh:
			delete(subs, msgCh)
		case msg := <-b.publishCh:
			for msgCh := range subs {
				select {
				case msgCh <- msg:
				default:
				}
			}
		}
	}
}

func (b *Broker) Stop() {
	close(b.stopCh)
}

func (b *Broker) Subscribe() chan pb.SubManyTimesResponse {
	msgCh := make(chan pb.SubManyTimesResponse , 5)
	b.subCh <- msgCh
	return msgCh
}

func (b *Broker) Unsubscribe(msgCh chan pb.SubManyTimesResponse) {
	b.unsubCh <- msgCh
}

func (b *Broker) Publish(msg pb.SubManyTimesResponse) {
	b.publishCh <- msg
}