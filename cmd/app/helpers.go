package main

import (
	"errors"
	"fmt"
	"log"
	"strconv"
	"strings"

	"github.com/craig-jarvis/tripwire2wanderer/internal/data"
)

const MinValidSystemID = 10000

func GetTripwireSignaturesAndWormholesBySystemID(systemID string, signatures []data.TripwireSignature, wormholes []data.TripwireWormhole) (filteredSignatures []data.TripwireSignature, filteredWormholes []data.TripwireWormhole) {

	// Get all signatures for the system
	for _, sig := range signatures {
		if sig.SystemID == systemID {
			filteredSignatures = append(filteredSignatures, sig)
		}
	}

	// Get all wormholes where the initialId is the system
	for _, sig := range filteredSignatures {
		for _, wh := range wormholes {
			if sig.ID == wh.InitialID {
				filteredWormholes = append(filteredWormholes, wh)
			}
		}
	}

	return filteredSignatures, filteredWormholes
}

// BuildWandererMapRecursive builds a map starting from the given system ID
func BuildWandererMapRecursive(systemID int, signatures []data.TripwireSignature, wormholes []data.TripwireWormhole) data.WandererConnectionsAndSystemsEnvelope {
	visited := make(map[string]bool)
	systems := make([]data.WandererSystem, 0)
	connections := make([]data.WandererConnection, 0)

	// Convert int to string for internal processing
	systemIDStr := strconv.Itoa(systemID)
	buildMapRecursive(systemIDStr, signatures, wormholes, visited, &systems, &connections)

	return data.WandererConnectionsAndSystemsEnvelope{
		Data: data.WandererConnectionsAndSystems{
			Systems:     systems,
			Connections: connections,
		},
	}
}

func buildMapRecursive(systemID string, signatures []data.TripwireSignature, wormholes []data.TripwireWormhole, visited map[string]bool, systems *[]data.WandererSystem, connections *[]data.WandererConnection) {
	if visited[systemID] {
		return
	}
	visited[systemID] = true

	// Get signatures for this system
	filteredSigs, filteredWormholes := GetTripwireSignaturesAndWormholesBySystemID(systemID, signatures, wormholes)

	// Add system (use first signature)
	if len(filteredSigs) > 0 {
		system, err := NewWandererSystemFromTripwireSignature(filteredSigs[0])
		if err != nil {
			log.Printf("Warning: failed to create system from signature %v: %v", filteredSigs[0], err)
		} else if system.SolarSystemID >= MinValidSystemID {
			*systems = append(*systems, system)
		}
	}

	// Process wormholes and recurse to target systems
	for _, wormhole := range filteredWormholes {
		connection, err := data.NewWandererConnectionFromTripwireWormhole(wormhole, &signatures)
		if err != nil {
			log.Printf("Warning: failed to create connection from wormhole %v: %v", wormhole, err)
			continue
		}
		if connection.SolarSystemSource == 0 || connection.SolarSystemTarget == 0 {
			continue
		}
		*connections = append(*connections, connection)

		// Find target system and recurse
		targetSig, err := data.FindSignatureByID(wormhole.SecondaryID, &signatures)
		if err != nil {
			continue
		}
		if targetSig.SystemID != "0" && !visited[targetSig.SystemID] {
			buildMapRecursive(targetSig.SystemID, signatures, wormholes, visited, systems, connections)
		}
	}
}

func CompareWandererEnvelopes(current *data.WandererConnectionsAndSystemsEnvelope, new data.WandererConnectionsAndSystemsEnvelope) data.WandererSystemAndConnectionsDeleteRequest {
	deleteRequest := data.WandererSystemAndConnectionsDeleteRequest{
		ConnectionIds: make([]string, 0),
		SystemIds:     make([]int, 0),
	}

	// Build a map of new systems by SolarSystemID for quick lookup
	newSystemMap := make(map[int]bool)
	for _, system := range new.Data.Systems {
		newSystemMap[system.SolarSystemID] = true
	}

	// Find systems in current that are not in new
	for _, system := range current.Data.Systems {
		if !newSystemMap[system.SolarSystemID] {
			deleteRequest.SystemIds = append(deleteRequest.SystemIds, system.SolarSystemID)
		}
	}

	// Build a map of new connections by source and target for quick lookup
	newConnectionMap := make(map[[2]int]bool)
	for _, conn := range new.Data.Connections {
		newConnectionMap[[2]int{conn.SolarSystemSource, conn.SolarSystemTarget}] = true
	}

	// Find connections in current that are not in new
	for _, conn := range current.Data.Connections {
		key := [2]int{conn.SolarSystemSource, conn.SolarSystemTarget}
		if !newConnectionMap[key] {
			deleteRequest.ConnectionIds = append(deleteRequest.ConnectionIds, conn.ID)
		}
	}

	return deleteRequest
}

func ConvertTripwireSigIdToEveId(sigID string) (string, error) {
	if sigID == "???" {
		return "", nil
	}

	if len(sigID) != 6 {
		return "", errors.New("invalid signature id")
	}

	letters := strings.ToUpper(sigID[:3])
	numbers := sigID[3:]

	return letters + "-" + numbers, nil
}

func NewWandererSignatureFromTripwireSignature(twSignature data.TripwireSignature) data.WandererSignature {
	solarSystemID, _ := strconv.Atoi(twSignature.SystemID)

	sigId, err := ConvertTripwireSigIdToEveId(twSignature.SignatureID)
	if err != nil {
		sigId = ""
	}

	return data.WandererSignature{
		CharacterEveID: twSignature.CreatedByID,
		EveID:          sigId,
		Group:          twSignature.Type,
		Kind:           "Cosmic Signature",
		Name:           twSignature.Name,
		SolarSystemID:  solarSystemID,
	}
}

func NewWandererSystemFromTripwireSignature(twSignature data.TripwireSignature) (data.WandererSystem, error) {
	var system data.WandererSystem

	if len(twSignature.SystemID) < 5 {
		return system, fmt.Errorf("invalid system ID: %s", twSignature.SystemID)
	}

	systemID, err := strconv.Atoi(twSignature.SystemID)
	if err != nil {
		return system, fmt.Errorf("failed to parse system ID: %w", err)
	}

	system.SolarSystemID = systemID
	system.Visible = true
	return system, nil
}
