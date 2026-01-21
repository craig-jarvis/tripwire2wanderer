package data

import (
	"errors"
	"strconv"
	"time"
)

// WandererConnection represents a single wormhole/connection in Wanderer
type WandererConnection struct {
	CustomInfo        string `json:"custom_info,omitzero"`
	ID                string `json:"id,omitzero"`
	Locked            bool   `json:"locked"`
	MapID             string `json:"map_id,omitzero"`
	MassStatus        int    `json:"mass_status,omitzero"`
	ShipSizeType      int    `json:"ship_size_type,omitzero"`
	SolarSystemSource int    `json:"solar_system_source,omitzero"`
	SolarSystemTarget int    `json:"solar_system_target,omitzero"`
	TimeStatus        int    `json:"time_status,omitzero"`
	Type              int    `json:"type,omitzero"`
	WormholeType      string `json:"wormhole_type,omitzero"`
}

type WandererSystem struct {
	ID            string  `json:"id,omitzero"`
	Name          string  `json:"name,omitzero"`
	Status        int     `json:"status,omitzero"`
	Tag           *string `json:"tag,omitzero"`
	Visible       bool    `json:"visible,omitzero"`
	Description   *string `json:"description,omitzero"`
	Labels        *string `json:"labels,omitzero"`
	InsertedAt    string  `json:"inserted_at,omitzero"`
	UpdatedAt     string  `json:"updated_at,omitzero"`
	Locked        bool    `json:"locked,omitzero"`
	MapID         string  `json:"map_id,omitzero"`
	TemporaryName *string `json:"temporary_name,omitzero"`
	SolarSystemID int     `json:"solar_system_id,omitzero"`
	PositionY     float64 `json:"position_y"`
	PositionX     float64 `json:"position_x"`
	CustomName    *string `json:"custom_name,omitzero"`
	OriginalName  string  `json:"original_name,omitzero"`
}

type WandererSignature struct {
	CharacterEveID string    `json:"character_eve_id,omitzero"`
	CustomInfo     string    `json:"custom_info,omitzero"`
	Description    string    `json:"description,omitzero"`
	EveID          string    `json:"eve_id,omitzero"`
	Group          string    `json:"group,omitzero"`
	ID             string    `json:"id,omitzero"`
	InsertedAt     time.Time `json:"inserted_at,omitzero"`
	Kind           string    `json:"kind,omitzero"`
	LinkedSystemID int       `json:"linked_system_id,omitzero"`
	Name           string    `json:"name,omitzero"`
	SolarSystemID  int       `json:"solar_system_id,omitzero"`
	Type           string    `json:"type,omitzero"`
	Updated        int       `json:"updated,omitzero"`
	UpdatedAt      time.Time `json:"updated_at,omitzero"`
}

// WandererConnectionsAndSystems represents a map with its connections and systems
type WandererConnectionsAndSystems struct {
	Connections []WandererConnection `json:"connections,omitzero"`
	Systems     []WandererSystem     `json:"systems,omitzero"`
}

// WandererConnectionsAndSystemsEnvelope represents the response from Wanderer API for connections and systems
type WandererConnectionsAndSystemsEnvelope struct {
	Data WandererConnectionsAndSystems `json:"data,omitzero"`
}

// WandererConnectionAndSystemCreateResponse represents a map with its connections and systems
type WandererConnectionAndSystemCreateResponse struct {
	Connections WandererConnectionAndSystemResponse `json:"connections,omitzero"`
	Systems     WandererConnectionAndSystemResponse `json:"systems,omitzero"`
}

type WandererConnectionAndSystemResponse struct {
	Updated int `json:"updated,omitzero"`
	Created int `json:"created,omitzero"`
}

type WandererConnectionAndSystemCreateResponseEnvelope struct {
	Data WandererConnectionAndSystemCreateResponse `json:"data,omitzero"`
}

type WandererSystemAndConnectionsDeleteRequest struct {
	ConnectionIds []string `json:"connection_ids,omitzero"`
	SystemIds     []int    `json:"system_ids,omitzero"`
}

