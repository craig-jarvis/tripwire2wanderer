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
	PositionY     float64 `json:"position_y,omitzero"`
	PositionX     float64 `json:"position_x,omitzero"`
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
