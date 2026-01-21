package data

type TripwireWormhole struct {
	ID          string `json:"id"`
	InitialID   string `json:"initialID"`
	SecondaryID string `json:"secondaryID"`
	SigType     string `json:"type"`
	Parent      string `json:"parent"`
	Life        string `json:"life"`
	Mass        string `json:"mass"`
	MaskID      string `json:"maskID"`
}

type TripwireSignature struct {
	ID             string  `json:"id"`
	SignatureID    string  `json:"signatureID"`
	SystemID       string  `json:"systemID"`
	Type           string  `json:"type"`
	Name           string  `json:"name"`
	Bookmark       *string `json:"bookmark"`
	LifeTime       string  `json:"lifeTime"`
	LifeLeft       string  `json:"lifeLeft"`
	LifeLength     string  `json:"lifeLength"`
	CreatedByID    string  `json:"createdByID"`
	CreatedByName  string  `json:"createdByName"`
	ModifiedByID   string  `json:"modifiedByID"`
	ModifiedByName string  `json:"modifiedByName"`
	ModifiedTime   string  `json:"modifiedTime"`
	MaskID         string  `json:"maskID"`
}