func NewWandererSystemFromTripwireSignature(twSignature TripwireSignature) WandererSystem {
	var system WandererSystem

	if len(twSignature.SystemID) < 5 {
		return system
	}
	system.SolarSystemID, _ = strconv.Atoi(twSignature.SystemID)
	system.Visible = true
	return system
}

func NewWandererConnectionFromTripwireWormhole(twWormhole TripwireWormhole, signatures *[]TripwireSignature) (WandererConnection, error) {
	var connection WandererConnection

	sourceSig, err := FindSignatureByID(twWormhole.InitialID, signatures)
	if err != nil {
		return connection, err
	}
	if len(sourceSig.SystemID) < 5 {
		return connection, nil
	}
	sysId, err := strconv.Atoi(sourceSig.SystemID)
	if err != nil {
		return connection, err
	}
	connection.SolarSystemSource = sysId

	targetSig, err := FindSignatureByID(twWormhole.SecondaryID, signatures)
	if err != nil {
		return connection, err
	}
	if len(targetSig.SystemID) < 5 {
		return connection, nil
	}
	sysId, err = strconv.Atoi(targetSig.SystemID)
	if err != nil {
		return connection, err
	}
	connection.SolarSystemTarget = sysId

	return connection, nil
}

func FindSignatureByID(id string, signatures *[]TripwireSignature) (TripwireSignature, error) {
	var emptySig TripwireSignature
	if signatures == nil {
		return emptySig, errors.New("signatures is nil")
	}

	for i := range *signatures {
		if (*signatures)[i].ID == id {
			return (*signatures)[i], nil
		}
	}
	return emptySig, errors.New("sig not found")
}

