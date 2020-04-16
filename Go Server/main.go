package main

import (
	"context"
	"fmt"
	"log"
	"net"

	"google.golang.org/grpc"

	"github.com/Subtitles/pb"
	"github.com/Subtitles/server"
)

var ipAddr string

var setAnchor *Anchor

type Anchor struct {
	PositionX float32
	PositionY float32
	PositionZ float32
	RotationW float32
	RotationX float32
	RotationY float32
	RotationZ float32
}

type Server struct {
	broker server.Broker
}

func (s *Server) SubGet(context.Context, *pb.SubGetRequest) (*pb.SubGetResponse, error) {
	log.Println("SubGet called")
	return &pb.SubGetResponse{
		AnchorPositionX: setAnchor.PositionX,
		AnchorPositionY: setAnchor.PositionY,
		AnchorPositionZ: setAnchor.PositionZ,
		AnchorRotationW: setAnchor.RotationW,
		AnchorRotationX: setAnchor.RotationX,
		AnchorRotationY: setAnchor.RotationY,
		AnchorRotationZ: setAnchor.RotationZ,
	}, nil
}

func (s *Server) SubSet(c context.Context, req *pb.SubSetRequest) (*pb.SubSetResponse, error) {
	log.Println("SubSet called")
	setAnchor = &Anchor{
		PositionX: req.AnchorPositionX,
		PositionY: req.AnchorPositionY,
		PositionZ: req.AnchorPositionZ,
		RotationW: req.AnchorRotationW,
		RotationX: req.AnchorRotationX,
		RotationY: req.AnchorPositionY,
		RotationZ: req.AnchorRotationZ,
	}
	log.Println(*setAnchor)

	go server.Initiate(s.broker)

	return &pb.SubSetResponse{
		IsSet: true,
	}, nil
}

func (s *Server) SubInitialCheck(context.Context, *pb.SubInitialCheckRequest) (*pb.SubInitialCheckResponse, error) {
	log.Println("SubInitialCheck called")

	if setAnchor != nil {
		return &pb.SubInitialCheckResponse{
			IsSet: true,
		}, nil
	}

	return &pb.SubInitialCheckResponse{
		IsSet: false,
	}, nil
}

func (s *Server) SubManyTimes(req *pb.SubManyTimesRequest, stream pb.SubsService_SubManyTimesServer) error {
	log.Printf("connected %s", req.IpAddr)
	subCh := s.broker.Subscribe()

	err := server.SRT(stream, subCh)
	if err != nil {
		log.Printf("disconnected %s", req.IpAddr)
		s.broker.Unsubscribe(subCh)
	}

	return nil
}

func main() {

	addrs, err := net.InterfaceAddrs()
	if err != nil {
		log.Fatal(err)
	}

	for _, a := range addrs {
		if ipnet, ok := a.(*net.IPNet); ok && !ipnet.IP.IsLoopback() {
			if ipnet.IP.To4() != nil {
				ipAddr = ipnet.IP.String()
			}
		}
	}

	broker := server.NewBroker()

	server := Server{
		broker: *broker,
	}

	go server.broker.Start()

	lis, err := net.Listen("tcp", fmt.Sprintf("%s:50051", ipAddr))
	if err != nil {
		log.Fatalf("Failed to listen: %v", err)
	} else {
		log.Printf("Running in %s:50051", ipAddr)
	}
	s := grpc.NewServer()
	pb.RegisterSubsServiceServer(s, &server)

	if err := s.Serve(lis); err != nil {
		log.Fatalf("Failed to serve: %v", err)
	}
}
