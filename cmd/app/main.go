package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"time"

	"github.com/craig-jarvis/tripwire2wanderer/internal/config"
	"github.com/craig-jarvis/tripwire2wanderer/internal/data"
	"github.com/craig-jarvis/tripwire2wanderer/internal/tripwire"
	"github.com/craig-jarvis/tripwire2wanderer/internal/wanderer"
)

func main() {
	// Load configuration from .env and environment variables
	cfg, err := config.Load()
	if err != nil {
		log.Fatalf("failed to load config: %v", err)
	}

	// Create Tripwire API client
	twClient := tripwire.New(cfg)

	// Create Wanderer API client
	wClient := wanderer.New(cfg)

	// Fetch wormholes
	wormholes, err := twClient.GetWormholes()
	if err != nil {
		log.Fatalf("failed to get wormholes: %v", err)
	}

	// Fetch signatures
	signatures, err := twClient.GetSignatures()
	if err != nil {
		log.Fatalf("failed to get signatures: %v", err)
	}

	// Output counts
	fmt.Printf("Wormholes: %d\n", len(wormholes))
	fmt.Printf("Signatures: %d\n", len(signatures))

	// Get current systems and connections from Wanderer
	ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancel()

	fmt.Println("\n--- Fetching current Wanderer data ---")
	wandererData, err := wClient.GetSystemsAndConnections(ctx)
	if err != nil {
		log.Fatalf("failed to get Wanderer data: %v", err)
	}

	// cfg.WandererHomeSystemID is now an int - no conversion needed
	mapResult := BuildWandererMapRecursive(cfg.WandererHomeSystemID, signatures, wormholes)
	log.Printf("wanderer request %d connections and %d systems", len(mapResult.Data.Connections), len(mapResult.Data.Systems))
	mapResult = data.DedupWandererEnvelope(mapResult)
	mapResult = data.CalculateSystemPositions(mapResult, cfg.WandererHomeSystemID, float64(cfg.PositionXSeparation), float64(cfg.PositionYSeparation))

	mapResultJSON, err := json.MarshalIndent(mapResult, "", "  ")
	if err != nil {
		log.Fatalf("failed to marshal Wanderer map data: %v", err)
	}
	fmt.Println(string(mapResultJSON))

	// fmt.Println("\nBuilt Wanderer Map Data:")

	deleteRequest := data.CompareWandererEnvelopes(*wandererData, mapResult)

	// // Delete no longer on map systems and connections
	// fmt.Println("\n--- Deleting old Wanderer data ---")
	err = wClient.DeleteSystemsAndConnections(deleteRequest)
	// if err != nil {
	// 	log.Fatalf("failed to delete Wanderer data: %v", err)
	// }

	// // Post current systems and connections for Wanderer
	fmt.Println("\n--- Adding/updating new sigs and connections ---")
	wandererResponse, err := wClient.SubmitConnectionsAndSystems(&mapResult)
	if err != nil {
		log.Fatalf("failed to get Wanderer data: %v", err)
	}

	wandererJSON, err := json.MarshalIndent(wandererResponse, "", "  ")

	fmt.Println(string(wandererJSON))
}