func CompareWandererEnvelopes(current, new WandererConnectionsAndSystemsEnvelope) WandererSystemAndConnectionsDeleteRequest {
	deleteRequest := WandererSystemAndConnectionsDeleteRequest{
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

// DedupWandererEnvelope removes duplicate systems and connections from the envelope
func DedupWandererEnvelope(envelope WandererConnectionsAndSystemsEnvelope) WandererConnectionsAndSystemsEnvelope {
	deduped := WandererConnectionsAndSystemsEnvelope{
		Data: WandererConnectionsAndSystems{
			Systems:     make([]WandererSystem, 0),
			Connections: make([]WandererConnection, 0),
		},
	}

	// Deduplicate systems by SolarSystemID
	systemMap := make(map[int]WandererSystem)
	for _, system := range envelope.Data.Systems {
		if system.SolarSystemID != 0 {
			// Keep the first occurrence
			if _, exists := systemMap[system.SolarSystemID]; !exists {
				systemMap[system.SolarSystemID] = system
			}
		}
	}

	// Convert map back to slice
	for _, system := range systemMap {
		deduped.Data.Systems = append(deduped.Data.Systems, system)
	}

	// Deduplicate connections by source-target pair
	connectionMap := make(map[[2]int]WandererConnection)
	for _, conn := range envelope.Data.Connections {
		key := [2]int{conn.SolarSystemSource, conn.SolarSystemTarget}
		// Keep the first occurrence
		if _, exists := connectionMap[key]; !exists {
			connectionMap[key] = conn
		}
	}

	// Convert map back to slice
	for _, conn := range connectionMap {
		deduped.Data.Connections = append(deduped.Data.Connections, conn)
	}

	return deduped
}

// CalculateSystemPositions calculates position X and Y for all systems in a tree layout
// starting from the home system. Parents are positioned to the left (lower Y) and centered
// between their children horizontally.
func CalculateSystemPositions(envelope WandererConnectionsAndSystemsEnvelope, homeSystemID int, positionXSeparation, positionYSeparation float64) WandererConnectionsAndSystemsEnvelope {
	result := envelope

	// Build adjacency map for the tree
	adjacency := make(map[int][]int)
	for _, conn := range envelope.Data.Connections {
		adjacency[conn.SolarSystemSource] = append(adjacency[conn.SolarSystemSource], conn.SolarSystemTarget)
		adjacency[conn.SolarSystemTarget] = append(adjacency[conn.SolarSystemTarget], conn.SolarSystemSource)
	}

	// Build system index map
	systemMap := make(map[int]*WandererSystem)
	for i := range result.Data.Systems {
		systemMap[result.Data.Systems[i].SolarSystemID] = &result.Data.Systems[i]
	}

	// If home system doesn't exist, return unchanged
	if _, exists := systemMap[homeSystemID]; !exists {
		return result
	}

	// Build tree structure using BFS from home system
	visited := make(map[int]bool)
	parent := make(map[int]int)
	children := make(map[int][]int)
	queue := []int{homeSystemID}
	visited[homeSystemID] = true

	for len(queue) > 0 {
		current := queue[0]
		queue = queue[1:]

		for _, neighbor := range adjacency[current] {
			if !visited[neighbor] {
				visited[neighbor] = true
				parent[neighbor] = current
				children[current] = append(children[current], neighbor)
				queue = append(queue, neighbor)
			}
		}
	}

	// Calculate positions accounting for child tree sizes
	positions := make(map[int]struct{ x, y float64 })
	nextYPosition := make(map[int]float64) // Track next available Y position at each depth level
	gridSize := 15.0                       // Grid increment size

	var calculatePosition func(systemID int, depth float64) float64
	calculatePosition = func(systemID int, depth float64) float64 {
		childList := children[systemID]
		level := int(depth)

		// Position current system at next available Y in this column
		y := nextYPosition[level]
		positions[systemID] = struct{ x, y float64 }{x: depth, y: y}

		if len(childList) == 0 {
			// Leaf node - just increment for next sibling
			nextYPosition[level] = y + positionYSeparation
			return positionYSeparation
		}

		// First child starts at same Y as parent
		nextYPosition[int(depth+positionXSeparation)] = y

		// Process children and track total height needed
		totalChildHeight := 0.0
		var firstChildY, lastChildY float64
		for i, childID := range childList {
			childHeight := calculatePosition(childID, depth+positionXSeparation)
			totalChildHeight += childHeight
			if i == 0 {
				firstChildY = positions[childID].y
			}
			lastChildY = positions[childID].y
		}

		// Adjust parent position based on children
		// Exception: don't move the home system itself
		if systemID != homeSystemID {
			if len(childList) == 1 {
				// Single child: parent aligns with child's final position
				positions[systemID] = struct{ x, y float64 }{x: depth, y: firstChildY}
			} else {
				// Multiple children: center parent between first and last child, snapped to grid
				centerY := (firstChildY + lastChildY) / 2.0
				// Round to nearest grid position
				centerY = float64(int(centerY/gridSize+0.5)) * gridSize
				positions[systemID] = struct{ x, y float64 }{x: depth, y: centerY}
			}
		}
		// If this is the home system, keep it at its original position

		// Update parent's column position to account for children's space
		nextYPosition[level] = y + totalChildHeight

		return totalChildHeight
	}

	// Start calculation from home system at X=0
	calculatePosition(homeSystemID, 0)

	// Special case: if home system has only one child, shift all descendants
	// so the first-level child aligns with home's Y position
	if len(children[homeSystemID]) == 1 {
		homeY := positions[homeSystemID].y
		firstChildID := children[homeSystemID][0]
		firstChildY := positions[firstChildID].y
		yOffset := homeY - firstChildY

		// Apply offset to all descendants of home
		for systemID, pos := range positions {
			if systemID != homeSystemID {
				positions[systemID] = struct{ x, y float64 }{x: pos.x, y: pos.y + yOffset}
			}
		}
	}

	// Apply calculated positions to systems
	for systemID, pos := range positions {
		if sys, exists := systemMap[systemID]; exists {
			sys.PositionX = pos.x
			sys.PositionY = pos.y
		}
	}

	return result
}
